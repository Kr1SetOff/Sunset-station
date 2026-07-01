using System.Numerics;
using Content.Shared._Sunset.MartialArts;
using Content.Shared._Sunset.MartialArts.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Input;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._Sunset.MartialArts;

/// <summary>
/// Draws the local player's recent martial arts combo inputs (Harm/Disarm/Grab) as a small row of
/// icons next to the cursor, similar to Goob Station's combo tracker.
/// </summary>
public sealed class ComboTrackerOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IInputManager _input = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    private static readonly TimeSpan DisplayDuration = TimeSpan.FromSeconds(4);
    private const float IconSize = 24f;
    private const float IconSpacing = 4f;
    private static readonly Vector2 CursorOffset = new(24, 24);

    private readonly Texture _harmIcon;
    private readonly Texture _disarmIcon;
    private readonly Texture _grabIcon;

    public ComboTrackerOverlay()
    {
        IoCManager.InjectDependencies(this);

        var spriteSys = _entMan.EntitySysManager.GetEntitySystem<SpriteSystem>();
        const string rsiPath = "/Textures/_Sunset/MartialArts/intents.rsi";
        _harmIcon = spriteSys.Frame0(new SpriteSpecifier.Rsi(new ResPath(rsiPath), "harm"));
        _disarmIcon = spriteSys.Frame0(new SpriteSpecifier.Rsi(new ResPath(rsiPath), "disarm"));
        _grabIcon = spriteSys.Frame0(new SpriteSpecifier.Rsi(new ResPath(rsiPath), "grab"));
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (_player.LocalEntity is not { } player ||
            !_entMan.TryGetComponent(player, out CanPerformComboComponent? combo) ||
            combo.LastAttacks.Count == 0)
            return false;

        return _timing.CurTime - combo.LastAttackTime < DisplayDuration && base.BeforeDraw(in args);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (_player.LocalEntity is not { } player ||
            !_entMan.TryGetComponent(player, out CanPerformComboComponent? combo))
            return;

        var mousePos = _input.MouseScreenPosition;
        if (mousePos.Window == WindowId.Invalid)
            return;

        var screen = args.ScreenHandle;
        var basePos = mousePos.Position + CursorOffset;

        for (var i = 0; i < combo.LastAttacks.Count; i++)
        {
            var origin = basePos + new Vector2((IconSize + IconSpacing) * i, 0);
            var box = UIBox2.FromDimensions(origin, new Vector2(IconSize, IconSize));

            screen.DrawRect(box, Color.Black.WithAlpha(0.5f));
            screen.DrawTextureRect(GetIcon(combo.LastAttacks[i]), box);
        }
    }

    private Texture GetIcon(ComboAttackType type) => type switch
    {
        ComboAttackType.Harm => _harmIcon,
        ComboAttackType.Disarm => _disarmIcon,
        ComboAttackType.Grab => _grabIcon,
        _ => _harmIcon,
    };
}
