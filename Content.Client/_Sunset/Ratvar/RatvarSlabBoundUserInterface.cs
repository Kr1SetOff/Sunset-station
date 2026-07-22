using System.Linq;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.Actions;
using Content.Shared._Sunset.Ratvar;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using JetBrains.Annotations;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Utility;

namespace Content.Client._Sunset.Ratvar;

/// <summary>
/// 🌇Sunset🌇 - the clockwork slab's ritual wheel, mirroring Content.Client._Sunset.Cult's dagger wheel
/// exactly (see that file for the general pattern/rationale). Lists the servant's three built-in slab
/// abilities (Stun/Teleport/Heal) - unlike the Nar'Sie dagger, these are always granted alongside the
/// slab itself rather than earned separately, so this wheel is never empty for an actual servant.
/// </summary>
[UsedImplicitly]
public sealed partial class RatvarSlabBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

    private static readonly string[] SlabActionIds =
    {
        "ActionRatvarSlabStun",
        "ActionRatvarSlabTeleport",
        "ActionRatvarSlabHeal",
    };

    // Ratvar's brass/gold, matching the altar and slab.
    private static readonly Color RatvarGold = Color.FromHex("#3a2a10");
    private static readonly Color RatvarGoldHover = Color.FromHex("#6b4a12");

    private SimpleRadialMenu? _menu;

    public RatvarSlabBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
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
            if (meta.EntityPrototype?.ID is not { } protoId || !SlabActionIds.Contains(protoId))
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
                BackgroundColor = RatvarGold,
                HoverBackgroundColor = RatvarGoldHover,
            });
        }

        _menu = this.CreateWindow<SimpleRadialMenu>();
        _menu.Track(Owner);
        _menu.SetButtons(options);
        _menu.OpenOverMouseScreenPosition();
    }
}
