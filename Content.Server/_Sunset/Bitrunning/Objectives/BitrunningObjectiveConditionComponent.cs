namespace Content.Server._Sunset.Bitrunning.Objectives;

/// <summary>
/// Marks an objective entity as tracking a specific quantum server's domain run. Added to an
/// avatar's mind when they connect into a domain (see QuantumServerSystem), so their character
/// objectives panel shows what the current run needs - not just a one-off popup on connect.
/// </summary>
[RegisterComponent]
public sealed partial class BitrunningObjectiveConditionComponent : Component
{
    /// <summary>The quantum server this objective's progress is read from.</summary>
    [DataField]
    public EntityUid? Server;
}
