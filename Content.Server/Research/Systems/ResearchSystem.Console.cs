using Content.Server.Power.EntitySystems;
using Content.Server.Research.Components;
using Content.Server._Starlight.Achievement; // Starlight: Achievements
using Content.Server.Station.Systems; // Starlight: Achievements
using Content.Shared.UserInterface;
using Content.Shared.Access.Components;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.IdentityManagement;
using Content.Shared.Research.Components;
using Content.Shared.Research.Prototypes;
using System.Linq;
using Content.Shared.Radio;

namespace Content.Server.Research.Systems;

public sealed partial class ResearchSystem
{
    [Dependency] private EmagSystem _emag = default!;
    [Dependency] private StationSystem _station = default!; // Starlight: Achievements
    [Dependency] private AchievementSystem _achievements = default!; // Starlight: Achievements

    private void InitializeConsole()
    {
        SubscribeLocalEvent<ResearchConsoleComponent, ConsoleUnlockTechnologyMessage>(OnConsoleUnlock);
        SubscribeLocalEvent<ResearchConsoleComponent, BeforeActivatableUIOpenEvent>(OnConsoleBeforeUiOpened);
        SubscribeLocalEvent<ResearchConsoleComponent, ResearchServerPointsChangedEvent>(OnPointsChanged);
        SubscribeLocalEvent<ResearchConsoleComponent, ResearchRegistrationChangedEvent>(OnConsoleRegistrationChanged);
        SubscribeLocalEvent<ResearchConsoleComponent, TechnologyDatabaseModifiedEvent>(OnConsoleDatabaseModified);
        SubscribeLocalEvent<ResearchConsoleComponent, TechnologyDatabaseSynchronizedEvent>(OnConsoleDatabaseSynchronized);
        SubscribeLocalEvent<ResearchConsoleComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void OnConsoleUnlock(EntityUid uid, ResearchConsoleComponent component, ConsoleUnlockTechnologyMessage args)
    {
        var act = args.Actor;

        if (!this.IsPowered(uid, EntityManager))
            return;

        if (!PrototypeManager.TryIndex<TechnologyPrototype>(args.Id, out var technologyPrototype))
            return;

        if (TryComp<AccessReaderComponent>(uid, out var access) && !_accessReader.IsAllowed(act, uid, access))
        {
            _popup.PopupEntity(Loc.GetString("research-console-no-access-popup"), act);
            return;
        }

        if (!UnlockTechnology(uid, args.Id, act))
            return;

        if (!_emag.CheckFlag(uid, EmagType.Interaction))
        {
            var getIdentityEvent = new TryGetIdentityShortInfoEvent(uid, act);
            RaiseLocalEvent(getIdentityEvent);

            var message = Loc.GetString(
                "research-console-unlock-technology-radio-broadcast",
                ("technology", Loc.GetString(technologyPrototype.Name)),
                ("amount", technologyPrototype.Cost),
                ("approver", getIdentityEvent.Title ?? string.Empty)
            );

            _radio.SendRadioMessage(uid, message, component.AnnouncementChannel, uid, escapeMarkup: false);

            if (technologyPrototype.RadioChannels.Any())
                foreach (var radioChannelId in technologyPrototype.RadioChannels)
                {
                    if (PrototypeManager.TryIndex<RadioChannelPrototype>(radioChannelId, out var radioChannel))
                    {
                        _radio.SendRadioMessage(uid, message, radioChannel, uid, suppressTTS: true, escapeMarkup: false); // Starlight
                    }
                }
        }
        // Starlight start: Achievements
        if (TryGetClientServer(uid, out var serverUid, out _)
            && serverUid is { } server
            && TryComp<TechnologyDatabaseComponent>(server, out var database)
            && database.CurrentTechnologyCards.Count == 0
            && _station.GetOwningStation(uid) is { } station)
        {
            _achievements.QueueUnlockAchievementForJobs("theory_of_everything", station, "ResearchDirector");
        }
        // Starlight end: Achievements
        SyncClientWithServer(uid);
        UpdateConsoleInterface(uid, component);
    }

    private void OnConsoleBeforeUiOpened(EntityUid uid, ResearchConsoleComponent component, BeforeActivatableUIOpenEvent args)
    {
        SyncClientWithServer(uid);
    }

    private void UpdateConsoleInterface(EntityUid uid, ResearchConsoleComponent? component = null, ResearchClientComponent? clientComponent = null)
    {
        if (!Resolve(uid, ref component, ref clientComponent, false))
            return;

        // 🌇Sunset🌇 - branching tech tree: compute every technology's availability against the
        // connected server's database (Researched/Available/PrereqsMet/Unavailable), reusing
        // IsTechnologyAvailable so discipline support + tier gating still apply exactly as before -
        // this only changes how the client draws things, not what's actually researchable.
        var researches = new Dictionary<string, ResearchAvailability>();
        var points = 0;

        if (TryGetClientServer(uid, out var serverUid, out var serverComponent, clientComponent) &&
            TryComp<TechnologyDatabaseComponent>(serverUid, out var database))
        {
            points = clientComponent.ConnectedToServer ? serverComponent.Points : 0;
            var disciplineTiers = GetDisciplineTiers(database);

            foreach (var tech in PrototypeManager.EnumeratePrototypes<TechnologyPrototype>())
            {
                if (database.UnlockedTechnologies.Contains(tech.ID))
                {
                    researches[tech.ID] = ResearchAvailability.Researched;
                }
                else if (!IsTechnologyAvailable(database, tech, disciplineTiers))
                {
                    researches[tech.ID] = ResearchAvailability.Unavailable;
                }
                else
                {
                    researches[tech.ID] = serverComponent.Points >= tech.Cost
                        ? ResearchAvailability.Available
                        : ResearchAvailability.PrereqsMet;
                }
            }
        }
        else
        {
            foreach (var tech in PrototypeManager.EnumeratePrototypes<TechnologyPrototype>())
                researches[tech.ID] = ResearchAvailability.Unavailable;
        }

        _uiSystem.SetUiState(uid, ResearchConsoleUiKey.Key, new ResearchConsoleBoundInterfaceState(points, researches));
    }

    private void OnPointsChanged(EntityUid uid, ResearchConsoleComponent component, ref ResearchServerPointsChangedEvent args)
    {
        if (!_uiSystem.IsUiOpen(uid, ResearchConsoleUiKey.Key))
            return;
        UpdateConsoleInterface(uid, component);
    }

    private void OnConsoleRegistrationChanged(EntityUid uid, ResearchConsoleComponent component, ref ResearchRegistrationChangedEvent args)
    {
        SyncClientWithServer(uid);
        UpdateConsoleInterface(uid, component);
    }

    private void OnConsoleDatabaseModified(EntityUid uid, ResearchConsoleComponent component, ref TechnologyDatabaseModifiedEvent args)
    {
        SyncClientWithServer(uid);
        UpdateConsoleInterface(uid, component);
    }

    private void OnConsoleDatabaseSynchronized(EntityUid uid, ResearchConsoleComponent component, ref TechnologyDatabaseSynchronizedEvent args)
    {
        UpdateConsoleInterface(uid, component);
    }

    private void OnEmagged(Entity<ResearchConsoleComponent> ent, ref GotEmaggedEvent args)
    {
        if (!_emag.CompareFlag(args.Type, EmagType.Interaction))
            return;

        if (_emag.CheckFlag(ent, EmagType.Interaction))
            return;

        args.Handled = true;
    }
}
