using Content.Shared._Goobstation.Changeling.Components;
using Content.Shared._Goobstation.Changeling.Systems;
using Content.Server.Polymorph.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Polymorph;

namespace Content.Server._Goobstation.Changeling;

public sealed partial class ChangelingRegenerateSystem : SharedChangelingRegenerateSystem
{
    [Dependency] private readonly PolymorphSystem _polymorph = default!;

    private EntityQuery<ChangelingIdentityComponent> _lingQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingRegenerateComponent, PolymorphedEvent>(OnPolymorphed);

        _lingQuery = GetEntityQuery<ChangelingIdentityComponent>();
    }

    private void OnPolymorphed(Entity<ChangelingRegenerateComponent> ent, ref PolymorphedEvent args)
    {
        if (_lingQuery.TryComp(ent, out var ling)
            && ling.IsInLastResort)
            return;

        _polymorph.CopyPolymorphComponent<ChangelingRegenerateComponent>(ent, args.NewEntity);
    }

    #region Helper Methods
    protected override void RegenerateChangelingBody(Entity<ChangelingRegenerateComponent> ent, BodyComponent bodyComp)
    {
        // Adapted for this fork: gibbing here is destructive (parts are deleted, not kept
        // around to restore), so there's nothing to reattach - regeneration just relies on
        // the damage healing done elsewhere in this ability.
    }
    #endregion
}
