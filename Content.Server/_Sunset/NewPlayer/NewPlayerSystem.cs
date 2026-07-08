// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Players.PlayTimeTracking;
using Content.Shared._Sunset.NewPlayer;
using Content.Shared.GameTicking;
using Robust.Server.Player;
using Robust.Shared.Player;

namespace Content.Server._Sunset.NewPlayer;

/// <summary>
/// НОВОЕ. Считает общий налёт игрока на сервере и держит <see cref="NewPlayerComponent"/> в актуальном
/// состоянии: ставит флаг новичка, пока налёт меньше порога, и снимает его при достижении порога.
/// Компонент навешивается на тело при каждом заспавне персонажа (<see cref="PlayerSpawnCompleteEvent"/>) -
/// без этого иконка/осмотр никогда не появляются, т.к. компонент больше нигде не добавляется ни в
/// прототипах, ни в другом коде. Пересчёт также происходит при вселении в тело и при каждом обновлении
/// налёта (<see cref="PlayTimeTrackingManager.SessionPlayTimeUpdated"/>), поэтому иконка и осмотр
/// обновляются сами по ходу раунда.
/// </summary>
public sealed class NewPlayerSystem : EntitySystem
{
    [Dependency] private readonly PlayTimeTrackingManager _playTime = default!;
    [Dependency] private readonly IPlayerManager _players = default!;

    /// <summary>
    /// Как часто (в секундах) перепроверять налёт у всех новичков, чтобы поймать переход через порог
    /// прямо во время раунда.
    /// </summary>
    private const float RefreshInterval = 60f;

    private float _accumulator;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NewPlayerComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);

        _playTime.SessionPlayTimeUpdated += OnSessionPlayTimeUpdated;
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _playTime.SessionPlayTimeUpdated -= OnSessionPlayTimeUpdated;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _accumulator += frameTime;
        if (_accumulator < RefreshInterval)
            return;

        _accumulator = 0f;

        var query = EntityQueryEnumerator<NewPlayerComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_players.TryGetSessionByEntity(uid, out var session))
                UpdateState((uid, comp), session);
        }
    }

    private void OnPlayerAttached(Entity<NewPlayerComponent> ent, ref PlayerAttachedEvent args)
    {
        UpdateState(ent, args.Player);
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        var comp = EnsureComp<NewPlayerComponent>(args.Mob);
        UpdateState((args.Mob, comp), args.Player);
    }

    private void OnSessionPlayTimeUpdated(ICommonSession session)
    {
        if (session.AttachedEntity is not { } uid)
            return;

        if (!TryComp<NewPlayerComponent>(uid, out var comp))
            return;

        UpdateState((uid, comp), session);
    }

    private void UpdateState(Entity<NewPlayerComponent> ent, ICommonSession session)
    {
        // Налёт ещё не загружен из БД — пересчитаем позже, при следующем обновлении.
        if (!_playTime.TryGetTrackerTimes(session, out _))
            return;

        // Сбрасываем активные таймеры в хранилище, чтобы прочитать максимально свежее значение.
        _playTime.FlushTracker(session);
        var playtime = _playTime.GetOverallPlaytime(session);
        var isNewbie = playtime < ent.Comp.Threshold;

        if (ent.Comp.Playtime == playtime && ent.Comp.IsNewbie == isNewbie)
            return;

        ent.Comp.Playtime = playtime;
        ent.Comp.IsNewbie = isNewbie;
        Dirty(ent);
    }
}
