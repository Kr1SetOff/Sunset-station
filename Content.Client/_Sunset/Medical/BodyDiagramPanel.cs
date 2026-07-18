using System.Linq;
using System.Numerics;
using Content.Shared._Starlight.Medical.Body.Part; // BodyPartType
using Content.Shared.Body.Components; // BodyComponent
using Content.Shared.Body.Part; // BodyPartComponent
using Content.Shared.Body.Systems; // SharedBodySystem
using Content.Shared.Damage.Components;
using Content.Shared.FixedPoint;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

namespace Content.Client._Sunset.Medical;

/// <summary>
/// 🌇Sunset🌇 - detailed per-body-part breakdown for the operating computer's health analyzer.
/// Lists each body part with an icon (from surgery_ui.rsi), its integrity/damage, whether it's
/// bleeding, and the organs inside it with their own condition. Hovering a part shows the full
/// detail in a tooltip. Reads the patient's body directly on the client (parts/organs are PVS
/// entities the operating computer's viewer already has), so it needs no extra server networking.
/// </summary>
public sealed class BodyDiagramPanel : BoxContainer
{
    private readonly IEntityManager _entity;
    private readonly SharedBodySystem _body;
    private readonly SpriteSystem _sprite;

    private static readonly ResPath SurgeryUiRsi = new("/Textures/_Sunset/Interface/Medical/surgery_ui.rsi");

    public BodyDiagramPanel(IEntityManager entity, IResourceCache _)
    {
        _entity = entity;
        _body = entity.System<SharedBodySystem>();
        _sprite = entity.System<SpriteSystem>();

        Orientation = LayoutOrientation.Vertical;
    }

    /// <summary>
    /// Rebuilds the diagram for the given body. Returns false if the target has no traversable body.
    /// </summary>
    public bool Populate(EntityUid body)
    {
        RemoveAllChildren();

        if (!_entity.HasComponent<BodyComponent>(body))
            return false;

        AddChild(new Label
        {
            Text = Loc.GetString("health-analyzer-window-body-diagram-header"),
            StyleClasses = { "LabelKeyText" },
            Margin = new Thickness(0, 0, 0, 6),
        });

        var parts = _body.GetBodyChildren(body).ToList();
        if (parts.Count == 0)
            return false;

        // Torso first, then head, then the limbs, laid out as a compact two-column grid of
        // bordered part cards rather than a long flat list.
        var grid = new GridContainer { Columns = 2 };
        AddChild(grid);

        foreach (var (partUid, part) in parts.OrderBy(p => PartOrder(p.Component.PartType)))
        {
            grid.AddChild(BuildPartCard(partUid, part));
        }

        return true;
    }

    private Control BuildPartCard(EntityUid partUid, BodyPartComponent part)
    {
        // Robust's Access analyzer flags chained member-access-then-call expressions straight off
        // a restricted component field (e.g. part.PartType.ToString()) as an 'Execute' access even
        // though PartType itself is only being read - copy it out to a plain local first.
        var partType = part.PartType;

        var (damage, maxDamage) = GetDamage(partUid);
        var bleeding = IsBleeding(partUid);
        var conditionColor = ConditionColor(damage, maxDamage);
        var partName = Loc.GetString("health-analyzer-window-body-part-" + partType.ToString().ToLowerInvariant());

        var card = new PanelContainer
        {
            Margin = new Thickness(2),
            MinWidth = 175,
            HorizontalExpand = true,
            MouseFilter = MouseFilterMode.Stop, // so the tooltip fires anywhere on the card
        };
        card.PanelOverride = new StyleBoxFlat
        {
            BackgroundColor = conditionColor.WithAlpha(0.05f),
            BorderColor = conditionColor.WithAlpha(0.6f),
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 6,
            ContentMarginRightOverride = 6,
            ContentMarginTopOverride = 5,
            ContentMarginBottomOverride = 5,
        };
        card.ToolTip = BuildTooltip(partUid, part, partName, damage, maxDamage, bleeding);

        var content = new BoxContainer { Orientation = LayoutOrientation.Vertical };

        var header = new BoxContainer { Orientation = LayoutOrientation.Horizontal };

        header.AddChild(new TextureRect
        {
            Texture = _sprite.Frame0(new SpriteSpecifier.Rsi(SurgeryUiRsi, PartIconState(partType))),
            SetSize = new Vector2(26, 26),
            Stretch = TextureRect.StretchMode.KeepAspectCentered,
            VerticalAlignment = VAlignment.Center,
            Margin = new Thickness(0, 0, 6, 0),
        });

        var titleColumn = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalAlignment = VAlignment.Center,
        };

        titleColumn.AddChild(new Label
        {
            Text = partName,
            FontColorOverride = Color.White,
        });

        var statusText = FormatCondition(damage, maxDamage);
        if (bleeding)
            statusText += "  " + Loc.GetString("health-analyzer-window-body-part-bleeding");

        titleColumn.AddChild(new Label
        {
            Text = statusText,
            FontColorOverride = bleeding ? Color.Red : conditionColor,
            StyleClasses = { "LabelSubText" },
        });

        header.AddChild(titleColumn);
        content.AddChild(header);

