using Robust.Shared.Serialization;

namespace Content.Shared._Sunset.Sandevistan;

[Serializable, NetSerializable]
public enum SandevistanState : byte
{
    Warning = 0,
    Shaking = 1,
    Stamina = 2,
    Damage = 3,
    Knockdown = 4,
    Disable = 5,
    Death = 6,
}
