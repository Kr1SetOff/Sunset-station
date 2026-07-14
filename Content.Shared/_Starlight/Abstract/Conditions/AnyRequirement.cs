using System.Linq;
using Robust.Shared.Player;

namespace Content.Shared._Starlight.Abstract.Conditions;

/// <summary>
/// Passes if ANY of its nested requirements pass, unlike a plain requirements: list (evaluated with
/// .All(), see GhostThemeSystem) which requires every entry to pass. Lets a single ghost theme (or
/// anything else gated by BaseRequirement) be unlocked by several unrelated routes at once - e.g.
/// "has this Discord role OR has this manually-set admin rank" - without needing a bespoke combined
/// requirement type for every such combination.
/// </summary>
public sealed partial class AnyRequirement : BaseRequirement
{
    [DataField(required: true)]
    public List<BaseRequirement> Requirements = [];

    public override string GetRequirementDescription()
    {
        base.GetRequirementDescription();

        return string.Join(" " + Loc.GetString("requirements-any-or") + " ",
            Requirements.Select(r => r.GetRequirementDescription()));
    }

    public override bool Handle(ICommonSession user)
    {
        base.Handle(user);

        return Requirements.Any(r => r.Handle(user));
    }
}
