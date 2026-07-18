using Content.Shared.Random;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(SecretRuleSystem))]
public sealed partial class SecretRuleComponent : Component
{
    /// <summary>
    /// The gamerules that get added by secret.
    /// </summary>
    [DataField("additionalGameRules")]
    public HashSet<EntityUid> AdditionalGameRules = new();

    /// <summary>
    /// 🌇Sunset🌇 - if set, this specific Secret rule always rolls from this weightedRandom pool
    /// instead of the server-wide game.secret_weight_prototype CVar. Lets a distinct preset (e.g.
    /// "Secret (Low-Pop)") pin the SecretLP table regardless of what the CVar says for the default
    /// Secret preset, so both pools can be offered side by side in the round-start vote.
    /// </summary>
    [DataField]
    public ProtoId<WeightedRandomPrototype>? WeightsOverride;
}
