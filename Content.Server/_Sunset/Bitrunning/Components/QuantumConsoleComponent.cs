using Content.Server._Sunset.Bitrunning.Systems;

namespace Content.Server._Sunset.Bitrunning.Components;

[RegisterComponent]
public sealed partial class QuantumConsoleComponent : Component
{
    [Access(typeof(QuantumConsoleSystem))]
    public EntityUid? LinkedServerId;
}
