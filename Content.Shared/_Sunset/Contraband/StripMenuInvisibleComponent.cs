using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Contraband;

/// <summary>
/// Ported from Goobstation/Reserve-Station as a data-only stub: nothing in this fork's strip menu
/// currently reads this marker, so it's inert for now - present so the ported armor entities that
/// reference it still validate and compile.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StripMenuInvisibleComponent : Component
{
}
