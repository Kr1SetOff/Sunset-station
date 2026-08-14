using Content.Server.Bible.Components;
using Content.Shared._Goobstation.Religion;
using Content.Shared.DoAfter;
using Content.Shared.Item;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._Goobstation.Religion.AlternatePrayable;

/// <summary>
/// Ported from Goob-Station's Religion system, adapted to check the server-only BibleUserComponent
/// directly instead of running in Shared code.
/// </summary>
public sealed class AlternatePrayableSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AlternatePrayableComponent, GetVerbsEvent<InteractionVerb>>(OnGetVerbs);
        SubscribeLocalEvent<AlternatePrayableComponent, AlternatePrayDoAfterEvent>(OnPrayDoAfter);
    }

    private void OnGetVerbs(Entity<AlternatePrayableComponent> ent, ref GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess
            || !args.CanInteract
            || args.Using is not { } item
            || !HasComp<ItemComponent>(item))
            return;

        if (ent.Comp.RequireBibleUser && !HasComp<BibleUserComponent>(args.User))
            return;

        var user = args.User;
        var target = ent.Owner;
        var comp = ent.Comp;

        args.Verbs.Add(new InteractionVerb
        {
            Act = () => StartPrayDoAfter(user, target, comp),
            Text = Loc.GetString("alternate-pray-prompt", ("item", target)),
            Icon = new SpriteSpecifier.Rsi(new ResPath("Objects/Specific/Chapel/bible.rsi"), "icon"),
            Priority = 30,
        });
    }

    private void StartPrayDoAfter(EntityUid user, EntityUid target, AlternatePrayableComponent comp)
    {
        if (_timing.CurTime > comp.NextPopup)
        {
            _popup.PopupEntity(Loc.GetString("alternate-pray-start", ("user", Name(user)), ("item", Name(target))), user);
            comp.NextPopup = _timing.CurTime + comp.PopupDelay;
        }

        var doAfterArgs = new DoAfterArgs(EntityManager, user, comp.PrayDoAfterDuration, new AlternatePrayDoAfterEvent(), target, target, target)
        {
            BreakOnDamage = true,
            BreakOnDropItem = true,
            BreakOnHandChange = true,
            NeedHand = true,
            BlockDuplicate = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnPrayDoAfter(EntityUid uid, AlternatePrayableComponent comp, ref AlternatePrayDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || TerminatingOrDeleted(args.User))
            return;

        var ev = new AlternatePrayEvent(args.User);
        RaiseLocalEvent(uid, ref ev);

        args.Repeat = comp.RepeatPrayer;
    }
}
