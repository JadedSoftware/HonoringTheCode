using System;
using System.Collections.Generic;
using Core;
using Core.Camera;
using Core.GameManagement;
using Core.UI;
using Core.Unit;
using Core.Unit.Targeting;
using Core.Unit.Warrior;
using TGS;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using EventSenderController = Core.GameManagement.EventSenders.EventSenderController;


/// <summary>
/// Manages mouse interactions with units.
/// 
/// </summary>
public class GameManagementController : MonoSingleton<GameManagementController>, IEvents
{
    public LayerMask enemiesHit;
    private TerrainGridSystem gridSystem;
    private IDamageable highlightedDamagable;
    private ISelectable highlightedSelectable;
    public InputControls inputControls;
    private LayersEnum layersEnum;
    private ISelectable selectedUnit;

    private UnitCommon selectedUnitToMeleeAttack;
    private IDamageable meleeDamageable;
    public bool isGamePaused { get; private set; }

    private SpecialMoveController specialMoveController;

    public WeaponListController weaponListController;

    private CameraController cameraController => CameraController.instance;
    private NavigableController navController => NavigableController.instance;
    private ActionRequestController actionRequestController => ActionRequestController.instance;

    private CameraOverlayController cameraOverlayController => FindObjectOfType<CameraOverlayController>();
    private GameUiController gameUIController => FindObjectOfType<GameUiController>();


    Collider[] colliderResults = new Collider[10];
    GrenadeAttack grenadeAttack;

    public override void Init()
    {
        inputControls = new InputControls();
        specialMoveController = new GameObject { }.AddComponent<SpecialMoveController>();
        weaponListController = new WeaponListController();
    }

    private void OnEnable()
    {
        inputControls.Enable();
        RegisterEvents();
    }

    public void OnDisable()
    {
        inputControls.Disable();
        UnRegisterEvents();
    }

    public void RegisterEvents()
    {
        EventSenderController.unitMovementComplete += UnitMovementCompleted;
        EventSenderController.unitMovementRequested += UnitMovementRequested;
        EventSenderController.unitMovementInitiated += UnitMovementInitiated;
        EventSenderController.unitSelected += UnitSelected;
        EventSenderController.unitDeselected += DeselectUnit;
        EventSenderController.onUnitTargeted += OnUnitTargeted;
        EventSenderController.mouseEnterSelectable += MouseOverSelectable;
        EventSenderController.mouseExitSelectable += MouseExitSelectable;
        EventSenderController.onEndTurn += OnEndTurn;
        EventSenderController.onGamePaused += GamePaused;
        inputControls.Select.LeftClick.performed += LeftClickAction;
        inputControls.Select.RightClick.performed += RightClickAction;

        
    }


    public void UnRegisterEvents()
    {
        EventSenderController.unitMovementComplete -= UnitMovementCompleted;
        EventSenderController.unitMovementRequested -= UnitMovementRequested;
        EventSenderController.unitMovementInitiated -= UnitMovementInitiated;
        EventSenderController.unitSelected -= UnitSelected;
        EventSenderController.unitDeselected -= DeselectUnit;
        EventSenderController.onUnitTargeted -= OnUnitTargeted;
        EventSenderController.mouseEnterSelectable -= MouseOverSelectable;
        EventSenderController.mouseExitSelectable -= MouseExitSelectable;
        EventSenderController.onEndTurn -= OnEndTurn;
        EventSenderController.onGamePaused -= GamePaused;
    }
    
    private void GamePaused(bool isPaused)
    {
        isGamePaused = isPaused;
        if (isGamePaused)
            inputControls.Disable();
        else
            inputControls.Enable();
    }

    private void MouseOverSelectable(ISelectable selectable)
    {
        if(isGamePaused) return;
        highlightedSelectable = selectable;
        if (selectable is IDamageable damageable) highlightedDamagable = damageable;
    }
    
    private void OnUnitTargeted(UnitCommon unittarget)
    {
        if(isGamePaused) return;
        highlightedDamagable = unittarget;
    }

    private void MouseExitSelectable(ISelectable selectable)
    {
        highlightedSelectable = null;
        if (selectable is IDamageable damageable)
            if (damageable == highlightedDamagable)
                highlightedDamagable = null;
    }

    private void UnitSelected(ISelectable unit)
    {
        selectedUnit = unit;
    }

    private void DeselectUnit()
    {
        selectedUnit = null;
    }

    private void UnitMovementInitiated(IMovable unit, Stack<int> navPath, INavigable endNavigable)
    {
    }

