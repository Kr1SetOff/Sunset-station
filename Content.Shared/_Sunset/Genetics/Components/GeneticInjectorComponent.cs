// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Sunset.Genetics.Components;

/// <summary>
///     A DNA injector that writes a stored genome subset into whoever it is used on.
///     Created from the DNA modifier console's transfer buffer, or pre-built (e.g. a clean-SE injector).
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GeneticInjectorComponent : Component
{
    /// <summary>If true, the injector wipes the structural-enzyme mutation blocks instead of copying any.</summary>
    [DataField, AutoNetworkedField]
    public bool CleanSe;

    [DataField, AutoNetworkedField]
    public bool ApplyUi;

    [DataField, AutoNetworkedField]
    public bool ApplyUe;

    [DataField, AutoNetworkedField]
    public bool ApplySe;

    [DataField, AutoNetworkedField]
    public List<int> Ui = new();

    [DataField, AutoNetworkedField]
    public List<int> Ue = new();

    [DataField, AutoNetworkedField]
    public List<int> Se = new();

    /// <summary>
    ///     If set, the injector activates this single mutation by raising its structural-enzyme block to
    ///     the activation threshold. Used by gene activator injectors printed from the DNA modifier console.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? ActivateMutation;

    /// <summary>Remaining uses before the injector is spent.</summary>
    [DataField, AutoNetworkedField]
    public int Uses = 1;

    /// <summary>How long it takes to inject someone (the do-after duration), in seconds.</summary>
    [DataField, AutoNetworkedField]
    public float InjectDelay = 3f;
}

/// <summary>Raised when a genetic injector finishes being injected into a target.</summary>
[Serializable, NetSerializable]
public sealed partial class GeneticInjectorDoAfterEvent : SimpleDoAfterEvent;
