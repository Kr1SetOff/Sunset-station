using System.Numerics;
using Content.Client.Examine;
using Content.Client.Stylesheets;
using Content.Shared._Sunset.TheBoys.Components;
using Content.Shared.Antag;
using Content.Shared.Tag;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client._Sunset.TheBoys;

/// <summary>
/// 🌇Sunset🌇 - draws each Boys team member's codename (Butcher/Hughie/Frenchie/Mother's Milk/Kimiko,
/// read off the same tags TheBoysRuleSystem already applies at selection) as a text label above their
/// head, in place of the old generic "Syndicate" faction icon. Only drawn for viewers who could have
/// seen that icon (admins with ShowAntagIconsComponent, or fellow team members), matching the removed
/// icon's showTo list (see Content.Shared._Sunset.TheBoys.Components).
///
/// DrawString only exists on the screen-space drawing handle (unlike status icons, which draw
/// world-space-below-FOV and get wall/darkness occlusion for free), so visibility here is
/// approximated with ExamineSystem.InRangeUnOccluded instead.
/// </summary>
public sealed class TheBoysNameOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    private readonly TagSystem _tag;
    private readonly ExamineSystem _examine;
    private readonly Font _font;

    private static readonly ProtoId<TagPrototype> ButcherTag = "TheBoysButcher";
    private static readonly ProtoId<TagPrototype> HughieTag = "TheBoysHughie";
    private static readonly ProtoId<TagPrototype> FrenchieTag = "TheBoysFrenchie";
    private static readonly ProtoId<TagPrototype> MothersMilkTag = "TheBoysMothersMilk";
    private static readonly ProtoId<TagPrototype> KimikoTag = "TheBoysKimiko";

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    public TheBoysNameOverlay()
    {
        IoCManager.InjectDependencies(this);
        _tag = _entity.System<TagSystem>();
        _examine = _entity.System<ExamineSystem>();
        _font = _resourceCache.NotoStack(variation: "Bold", size: 12);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (_player.LocalEntity is not { } viewer)
            return;

        var canSeeLabels = _entity.HasComponent<ShowAntagIconsComponent>(viewer) ||
                            _entity.HasComponent<TheBoysTeammateComponent>(viewer) ||
                            _entity.HasComponent<TheBoysButcherComponent>(viewer);
        if (!canSeeLabels)
            return;

        var teammates = _entity.AllEntityQueryEnumerator<TheBoysTeammateComponent, TransformComponent>();
        while (teammates.MoveNext(out var uid, out _, out var xform))
        {
            if (xform.MapID == args.MapId)
                DrawLabel(args, viewer, uid);
        }

        var butchers = _entity.AllEntityQueryEnumerator<TheBoysButcherComponent, TransformComponent>();
        while (butchers.MoveNext(out var uid, out _, out var xform))
        {
            if (xform.MapID == args.MapId)
                DrawLabel(args, viewer, uid);
        }
    }

    private void DrawLabel(in OverlayDrawArgs args, EntityUid viewer, EntityUid uid)
    {
        if (viewer == uid)
            return;

        if (GetCodename(uid) is not { } codename)
            return;

        if (!_examine.InRangeUnOccluded(viewer, uid))
            return;

        var worldPos = _entity.GetComponent<TransformComponent>(uid).WorldPosition + new Vector2(0f, 0.9f);
        var screenPos = _eye.WorldToScreen(worldPos).Rounded();

        var textSize = args.ScreenHandle.GetDimensions(_font, codename, 1f);
        args.ScreenHandle.DrawString(_font, screenPos - new Vector2(textSize.X / 2f, 0f), codename, 1f, Color.Gold);
    }

    private string? GetCodename(EntityUid uid)
    {
        if (_tag.HasTag(uid, ButcherTag))
            return Loc.GetString("role-subtype-theboys-butcher");
        if (_tag.HasTag(uid, HughieTag))
            return Loc.GetString("role-subtype-theboys-hughie");
        if (_tag.HasTag(uid, FrenchieTag))
            return Loc.GetString("role-subtype-theboys-frenchie");
        if (_tag.HasTag(uid, MothersMilkTag))
            return Loc.GetString("role-subtype-theboys-mothersmilk");
        if (_tag.HasTag(uid, KimikoTag))
            return Loc.GetString("role-subtype-theboys-kimiko");

        return null;
    }
}
