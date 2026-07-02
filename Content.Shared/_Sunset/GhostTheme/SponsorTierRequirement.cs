using Content.Shared._Starlight.Abstract.Conditions;
using Content.Shared._Sunset.SponsorTier;
using Robust.Shared.Player;

namespace Content.Shared._Sunset.GhostTheme;

/// <summary>
/// Gates a ghost theme (or anything else using <see cref="BaseRequirement"/>) behind a minimum Boosty
/// sponsor tier. Modeled on <see cref="DiscordRolesRequirement"/> but reads from our own local
/// sponsor-tier service instead of the external _NullLink cluster.
/// </summary>
public sealed partial class SponsorTierRequirement : BaseRequirement
{
    [Dependency] private ISunsetSponsorTierReader _sponsorTiers = default!;

    [DataField(required: true)]
    public int MinTier;

    public override string GetRequirementDescription()
    {
        base.GetRequirementDescription();
        return Loc.GetString("sunset-sponsor-tier-requirement-fail", ("tier", MinTier));
    }

    public override bool Handle(ICommonSession user)
    {
        base.Handle(user);
        return _sponsorTiers.GetSponsorTier(user) >= MinTier;
    }
}
