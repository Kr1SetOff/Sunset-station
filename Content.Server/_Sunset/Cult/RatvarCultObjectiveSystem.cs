using Content.Server.Antag;
using Content.Server.Objectives.Systems;
using Content.Shared._Sunset.Cult;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;

namespace Content.Server._Sunset.Cult;

/// <summary>
/// 🌇Sunset🌇 - progress tracking for the Ratvar cult's objectives (see
/// Resources/Prototypes/_Sunset/Cult/objectives.yml), plus handing the head cultist their extra
/// "summon Ratvar" objective on top of the shared ones every cultist gets via AntagObjectivesComponent
/// (Content.Server.Antag.AntagObjectivesSystem) - that generic system applies the same fixed list to
/// every selection under the rule, with no way to special-case just the leader definition.
/// </summary>
public sealed class RatvarCultObjectiveSystem : EntitySystem
{
    [Dependency] private readonly NumberObjectiveSystem _number = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RatvarCultConvertConditionComponent, ObjectiveGetProgressEvent>(OnGetConvertProgress);
        SubscribeLocalEvent<RatvarCultSummonConditionComponent, ObjectiveGetProgressEvent>(OnGetSummonProgress);
        SubscribeLocalEvent<RatvarCultLeaderComponent, AfterAntagEntitySelectedEvent>(OnLeaderSelected);
    }

    private void OnLeaderSelected(Entity<RatvarCultLeaderComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        if (_mind.TryGetMind(args.EntityUid, out var mindId, out var mind))
            _mind.TryAddObjective(mindId, mind, "RatvarCultSummonObjective");
    }

    private void OnGetConvertProgress(Entity<RatvarCultConvertConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var target = _number.GetTarget(ent.Owner);
        var converted = 0;

        var ruleQuery = EntityQueryEnumerator<RatvarCultRuleComponent>();
        while (ruleQuery.MoveNext(out _, out var ruleComp))
            converted = ruleComp.ConvertedCount;

        args.Progress = target == 0 ? 1f : MathF.Min(converted / (float) target, 1f);
    }

    private void OnGetSummonProgress(Entity<RatvarCultSummonConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var summoned = false;

        var ruleQuery = EntityQueryEnumerator<RatvarCultRuleComponent>();
        while (ruleQuery.MoveNext(out _, out var ruleComp))
            summoned |= ruleComp.RatvarSummoned;

        args.Progress = summoned ? 1f : 0f;
    }
}
