using Content.Server.Bible.Components;
using Content.Shared._Goobstation.Religion;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Verbs;

namespace Content.Server._Goobstation.Religion.Nullrod;

/// <summary>
/// Ported from Goob-Station's RecallPrayableSystem, trimmed down to the "Normal" recall case only
/// (Unremoveable/DualWield/Embedded nullrod variants aren't ported in this fork).
/// </summary>
public sealed class RecallNullrodSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RecallNullrodComponent, GetVerbsEvent<ActivationVerb>>(OnGetVerb);
        SubscribeLocalEvent<RecallNullrodComponent, RecallNullrodDoAfterEvent>(OnDoAfter);
    }

    private void OnGetVerb(Entity<RecallNullrodComponent> ent, ref GetVerbsEvent<ActivationVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !TryComp<BibleUserComponent>(args.User, out var bibleUser))
            return;

        var user = args.User;
        args.Verbs.Add(new ActivationVerb
        {
            Text = Loc.GetString("chaplain-recall-verb"),
            Act = () =>
            {
                if (bibleUser.NullRod == null)
                {
                    _popup.PopupEntity(Loc.GetString("chaplain-recall-no-nullrod"), user, user);
                    return;
                }

                var doAfterArgs = new DoAfterArgs(EntityManager, user, ent.Comp.DoAfterDuration, new RecallNullrodDoAfterEvent(), ent.Owner)
                {
                    BreakOnMove = true,
                    NeedHand = true,
                };

                _doAfter.TryStartDoAfter(doAfterArgs);
            },
        });
    }

    private void OnDoAfter(Entity<RecallNullrodComponent> ent, ref RecallNullrodDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || TerminatingOrDeleted(args.User))
            return;

        if (!_hands.TryGetEmptyHand(args.User, out _))
        {
            _popup.PopupEntity(Loc.GetString("chaplain-recall-hands-full"), args.User, args.User);
            return;
        }

        if (!TryComp<BibleUserComponent>(args.User, out var bibleUser) || bibleUser.NullRod is not { } nullrod)
            return;

        args.Handled = true;

        if (TerminatingOrDeleted(nullrod))
        {
            _popup.PopupEntity(Loc.GetString("chaplain-recall-nullrod-gone"), args.User, args.User);
            bibleUser.NullRod = null;
            return;
        }

        if (_hands.IsHolding(args.User, nullrod))
        {
            _popup.PopupEntity(Loc.GetString("chaplain-recall-nullrod-already-in-hand", ("nullrod", nullrod)), args.User, args.User);
            return;
        }

        if (!_hands.TryPickupAnyHand(args.User, nullrod))
            return;

        _popup.PopupEntity(Loc.GetString("chaplain-recall-nullrod-recalled", ("nullrod", nullrod)), args.User, args.User);
    }
}
