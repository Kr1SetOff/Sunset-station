using System.Linq;
using Content.Shared._Starlight.Abstract.Conditions;
using Content.Shared.Administration.Managers;
using Robust.Shared.Player;

namespace Content.Shared._Sunset.GhostTheme;

/// <summary>
/// Gates a ghost theme (or anything else using <see cref="BaseRequirement"/>) behind the admin's
/// manually-set rank/title (AdminData.Title, set via the admin panel / database - not the
/// Discord-role-synced ranks in Resources/Prototypes/_NullLink/adminRanks.yml, which
/// DiscordRolesRequirement already covers). Passes if the admin's title matches (case-insensitively)
/// any entry in Titles.
/// </summary>
public sealed partial class AdminTitleRequirement : BaseRequirement
{
    [Dependency] private ISharedAdminManager _admin = default!;

    [DataField(required: true)]
    public List<string> Titles = [];

    public override string GetRequirementDescription()
    {
        base.GetRequirementDescription();

        return Loc.GetString("sunset-admin-title-requirement-fail", ("titles", string.Join(", ", Titles)));
    }

    public override bool Handle(ICommonSession user)
    {
        base.Handle(user);

        var title = _admin.GetAdminData(user)?.Title;
        return title != null && Titles.Any(t => string.Equals(t, title, StringComparison.OrdinalIgnoreCase));
    }
}
