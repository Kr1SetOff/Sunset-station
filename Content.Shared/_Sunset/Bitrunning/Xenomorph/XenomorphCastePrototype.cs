using Robust.Shared.Prototypes;

namespace Content.Shared._Sunset.Bitrunning.Xenomorph;

// 🌇Sunset🌇 - ported from Orion-Station-14's Content.Shared._White.Xenomorphs.Caste.XenomorphCastePrototype.
[Prototype("xenomorphCaste")]
public sealed partial class XenomorphCastePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Name = string.Empty;
}
