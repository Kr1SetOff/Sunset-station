using Content.Server._Sunset.StationAi;
using Content.Server.Administration;
using Content.Server.Radio.EntitySystems;
using Content.Shared._Sunset.StationAi;
using Content.Shared.GameTicking;
using Content.Shared.Popups;
using Content.Shared.Roles;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Player;

namespace Content.Server.Silicons.StationAi;

// 🌇Sunset🌇 - the Station AI automatically greets late-joining crew over the radio when they arrive.
// The AI player can customize the greeting template via an action ({name}/{job} placeholders).
public sealed partial class StationAiSystem
{
    [Dependency] private RadioSystem _radioArrivals = default!;
    [Dependency] private QuickDialogSystem _quickDialog = default!;

    private const int MaxGreetingLength = 200;

    private void InitializeArrivals()
    {
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnedAnnounceArrival);
        SubscribeLocalEvent<StationAiHeldComponent, StationAiCustomGreetingActionEvent>(OnCustomizeGreeting);
    }

    private void OnCustomizeGreeting(Entity<StationAiHeldComponent> ent, ref StationAiCustomGreetingActionEvent args)
    {
        if (args.Handled || !TryComp<ActorComponent>(ent.Owner, out var actor))
            return;

        args.Handled = true;
        var uid = ent.Owner;

        _quickDialog.OpenDialog(actor.PlayerSession,
            Loc.GetString("stationai-greeting-dialog-title"),
            Loc.GetString("stationai-greeting-dialog-prompt"),
            (LongString greeting) =>
            {
                var text = greeting.String.Trim();
                if (text.Length > MaxGreetingLength)
                    text = text[..MaxGreetingLength];

                var comp = EnsureComp<StationAiCustomGreetingComponent>(uid);
                comp.Greeting = text.Length == 0 ? null : text;

                _popups.PopupEntity(text.Length == 0
                        ? Loc.GetString("stationai-greeting-reset")
                        : Loc.GetString("stationai-greeting-set", ("greeting", text)),
                    uid, uid);
            });
    }

    private void OnPlayerSpawnedAnnounceArrival(PlayerSpawnCompleteEvent args)
    {
        // Round-start spawns aren't "arrivals" in the sense the AI would announce, and silent
        // spawns (admin/observer takeovers etc.) shouldn't page the whole crew either.
        if (!args.LateJoin || args.Silent)
            return;

        var name = MetaData(args.Mob).EntityName;
        var jobTitle = args.JobId != null && _proto.TryIndex<JobPrototype>(args.JobId, out var job)
            ? job.LocalizedName
            : Loc.GetString("stationai-arrival-announcement-unknown-job");

        var query = AllEntityQuery<StationAiCoreComponent>();
        while (query.MoveNext(out var coreUid, out var core))
        {
            if (_station.GetOwningStation(coreUid) != args.Station)
                continue;

            if (!TryGetHeld((coreUid, core), out var held))
                continue;

            var message = TryComp<StationAiCustomGreetingComponent>(held.Value, out var custom)
                          && !string.IsNullOrWhiteSpace(custom.Greeting)
                ? custom.Greeting.Replace("{name}", name).Replace("{job}", jobTitle)
                : Loc.GetString("stationai-arrival-announcement", ("name", name), ("job", jobTitle));

            _radioArrivals.SendRadioMessage(held.Value, message, "Common", held.Value);
        }
    }
}
