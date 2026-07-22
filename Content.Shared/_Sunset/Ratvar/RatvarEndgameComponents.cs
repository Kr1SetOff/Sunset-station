namespace Content.Shared._Sunset.Ratvar;

/// <summary>
/// 🌇Sunset🌇 - Ratvar's Workshop, porting the ss220.space RU wiki's endgame chain: interacting with it
/// crafts a one-time Strange Shard, consuming brass/energy.
/// </summary>
[RegisterComponent]
public sealed partial class RatvarWorkshopComponent : Component
{
    [DataField]
    public int BrassCost = 1;

    [DataField]
    public float EnergyCost = 500f;

    [DataField]
    public TimeSpan CraftTime = TimeSpan.FromSeconds(15);
}

/// <summary>
/// 🌇Sunset🌇 - the Strange Shard, porting the RU wiki's one-time crystal used to place on the Altar
/// and begin the final summoning rite.
/// </summary>
[RegisterComponent]
public sealed partial class RatvarStrangeShardComponent : Component;
