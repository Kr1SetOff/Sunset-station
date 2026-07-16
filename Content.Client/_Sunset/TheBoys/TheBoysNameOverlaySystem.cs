using Robust.Client.Graphics;

namespace Content.Client._Sunset.TheBoys;

/// <summary>
/// 🌇Sunset🌇 - registers/unregisters TheBoysNameOverlay. Split out from the overlay itself since
/// Overlay isn't an EntitySystem and can't own IOverlayManager registration on its own.
/// </summary>
public sealed class TheBoysNameOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay.AddOverlay(new TheBoysNameOverlay());
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _overlay.RemoveOverlay<TheBoysNameOverlay>();
    }
}
