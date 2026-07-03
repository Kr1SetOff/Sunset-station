using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Shared._Sunset.Homelander;
using Content.Shared.Chat;
using Robust.Shared.Player;

namespace Content.Server._Sunset.Homelander;

/// <summary>
/// Gives entities with <see cref="HyperHearingComponent"/> the ability to hear
/// whispers clearly from far beyond normal earshot. Normal whispers fade out at
/// 5 tiles; this re-delivers them clearly between there and <see cref="MaxRange"/>.
/// Additive only - it never alters normal chat for anyone else.
/// </summary>
public sealed class HomelanderHearingSystem : EntitySystem
{
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    private const float MinRange = 5f;
    private const float MaxRange = 12f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EntitySpokeEvent>(OnSpoke);
    }

    private void OnSpoke(EntitySpokeEvent ev)
    {
        if (!ev.IsWhisper)
            return;

        var sourceXform = Transform(ev.Source);
        var sourcePos = _xform.GetWorldPosition(sourceXform);

        var query = EntityQueryEnumerator<HyperHearingComponent, ActorComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var actor, out var xform))
        {
            if (uid == ev.Source || xform.MapID != sourceXform.MapID)
                continue;

            var dist = (_xform.GetWorldPosition(xform) - sourcePos).Length();
            if (dist <= MinRange || dist > MaxRange)
                continue;

            var wrapped = Loc.GetString("homelander-hyper-hearing-whisper",
                ("name", Name(ev.Source)),
                ("message", ev.Message.Text));

            _chat.ChatMessageToOne(ChatChannel.Whisper, ev.Message.Text, wrapped, ev.Source, false, actor.PlayerSession.Channel);
        }
    }
}
