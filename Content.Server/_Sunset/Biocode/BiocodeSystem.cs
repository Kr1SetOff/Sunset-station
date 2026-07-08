// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Chat;
using Content.Shared.Clothing;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared._Sunset.Biocode;
using Robust.Shared.Player;

namespace Content.Server._Sunset.Biocode;

/// <summary>
/// НОВАЯ МЕХАНИКА «Биокод» (см. <see cref="BiocodeComponent"/>).
/// Реализует установку биокода вербом, блокировку использования предмета всеми куклами кроме
/// куклы-владельца (отказ дублируется попапом на экране и сообщением в чат)
/// и самоуничтожение скафандра, надетого чужаком, с обратным отсчётом в чат.
///
/// Серверная система; всё исполняется в основном потоке ECS.
/// </summary>
public sealed class BiocodeSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly Robust.Shared.Audio.Systems.SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BiocodeComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);

        // Блокировка использования предмета чужими.
        SubscribeLocalEvent<BiocodeComponent, ActivatableUIOpenAttemptEvent>(OnUiOpenAttempt);
        SubscribeLocalEvent<BiocodeComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<BiocodeComponent, AttemptShootEvent>(OnAttemptShoot);

        // Скафандр: захват чужака и запрет снятия.
        SubscribeLocalEvent<BiocodeComponent, ClothingGotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<BiocodeComponent, BeingUnequippedAttemptEvent>(OnUnequipAttempt);
    }

    /// <summary>
    /// Верб «Установить биокод» — доступен, пока биокод не установлен, любому игроку (с сессией).
    /// Привязка идёт к самой кукле (EntityUid тела), а не к аккаунту.
    /// </summary>
    private void OnGetVerbs(Entity<BiocodeComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        // Уже привязан — верб не показываем.
        if (ent.Comp.OwnerEntity != null)
            return;

        if (!HasComp<ActorComponent>(args.User))
            return;

        var user = args.User;
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("biocode-verb-install"),
            Act = () => Install(ent, user),
        });
    }

    private void Install(Entity<BiocodeComponent> ent, EntityUid user)
    {
        if (ent.Comp.OwnerEntity != null)
            return;

        ent.Comp.OwnerEntity = user;
        ent.Comp.OwnerName = Name(user);

        _popup.PopupEntity(Loc.GetString("biocode-installed"), ent.Owner, user, PopupType.Medium);
        _audio.PlayPvs(ent.Comp.InstallSound, ent.Owner);
    }

    // --- Блокировка использования ---

    private void OnUiOpenAttempt(Entity<BiocodeComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (IsForeign(ent.Comp, args.User))
        {
            args.Cancel();
            DenyPopup(ent, args.User);
        }
    }

    private void OnUseInHand(Entity<BiocodeComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        if (IsForeign(ent.Comp, args.User))
        {
            args.Handled = true;
            DenyPopup(ent, args.User);
        }
    }

    private void OnAttemptShoot(Entity<BiocodeComponent> ent, ref AttemptShootEvent args)
    {
        if (args.Cancelled)
            return;

        if (IsForeign(ent.Comp, args.User))
        {
            args.Cancelled = true;
            DenyPopup(ent, args.User);
        }
    }

    // --- Скафандр ---

    private void OnGotEquipped(Entity<BiocodeComponent> ent, ref ClothingGotEquippedEvent args)
    {
        if (!ent.Comp.DetonateOnForeignWear || ent.Comp.Detonating)
            return;

        // Надел владелец или биокод ещё не установлен — ничего не делаем.
        if (!IsForeign(ent.Comp, args.Wearer))
            return;

        ent.Comp.Detonating = true;
        ent.Comp.DetonationTimer = ent.Comp.DetonationDelay;
        ent.Comp.AnnounceAccumulator = 0f;
        ent.Comp.LastAnnounced = -1;
        ent.Comp.TrappedWearer = args.Wearer;

        _popup.PopupEntity(Loc.GetString("biocode-suit-trapped"), args.Wearer, args.Wearer, PopupType.LargeCaution);
        _chat.TrySendInGameICMessage(ent.Owner, Loc.GetString("biocode-detonation-start"), InGameICChatType.Speak, false);
    }

    private void OnUnequipAttempt(Entity<BiocodeComponent> ent, ref BeingUnequippedAttemptEvent args)
    {
        if (!ent.Comp.DetonateOnForeignWear || ent.Comp.OwnerEntity == null)
            return;

        // Чужому снять нельзя — скафандр держит его.
        if (IsForeign(ent.Comp, args.UnEquipTarget))
            args.Cancel();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BiocodeComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.Detonating)
                continue;

            comp.DetonationTimer -= frameTime;

            var secondsLeft = (int) MathF.Ceiling(comp.DetonationTimer);
            if (secondsLeft > 0 && secondsLeft != comp.LastAnnounced)
            {
                comp.LastAnnounced = secondsLeft;
                _chat.TrySendInGameICMessage(
                    uid,
                    Loc.GetString("biocode-detonation-countdown", ("seconds", secondsLeft)),
                    InGameICChatType.Speak,
                    false);
            }

            if (comp.DetonationTimer <= 0f)
                Detonate((uid, comp));
        }
    }

    private void Detonate(Entity<BiocodeComponent> ent)
    {
        ent.Comp.Detonating = false;

        _chat.TrySendInGameICMessage(ent.Owner, Loc.GetString("biocode-detonation-boom"), InGameICChatType.Speak, false);

        _explosion.QueueExplosion(
            ent.Owner,
            ExplosionSystem.DefaultExplosionPrototypeId,
            ent.Comp.ExplosionTotalIntensity,
            ent.Comp.ExplosionSlope,
            ent.Comp.ExplosionMaxTileIntensity,
            maxTileBreak: 2);

        QueueDel(ent.Owner);
    }

    // --- Вспомогательное ---

    /// <summary>
    /// Чужая ли это кукла для предмета: биокод установлен и это не то самое тело, которое его
    /// привязало. Кто управляет телом — не важно: биокод по РП считывает ДНК тела.
    /// </summary>
    private bool IsForeign(BiocodeComponent comp, EntityUid user)
    {
        if (comp.OwnerEntity == null)
            return false;

        return user != comp.OwnerEntity.Value;
    }

    /// <summary>
    /// Отказ чужаку: попап на экране + то же сообщение строкой в чат (если это игрок).
    /// </summary>
    private void DenyPopup(Entity<BiocodeComponent> ent, EntityUid user)
    {
        var message = Loc.GetString("biocode-locked");
        _popup.PopupEntity(message, ent.Owner, user, PopupType.MediumCaution);

        if (TryComp<ActorComponent>(user, out var actor))
            _chatManager.DispatchServerMessage(actor.PlayerSession, message);
    }
}
