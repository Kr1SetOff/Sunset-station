using System.Numerics;
using Content.Shared.Drugs;
using Content.Shared.StatusEffectNew;
using Robust.Client.Player;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Client.Drugs;

/// <summary>
/// 🌇Sunset🌇 - while the local player has an active <see cref="HallucinationMobsComponent"/> (added
/// alongside the shared "hallucinations" status effect by <see cref="Content.Shared.Drugs.AddHallucinationTheme"/>),
/// periodically spawns a themed, purely client-side "illusion" entity nearby - it's never sent to the
/// server or any other client, so as far as the game is concerned it never existed. Reads the theme's
/// <see cref="HallucinationMobsComponent.Mob"/> fresh each time it spawns one, so it doesn't matter
/// whether that component landed before or after this system starts tracking the effect.
/// </summary>
public sealed class HallucinationMobSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private const float MinSpawnInterval = 12f;
    private const float MaxSpawnInterval = 25f;
    private const float IllusionLifetime = 4.5f;
    private const float SpawnDistance = 4f;

    private sealed class Tracker
    {
        public EntityUid Target;
        public float NextSpawnIn;
        public EntityUid? Illusion;
        public float IllusionTimeLeft;
    }

    // Keyed by the status effect entity (the one carrying HallucinationMobsComponent).
    private readonly Dictionary<EntityUid, Tracker> _tracked = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HallucinationMobsComponent, StatusEffectAppliedEvent>(OnApplied);
        SubscribeLocalEvent<HallucinationMobsComponent, StatusEffectRemovedEvent>(OnRemoved);

        SubscribeLocalEvent<HallucinationMobsComponent, StatusEffectRelayedEvent<LocalPlayerAttachedEvent>>(OnPlayerAttached);
        SubscribeLocalEvent<HallucinationMobsComponent, StatusEffectRelayedEvent<LocalPlayerDetachedEvent>>(OnPlayerDetached);
    }

    private void OnApplied(Entity<HallucinationMobsComponent> ent, ref StatusEffectAppliedEvent args)
    {
        if (_player.LocalEntity != args.Target)
            return;

        StartTracking(ent, args.Target);
    }

    private void OnRemoved(Entity<HallucinationMobsComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (_player.LocalEntity != args.Target)
            return;

        StopTracking(ent);
    }

    private void OnPlayerAttached(Entity<HallucinationMobsComponent> ent, ref StatusEffectRelayedEvent<LocalPlayerAttachedEvent> args)
    {
        if (_player.LocalEntity is { } local)
            StartTracking(ent, local);
    }

    private void OnPlayerDetached(Entity<HallucinationMobsComponent> ent, ref StatusEffectRelayedEvent<LocalPlayerDetachedEvent> args)
    {
        StopTracking(ent);
    }

    private void StartTracking(EntityUid statusEffect, EntityUid target)
    {
        if (_tracked.ContainsKey(statusEffect))
            return;

        _tracked[statusEffect] = new Tracker
        {
            Target = target,
            NextSpawnIn = _random.NextFloat(MinSpawnInterval / 2f, MaxSpawnInterval / 2f),
        };
    }

    private void StopTracking(EntityUid statusEffect)
    {
        if (!_tracked.Remove(statusEffect, out var tracker))
            return;

        if (tracker.Illusion is { } illusion)
            QueueDel(illusion);
    }

    private readonly List<EntityUid> _toStop = new();

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Collect first, remove after - StopTracking mutates _tracked, which we can't do while
        // enumerating it below.
        _toStop.Clear();

        foreach (var (statusEffect, tracker) in _tracked)
        {
            if (!Exists(tracker.Target))
            {
                _toStop.Add(statusEffect);
                continue;
            }

            if (tracker.Illusion is { } illusion)
            {
                tracker.IllusionTimeLeft -= frameTime;
                if (tracker.IllusionTimeLeft <= 0f || !Exists(illusion))
                {
                    QueueDel(illusion);
                    tracker.Illusion = null;
                    tracker.NextSpawnIn = _random.NextFloat(MinSpawnInterval, MaxSpawnInterval);
                    continue;
                }

                // Drift unsettlingly toward whoever's tripping instead of just standing there.
                var illusionXform = Transform(illusion);
                var targetCoords = Transform(tracker.Target).Coordinates;
                var toTarget = targetCoords.Position - illusionXform.Coordinates.Position;
                if (toTarget.LengthSquared() > 0.01f)
                    _transform.SetLocalPosition(illusion, illusionXform.Coordinates.Position + toTarget * (frameTime * 0.15f), illusionXform);

                continue;
            }

            tracker.NextSpawnIn -= frameTime;
            if (tracker.NextSpawnIn > 0f)
                continue;

            if (!TryComp<HallucinationMobsComponent>(statusEffect, out var theme))
            {
                // Component hasn't landed yet (or this hallucination has no theme for some reason) -
                // just wait for the next cycle instead of spawning something generic.
                tracker.NextSpawnIn = _random.NextFloat(MinSpawnInterval, MaxSpawnInterval);
                continue;
            }

            tracker.Illusion = SpawnIllusion(theme.Mob, tracker.Target);
            tracker.IllusionTimeLeft = IllusionLifetime;
        }

        foreach (var statusEffect in _toStop)
            StopTracking(statusEffect);
    }

    private EntityUid SpawnIllusion(string mob, EntityUid target)
    {
        var offset = _random.NextVector2(SpawnDistance, SpawnDistance);
        var targetCoords = Transform(target).Coordinates;
        var coords = targetCoords.WithPosition(targetCoords.Position + offset);

        // Purely client-side - EntityManager.SpawnEntity here never touches the network, so this
        // "mob" never exists for the server or anyone else, exactly as intended.
        return EntityManager.SpawnEntity(mob, coords);
    }
}
