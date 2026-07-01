using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.MartialArts.Components;

/// <summary>
/// Marks an entity as having learned a martial art style. Exclusive - an entity can only know one
/// style at a time. Stores the entity's original fist damage so it can be restored if the style
/// is ever removed.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MartialArtsKnowledgeComponent : Component
{
    [DataField, AutoNetworkedField]
    public MartialArtStyle Style = MartialArtStyle.None;

    [DataField]
    public DamageSpecifier? OriginalFistDamage;
}
