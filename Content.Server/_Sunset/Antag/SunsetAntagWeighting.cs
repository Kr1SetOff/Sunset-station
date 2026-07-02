using System.Linq;
using Content.Server.Antag.Components;

namespace Content.Server._Sunset.Antag;

/// <summary>
/// Boosty sponsor-tier weighting for antagonist selection. Tiers 1-4 get a bigger chance at specific
/// (or, for tier 4, all) antag roles by being duplicated into the preferred-pool lottery in
/// <see cref="Content.Server.Antag.AntagSelectionSystem.GetPlayerPool"/> - see that call site for how
/// the returned multiplier is used. Tier 5's 99% guarantee is NOT handled here - it's a separate,
/// pool-bypassing forced-assignment pass, since a guarantee is categorically different from "better odds".
/// </summary>
public static class SunsetAntagWeighting
{
    private static readonly HashSet<string> Tier1Roles = new()
    {
        "Thief", "Zombie", "InitialInfected",
    };

    private static readonly HashSet<string> Tier2Roles = new(Tier1Roles)
    {
        "Traitor", "TraitorSleeper", "Nukeops", "NukeopsMedic", "NukeopsCommander",
    };

    private static readonly HashSet<string> Tier3ExplicitRoles = new(Tier2Roles)
    {
        "Vampire", "Thrall", "CosmicAntagCultist", "Devil", "HeadRev", "Rev",
    };

    /// <summary>
    /// Returns how many EXTRA times a sponsor's session should be duplicated into the preferred pool for
    /// this antag definition (0 = no boost). Tier 5 always returns 0 here - see the forced-assignment pass instead.
    /// </summary>
    public static int GetWeightMultiplier(int tier, AntagSelectionDefinition def)
    {
        switch (tier)
        {
            case 1:
                return MatchesAny(def, Tier1Roles) ? 2 : 0;
            case 2:
                return MatchesAny(def, Tier2Roles) ? 3 : 0;
            case 3:
                // "all ghost-role-spawned antagonists" is structural (any def with a spawner fallback),
                // plus the explicitly requested named roles.
                return MatchesAny(def, Tier3ExplicitRoles) || def.SpawnerPrototype != null ? 4 : 0;
            case 4:
                // Broader (every antag role) but shallower per-role than tier 3's curated boost, and never a guarantee.
                return 3;
            default:
                return 0;
        }
    }

    private static bool MatchesAny(AntagSelectionDefinition def, HashSet<string> roles)
    {
        return def.PrefRoles.Any(r => roles.Contains(r.Id));
    }
}
