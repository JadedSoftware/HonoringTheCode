using System.Collections.Generic;
using Core.GameManagement;
using UnityEngine;
using EventSenderController = Core.GameManagement.EventSenders.EventSenderController;

/// <summary>
/// Handles action requests from units
/// Provides a navPath to a unit on move request
/// </summary>
public class ActionRequestController : MonoBehaviour
{
    private NavPathJobs navPathFinder => NavPathJobs.instance;
    private static ActionRequestController _instance;
    public static ActionRequestController instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType(typeof(ActionRequestController)) as ActionRequestController;

            return _instance;
        }
        set => _instance = value;
    }

    private void OnEnable()
    {
        RegisterEvents();
    }

    private void RegisterEvents()
    {
        EventSenderController.onBeginTurn += OnBeingTurn;
        EventSenderController.onEndTurn += OnEndTurn;
    }

    private void OnBeingTurn(SelectableTypes endingtype)
    {
        
    }

    private void OnEndTurn()
    {

    }

    public Stack<int> RequestNavPath(IMovable unit, INavigable endNav)
    {
        EventSenderController.UnitMovementRequested(unit, endNav);
        var pathTime = Time.realtimeSinceStartup;
        var navPath = navPathFinder.FindPath(unit.GetCurrentNavigable().GetNavIndex(), endNav.GetNavIndex());
        if (DebugController.instance.isDebugEnabled)
            Debug.Log("Job A* : " + (Time.realtimeSinceStartup - pathTime) * 1000f);
        return navPath;
    }

    public bool RequestAttackAction(IWarrior warrior, IDamageable damageable)
    {
        var cost = warrior.AttackActionCost();
        var currentPoints = warrior.currentActionPoints;
        return cost <= currentPoints;
    }
}