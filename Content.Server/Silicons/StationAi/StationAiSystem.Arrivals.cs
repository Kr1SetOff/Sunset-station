using Content.Server.Radio.EntitySystems;
using Content.Shared.GameTicking;
using Content.Shared.Roles;
using Content.Shared.Silicons.StationAi;

namespace Content.Server.Silicons.StationAi;

// 🌇Sunset🌇 - the Station AI automatically greets late-joining crew over the radio when they arrive.
public sealed partial class StationAiSystem
{
    [Dependency] private RadioSystem _radioArrivals = default!;

    private void InitializeArrivals()
    {
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnedAnnounceArrival);
    }

    private void OnPlayerSpawnedAnnounceArrival(PlayerSpawnCompleteEvent args)
    {
        // Round-start spawns aren't "arrivals" in the sense the AI would announce, and silent
        // spawns (admin/observer takeovers etc.) shouldn't page the whole crew either.
        if (!args.LateJoin || args.Silent)
            return;

        var jobTitle = args.JobId != null && _proto.TryIndex<JobPrototype>(args.JobId, out var job)
            ? job.LocalizedName
            : Loc.GetString("stationai-arrival-announcement-unknown-job");

        var message = Loc.GetString("stationai-arrival-announcement",
            ("name", MetaData(args.Mob).EntityName),
            ("job", jobTitle));

        var query = AllEntityQuery<StationAiCoreComponent>();
        while (query.MoveNext(out var coreUid, out var core))
        {
            if (_station.GetOwningStation(coreUid) != args.Station)
                continue;

            if (!TryGetHeld((coreUid, core), out var held))
                continue;

            _radioArrivals.SendRadioMessage(held.Value, message, "Common", held.Value);
        }
    }
}
