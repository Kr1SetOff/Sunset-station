using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Preferences.Loadouts.Effects;

/// <summary>
/// 🌇Sunset🌇 - restricts a loadout item to specific jobs, keyed off the RoleLoadout it's currently
/// being validated for (RoleLoadout.Role - i.e. which job's loadout window this is), rather than
/// playtime in that role. Lets a single shared loadoutGroup (e.g. the sponsor tab) hold items that are
/// only actually pickable while making a character for one particular job.
/// </summary>
public sealed partial class RoleRestrictionLoadoutEffect : LoadoutEffect
{
    [DataField(required: true)]
    public List<ProtoId<RoleLoadoutPrototype>> Roles = new();

    public override bool Validate(
        HumanoidCharacterProfile profile,
        RoleLoadout loadout,
        ICommonSession? session,
        IDependencyCollection collection,
        out FormattedMessage reason)
    {
        var success = Roles.Contains(loadout.Role);

        reason = FormattedMessage.FromMarkupPermissive(Loc.GetString(
            success ? "loadout-role-restriction-pass" : "loadout-role-restriction-fail",
            ("roles", string.Join(", ", Roles.Select(r => r.Id)))));

        return success;
    }
}
