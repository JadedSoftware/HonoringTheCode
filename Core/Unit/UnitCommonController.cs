using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Camera;
using Core.GameManagement;
using Core.GameManagement.Interfaces;
using Core.Unit.Specials;
using Core.Unit.Warrior;
using UnityEngine;
using EventSenderController = Core.GameManagement.EventSenders.EventSenderController;

public enum UnitIndexDirection
{
    Next,
    Previous
}

public enum CompassDirections
{
    E = 0,
    NE = 1,
    N = 2,
    NW = 3,
    W = 4,
    SW = 5,
    S = 6,
    SE = 7,
    None = 8
}

public class BestTarget
{
    public float bearing;
    public float distance;
    public float navigableCount;
    public UnitCommon unit;

    public BestTarget(UnitCommon unit, float bearing, float distance)
    {
        this.unit = unit;
        this.bearing = bearing;
        this.distance = distance;
    }
}

/// <summary>
/// Keeps track of all units within a scene. Provides targeting information between units and provides a service to keep track of unit locations between units.
/// Keeps track if mouse is over a unit
/// Selects a unit on the beginning of a turn
/// </summary>
public class UnitCommonController : MonoSingleton<UnitCommonController>
{
    public List<PlayerUnit> allPlayerUnits;
    public List<AIUnit> allAiUnits;
    public List<UnitCommon> allUnits;
    public int selectedPlayerIndex;
    private readonly List<IMovable> allMoveables = new();
    private ISelectable mouseOverUnit;
    public UnitCommon pointerOverDamagable { get; private set; }

    private void OnEnable()
    {
        EventSenderController.mouseEnterSelectable += MouseEnterSelectable;
        EventSenderController.mouseExitSelectable += MouseExitSelectable;
        EventSenderController.unitSelected += OnUnitSelected;
        EventSenderController.unitDeselected += OnUnitDeselected;
        EventSenderController.initiateDamagableDeath += DamageableDeathInitiated;
        EventSenderController.onBeginTurn += BeginTurn;
        EventSenderController.onEndTurn += EndTurn;
        EventSenderController.damagableUnitEnter += PointerOverDamagable;
        EventSenderController.damagableUnitExit += PointerLeaveDamagable;
        EventSenderController.onOverlayChanged += OverlayChanged;
    }

    private void OnDisable()
    {
        EventSenderController.mouseEnterSelectable -= MouseEnterSelectable;
        EventSenderController.mouseExitSelectable -= MouseExitSelectable;
        EventSenderController.unitSelected -= OnUnitSelected;
        EventSenderController.unitDeselected -= OnUnitDeselected;
        EventSenderController.initiateDamagableDeath -= DamageableDeathInitiated;
        EventSenderController.onBeginTurn -= BeginTurn;
        EventSenderController.onEndTurn -= EndTurn;
        EventSenderController.damagableUnitEnter -= PointerOverDamagable;
        EventSenderController.damagableUnitExit -= PointerLeaveDamagable;
        EventSenderController.onOverlayChanged -= OverlayChanged;
    }

    private void PointerOverDamagable(UnitCommon unit)
    {
        pointerOverDamagable = unit;
    }

    private void PointerLeaveDamagable(UnitCommon unit)
    {
        if (pointerOverDamagable == unit) pointerOverDamagable = null;
    }

    private void MouseEnterSelectable(ISelectable unit)
    {
        mouseOverUnit = unit;
    }

    private void MouseExitSelectable(ISelectable selectable)
    {
        mouseOverUnit = null;
    }

    public bool isOverSelectable()
    {
        return mouseOverUnit != null;
    }

    public void RegisterMovable(IMovable movable, bool isActive)
    {
        if (isActive)
            allMoveables.Add(movable);
        else
            allMoveables.Remove(movable);
    }

    public List<IMovable> MovablesByType(SelectableTypes type)
    {
        return allMoveables.Where(a => a.GetUnitType() == type).ToList();
    }