        // Organs contained in this part, listed under a thin divider.
        var organs = _body.GetPartOrgans(partUid, part).ToList();
        if (organs.Count > 0)
        {
            var divider = new PanelContainer
            {
                MinHeight = 1,
                Margin = new Thickness(0, 4),
            };
            divider.PanelOverride = new StyleBoxFlat(Color.Gray.WithAlpha(0.35f));
            content.AddChild(divider);

            foreach (var (organUid, _) in organs)
            {
                var (organDamage, organMax) = GetDamage(organUid);
                var organName = _entity.HasComponent<MetaDataComponent>(organUid)
                    ? _entity.GetComponent<MetaDataComponent>(organUid).EntityName
                    : Loc.GetString("health-analyzer-window-body-part-organ");

                var organRow = new BoxContainer
                {
                    Orientation = LayoutOrientation.Horizontal,
                    Margin = new Thickness(0, 1),
                };
                organRow.AddChild(new Label
                {
                    Text = "• " + organName,
                    StyleClasses = { "LabelSubText" },
                    HorizontalExpand = true,
                });
                organRow.AddChild(new Label
                {
                    Text = FormatCondition(organDamage, organMax),
                    StyleClasses = { "LabelSubText" },
                    FontColorOverride = ConditionColor(organDamage, organMax),
                });
                content.AddChild(organRow);
            }
        }

        card.AddChild(content);
        return card;
    }

    private string BuildTooltip(EntityUid partUid, BodyPartComponent part, string partName, FixedPoint2 damage, FixedPoint2 maxDamage, bool bleeding)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(partName);
        sb.AppendLine(Loc.GetString("health-analyzer-window-body-part-tooltip-damage", ("damage", damage), ("max", maxDamage)));

        if (bleeding)
            sb.AppendLine(Loc.GetString("health-analyzer-window-body-part-tooltip-bleeding"));

        // Per-damage-type breakdown of this part.
        if (_entity.TryGetComponent<DamageableComponent>(partUid, out var damageable))
        {
            foreach (var (type, amount) in damageable.Damage.DamageDict)
            {
                if (amount > 0)
                    sb.AppendLine($"  {type}: {amount}");
            }
        }

        var organs = _body.GetPartOrgans(partUid, part).ToList();
        if (organs.Count > 0)
        {
            sb.AppendLine(Loc.GetString("health-analyzer-window-body-part-tooltip-organs"));
            foreach (var (organUid, _) in organs)
            {
                var name = _entity.HasComponent<MetaDataComponent>(organUid)
                    ? _entity.GetComponent<MetaDataComponent>(organUid).EntityName
                    : "?";
                var (d, m) = GetDamage(organUid);
                sb.AppendLine($"  {name}: {FormatCondition(d, m)}");
            }
        }

        return sb.ToString().TrimEnd();
    }

    // Body parts/organs don't expose a single clean "max integrity" value, so 100 is used as a
    // reference scale purely for the tooltip's "X / 100" readout and the colour thresholds below.
    private const int ReferenceMaxDamage = 100;

    private (FixedPoint2 Damage, FixedPoint2 Max) GetDamage(EntityUid uid)
    {
        if (!_entity.TryGetComponent<DamageableComponent>(uid, out var damageable))
            return (FixedPoint2.Zero, FixedPoint2.New(ReferenceMaxDamage));

        return (damageable.TotalDamage, FixedPoint2.New(ReferenceMaxDamage));
    }

    private bool IsBleeding(EntityUid uid)
    {
        // A part is "bleeding" if it hosts an active bleeding wound; approximated here by the
        // part's own damageable carrying slash/piercing damage (the wound-causing types).
        if (!_entity.TryGetComponent<DamageableComponent>(uid, out var damageable))
            return false;

        var slash = damageable.Damage.DamageDict.GetValueOrDefault("Slash");
        var pierce = damageable.Damage.DamageDict.GetValueOrDefault("Piercing");
        return slash + pierce >= 15;
    }

    private static string FormatCondition(FixedPoint2 damage, FixedPoint2 max)
    {
        if (damage <= 0)
            return Loc.GetString("health-analyzer-window-body-part-healthy");

        return Loc.GetString("health-analyzer-window-body-part-damaged", ("damage", damage));
    }

    private static Color ConditionColor(FixedPoint2 damage, FixedPoint2 max)
    {
        if (damage <= 0)
            return Color.LimeGreen;
        if (damage < 30)
            return Color.Yellow;
        if (damage < 60)
            return Color.Orange;
        return Color.Red;
    }

    private static int PartOrder(BodyPartType type) => type switch
    {
        BodyPartType.Head => 0,
        BodyPartType.Torso => 1,
        BodyPartType.Arm => 2,
        BodyPartType.Hand => 3,
        BodyPartType.Leg => 4,
        BodyPartType.Foot => 5,
        _ => 6,
    };

    private static string PartIconState(BodyPartType type) => type switch
    {
        BodyPartType.Head => "surgery_head",
        BodyPartType.Torso => "surgery_chest",
        BodyPartType.Arm or BodyPartType.Hand => "surgery_arms",
        BodyPartType.Leg or BodyPartType.Foot => "surgery_legs",
        _ => "surgery_any",
    };
}
