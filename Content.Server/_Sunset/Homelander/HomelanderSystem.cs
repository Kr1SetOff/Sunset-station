using System.Numerics;
using Content.Server.Antag;
using Content.Shared._Sunset.Homelander;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Jittering;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Server._Sunset.Homelander;

/// <summary>
/// Drives the Homelander antagonist: applies his damage resistance on selection, heat vision (an
/// instant burning line rather than sunset-station's cursor-following beam - see
/// HomelanderHeatVisionEvent), an intimidating scream that knocks nearby crew down, a ground-pound
/// shockwave (replacing the flight-landing-triggered one, since this fork has no Flight system), and
/// a passive fear aura that makes nearby crew jitter/stutter.
/// </summary>
public sealed class HomelanderSystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private readonly SharedStutteringSystem _stutter = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    private EntityQuery<MobStateComponent> _mobQuery;

    private DamageSpecifier _wallDamage = default!;

    private const float BeamThickness = 0.3f;
    private const string WallTag = "Wall";

    private const float FearRange = 4f;
    private const float FearInterval = 2.5f;

    public override void Initialize()
    {
        base.Initialize();

        _mobQuery = GetEntityQuery<MobStateComponent>();
        _wallDamage = new DamageSpecifier { DamageDict = new() { { "Structural", 50 } } };

        SubscribeLocalEvent<HomelanderComponent, AfterAntagEntitySelectedEvent>(OnAntagSelected);
        SubscribeLocalEvent<HomelanderHeatVisionEvent>(OnHeatVision);
        SubscribeLocalEvent<HomelanderScreamEvent>(OnScream);
        SubscribeLocalEvent<HomelanderShockwaveEvent>(OnShockwave);
    }

    private void OnAntagSelected(Entity<HomelanderComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        _damageable.SetDamageModifierSetId(ent.Owner, ent.Comp.DamageModifierSet);
        _antag.SendBriefing(ent.Owner, Loc.GetString("homelander-role-greeting"), null, null);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HomelanderComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            comp.FearAccumulator += frameTime;
            if (comp.FearAccumulator < FearInterval)
                continue;

            comp.FearAccumulator = 0f;

            foreach (var other in _lookup.GetEntitiesInRange(uid, FearRange))
            {
                if (other == uid || !HasComp<HumanoidAppearanceComponent>(other))
                    continue;

                if (_mobQuery.TryGetComponent(other, out var mob) && mob.CurrentState == MobState.Dead)
                    continue;

                _jitter.DoJitter(other, TimeSpan.FromSeconds(2), true);
                _stutter.DoStutter(other, TimeSpan.FromSeconds(4), true);
            }
        }
    }

    private void OnHeatVision(HomelanderHeatVisionEvent ev)
    {
        if (ev.Handled)
            return;

        ev.Handled = true;

        var uid = ev.Performer;
        var xform = Transform(uid);
        var gazerPos = _xform.GetWorldPosition(xform);
        var facing = xform.LocalRotation.ToWorldVec();
        if (facing.LengthSquared() < 0.01f)
            facing = new Vector2(0, -1);
        facing = Vector2.Normalize(facing);

        var box = new Box2(gazerPos + new Vector2(0f, -BeamThickness), gazerPos + new Vector2(ev.Range, BeamThickness));
        var boxRot = new Box2Rotated(box, facing.ToAngle(), gazerPos);

        foreach (var hit in _lookup.GetEntitiesIntersecting(xform.MapID, boxRot, LookupFlags.Dynamic | LookupFlags.Static))
        {
            if (hit == uid)
                continue;

            if (_mobQuery.HasComponent(hit))
            {
                _damageable.TryChangeDamage(hit, ev.Damage, origin: uid);
                continue;
            }

            if (_tag.HasTag(hit, WallTag))
                _damageable.TryChangeDamage(hit, _wallDamage, ignoreResistances: true, origin: uid);
        }
    }

    private void OnScream(HomelanderScreamEvent ev)
    {
        if (ev.Handled)
            return;

        ev.Handled = true;

        var uid = ev.Performer;
        foreach (var other in _lookup.GetEntitiesInRange(uid, ev.Range))
        {
            if (other == uid || !HasComp<HumanoidAppearanceComponent>(other))
                continue;

            if (_mobQuery.TryGetComponent(other, out var mob) && mob.CurrentState == MobState.Dead)
                continue;

            _stun.TryKnockdown(other, TimeSpan.FromSeconds(ev.KnockdownSeconds), true);
        }
    }

    private void OnShockwave(HomelanderShockwaveEvent ev)
    {
        if (ev.Handled)
            return;

        ev.Handled = true;

        var uid = ev.Performer;
        var mapPos = _xform.GetMapCoordinates(uid);
        if (mapPos.MapId == MapId.Nullspace)
            return;

        foreach (var (ent, physics) in _lookup.GetEntitiesInRange<PhysicsComponent>(mapPos, ev.Range,
                     flags: LookupFlags.Dynamic | LookupFlags.Sundries))
        {
            if (ent == uid || physics.BodyType == BodyType.Static)
                continue;

            var displacement = _xform.GetWorldPosition(ent) - mapPos.Position;
            var dist = displacement.Length();
            if (dist < 0.01f)
                continue;

            _physics.ApplyLinearImpulse(ent, displacement / dist * ev.Force * physics.Mass, body: physics);

            if (HasComp<HumanoidAppearanceComponent>(ent))
                _stun.TryKnockdown(ent, TimeSpan.FromSeconds(ev.KnockdownSeconds), true);
        }
    }
}
