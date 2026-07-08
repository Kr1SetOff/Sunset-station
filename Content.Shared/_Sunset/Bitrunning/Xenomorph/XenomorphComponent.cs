using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Sunset.Bitrunning.Xenomorph;

// 🌇Sunset🌇 - ported from Orion-Station-14's Content.Shared._White.Xenomorphs.Xenomorph.XenomorphComponent,
// stripped down to just the caste marker. The full upstream component also carries weed-healing and
// hivemind-chat data, both meaningless for a bitrunning domain (no xenomorph weeds are placed on the
// maps, and these mobs are plain HTN-driven NPCs with no player ever speaking through them).
[RegisterComponent, NetworkedComponent]
public sealed partial class XenomorphComponent : Component
{
    [DataField(required: true)]
    public ProtoId<XenomorphCastePrototype> Caste;
}
