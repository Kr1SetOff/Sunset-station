using Robust.Client.Graphics;

namespace Content.Client._Sunset.MartialArts;

/// <summary>
/// Registers/unregisters the <see cref="ComboTrackerOverlay"/> for the lifetime of gameplay. The
/// overlay itself decides frame-to-frame whether there's anything to draw.
/// </summary>
public sealed class ComboTrackerSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;

    private ComboTrackerOverlay? _overlay;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new ComboTrackerOverlay();
        _overlayManager.AddOverlay(_overlay);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        if (_overlay != null)
            _overlayManager.RemoveOverlay(_overlay);
    }
}
