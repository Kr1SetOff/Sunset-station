// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.FixedPoint;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared._Sunset.Genetics.Components;

namespace Content.Server._Sunset.Genetics;

/// <summary>
///     Genetic instability effects. Mirrors the SS13 thresholds:
///     0–20 nothing, 25–35 burns, 40–65 deformity (cellular damage), 70+ lethal.
/// </summary>
public sealed partial class GeneticsSystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;

    private const float UpdateInterval = 5f;
    private float _accumulator;

    // Thresholds.
    private const float BurnThreshold = 25f;
    private const float DeformThreshold = 40f;
    private const float LethalThreshold = 70f;

    private void InitializeInstability()
    {
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _accumulator += frameTime;
        if (_accumulator < UpdateInterval)
            return;
        _accumulator -= UpdateInterval;

        var query = EntityQueryEnumerator<GenomeComponent>();
        while (query.MoveNext(out var uid, out var genome))
        {
            ApplyInstabilityEffects(uid, genome);
        }
    }

    private void ApplyInstabilityEffects(EntityUid uid, GenomeComponent genome)
    {
        if (genome.Instability < BurnThreshold)
            return;

        DamageSpecifier damage;
        if (genome.Instability >= LethalThreshold)
            damage = MakeDamage("Cellular", 25);
        else if (genome.Instability >= DeformThreshold)
            damage = MakeDamage("Cellular", 6);
        else
            damage = MakeDamage("Heat", 4);

        _damageable.TryChangeDamage(uid, damage, ignoreResistances: true);
    }

    private static DamageSpecifier MakeDamage(string type, int amount)
    {
        return new DamageSpecifier
        {
            DamageDict = new Dictionary<string, FixedPoint2> { { type, amount } },
        };
    }
}