    public int RegisterUnit(UnitCommon unitCommon)
    {
        allUnits.Add(unitCommon);
        switch (unitCommon)
        {
            case PlayerUnit playerUnit:
                allPlayerUnits.Add(playerUnit);
                break;
            case AIUnit aiUnit:
                allAiUnits.Add(aiUnit);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return allUnits.IndexOf(unitCommon);
    }

    private void OnUnitSelected(ISelectable unit)
    {
        switch (unit.GetSelectableType())
        {
            case SelectableTypes.Player:
                if (unit is PlayerUnit playerUnit)
                    selectedPlayerIndex = allPlayerUnits.IndexOf(playerUnit);
                break;
            case SelectableTypes.AI:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnUnitDeselected()
    {
    }

    //Activated by Button in top right
    public void UnitSelectDirection(UnitIndexDirection direction)
    {
        EventSenderController.UnitDeselected();
        var newSelectable = GetPlayerUnitDirection(direction).GetSelectable();
        EventSenderController.UnitSelected(newSelectable);
    }

    public void SelectPlayerUnitWithAPDirection(UnitIndexDirection direction)
    {
        int nextUnitIndex = GetPlayerUnitIndexDirection(selectedPlayerIndex, direction);
        while (selectedPlayerIndex != nextUnitIndex) {
            UnitCommon unit = allPlayerUnits[nextUnitIndex];
            if (unit.Actionable.currentActionPoints > 0) {
                EventSenderController.UnitDeselected();
                EventSenderController.UnitSelected(unit.GetSelectable());
                break;
            }
            nextUnitIndex = GetPlayerUnitIndexDirection(nextUnitIndex, direction);
        }
    }

    private void DamageableDeathInitiated(AttackAction attackAction)
    {
        var unit = attackAction.damageable.GetUnit();
        switch (unit)
        {
            case PlayerUnit playerUnit:
                if (allPlayerUnits.Contains(unit))
                    allPlayerUnits.Remove(playerUnit);
                break;
            case AIUnit aiUnit:
                if (allAiUnits.Contains(unit))
                    allAiUnits.Remove(aiUnit);
                break;
        }
        if (allUnits.Contains(unit))
            allUnits.Remove(unit);
        if (allMoveables.Contains(unit.GetMovable()))
            allMoveables.Remove(unit);
    }

    private UnitCommon GetPlayerUnitDirection(UnitIndexDirection direction)
    {
        return allPlayerUnits[GetPlayerUnitIndexDirection(selectedPlayerIndex, direction)];
    }

    private int GetPlayerUnitIndexDirection(int index, UnitIndexDirection direction) {
        if (allPlayerUnits.Count == 0) throw new InvalidOperationException("No player units to get.");
        if (index > allPlayerUnits.Count - 1 || index < 0) throw new  ArgumentOutOfRangeException(nameof(index), index, null);

        var nextIndex = direction switch
        {
            UnitIndexDirection.Next => selectedPlayerIndex + 1 > allPlayerUnits.Count - 1 
                ? 0 
                : selectedPlayerIndex + 1,
            UnitIndexDirection.Previous => selectedPlayerIndex == 0
                ? allPlayerUnits.Count - 1
                : selectedPlayerIndex - 1,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };

        return nextIndex;
    }

    private void BeginTurn(SelectableTypes type)
    {
        switch (type)
        {
            case SelectableTypes.Player:
                EventSenderController.UnitSelected(allPlayerUnits.FirstOrDefault()?.GetSelectable());
                break;
            case SelectableTypes.AI:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private void EndTurn()
    {
        EventSenderController.UnitDeselected();
    }


    private UnitCommon UnitByIndex(int index)
    {
        return allUnits[index];
    }

    private void OverlayChanged(CameraOverlayStates overlayState)
    {
        if (overlayState == CameraOverlayStates.Hookshot)
        {
            List<IHookshotable> hookshotables = new();
            var unit = GameManagementController.instance.GetCurrentUnit();
            var specialMove = unit.specialMovesList.FirstOrDefault(x => x.specialType == SpecialTypes.Hookshot);
            if (specialMove is SpecialHookshot {canAttackEnemies: true})
            {
                var startPos = unit.motor.transform.position + unit.motor.Capsule.center;
                var targets = allUnits.Where(x => x.GetUnitType() != unit.GetUnitType());
                foreach (var target in targets)
                {
                    var ray = new Ray(startPos,
                        (target.motor.transform.position + target.motor.Capsule.center) - startPos);
                    if (Physics.Raycast(ray, out RaycastHit hitInfo,
                            Vector3.Distance(startPos, target.currentPos) * 1.25f,
                            LayerMaskHelper.selectableMask))
                    {
                        if (hitInfo.collider.gameObject == target.gameObject)
                        {
                            hookshotables.Add(target);
                        }
                    }
                }
                EventSenderController.HookshotTargetsAvailable(hookshotables, startPos);
            }
        }
    }

    public bool CheckActionPoints(UnitCommon selectedUnit, IAction actionData)
    {
        return selectedUnit.currentActionPoints >= actionData.actionPointCost;
    }
}