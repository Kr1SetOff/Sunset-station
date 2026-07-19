// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared._Sunset.Genetics;
using Content.Shared._Sunset.Genetics.Components;
using Content.Shared.Interaction;
using Content.Shared.Physics;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;

namespace Content.Server._Sunset.Genetics;

/// <summary>
///     Handles the active genetic powers granted by mutation actions: the telekinetic throw, pyrokinesis
///     (ignite) and the short-range blink teleport. The events are raised on the performer, who always
///     carries a <see cref="GenomeComponent"/>, so we subscribe directed on that component.
/// </summary>
public sealed class GeneticPowerSystem : EntitySystem
{
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;

    /// <summary>How hard the telekinesis gene throws its target.</summary>
    private const float TelekinesisThrowSpeed = 6f;

    /// <summary>How far (in tiles) the telekinesis gene throws its target.</summary>
    private const float TelekinesisThrowDistance = 8f;

    /// <summary>How long the telekinesis gene knocks its target down for.</summary>
    private static readonly TimeSpan TelekinesisKnockdown = TimeSpan.FromSeconds(2);

    /// <summary>Fire stacks added by the pyrokinesis gene.</summary>
    private const float PyrokinesisFireStacks = 4f;

    /// <summary>How far (in tiles) the blink gene can teleport its user.</summary>
    private const float BlinkRange = 6f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GenomeComponent, GeneTelekinesisActionEvent>(OnTelekinesis);
        SubscribeLocalEvent<GenomeComponent, GenePyrokinesisActionEvent>(OnPyrokinesis);
        SubscribeLocalEvent<GenomeComponent, GeneBlinkActionEvent>(OnBlink);
    }

    private void OnTelekinesis(Entity<GenomeComponent> ent, ref GeneTelekinesisActionEvent args)
    {
        if (args.Handled || args.Target == ent.Owner)
            return;

        // Hurl the target away from the caster.
        var casterPos = _transform.GetMapCoordinates(ent.Owner);
        var targetPos = _transform.GetMapCoordinates(args.Target);
        if (casterPos.MapId != targetPos.MapId)
            return;

        var direction = targetPos.Position - casterPos.Position;
        if (direction.LengthSquared() < 0.01f)
            direction = new Vector2(0, 1);

        // Knock them down so the toss reads clearly, but don't scatter their held items.
        _stun.TryKnockdown(args.Target, TelekinesisKnockdown, refresh: true, drop: false);
        _throwing.TryThrow(args.Target, direction.Normalized() * TelekinesisThrowDistance, TelekinesisThrowSpeed, ent.Owner);

        args.Handled = true;
    }

    private void OnPyrokinesis(Entity<GenomeComponent> ent, ref GenePyrokinesisActionEvent args)
    {
        if (args.Handled || args.Target == ent.Owner)
            return;

        var flammable = EnsureComp<FlammableComponent>(args.Target);
        _flammable.AdjustFireStacks(args.Target, PyrokinesisFireStacks, flammable, ignite: true);
        args.Handled = true;
    }

    private void OnBlink(Entity<GenomeComponent> ent, ref GeneBlinkActionEvent args)
    {
        if (args.Handled)
            return;

        var transform = Transform(ent.Owner);
        if (transform.MapID != _transform.GetMapId(args.Target) ||
            !_interaction.InRangeUnobstructed(ent.Owner, args.Target, BlinkRange, CollisionGroup.Opaque, popup: true))
        {
            return;
        }

        _transform.SetCoordinates(ent.Owner, args.Target);
        _transform.AttachToGridOrMap(ent.Owner, transform);
        args.Handled = true;
    }
}
