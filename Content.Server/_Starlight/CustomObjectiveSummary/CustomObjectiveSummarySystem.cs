using Content.Server.Administration.Logs;
using Content.Shared._Starlight.CustomObjectiveSummary;
using Content.Shared._Starlight.Railroading;
using Content.Shared._Starlight.Railroading.Components;
using Content.Shared.Database;
using Content.Shared.Mind;
using Robust.Server.Player;
using Robust.Shared.Network;

namespace Content.Server._Starlight.CustomObjectiveSummary;

public sealed partial class CustomObjectiveSummarySystem : EntitySystem
{
    [Dependency] private IServerNetManager _net = default!;
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private IAdminLogManager _adminLog = default!;
    [Dependency] private IPlayerManager _players = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<EvacShuttleLeftEvent>(OnEvacShuttleLeft);

        _net.RegisterNetMessage<CustomObjectiveClientSetObjective>(OnCustomObjectiveFeedback);
    }

    /// <summary>
    /// 🌇Sunset🌇 - Story/RP objectives (RetrieveContrabandObjective, ArmsDealerObjective, etc. - the
    /// open-ended ones this summary feature actually exists for) are granted through the Railroading
    /// system as cards on the mind's owned entity, not through MindComponent.Objectives. Checking only
    /// Objectives.Count meant the exact players who need to explain what they did never got the
    /// "write a summary" prompt in the first place, and had their submission silently dropped even if
    /// they somehow triggered it anyway.
    /// </summary>
    private bool HasAnyObjective(Entity<MindComponent> mind)
    {
        if (mind.Comp.Objectives.Count > 0)
            return true;

        if (mind.Comp.OwnedEntity is not { } ent || !TryComp<RailroadableComponent>(ent, out var railroadable))
            return false;

        return railroadable.ActiveCard != null || railroadable.Completed is { Count: > 0 };
    }

    private void OnCustomObjectiveFeedback(CustomObjectiveClientSetObjective msg)
    {
        if (!_mind.TryGetMind(msg.MsgChannel.UserId, out var mind))
            return;

        if (!HasAnyObjective(mind.Value))
            return;

        var comp = EnsureComp<CustomObjectiveSummaryComponent>(mind.Value);

        comp.ObjectiveSummary = msg.Summary;
        Dirty(mind.Value.Owner, comp);

        _adminLog.Add(LogType.ObjectiveSummary, $"{ToPrettyString(mind.Value.Comp.OwnedEntity)} wrote objective summery: {msg.Summary}");
    }

    private void OnEvacShuttleLeft(EvacShuttleLeftEvent args)
    {
        var allMinds = _mind.GetAliveHumans();

        // Assumes the assistant is still there at the end of the round.
        foreach (var mind in allMinds)
        {
            // Only send the popup to people with objectives.
            if (!HasAnyObjective(mind))
                continue;

            // Get the session from the mind's owned entity
            if (mind.Comp.OwnedEntity == null || !_players.TryGetSessionByEntity(mind.Comp.OwnedEntity.Value, out var session))
                continue;

            RaiseNetworkEvent(new CustomObjectiveSummaryOpenMessage(), session);
        }
    }
}
