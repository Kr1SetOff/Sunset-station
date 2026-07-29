// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;

namespace Content.Shared._Goobstation.Temperature;

public sealed class TemperatureImmunityEvent(float currentTemperature) : EntityEventArgs
{
    public float CurrentTemperature = currentTemperature;
    public readonly float IdealTemperature = 310.15f; // 37C, human body temperature
}

[ByRefEvent]
public record struct BeforeTemperatureChange(
    float CurrentTemperature,
    float LastTemperature,
    float TemperatureDelta);
