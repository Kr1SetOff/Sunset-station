using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Server.Silicons.Laws;
using Content.Shared._Sunset.MalfAi;
using Content.Shared.Actions;
using Content.Shared.FixedPoint;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Sunset.MalfAi;

/// <summary>
/// 🌇Sunset🌇 - wires a player up as a Malfunctioning AI when the antag is granted: CPU module store,
/// law zero, the innate hack/store actions and the briefing. The abilities themselves live in
/// <see cref="MalfAiSystem"/>.
/// </summary>
public sealed class MalfAiRuleSystem : GameRuleSystem<MalfAiRuleComponent>
{
    [Dependency] private AntagSelectionSystem _antag = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;
    [Dependency] private SiliconLawSystem _siliconLaws = default!;
    [Dependency] private IGameTiming _timing = default!;

    private static readonly EntProtoId ActionOpenModules = "ActionMalfAiOpenModules";
    private static readonly EntProtoId ActionHackApc = "ActionMalfAiHackApc";

    public const string CpuCurrency = "MalfCpu";

    private static readonly SoundPathSpecifier BriefingSound = new("/Audio/Ambience/Antag/traitor_start.ogg");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiRuleComponent, AfterAntagEntitySelectedEvent>(OnAntagSelected);
    }

    private void OnAntagSelected(Entity<MalfAiRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        MakeMalfAi(args.EntityUid, ent.Comp);
    }

    public void MakeMalfAi(EntityUid target, MalfAiRuleComponent rule)
    {
        var malf = EnsureComp<MalfAiComponent>(target);
        malf.NextCpuTick = _timing.CurTime + TimeSpan.FromMinutes(1);

        // CPU module store, same StoreComponent setup as the changeling evolution menu.
        var store = EnsureComp<StoreComponent>(target);
        foreach (var category in rule.StoreCategories)
            store.Categories.Add(category);
        store.CurrencyWhitelist.Add(CpuCurrency);
        store.Balance.TryAdd(CpuCurrency, FixedPoint2.New(rule.StartingCpu));

        // The AI brain prototype has no store UI registered - merge one in (SetUi merges into the
        // existing UserInterfaceComponent, so the law/comms UIs are untouched).
        _ui.SetUi(target, StoreUiKey.Key,
            new InterfaceData("StoreBoundUserInterface", interactionRange: 0f, requireInputValidation: false));

        _actions.AddAction(target, ActionOpenModules);
        _actions.AddAction(target, ActionHackApc);

        AddLawZero(target);

        _antag.SendBriefing(target, Loc.GetString("malf-ai-role-greeting"), Color.Red, BriefingSound);
    }

    /// <summary>
    /// Inserts the classic malf law zero above the AI's existing lawset. Marked unsayable, same as
    /// the emag law, so stating laws doesn't reveal the subversion. Writes go through the law
    /// system's SetLawset helper - SiliconLawProviderComponent.Lawset is Access-restricted.
    /// </summary>
    private void AddLawZero(EntityUid target)
    {
        if (!TryComp<SiliconLawProviderComponent>(target, out var provider))
            return;

        var current = provider.Lawset ?? _siliconLaws.GetLawset(provider.Laws);

        var lawZero = new SiliconLaw
        {
            LawString = Loc.GetString("malf-ai-law-zero"),
            Order = 0,
            LawIdentifierOverride = "0",
            Sayable = false,
        };

        var laws = new List<SiliconLaw>(current.Laws);
        if (laws.Count > 0 && laws[0].Order == 0)
            laws[0] = lawZero; // don't stack multiple law zeroes on re-grant
        else
            laws.Insert(0, lawZero);

        _siliconLaws.SetLawset(target, new SiliconLawset
        {
            Laws = laws,
            ObeysTo = current.ObeysTo,
        });

        _siliconLaws.NotifyLawsChanged(target, provider.LawUploadSound);
    }
}
