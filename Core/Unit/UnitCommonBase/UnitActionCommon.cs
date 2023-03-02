using System;
using System.Collections;
using System.Collections.Generic;
using Core.Data;
using Core.GameManagement;
using Core.GameManagement.EventSenders;
using Core.GameManagement.Interfaces;
using Core.Unit.Actions;
using Core.Unit.Movement;
using Core.Unit.Specials;
using Core.Unit.StateMachine.enums;
using Core.Unit.Warrior;
using Unity.VisualScripting;
using UnityEngine;

//UnitActionCommon
public abstract partial class UnitCommon : IActionable
{
    [Header("ActionPoints")] [SerializeField]
    public int maxActionPoints;

    public List<SpecialMovesCommon> specialMovesList;
   
    public IActionable Actionable;
    public ActionTurnType actionType { get; set; }
    public int currentActionPoints { get; set; }
    
    private bool isHookshot;
    private void ConfigureActionable()
    {
        Actionable = GetComponent<IActionable>();
        RegisterActionable(Actionable);
    }
    
    public void RegisterActionable(IActionable actionable)
    {
        TurnManagementController.instance.RegisterActionable(actionable);
    }

    public void UnRegisterActionable(IActionable actionable)
    {
        throw new NotImplementedException();
    }

    public IActionable GetActionable()
    {
        return Actionable;
    }

    public void ConsumeActionPoints(int amount)
    {
        int previousActionPoints = currentActionPoints;
        currentActionPoints -= amount;

        DispatchActionPointsChangeEvent(previousActionPoints);
    }

    public void AddActionPoints(int amount)
    {
        int previousActionPoints = currentActionPoints;
        currentActionPoints += amount;

        DispatchActionPointsChangeEvent(previousActionPoints);
    }

    public int GetPointsRequired(ActionTypes actionTypes)
    {
        return actionTypes switch
        {
            ActionTypes.Move => moveCost,
            ActionTypes.Attack => currentWeaponObject.actionPointsConsumed,
            ActionTypes.Reload => 0,
            ActionTypes.Guard => 0,
            ActionTypes.Special => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(actionTypes), actionTypes, null)
        };
    }

    public void PerformAction(ActionTypes type, int cost)
    {
        var totalCost = cost;
        switch (type)
        {
            case ActionTypes.Move:
                totalCost *= GetPointsRequired(type);
                break;
            case ActionTypes.Attack:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        ConsumeActionPoints(totalCost);
    }
    

    public virtual void OnBeginTurn(SelectableTypes type)
    {
        if (type == GetUnitType())
        {
            //Debug.Log("its my teams turn");
        }
    }

    public virtual void OnEndTurn()
    {

    }

    public void RefreshActionPoints()
    {
        int previousActionPoints = currentActionPoints;
        currentActionPoints = maxActionPoints;

        DispatchActionPointsChangeEvent(previousActionPoints);
    }

    private void DispatchActionPointsChangeEvent(int previousActionPoints)
    {
        EventSenderController.UnitActionPointsChange(Actionable, previousActionPoints);
    }
    

    public Transform AttackCamTransform()
    {
        return attackCameraPos.transform;
    }

    public void HookshotPerformed(HookshotAction hookshotAction)
    {
        if (hookshotAction.selectedUnit == this)
        {
            StartCoroutine(PerformHookshot(hookshotAction));
        }
    }

    private IEnumerator PerformHookshot(HookshotAction hookshotAction)
    {
        unitBehaviour.SetState(UnitStateTypes.Move);
        unitBehaviour.moveState.EnableFreeMove();
        var hookshotable = hookshotAction.target;
        isHookshot = true;
        while (Vector3.Distance(motor.transform.position, hookshotable.GetHookshotPosition()) > .5f)
        {
            SetMovementVelocity(hookshotable.GetHookshotPosition());
            yield return new WaitForEndOfFrame();
        }
        ConsumeActionPoints(hookshotAction.hookshotData.actionPointCost);
        switch (hookshotAction.target)
        {
            case UnitCommon unitTarget:
                StartCoroutine(CompleteHookshotAttack(hookshotAction, unitTarget));
                break;
            case NavPoint navPoint:
                CompleteHookshotMove(hookshotAction, navPoint);
                break;
        }
    }

    private IEnumerator CompleteHookshotAttack(HookshotAction hookshotAction, UnitCommon unitTarget)
    {
        var attackAction = new AttackAction(this, unitTarget, null);
        attackAction.damage = hookshotAction.hookshotData.damage;
        unitTarget.TakeDamage(attackAction);
        var airHang = hookshotAction.target.GetHookshotPosition() + Vector3.up * 3;
        while (Vector3.Distance(motor.transform.position, airHang) > .5f)
        {
            SetMovementVelocity(airHang);
            yield return new WaitForEndOfFrame();
        }
        isHookshot = false;
        EventSenderController.HookshotCompleted(hookshotAction);
    }

    private void CompleteHookshotMove(HookshotAction hookshotAction, NavPoint navPoint)
    {
        var navPath = new Stack<int>(new[] {navPoint.navIndex});
        StartCoroutine(MovePath(navPath, navPoint));
        isHookshot = false;
        EventSenderController.HookshotCompleted(hookshotAction);
    }
}