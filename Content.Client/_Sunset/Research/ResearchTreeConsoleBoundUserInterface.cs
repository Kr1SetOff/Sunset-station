using System.Linq;
using Content.Shared.Research.Components;
using Content.Shared.Research.Prototypes;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._Sunset.Research;

/// <summary>
/// 🌇Sunset🌇 - replaces ResearchConsoleBoundUserInterface for the R&amp;D console, wiring the
/// branching ResearchTreeMenu instead of the flat card-list ResearchConsoleMenu.
/// </summary>
[UsedImplicitly]
public sealed class ResearchTreeConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private ResearchTreeMenu? _menu;

    public ResearchTreeConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<ResearchTreeMenu>();
        _menu.SetEntity(Owner);

        _menu.OnTechnologyCardPressed += id => SendMessage(new ConsoleUnlockTechnologyMessage(id));
        _menu.OnServerButtonPressed += () => SendMessage(new ConsoleServerSelectionMessage());
    }

    public override void OnProtoReload(PrototypesReloadedEventArgs args)
    {
        base.OnProtoReload(args);

        if (!args.WasModified<TechnologyPrototype>())
            return;

        if (State is not ResearchConsoleBoundInterfaceState state)
            return;

        _menu?.UpdatePanels(state.Researches);
        _menu?.UpdateInformationPanel(state.Points);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not ResearchConsoleBoundInterfaceState castState || _menu == null)
            return;

        if (!_menu.List.SequenceEqual(castState.Researches))
            _menu.UpdatePanels(castState.Researches);

        if (_menu.Points != castState.Points)
            _menu.UpdateInformationPanel(castState.Points);
    }
}
