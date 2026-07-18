using Content.Shared._Starlight.Antags.Vampires.Components;
using Content.Shared.Body.Components;
using Content.Shared.Verbs;

namespace Content.Server._Starlight.Antags.Vampires.Systems;

// 🌇Sunset🌇 - RMB "Drink blood" verb for vampires with extended fangs. The base Starlight
// mechanic only listens for a bare-handed LEFT click (BeforeInteractHand/AfterInteract), which
// players routinely read as "right-click to drink" - now both work: the verb runs the exact same
// validation + StartDrinkDoAfter path as the left-click handlers.
public sealed partial class VampireSystem
{
    private void InitializeSunsetBiteVerb()
    {
        SubscribeLocalEvent<BloodstreamComponent, GetVerbsEvent<AlternativeVerb>>(OnGetBiteVerb);
    }

    private void OnGetBiteVerb(Entity<BloodstreamComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.User == ent.Owner)
            return;

        if (!TryComp<VampireComponent>(args.User, out var vampire) || !vampire.FangsExtended)
            return;

        var user = args.User;
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("vampire-verb-drink"),
            Priority = 10,
            Act = () => TryStartDrinkFromVerb(user, vampire, ent.Owner),
        });
    }

    /// <summary>
    /// Mirrors the validation chain of OnBeforeInteractHand, then starts the same drink do-after.
    /// </summary>
    private void TryStartDrinkFromVerb(EntityUid uid, VampireComponent comp, EntityUid target)
    {
        if (!comp.FangsExtended || !Exists(target) || target == uid || !HasComp<BloodstreamComponent>(target))
            return;

        if (IsInvalidDrinkTarget(uid, target))
            return;

        if (IsProtectedByFaith(target) && comp.FullPower != true)
        {
            _popup.PopupEntity(Loc.GetString("vampire-target-protected-by-faith"), uid, uid, Shared.Popups.PopupType.MediumCaution);
            return;
        }

        if (IsMouthBlocked(uid))
        {
            _popup.PopupEntity(Loc.GetString("vampire-mouth-covered"), uid, uid);
            return;
        }

        StartDrinkDoAfter(uid, comp, target, showPopup: true);
    }
}
