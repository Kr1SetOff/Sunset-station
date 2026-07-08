using Content.Shared._Sunset.CustomLawboard;
using Robust.Client.UserInterface;

namespace Content.Client._Sunset.CustomLawboard;

public sealed class CustomLawboardSystem : SharedCustomLawboardSystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    protected override void DirtyUI(EntityUid uid, CustomLawboardComponent? customLawboard)
    {
        if (_ui.TryGetOpenUi<CustomLawboardBoundInterface>(uid, CustomLawboardUiKey.Key, out var bui))
            bui.Update();
    }
}
