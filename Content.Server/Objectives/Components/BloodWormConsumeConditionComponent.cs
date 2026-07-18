namespace Content.Server.Objectives.Components;

/// <summary>
/// 🌇Sunset🌇 - objective condition checking a blood worm's lifetime total consumed blood
/// (BloodWormComponent.ConsumedBlood) against a target amount. See BloodWormConsumeConditionSystem.
/// </summary>
[RegisterComponent]
public sealed partial class BloodWormConsumeConditionComponent : Component
{
    [DataField]
    public float Amount = 800f;
}
