using Content.Client.Alerts;
using Content.Shared._Sunset.BloodWorm;
using Robust.Client.GameObjects;

namespace Content.Client._Sunset.BloodWorm;

/// <summary>
/// 🌇Sunset🌇 - client half of the blood worm's blood-counter alert: renders BloodWormComponent's
/// networked ConsumedBlood as four digit-glyph layers, same technique as the vampire's blood-drunk
/// counter (Content.Client._Starlight.Antags.Vampires.VampireSystem.OnUpdateAlert).
/// </summary>
public sealed class BloodWormSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodWormComponent, UpdateAlertSpriteEvent>(OnUpdateAlert);
    }

    private void OnUpdateAlert(EntityUid uid, BloodWormComponent comp, ref UpdateAlertSpriteEvent args)
    {
        if (args.Alert.AlertKey.AlertType != "BloodWormBlood")
            return;

        var value = Math.Clamp((int) comp.ConsumedBlood, 0, 9999);
        var d1 = value / 1000 % 10;
        var d2 = value / 100 % 10;
        var d3 = value / 10 % 10;
        var d4 = value % 10;

        _sprite.LayerSetRsiState((args.SpriteViewEnt, args.SpriteViewEnt.Comp), BloodWormVisualLayers.Digit1, d1.ToString());
        _sprite.LayerSetRsiState((args.SpriteViewEnt, args.SpriteViewEnt.Comp), BloodWormVisualLayers.Digit2, d2.ToString());
        _sprite.LayerSetRsiState((args.SpriteViewEnt, args.SpriteViewEnt.Comp), BloodWormVisualLayers.Digit3, d3.ToString());
        _sprite.LayerSetRsiState((args.SpriteViewEnt, args.SpriteViewEnt.Comp), BloodWormVisualLayers.Digit4, d4.ToString());
    }
}
