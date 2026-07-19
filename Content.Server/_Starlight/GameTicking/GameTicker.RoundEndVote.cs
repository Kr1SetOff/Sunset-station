using Content.Server.GameTicking;
using Content.Server.RoundEnd;
using Content.Server.Voting.Managers;
using Content.Shared.Starlight.CCVar;
using Content.Shared.Voting;
using Robust.Server.Player;
using Robust.Shared.Timing;
using Robust.Shared.Configuration;

namespace Content.Server._Starlight.GameTicking;

public sealed partial class RoundEndVoteSystem : EntitySystem
{
    [Dependency] private GameTicker _gameTicker = default!;
    [Dependency] private IGameTiming _gameTiming = default!;
    [Dependency] private IVoteManager _voteManager = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IPlayerManager _playerManager = default!;

    private TimeSpan? _voteStartTime;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundEndSystemChangedEvent>(OnRoundEndSystemChange);
    }

    public void OnRoundEndSystemChange(RoundEndSystemChangedEvent args)
    {
        if (_playerManager.PlayerCount < _cfg.GetCVar(StarlightCCVars.MinPlayerToVote))
        {
            Log.Warning($"Not enought players, player count: {_playerManager.PlayerCount}");
            return;
        }

        _voteStartTime = _gameTiming.CurTime + _gameTicker.LobbyDuration - TimeSpan.FromSeconds(_cfg.GetCVar(StarlightCCVars.VotingsDelay));
        Log.Warning($"Vote will start at {_voteStartTime}");

        if (_cfg.GetCVar(StarlightCCVars.ResetPresetAfterRestart))
            _gameTicker.SetGamePreset("Secret");
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_gameTicker.RunLevel != GameRunLevel.PreRoundLobby || _voteStartTime == null)
            return;

        if (_gameTiming.CurTime >= _voteStartTime)
        {
            StartRoundEndVotes();
            _voteStartTime = null;
        }
    }

    public void StartRoundEndVotes()
    {
        if (_gameTicker.RunLevel != GameRunLevel.PreRoundLobby)
            return;

        // 🌇Sunset🌇 - unlike the player-facing "createvote" command, CreateStandardVote() here is
        // called with no initiator session, so it never goes through CanCallVote()'s "don't stack a
        // vote of this type on top of one that's already active/on cooldown" check. If this method
        // ever runs more than once for the same lobby (e.g. RoundEndSystemChangedEvent firing again
        // before the round actually restarts), it used to just create another vote on top of the
        // existing one every time - hence votes piling up more and more across rounds.
        if (_cfg.GetCVar(StarlightCCVars.RunMapVoteAfterRestart) &&
            !_voteManager.IsStandardVoteActiveOrOnCooldown(StandardVoteType.Map))
        {
            _voteManager.CreateStandardVote(null, StandardVoteType.Map);
        }

        if (_cfg.GetCVar(StarlightCCVars.RunPresetVoteAfterRestart) &&
            !_voteManager.IsStandardVoteActiveOrOnCooldown(StandardVoteType.Preset))
        {
            _voteManager.CreateStandardVote(null, StandardVoteType.Preset);
        }
    }
}
