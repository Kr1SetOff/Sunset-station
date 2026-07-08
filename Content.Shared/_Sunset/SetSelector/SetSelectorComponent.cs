using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Sunset.SetSelector;

// 🌇Sunset🌇 - ported from Goobstation/Reserve-Station: lets an entity offer a handful of
// randomly-rolled "sets" (see SelectableSetPrototype) and, once the player picks one via the
// SetSelectorMenu UI, spawn only the contents of that chosen set.
/// <summary>
/// This component stores the possible contents of the selector,
/// which can be selected via the interface.
/// </summary>
[RegisterComponent, Access(typeof(SetSelectorSystem))]
public sealed partial class SetSelectorComponent : Component
{
    /// <summary>
    /// List of sets available for selection
    /// </summary>
    [DataField]
    public List<ProtoId<SelectableSetPrototype>> PossibleSets = [];

    [DataField]
    public List<ProtoId<SelectableSetPrototype>> AvailableSets = [];

    [DataField]
    public List<int> SelectedSets = [];

    /// <summary>
    /// Max number of sets you can select.
    /// </summary>
    [DataField]
    public int MaxSelectedSets = 1;

    /// <summary>
    /// Max number of sets that would be available for selection. -1 if all should be available.
    /// </summary>
    [DataField]
    public int SetsToSelect = -1;

    /// <summary>
    /// What entity all the spawned items will appear inside, if any.
    /// </summary>
    [DataField]
    public EntProtoId? SpawnedStoragePrototype;

    /// <summary>
    /// Container ID of the spawned storage.
    /// </summary>
    [DataField]
    public string? SpawnedStorageContainer;

    /// <summary>
    /// If true, will try to open spawned storage as EntityStorage.
    /// </summary>
    [DataField]
    public bool OpenSpawnedStorage;

    [DataField]
    public SoundSpecifier? ApproveSound;
}
