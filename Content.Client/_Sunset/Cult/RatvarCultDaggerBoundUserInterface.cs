using System.Linq;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.Actions;
using Content.Shared._Sunset.Cult;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using JetBrains.Annotations;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Utility;

namespace Content.Client._Sunset.Cult;

/// <summary>
/// 🌇Sunset🌇 - the Nar'Sie cult dagger's ritual wheel (internal ids still say "Ratvar" - see the TODO
/// note in Content.Server._Sunset.Cult.RatvarCultSystem). Activating the dagger in-hand opens a radial
/// menu listing whichever cult ritual actions (see Resources/Prototypes/_Sunset/Cult/actions.yml) the
/// wielder currently holds - a non-cultist who picks up a stray dagger just sees an empty wheel, since
/// they were never granted any of those actions. Picking an entry arms that action's normal
/// entity/world-target picking mode via ActionUIController.ActivateAction, so the actual ritual logic
/// (DoAfters, conversion, etc.) is unchanged - this only changes how the ability is reached.
/// </summary>
[UsedImplicitly]
public sealed partial class RatvarCultDaggerBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

    private static readonly string[] RitualActionIds =
    {
        "ActionRatvarCultConvert",
        "ActionRatvarCultCommune",
        "ActionRatvarCultScribeSummon",
        "ActionRatvarCultScribeWall",
        "ActionRatvarCultScribeBloodBoil",
        "ActionRatvarCultScribeTeleport",
        "ActionRatvarCultScribeRaiseDead",
        "ActionRatvarCultScribeApocalypse",
    };

    // Nar'Sie's signature blood-red, matching the runes and the "Nar'Sie has risen" announcement color.
    private static readonly Color RatvarBrass = Color.FromHex("#3a0a0a");
    private static readonly Color RatvarBrassHover = Color.FromHex("#6b1212");

    private SimpleRadialMenu? _menu;

    public RatvarCultDaggerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        if (_playerManager.LocalSession?.AttachedEntity is not { } wielder ||
            !EntMan.TryGetComponent<ActionsComponent>(wielder, out var actionsComp))
        {
            return;
        }

        var controller = _uiManager.GetUIController<ActionUIController>();
        var options = new List<RadialMenuOptionBase>();

        foreach (var actionId in actionsComp.Actions)
        {
            var meta = EntMan.GetComponent<MetaDataComponent>(actionId);
            if (meta.EntityPrototype?.ID is not { } protoId || !RitualActionIds.Contains(protoId))
                continue;

            var tooltip = meta.EntityName;
            if (!string.IsNullOrEmpty(meta.EntityDescription))
                tooltip += "\n" + meta.EntityDescription;

            SpriteSpecifier? icon = null;
            if (EntMan.TryGetComponent<ActionComponent>(actionId, out var actionComp))
                icon = actionComp.Icon;

            var capturedId = actionId;
            options.Add(new RadialMenuActionOption<EntityUid>(id => controller.ActivateAction(id), capturedId)
            {
                ToolTip = tooltip,
                IconSpecifier = RadialMenuIconSpecifier.With(icon),
                BackgroundColor = RatvarBrass,
                HoverBackgroundColor = RatvarBrassHover,
            });
        }

        _menu = this.CreateWindow<SimpleRadialMenu>();
        _menu.Track(Owner);
        _menu.SetButtons(options);
        _menu.OpenOverMouseScreenPosition();
    }
}
