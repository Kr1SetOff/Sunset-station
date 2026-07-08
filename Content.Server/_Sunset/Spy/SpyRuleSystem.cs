using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Server.Roles;
using Content.Shared._Sunset.Spy;

namespace Content.Server._Sunset.Spy;

/// <summary>
/// A /tg/station-style Spy Thief antag: a solo corporate-espionage antagonist that steals a set of
/// objective items (see Resources/Prototypes/_Sunset/Spy/objectives.yml) using gear bought from the
/// Spy Uplink (Resources/Prototypes/_Sunset/Spy/uplink_items.yml, uplink_catalog.yml), and wins by
/// escaping alive with the loot. All the actual selection/objective/gear logic is the same generic
/// AntagSelection + AntagRandomObjectives + StealCondition + EscapeShuttleCondition machinery Thief
/// and Traitor use - this system only sends the round-start briefing, mirroring ThiefRuleSystem.
/// </summary>
public sealed partial class SpyRuleSystem : GameRuleSystem<SpyRuleComponent>
{
    [Dependency] private AntagSelectionSystem _antag = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpyRuleComponent, AfterAntagEntitySelectedEvent>(AfterAntagSelected);
        SubscribeLocalEvent<SpyRoleComponent, GetBriefingEvent>(OnGetBriefing);
    }

    private void AfterAntagSelected(Entity<SpyRuleComponent> rule, ref AfterAntagEntitySelectedEvent args)
    {
        _antag.SendBriefing(args.EntityUid, MakeBriefing(), null, null);
    }

    private void OnGetBriefing(Entity<SpyRoleComponent> role, ref GetBriefingEvent args)
    {
        args.Append(MakeBriefing());
    }

    private string MakeBriefing() => Loc.GetString("spy-role-greeting");
}
