using Content.Shared._Starlight.Vampire.Components;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Reaction;

namespace Content.Server._Goobstation.Religion;

/// <summary>
/// Bridges the dynamic UnholyComponent marker (added/removed at runtime by VampireSystem,
/// DevilSystem, etc) to the static, YAML-only ReactiveComponent.ReactiveGroups list that
/// Resources/Prototypes/Reagents/medicine.yml's Holywater reagent already keys its "Unholy" splash
/// reaction (5 heat damage + scream, see reactiveEffects.Unholy in that file) off of. Without this,
/// that reaction is entirely dead - no entity in the codebase ever has the "Unholy" reactive group,
/// since nothing marks it on species/mob prototypes (unholy-ness isn't a fixed species trait here,
/// it's a temporary antag state).
/// </summary>
public sealed class UnholyReactiveSystem : EntitySystem
{
    private static readonly HashSet<ReactionMethod> UnholyReactiveMethods = new() { ReactionMethod.Touch };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UnholyComponent, ComponentStartup>(OnUnholyStartup);
        SubscribeLocalEvent<UnholyComponent, ComponentShutdown>(OnUnholyShutdown);
    }

    private void OnUnholyStartup(EntityUid uid, UnholyComponent component, ComponentStartup args)
    {
        var reactive = EnsureComp<ReactiveComponent>(uid);
        reactive.ReactiveGroups ??= new();
        reactive.ReactiveGroups["Unholy"] = UnholyReactiveMethods;
    }

    private void OnUnholyShutdown(EntityUid uid, UnholyComponent component, ComponentShutdown args)
    {
        if (TryComp<ReactiveComponent>(uid, out var reactive))
            reactive.ReactiveGroups?.Remove("Unholy");
    }
}
