using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared.Chemistry;

[UsedImplicitly]
public sealed partial class ReactiveSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _proto = default!;

    public void DoEntityReaction(EntityUid uid, Solution solution, ReactionMethod method)
    {
        foreach (var reagent in solution.Contents.ToArray())
        {
            ReactionEntity(uid, method, reagent);
        }
    }

    public void ReactionEntity(EntityUid uid, ReactionMethod method, ReagentQuantity reagentQuantity)
    {
        if (reagentQuantity.Quantity == FixedPoint2.Zero)
            return;

        // We throw if the reagent specified doesn't exist.
        if (!_proto.Resolve<ReagentPrototype>(reagentQuantity.Reagent.Prototype, out var proto))
            return;

        var ev = new ReactionEntityEvent(method, reagentQuantity, proto);
        RaiseLocalEvent(uid, ref ev);

        // Goobstation - relay for systems that need touch reactions but cannot subscribe to
        // ReactionEntityEvent on ReactiveComponent, since SharedEntityEffectsSystem already owns
        // that (comp, event) pair and duplicate directed subscriptions throw.
        if (method == ReactionMethod.Touch)
        {
            var relayEv = new TouchReactionRelayEvent();
            RaiseLocalEvent(uid, ref relayEv);
        }
    }
}
public enum ReactionMethod
{
Touch,
Injection,
Ingestion,
}

[ByRefEvent]
public readonly record struct ReactionEntityEvent(ReactionMethod Method, ReagentQuantity ReagentQuantity, ReagentPrototype Reagent);

/// <summary>
/// Goobstation - relay of <see cref="ReactionEntityEvent"/> for touch reactions.
/// Exists purely so more than one system can react to being splashed.
/// </summary>
[ByRefEvent]
public readonly record struct TouchReactionRelayEvent();
