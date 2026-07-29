using Content.Server.Polymorph.Systems;
using Robust.Shared.IoC;
using Robust.Shared.Serialization.Manager;

namespace Content.Server._Goobstation.Changeling;

/// <summary>
///     Goob-Station's PolymorphSystem has a CopyPolymorphComponent helper this fork's
///     PolymorphSystem doesn't have; this extension replicates it so ported Changeling code
///     (which transforms the player into other mob prototypes and needs to carry components
///     like ChangelingIdentityComponent over to the new body) didn't need call-site changes.
/// </summary>
public static class PolymorphSystemExtensions
{
    public static void CopyPolymorphComponent<T>(this PolymorphSystem polymorph, EntityUid oldEntity, EntityUid newEntity)
        where T : Component
    {
        var entMan = IoCManager.Resolve<IEntityManager>();

        if (!entMan.TryGetComponent<T>(oldEntity, out var comp))
            return;

        var serialization = IoCManager.Resolve<ISerializationManager>();
        var copy = serialization.CreateCopy(comp, notNullableOverride: true);
        entMan.AddComponent(newEntity, copy);
    }
}
