using Content.Server.Ghost.Roles.Components;
using Content.Server.Ghost.Roles.Raffles;
using Content.Shared.Roles.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.Ghost.Roles;

// 🌇Sunset🌇 - auto-upgrades the raffle decider for antagonist ghost roles (nukeops reinforcement, wizard,
// revenant, dragon, etc.) to the sponsor-weighted one, without having to touch every antag prototype's
// raffle config. Kept in its own partial file matching this repo's convention (see AntagSelectionSystem.Sunset.cs).
public sealed partial class GhostRoleSystem
{
    private const string SunsetSponsorAntagDeciderId = "sunsetSponsorAntag";

    /// <summary>
    /// Whether this ghost role hands out an antagonist mind role. Determined by checking the mind role
    /// prototypes it would grant (see <see cref="GhostRoleComponent.MindRoles"/>) for
    /// <see cref="MindRoleComponent.Antag"/> - the same flag <c>SharedRoleSystem.MindIsAntagonist</c>
    /// checks post-assignment, just read off the prototype ahead of time since no mind exists yet mid-raffle.
    /// </summary>
    private bool IsSunsetAntagGhostRole(GhostRoleComponent ghostRole)
    {
        foreach (var mindRoleId in ghostRole.MindRoles)
        {
            if (!_prototype.TryIndex(mindRoleId, out var mindRoleProto))
                continue;

            if (mindRoleProto.TryGetComponent<MindRoleComponent>(out var mindRoleComp, _ent.ComponentFactory)
                && mindRoleComp.Antag)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Picks which raffle decider to actually use for this ghost role - the sponsor-weighted one for
    /// antagonist ghost roles, otherwise whatever the prototype configured (normally "default"/Rng).
    /// A ghost role that explicitly configured a NON-default decider keeps it - that's how roles
    /// with their own sponsor odds (e.g. the blood worm's "sunsetBloodWormRaffle") opt out of the
    /// generic auto-upgrade.
    /// </summary>
    private ProtoId<GhostRoleRaffleDeciderPrototype> GetSunsetRaffleDecider(
        GhostRoleComponent ghostRole,
        ProtoId<GhostRoleRaffleDeciderPrototype> configured)
    {
        if (configured != "default")
            return configured;

        return IsSunsetAntagGhostRole(ghostRole) ? SunsetSponsorAntagDeciderId : configured;
    }
}
