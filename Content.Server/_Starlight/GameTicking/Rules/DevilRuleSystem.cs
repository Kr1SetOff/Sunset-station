using Content.Server.Antag;
using Content.Server._Starlight.GameTicking.Rules.Components;
using Content.Server.GameTicking.Rules;
using Content.Server.Roles;
using Content.Shared._Starlight.Devil;
using Content.Shared._Starlight.Roles.Components;
using Content.Shared._Starlight.Vampire.Components;
using Content.Shared.Clumsy;
using Content.Shared.Roles;
using Content.Shared.Speech.Components;

namespace Content.Server._Starlight.GameTicking.Rules;

public sealed partial class DevilRuleSystem : GameRuleSystem<DevilRuleComponent>
{
    [Dependency] private AntagSelectionSystem _antag = default!;
    [Dependency] private SharedRoleSystem _role = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DevilRuleComponent, AfterAntagEntitySelectedEvent>(AfterAntagSelected);

        SubscribeLocalEvent<DevilRoleComponent, GetBriefingEvent>(OnGetBriefing);

        SubscribeLocalEvent<RoleRemovedEvent>(OnRoleRemoved);
    }

    private void AfterAntagSelected(EntityUid uid, DevilRuleComponent comp, ref AfterAntagEntitySelectedEvent args)
    {
        EnsureComp<DevilComponent>(args.EntityUid);
        _antag.SendBriefing(args.EntityUid, MakeBriefing(), null, null);
    }

    private void OnGetBriefing(EntityUid uid, DevilRoleComponent comp, ref GetBriefingEvent args) => args.Append(MakeBriefing());

    private string MakeBriefing() => Loc.GetString("devil-role-briefing");

    /// <summary>
    /// Strips the Devil body components (and with them, via DevilSystem.OnShutdown, every action
    /// and cosmetic effect the role granted) once no Devil mind role remains on the mind - e.g.
    /// after an admin uses the "Remove antag" verb.
    /// </summary>
    private void OnRoleRemoved(RoleRemovedEvent args)
    {
        if (_role.MindHasRole<DevilRoleComponent>(args.MindId))
            return;

        if (args.Mind.OwnedEntity is not { } body)
            return;

        RemComp<DevilComponent>(body);
        RemComp<ActiveListenerComponent>(body);
        RemComp<UnholyComponent>(body);
        RemComp<ClumsyComponent>(body);
    }
}
