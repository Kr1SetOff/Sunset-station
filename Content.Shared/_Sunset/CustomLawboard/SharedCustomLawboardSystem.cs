using System.Linq;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Popups;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;

namespace Content.Shared._Sunset.CustomLawboard;

public abstract class SharedCustomLawboardSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedSiliconLawSystem _siliconLaw = default!;

    public static readonly int MaxLaws = 15;
    public static readonly int MaxLawLength = 512; // arbitrary limits, matches upstream

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CustomLawboardComponent, CustomLawboardChangeLawsMessage>(OnChangeLaws);
    }

    public static List<SiliconLaw> SanitizeLaws(List<SiliconLaw> listToSanitize)
    {
        var sanitizedLaws = new List<SiliconLaw>();
        foreach (var law in listToSanitize.Take(MaxLaws))
        {
            var sanitizedLaw = law.LawString.Replace("\n", " "); // newlines mess up chat when the law is stated

            if (sanitizedLaw.Length > MaxLawLength)
                sanitizedLaw = sanitizedLaw[..MaxLawLength];

            sanitizedLaws.Add(new SiliconLaw
            {
                LawString = sanitizedLaw,
                Order = law.Order,
                LawIdentifierOverride = law.LawIdentifierOverride,
            });
        }
        return sanitizedLaws;
    }

    public static SiliconLawset CreateLawset(List<SiliconLaw> laws)
    {
        return new SiliconLawset { Laws = laws };
    }

    private void OnChangeLaws(EntityUid uid, CustomLawboardComponent customLawboard, CustomLawboardChangeLawsMessage args)
    {
        EnsureComp<SiliconLawProviderComponent>(uid);
        var sanitizedLaws = SanitizeLaws(args.Laws);
        var lawset = CreateLawset(sanitizedLaws);

        customLawboard.Laws = sanitizedLaws;
        _siliconLaw.SetLawset(uid, lawset);
        _adminLogger.Add(LogType.Action, $"{ToPrettyString(args.Actor)} changed laws on {ToPrettyString(uid)}");
        Dirty(uid, customLawboard);

        if (args.Popup)
            _popup.PopupClient(Loc.GetString("custom-lawboard-updated"), args.Actor, args.Actor);

        DirtyUI(uid, customLawboard);
    }

    protected virtual void DirtyUI(EntityUid uid, CustomLawboardComponent? customLawboard) { }
}
