using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Voidwalker;

/// <summary>
/// 🌇Sunset🌇 - permanent marker left behind when a VoidTumorComponent finishes growing without
/// being surgically removed in time: the victim stays visibly void-touched forever (client-side
/// dark tint, see VoidTumorVisualsSystem). Purely cosmetic for now - a hook for future content.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedVoidwalkerSystem))]
public sealed partial class VoidConsumedComponent : Component;
