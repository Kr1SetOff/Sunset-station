using Content.Shared.Store;
using Robust.Shared.Prototypes;

namespace Content.Server._Sunset.MalfAi;

/// <summary>
/// 🌇Sunset🌇 - game rule component for the Malfunctioning AI antagonist. Only ever started via the
/// admin antag menu (see AdminVerbSystem.Antags.cs); the rule has no automatic round-start selection.
/// </summary>
[RegisterComponent]
public sealed partial class MalfAiRuleComponent : Component
{
    /// <summary>
    /// Store categories added to the malf module shop.
    /// </summary>
    [DataField]
    public List<ProtoId<StoreCategoryPrototype>> StoreCategories = new()
    {
        "MalfAiDestructive",
        "MalfAiUtility",
    };

    /// <summary>
    /// Starting CPU balance. tg: 50.
    /// </summary>
    [DataField]
    public float StartingCpu = 50f;
}
