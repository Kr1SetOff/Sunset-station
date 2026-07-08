// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Sunset.Genetics;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Sunset.Genetics.UI;

[UsedImplicitly]
public sealed class DnaModifierConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private DnaModifierConsoleWindow? _window;

    public DnaModifierConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<DnaModifierConsoleWindow>();

        _window.OnRadiate += (category, block, subBlock) =>
            SendMessage(new DnaModifierRadiateMessage(category, block, subBlock));
        _window.OnPulse += category => SendMessage(new DnaModifierPulseMessage(category));
        _window.OnEject += () => SendMessage(new DnaModifierEjectMessage());
        _window.OnBuffer += (action, slot) => SendMessage(new DnaModifierBufferMessage(action, slot));
        _window.OnPrintActivator += id => SendMessage(new DnaModifierPrintActivatorMessage(id));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is DnaModifierConsoleBoundUserInterfaceState modifierState)
            _window?.Populate(modifierState);
    }
}