    private void LeftClickAction(InputAction.CallbackContext ctx)
    {
        var mousePosScreen = CameraController.instance.MouseScreenPosition();
        bool isMouseOverGameWindow = (!(0 > mousePosScreen.x || 0 > mousePosScreen.y ||
                                        1 < mousePosScreen.x || 1 < mousePosScreen.y));
        if (GameOverlayController.instance.IsPointerOverUI || isGamePaused || !isMouseOverGameWindow)
            return;
        if (CameraOverlayController.instance.currentOverlayState == CameraOverlayStates.Hookshot)
        {
            HookshotLeftClick();
            return;
        }
        if(CameraOverlayController.instance.currentOverlayState == CameraOverlayStates.MeleeAttack)
        {
            if (selectedUnit is IMovable _moveable)
            {
                INavigable moveNavPoint;
                moveNavPoint = navController.highlightedNavigable;
                if (moveNavPoint == null) return;
                if (!navController.meleeMoveCells.Contains(moveNavPoint) && !moveNavPoint.IsOccupied())
                {
                    return;
                }
                else
                {
                    if (navController.IsPossibleNav(moveNavPoint) && !moveNavPoint.IsOccupied())
                    {
                        var navPath = actionRequestController.RequestNavPath(_moveable, moveNavPoint);
                        if (navPath.Count > 0)
                            EventSenderController.UnitMovementPerformed(_moveable, navPath, moveNavPoint);
                        
                    }
                }
            }
            return;
        }
        if(CameraOverlayController.instance.currentOverlayState == CameraOverlayStates.Grenade)
        {
        }
        switch (cameraController.cameraStateMachine.CurrentKey)
        {
            case CameraViewStates.Topdown:
                TopdownLeftClick();
                break;
            case CameraViewStates.Attack:
            case CameraViewStates.AttackOrbit:
                if (CameraController.instance.attackViewState.currentTargetingObject != null)
                    UnitAttackRequested();
                else
                {
                    AttackViewLeftClick();
                }
                break;
            case CameraViewStates.EnemyTurn:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    private void AttackViewLeftClick()
    {
        //todo throw new NotImplementedException();
    }

    private void TopdownLeftClick()
    {
        RaycastHit hit;
        var ray = cameraController.GetMouseRay();

        if (Physics.Raycast(ray, out hit, 100, LayerMaskHelper.selectableMask))
        {
            var layer = (LayersEnum) hit.collider.gameObject.layer;
            switch (layer)
            {
                case LayersEnum.Terrain:
                    NavpointRay(hit);
                    break;
                case LayersEnum.Unit:
                    UnitRay(hit);
                    break;
                case LayersEnum.Obstacle:
                    NavpointRay(hit);
                    break;
                case LayersEnum.GrapplePoint:
                    NavpointRay(hit);
                    break;
                case LayersEnum.UI:
                    break;
                default:
                    Debug.Log(layer + " not accounted for");
                    break;
            }
        }
    }

    private void RightClickAction(InputAction.CallbackContext ctx)
    {
        if(isGamePaused) return;
        switch (cameraController.ViewState())
        {
            case CameraViewStates.Topdown:
                EventSenderController.UnitDeselected();
                break;
            case CameraViewStates.Attack:
                EventSenderController.EnterTopdownView();
                break;
            case CameraViewStates.EnemyTurn:
                break;
            case CameraViewStates.AttackOrbit:
                EventSenderController.EnterTopdownView();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    private void NavpointRay(RaycastHit hit)
    {
        if(selectedUnitToMeleeAttack != null) selectedUnitToMeleeAttack = null;

        if(CameraOverlayController.instance.currentOverlayState == CameraOverlayStates.Grenade)
        {
            if (selectedUnit is IMovable _moveable)
            {
                INavigable moveNavPoint;
                moveNavPoint = navController.highlightedNavigable;
                if (moveNavPoint == null) return;
                if (!navController.grenadeThrowingCells.Contains(moveNavPoint.GetNavIndex()) && !moveNavPoint.IsOccupied())
                {
                    return;
                }
                else
                {
                    GrenadeAttack(hit);
                }
            }
        }
        else
        {
            if (selectedUnit is IMovable _moveable)
            {
                INavigable moveNavPoint;
                moveNavPoint = navController.highlightedNavigable ?? navController.NavAtPosition(hit);
                if (moveNavPoint == null) return;
                if (navController.IsPossibleNav(moveNavPoint) && !moveNavPoint.IsOccupied())
                {
                    var unit = selectedUnit.GetUnit();
                    unit.rifleIdleState = unit.animancer.Play(unit._RifleIdle);
                    unit.rifleIdleState.NormalizedEndTime = 0.05f;
                    unit.rifleIdleState.Events.OnEnd = unit.OnEventEndPlayMoveAnimation;

                    var navPath = actionRequestController.RequestNavPath(_moveable, moveNavPoint);
                    if (navPath.Count > 0)
                        EventSenderController.UnitMovementPerformed(_moveable, navPath, moveNavPoint);
                }
            }
        }
    }
    private void UnitRay(RaycastHit hit)
    {
        if (selectedUnit is IDamageable damageable) meleeDamageable = damageable;
        selectedUnitToMeleeAttack = hit.collider.GetComponent<UnitCommon>();
        var clickedUnit = hit.collider.GetComponent<UnitCommon>();
        if (clickedUnit)
            switch (clickedUnit.GetUnitType())
            {
                case SelectableTypes.Player:
                    if (selectedUnit == null)
                    {
                        EventSenderController.UnitSelected(clickedUnit);
                        return;
                    }

                    if (selectedUnit != null && selectedUnit != clickedUnit.GetSelectable())
                    {
                        EventSenderController.UnitDeselected();
                        EventSenderController.UnitSelected(clickedUnit);
                        return;
                    }

                    if (selectedUnit != null && selectedUnit == clickedUnit.GetSelectable())
                    {
                        EventSenderController.UnitDeselected();
                    }

                    break;
                case SelectableTypes.AI:

                    gameUIController.ToggleAttackUI(true);
                    gameUIController.PositionAttackUIAtTarget(hit.collider.transform.position);

                    GetCurrentNavigableOverlap(clickedUnit);                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
    }

    public void GetCurrentNavigableOverlap(UnitCommon clickedUnit)
    {
        foreach (var linkedNav in clickedUnit.SelectableCurrentNavigable().GetLinkedNavigables())
        {
            if (NavigableController.instance.possibleNavPoints.Contains(linkedNav.GetNavIndex()))
            {
                NavigableController.instance.meleeMoveCells.Add(linkedNav);
            }
        }
    }

    private void HookshotLeftClick()
    {
        EventSenderController.HookshotAttackRequested();
    }

    private void UnitMeleeAttackRequested()
    {
        meleeDamageable = selectedUnitToMeleeAttack;
        if (IsUnitSelected() && meleeDamageable != null)
            if (selectedUnit is IWarrior warrior)
                if (actionRequestController.RequestAttackAction(warrior,
                        meleeDamageable))
                {
                    var attackAction = new AttackAction(warrior, meleeDamageable, TargetingController.instance.currentTargetingObject);
                    EventSenderController.AttackInitiated(attackAction);
                }
    }

    private void UnitAttackRequested()
    {
        if (IsUnitSelected() && highlightedDamagable != null)
            if (selectedUnit is IWarrior warrior)
                if (actionRequestController.RequestAttackAction(warrior,
                        highlightedDamagable))
                {
                    var attackAction = new AttackAction(warrior, highlightedDamagable, TargetingController.instance.currentTargetingObject);
                    EventSenderController.AttackInitiated(attackAction);
                }
    }

    private void UnitMovementRequested(IMovable unit, INavigable endnavigable)
    {
    }

    private void UnitMovementCompleted(IMovable unit)
    {        
        UnitMeleeAttackRequested();

        gameUIController.ToggleAttackUI(false);

        cameraOverlayController.camOverlayStateMachine.ForceSetState(CameraOverlayStates.Movement);
    }

    private void MiddleClickAction(InputAction.CallbackContext ctx)
    {
    }

    public ISelectable GetCurrentSelectable()
    {
        return selectedUnit;
    }

    public UnitCommon GetCurrentUnit()
    {
        return selectedUnit.GetUnit();
    }

    public bool IsUnitSelected()
    {
        return selectedUnit != null;
    }

    public IDamageable GetSelectedDamagable()
    {
        return highlightedDamagable;
    }

    private void OnEndTurn()
    {

    }

    private void GrenadeAttack(RaycastHit hit)
    {
        EffectsController.instance.ActivateGrenadeEffect(hit.point);

        var numColliders = Physics.OverlapSphereNonAlloc(hit.point, 5f, colliderResults, enemiesHit);

        for (int i = 0; i < numColliders; i++)
        {
            if (colliderResults[i].transform.root.gameObject.name.Contains("Enemies"))
            {
                grenadeAttack = new GrenadeAttack(GetCurrentUnit().GetWarrior(),
                            colliderResults[i].transform.GetComponent<UnitCommon>(),
                            null);
                grenadeAttack.damage = 10;
                colliderResults[i].transform.GetComponent<UnitCommon>().TakeDamage(grenadeAttack);
            }
        }
    }
}