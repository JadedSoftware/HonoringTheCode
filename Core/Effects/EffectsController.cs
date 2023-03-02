using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Camera;
using Core.Effects;
using Core.GameManagement;
using Core.GameManagement.Interfaces;
using Core.Helpers;
using Core.Unit.Warrior;
using RootMotion.FinalIK;
using TGS;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using EventSenderController = Core.GameManagement.EventSenders.EventSenderController;

/// <summary>
/// Handles creating effects at the specified locations.
/// </summary>
public class EffectsController : MonoBehaviour, IEvents
{
    private static EffectsController _instance;
    public EffectScriptableObjects effectScriptable;
    public GrappleMarkerEffect grappleMarkerEffect;

    [FormerlySerializedAs("navPointEffect")]
    public HookshotEffect hookshotEffect;

    public GrenadeEffect grenadeEffect;
    public GameObject grenadeHoverEffect;

    public DamagePopEffect damagePopEffect;
    private readonly Dictionary<INavigable, GameObject> activeEffects = new();
    private readonly List<INavigable> markedForRemoval = new();
    private INavigable highlightedNavigable;
    public ISelectable highlightedSelectable;

    public static EffectsController instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType(typeof(EffectsController)) as EffectsController;

            return _instance;
        }
        set => _instance = value;
    }

    private NavigableController navControl => NavigableController.instance;
    private GameManagementController gameManager => GameManagementController.instance;
    private NavPathJobs navPathJobs => NavPathJobs.instance;

    private void OnEnable()
    {
        RegisterEvents();
    }

    private void OnDisable()
    {
        UnRegisterEvents();
    }

    public void RegisterEvents()
    {
        EventSenderController.mouseEnterSelectable += MoveEnterSelectable;
        EventSenderController.mouseExitSelectable += MouseExitSelectable;
        EventSenderController.mouseEnterCell += MouseEnterCell;
        EventSenderController.mouseExitCell += MouseExitCell;
        EventSenderController.unitSelected += UnitSelected;
        EventSenderController.unitDeselected += UnitDeselected;
        EventSenderController.unitMovementInitiated += UnitMovementInitiated;
        EventSenderController.unitMovementComplete += UnitMovementComplete;
        EventSenderController.navigableHighlighted += NaviagbleHighlighted;
        EventSenderController.mouseOverNav += MouseOverNavigable;
        EventSenderController.mouseExitNav += MouseExitNavigable;
        EventSenderController.damagableUnitEnter += MouseEnterDamageable;
        EventSenderController.damagableUnitExit += MouseExitDamageable;
        EventSenderController.removeNaviHighlighted += RemoveNaviEffects;
        EventSenderController.onAttackPerformed += AttackPerformed;
        EventSenderController.onAttackCompleted += AttackCompleted;
    }

    public void UnRegisterEvents()
    {
        EventSenderController.mouseEnterSelectable -= MoveEnterSelectable;
        EventSenderController.mouseExitSelectable -= MouseExitSelectable;
        EventSenderController.mouseEnterCell -= MouseEnterCell;
        EventSenderController.mouseExitCell -= MouseExitCell;
        EventSenderController.unitSelected -= UnitSelected;
        EventSenderController.unitDeselected -= UnitDeselected;
        EventSenderController.unitMovementInitiated -= UnitMovementInitiated;
        EventSenderController.unitMovementComplete -= UnitMovementComplete;
        EventSenderController.navigableHighlighted -= NaviagbleHighlighted;
        EventSenderController.mouseOverNav -= MouseOverNavigable;
        EventSenderController.mouseExitNav += MouseExitNavigable;
        EventSenderController.damagableUnitEnter += MouseEnterDamageable;
        EventSenderController.damagableUnitExit += MouseExitDamageable;
        EventSenderController.removeNaviHighlighted -= RemoveNaviEffects;
        EventSenderController.onAttackPerformed -= AttackPerformed;
        EventSenderController.onAttackCompleted -= AttackCompleted;
    }

    private void MoveEnterSelectable(ISelectable selectable)
    {
        highlightedSelectable = selectable;
        if (!activeEffects.ContainsKey(selectable.SelectableCurrentNavigable())
            && gameManager.IsUnitSelected()
            && gameManager.GetCurrentSelectable() != selectable)
            NaviagbleHighlighted(selectable.SelectableCurrentNavigable());
    }

    private void MouseExitSelectable(ISelectable selectable)
    {
        highlightedSelectable = null;
        if (activeEffects.ContainsKey(selectable.SelectableCurrentNavigable()))
            RemoveNaviEffects(selectable.SelectableCurrentNavigable());
    }

    private void MouseEnterCell(TerrainGridSystem gridsystem, int cellindex)
    {
        var cell = navControl.GetNavigable(cellindex);
        if (!cell.IsOccupied() || navControl.selectedNavigable == null) return;
        if (navControl.selectedNavigable.GetMovable() is not null || cell.GetMovable() is not null)
            if (navControl.selectedNavigable.GetMovable() == cell.GetMovable()
                || navControl.selectedNavigable.GetMovable().GetUnitType() != cell.GetMovable().GetUnitType()
                || activeEffects.ContainsKey(cell))
                return;
        //NaviagbleHighlighted(cell);
        if (gameManager.IsUnitSelected())
        {
            var navigable = navControl.GetNavFromCellIndex(cellindex, gridsystem);
            if (activeEffects.ContainsKey(navigable)) return;
            var gradient = GetEffectGradient(ColorType.Blue);
            var effect = GetEffectType(EffectActionType.CellHighlighted);
            if (effect.effectType == EffectType.Particle)
            {
                Debug.Log("Play Particle Effect");
                var visualEffect = Instantiate(effect.particleEffect);
                visualEffect.transform.position = navigable.GetPosition();
                var mainEffect = visualEffect.hexParticleEffects.FirstOrDefault(a => a.name == "MiniZone");
                mainEffect.effectGO.SetActive(true);
                activeEffects.Add(navigable, visualEffect.gameObject);
                var col = mainEffect.hexParticleSystem.colorOverLifetime;
                col.color = gradient;
            }
        }
    }

    private void MouseExitCell(TerrainGridSystem gridsystem, int cellindex)
    {
        var navigable = navControl.GetNavFromCellIndex(cellindex, gridsystem);
        if (highlightedSelectable == null) RemoveNaviEffects(navigable);
    }

    private void OnCellHighlight(TerrainGridSystem sender, INavigable cell)
    {
    }

    private void MouseOverNavigable(INavigable navigable)
    {
        if (CameraOverlayController.instance.currentOverlayState == CameraOverlayStates.Hookshot)
        {
            if (navigable is NavPoint navPoint)
            {
                if (SpecialMoveController.instance.hookShotTargets.Contains(navPoint))
                    ActivateHookshot(navPoint);
            }

            return;
        }

        if (navigable is NavGrapplePoint grapplePoint)
        {
            if (gameManager.IsUnitSelected())
            {
                highlightedNavigable = grapplePoint;
                grapplePoint.grappleMarkerEffect.EnableGrappleHighlight();
            }
        }
    }

    private void MouseExitNavigable(INavigable navigable)
    {
        if (CameraOverlayController.instance.currentOverlayState == CameraOverlayStates.Hookshot)
        {
            if (navigable is NavPoint navPoint)
            {
                if(SpecialMoveController.instance.hookShotTargets.Contains(navPoint))
                    DeactivateHookshot(navPoint);
            }
            return;
        }

        if (navigable is NavGrapplePoint grapplePoint)
            if (grapplePoint.grappleMarkerEffect != null)
            {
                highlightedNavigable = null;
                grapplePoint.grappleMarkerEffect.DisableGrappleHighlight();
            }
    }
    
    private void MouseEnterDamageable(UnitCommon unit)
    {
        if (CameraOverlayController.instance.currentOverlayState == CameraOverlayStates.Hookshot)
        {
            if (SpecialMoveController.instance.hookShotTargets.Contains(unit))
            {
                ActivateHookshot(unit);
            }
        }
    }
    private void MouseExitDamageable(UnitCommon unit)
    {
        if (CameraOverlayController.instance.currentOverlayState == CameraOverlayStates.Hookshot)
        {
            if (SpecialMoveController.instance.hookShotTargets.Contains(unit))
            {
                DeactivateHookshot(unit);
            }        
        }
    }
    
    private void NaviagbleHighlighted(INavigable navigable)
    {
        //EventRequestController.NavigableHighlighted(cell);
    }

    private void RemoveNaviEffects(INavigable navigable)
    {
        if (!activeEffects.ContainsKey(navigable)) return;
        activeEffects[navigable].SetActive(false);
        activeEffects.Remove(navigable);
        //StartCoroutine(TimedRemoveEffects(navigable));
    }

    private IEnumerator TimedRemoveEffects(INavigable navigable)
    {
        yield return new WaitForSeconds(.25f);
        if (!markedForRemoval.Contains(navigable)) yield break;
        if (activeEffects.ContainsKey(navigable))
        {
            activeEffects[navigable].SetActive(false);
            activeEffects.Remove(navigable);
        }
    }

    private void UnitSelected(ISelectable unit)
    {
        var gradient = GetEffectGradient(ColorType.Green);
    }

    private void UnitDeselected()
    {
        var effectsToRemove = activeEffects.Select(activeEffect => activeEffect.Key).ToList();
        foreach (var navigable in effectsToRemove) RemoveNaviEffects(navigable);
    }

    private void UnitMovementInitiated(IMovable unit, Stack<int> navpath, INavigable endnavigable)
    {
        var gradient = GetEffectGradient(ColorType.Green);
        var effect = GetEffectType(EffectActionType.MoveEffect);
        if (effect.effectType == EffectType.VfxGraph)
        {
            var visualEffect = Instantiate(effect.vfxEffect);
            visualEffect.transform.position = endnavigable.GetPosition();
        }

        if (effect.effectType == EffectType.Particle)
        {
        }
    }

    private void AttackCompleted(AttackAction attackaction)
    {
        var damageEffect = Instantiate(damagePopEffect, transform);
        damageEffect.transform.position = attackaction.damageable.DamageTextPosition();
        damageEffect.transform.LookAt(CameraController.instance.mainCamera.transform);
        damageEffect.SetText(attackaction.damage.ToString());
    }

    private void AttackPerformed(AttackAction attackaction)
    {
    }

    private HexHighlightEffects GetEffectType(EffectActionType moveEffect)
    {
        var effect = effectScriptable.hexHighlightEffects.First(a => a.actionType == moveEffect);
        return effect;
    }

    private void UnitMovementComplete(IMovable unit)
    {
    }

    private Gradient GetEffectGradient(ColorType colorType)
    {
        var colorGradient = effectScriptable.effectsGradientColors.First(a => a.color == colorType);
        return colorGradient.gradient;
    }

    private void SetMovableNavs(ISelectable unit, Gradient gradient, List<INavigable> navList)
    {
        foreach (var navigable in navList)
        {
            var effect = GetEffectType(EffectActionType.UnitSelected);
            if (effect.effectType == EffectType.Particle)
            {
                var visualEffect = Instantiate(effect.particleEffect);
                visualEffect.transform.position = navigable.GetPosition();
                var mainEffect = visualEffect.hexParticleEffects.FirstOrDefault(a => a.name == "MiniZone");
                mainEffect.effectGO.SetActive(true);
                activeEffects.Add(navigable, visualEffect.gameObject);
                var col = mainEffect.hexParticleSystem.colorOverLifetime;
                col.color = gradient;
            }
        }
    }

    public void ActivateGrenadeEffect(Vector3 position)
    {
        var visualEffect = Instantiate(grenadeEffect);
        visualEffect.transform.position = position;
    }

    public void HighlightHookshotPoint(IHookshotable hookShotable, Vector3 startPos)
    {
        if (hookShotable.hookshotEffect == null)
        {
            var effect = Instantiate(hookshotEffect, transform, true);
            effect.Init(hookShotable);
        }

        DrawingController.DrawRay(startPos, hookShotable.hookshotEffect.transform.position,
            ColorHelper.GetColor(ColorPallete.Yellow, ColorShade.Light), 3f);
        hookShotable.hookshotEffect.EngageEffect();
    }

    public void ActivateHookshot(IHookshotable hookShotable)
    {
        EventSenderController.HookshotTargetActive(hookShotable);
        hookShotable.hookshotEffect.EngageHighlight();
    }

    private void DeactivateHookshot(IHookshotable hookShotable)
    {
        EventSenderController.HookshotTargetDeactive();
        hookShotable.hookshotEffect.DisengageHighlight();
    }
}