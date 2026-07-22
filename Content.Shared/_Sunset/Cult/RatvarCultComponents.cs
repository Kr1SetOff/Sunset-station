using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Cult;

/// <summary>
/// 🌇Sunset🌇 - marks a follower of the Ratvar cult (both regular cultists and the head cultist have
/// this). Granted at round start by the RatvarCult AntagSelection definitions, or at runtime by
/// Content.Server._Sunset.Cult.RatvarCultSystem when someone is converted.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class RatvarCultistComponent : Component;

/// <summary>
/// 🌇Sunset🌇 - marks the round's head cultist (chosen at round start, like Nukeops' commander -
/// unlike the classic lore's "no formal leader," the standalone game mode always has one so the round
/// has a clear rallying point/scapegoat). Always paired with RatvarCultistComponent.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class RatvarCultLeaderComponent : Component;

/// <summary>
/// 🌇Sunset🌇 - marks a cultist who has already carved their blood magic mark (see
/// RatvarCultPrepareBloodMagicActionEvent), so the one-time starter kit can't be granted twice.
/// </summary>
[RegisterComponent]
public sealed partial class RatvarBloodMagicPreparedComponent : Component;
