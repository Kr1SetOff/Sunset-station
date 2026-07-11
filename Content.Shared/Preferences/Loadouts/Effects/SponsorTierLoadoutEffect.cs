using System.Diagnostics.CodeAnalysis;
using Content.Shared._Sunset.SponsorTier;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Shared.Preferences.Loadouts.Effects;

/// <summary>
/// 🌇Sunset🌇 - restricts a loadout item to players with a minimum Boosty sponsor tier. Deliberately its
/// own effect rather than reusing JobRequirementLoadoutEffect - that one no-ops entirely whenever
/// game.role_loadout_timers is disabled, which is meant to waive playtime requirements, not sponsor
/// perks, and would otherwise let anyone get sponsor-only items just by flipping that cvar.
/// </summary>
public sealed partial class SponsorTierLoadoutEffect : LoadoutEffect
{
    [DataField(required: true)]
    public int MinTier;

    public override bool Validate(
        HumanoidCharacterProfile profile,
        RoleLoadout loadout,
        ICommonSession? session,
        IDependencyCollection collection,
        out FormattedMessage reason)
    {
        var tier = session is null ? 0 : collection.Resolve<ISunsetSponsorTierReader>().GetSponsorTier(session);
        var success = tier >= MinTier;

        reason = FormattedMessage.FromMarkupPermissive(Loc.GetString(
            success ? "loadout-sponsor-tier-restriction-pass" : "loadout-sponsor-tier-restriction-fail",
            ("tier", MinTier)));

        return success;
    }
}
