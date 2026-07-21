using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Content.Shared._Starlight.Achievement;
using Robust.Shared.ContentPack;
using Robust.Shared.Utility;
using Starlight.NullLink;

namespace Content.Server._NullLink.PlayerData;

// Local on-disk fallback for achievements. The primary backend is the NullLink cluster;
// when it is unavailable (not configured / offline) unlocks and progress would otherwise
// silently no-op and vanish on restart, so they are mirrored to data/Achievements/*.json.
public sealed partial class NullLinkPlayerManager
{
    [Dependency] private IResourceManager _resourceManager = default!;

    private static readonly ResPath LocalAchievementDir = new("/Achievements");

    private static readonly JsonSerializerOptions LocalStoreJsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    };

    private sealed class LocalAchievementRecord
    {
        public string AchievementId { get; set; } = string.Empty;
        public string? UnlockingCharacter { get; set; }
        public string? GrantingServer { get; set; }
        public DateTime UnlockTime { get; set; }
    }

    private sealed class LocalAchievementFile
    {
        public List<LocalAchievementRecord> Achievements { get; set; } = [];
        public Dictionary<string, double> Progress { get; set; } = [];
    }

    private ResPath GetLocalAchievementPath(Guid userId)
        => LocalAchievementDir / $"{userId}.json";

    private void LoadLocalAchievements(Guid userId, PlayerData playerData)
    {
        try
        {
            var path = GetLocalAchievementPath(userId);
            if (!_resourceManager.UserData.Exists(path))
            {
                // Nothing stored yet - the player simply has no unlocks. Mark the cache
                // hydrated so unlock checks work even if the cluster never answers.
                lock (playerData.AchievementSyncRoot)
                    playerData.AchievementCacheHydrated = true;
                return;
            }

            LocalAchievementFile? file;
            using (var stream = _resourceManager.UserData.OpenRead(path))
            {
                file = JsonSerializer.Deserialize<LocalAchievementFile>(stream, LocalStoreJsonOptions);
            }

            if (file == null)
                return;

            var localAchievements = file.Achievements
                .Where(record => !string.IsNullOrEmpty(record.AchievementId))
                .Select(record => new Achievement
                {
                    AchievementId = record.AchievementId,
                    UnlockingCharacter = record.UnlockingCharacter ?? string.Empty,
                    GrantingServer = record.GrantingServer ?? string.Empty,
                    UnlockTime = record.UnlockTime,
                });

            lock (playerData.AchievementSyncRoot)
            {
                playerData.UnlockedAchievements = [.. MergeAchievements(playerData.UnlockedAchievements, localAchievements)];
                playerData.AchievementCacheHydrated = true;
            }

            foreach (var (key, value) in file.Progress)
            {
                playerData.AchievementProgress.AddOrUpdate(key, value, (_, existing) => Math.Max(existing, value));
            }
        }
        catch (Exception ex)
        {
            _sawmill.Error($"Failed to load local achievements for {userId}: {ex}");
        }
    }

    private void SaveLocalAchievements(Guid userId)
    {
        if (!_playerById.TryGetValue(userId, out var playerData))
            return;

        SaveLocalAchievements(userId, playerData);
    }

    private void SaveLocalAchievements(Guid userId, PlayerData playerData)
    {
        try
        {
            LocalAchievementFile file;
            lock (playerData.AchievementSyncRoot)
            {
                file = new LocalAchievementFile
                {
                    Achievements = playerData.UnlockedAchievements
                        .Select(achievement => new LocalAchievementRecord
                        {
                            AchievementId = achievement.AchievementId,
                            UnlockingCharacter = achievement.UnlockingCharacter,
                            GrantingServer = achievement.GrantingServer,
                            UnlockTime = achievement.UnlockTime,
                        })
                        .ToList(),
                    Progress = new Dictionary<string, double>(playerData.AchievementProgress),
                };
            }

            _resourceManager.UserData.CreateDir(LocalAchievementDir);

            var path = GetLocalAchievementPath(userId);
            using var stream = _resourceManager.UserData.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
            JsonSerializer.Serialize(stream, file, LocalStoreJsonOptions);
        }
        catch (Exception ex)
        {
            _sawmill.Error($"Failed to save local achievements for {userId}: {ex}");
        }
    }
}
