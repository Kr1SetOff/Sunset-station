using Content.Shared._Sunset.CustomLawboard;
using Content.Shared.Silicons.Laws;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Sunset.CustomLawboard;

/// <summary>
/// Initializes a <see cref="LawboardSiliconLawUi"/> and updates it when new server messages are received.
/// </summary>
[UsedImplicitly]
public sealed class CustomLawboardBoundInterface : BoundUserInterface
{
    [ViewVariables]
    private LawboardSiliconLawUi? _window;

    public CustomLawboardBoundInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<LawboardSiliconLawUi>();
        _window.LawsChangedEvent += OnLawsChanged;

        Update();
    }

    public void Update()
    {
        if (_window == null)
            return;

        var customLawboard = EntMan.EnsureComponent<CustomLawboardComponent>(Owner);
        _window.SetLaws(customLawboard.Laws);
    }

    private void OnLawsChanged(List<SiliconLaw> value, bool popup)
    {
        SendPredictedMessage(new CustomLawboardChangeLawsMessage(value, popup));
    }
}
