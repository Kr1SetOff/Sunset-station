namespace Content.Server._Sunset.Voidwalker;

/// <summary>
/// 🌇Sunset🌇 - anchor component for VoidwalkerSpawnRuleSystem (GameRuleSystem&lt;T&gt; needs one).
/// The VoidwalkerSpawn rule itself is pure YAML these days (SpaceSpawnRule + AntagSpawner +
/// AntagSelection raffle-spawner, see Resources/Prototypes/_Sunset/Voidwalker/game_rule.yml) and
/// doesn't carry this component; the system lives on for the mind-added handling (objective +
/// station-direction message) and the admin verb's PlaceInSpaceNearStation helper.
/// </summary>
[RegisterComponent, Access(typeof(VoidwalkerSpawnRuleSystem))]
public sealed partial class VoidwalkerSpawnRuleComponent : Component;
