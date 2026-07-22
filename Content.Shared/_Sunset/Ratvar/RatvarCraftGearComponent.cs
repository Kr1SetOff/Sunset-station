using Robust.Shared.Prototypes;

namespace Content.Shared._Sunset.Ratvar;

/// <summary>
/// 🌇Sunset🌇 - data-driven "craft this piece of enchanted gear" action, porting the ss220.space RU
/// wiki's per-item brass/energy/time costs (spear, hammer, sword, buckler, robe, cuirass, gauntlets).
/// One component + one event/handler (RatvarCraftGearActionEvent, RatvarServantSystem.OnCraftGear)
/// covers every craftable item instead of a separate class per item.
/// </summary>
[RegisterComponent]
public sealed partial class RatvarCraftGearComponent : Component
{
    [DataField(required: true)]
    public int BrassCost;

    [DataField(required: true)]
    public float EnergyCost;

    [DataField(required: true)]
    public TimeSpan CraftTime;

    [DataField(required: true)]
    public EntProtoId ResultProto;
}
