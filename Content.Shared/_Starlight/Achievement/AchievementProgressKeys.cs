namespace Content.Shared._Starlight.Achievement;

public static class AchievementProgressKeys
{
    public const string SpawnCount = "spawn.count";
    public const string SpawnLateJoinCount = "spawn.latejoin.count";
    public const string SpawnRoundStartCount = "spawn.roundstart.count";
    public const string VampireBloodDrank = "vampire.blooddrank";
    public const string AlcoholDrank = "drink.alcohol";

    // 🌇Sunset🌇 - progress keys for achievements ported from sunset-station.
    public const string DeathCount = "sunset.death.count";
    public const string ReviveCount = "sunset.revive.count";
    public const string SlipCount = "sunset.slip.count";
    public const string ExamineCount = "sunset.examine.count";
    public const string SpaceWithoutSuitSeconds = "sunset.space_without_suit.seconds";

    public static string SpawnJob(string jobId) => $"spawn.job.{jobId}";
    public static string StorePurchase(string listingId) => $"store.purchase.{listingId}";
    public static string SlipJob(string jobId) => $"sunset.slip.job.{jobId}";
}
