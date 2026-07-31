using Content.Shared._Goobstation.Changeling.Actions;
using Content.Shared._Goobstation.Changeling.Components;
using Content.Shared._Goobstation.InternalResources.Events;
using Content.Shared._Goobstation.LightDetection.Components;
using Content.Shared._Goobstation.Overlays;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Atmos.Components;
using Content.Shared.Popups;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Robust.Shared.Timing;

namespace Content.Shared._Goobstation.Changeling.Systems;

public abstract class SharedDarknessAdaptionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;

    private EntityQuery<GoobNightVisionComponent> _nvgQuery;
    private EntityQuery<StealthOnMoveComponent> _stealthOnMoveQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DarknessAdaptionComponent, ComponentStartup>(OnMapInit);
        SubscribeLocalEvent<DarknessAdaptionComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<DarknessAdaptionComponent, ActionDarknessAdaptionEvent>(OnToggleAbility);
        SubscribeLocalEvent<DarknessAdaptionComponent, InternalResourcesRegenModifierEvent>(OnChangelingChemicalRegenEvent);

        _nvgQuery = GetEntityQuery<GoobNightVisionComponent>();
        _stealthOnMoveQuery = GetEntityQuery<StealthOnMoveComponent>();
    }

    private void OnMapInit(Entity<DarknessAdaptionComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.UpdateTimer = _timing.CurTime + ent.Comp.UpdateDelay;

        // so this doesnt mess over any other abilities or systems
        ent.Comp.HadLightDetection = HasComp<LightDetectionComponent>(ent);

        EnsureNightVision(ent);

        ent.Comp.ActionEnt = _actions.AddAction(ent, ent.Comp.ActionId);

        Dirty(ent);
    }

    private void OnShutdown(Entity<DarknessAdaptionComponent> ent, ref ComponentShutdown args)
    {
        HandleSpecialComponents(ent);
        HandleAlerts(ent, false);
        SetAdaptingBool(ent, false);

        RemCompDeferred<GoobNightVisionComponent>(ent);

        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);
    }

    #region Event Handlers

    private void OnToggleAbility(Entity<DarknessAdaptionComponent> ent, ref ActionDarknessAdaptionEvent args)
    {
        ent.Comp.Active = !ent.Comp.Active;
        DirtyField(ent, ent.Comp, nameof(DarknessAdaptionComponent.Active));

        var popup = ent.Comp.Active ? ent.Comp.ActivePopup : ent.Comp.InactivePopup;

        if (!ent.Comp.Active)
        {
            AdjustAdaption(ent, ent.Comp.Active);
            HandleSpecialComponents(ent);
            HandleAlerts(ent, ent.Comp.Active);
        }
        else
            EnsureComp<LightDetectionComponent>(ent);

        DoPopup(ent, popup);

        args.Handled = true;
    }

    private void OnChangelingChemicalRegenEvent(Entity<DarknessAdaptionComponent> ent, ref InternalResourcesRegenModifierEvent args)
    {
        if (args.Data.InternalResourcesType != ent.Comp.ResourceType
            || !ent.Comp.Adapting)
            return;

        args.Modifier -= ent.Comp.ChemicalModifier;
    }
    #endregion

    #region Helper Methods
    protected void DoAbility(Entity<DarknessAdaptionComponent> ent, bool state)
    {
        if (FireInvalidCheck(ent))
        {
            AdjustAdaption(ent, false);
            HandleAlerts(ent, false);
            return;
        }

        AdjustAdaption(ent, state);
        HandleAlerts(ent, state);
    }

    private void AdjustAdaption(Entity<DarknessAdaptionComponent> ent, bool adapting)
    {
        if (adapting)
            EnsureAndSetStealth(ent);
        else
            RemCompDeferred<StealthComponent>(ent);

        if (!_nvgQuery.HasComp(ent))
            EnsureNightVision(ent);

        var nvg = Comp<GoobNightVisionComponent>(ent);
        nvg.IsActive = adapting;
        Dirty(ent, nvg);

        SetAdaptingBool(ent, adapting);
    }

    private void HandleAlerts(Entity<DarknessAdaptionComponent> ent, bool show)
    {
        if (show && !ent.Comp.AlertDisplayed)
        {
            _alerts.ShowAlert(
                ent.Owner,
                ent.Comp.AlertId);

            ent.Comp.AlertDisplayed = true;
        }

        if (!show && ent.Comp.AlertDisplayed)
        {
            _alerts.ClearAlert(
                ent.Owner,
                ent.Comp.AlertId);

            ent.Comp.AlertDisplayed = false;
        }

        DirtyField(ent, ent.Comp, nameof(DarknessAdaptionComponent.AlertDisplayed));
    }

    private bool FireInvalidCheck(Entity<DarknessAdaptionComponent> ent)
    {
        return TryComp<Content.Shared.Atmos.Components.FlammableComponent>(ent, out var flammable) && flammable.OnFire;
    }

    private void DoPopup(Entity<DarknessAdaptionComponent> ent, LocId popup)
    {
        _popup.PopupClient(Loc.GetString(popup), ent, ent);
    }

    private void SetAdaptingBool(Entity<DarknessAdaptionComponent> ent, bool adapting)
    {
        if (adapting)
            ent.Comp.Adapting = true;

        else if (!ent.Comp.Active
            || !adapting)
            ent.Comp.Adapting = false;

        DirtyField(ent, ent.Comp, nameof(DarknessAdaptionComponent.Adapting));
    }

    private void EnsureAndSetStealth(Entity<DarknessAdaptionComponent> ent)
    {
        var stealth = EnsureComp<StealthComponent>(ent);

        _stealth.SetEnabled(ent, true, stealth);
        _stealth.SetVisibility(ent, ent.Comp.Visibility, stealth);

        if (_stealthOnMoveQuery.HasComp(ent))
            RemCompDeferred<StealthOnMoveComponent>(ent);
    }

    private void EnsureNightVision(Entity<DarknessAdaptionComponent> ent)
    {
        var nightVision = EnsureComp<GoobNightVisionComponent>(ent);

        nightVision.IsActive = false;
        nightVision.Color = Color.FromHex("#606cb3");
        nightVision.ActivateSound = null;
        nightVision.DeactivateSound = null;
        _actions.RemoveAction(ent.Owner, nightVision.ToggleActionEntity);

        Dirty(ent, nightVision);
    }

    private void HandleSpecialComponents(Entity<DarknessAdaptionComponent> ent)
    {
        if (!ent.Comp.HadLightDetection)
            RemCompDeferred<LightDetectionComponent>(ent); // saves on performance if the ability isn't active

        RemCompDeferred<StealthComponent>(ent);
        RemCompDeferred<StealthOnMoveComponent>(ent);
    }
    #endregion
}
