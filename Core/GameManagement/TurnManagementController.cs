using System;
using System.Collections.Generic;
using System.Linq;
using Core.Data;
using Core.GameManagement.EventSenders;
using Core.Interfaces;
using Core.Unit.Warrior;
using Unity.VisualScripting;
using UnityEngine;

namespace Core.GameManagement
{
    /// <summary>
    /// Handles changing between User and Ai turns.
    /// </summary>
    public class TurnManagementController : MonoSingleton<TurnManagementController>, IDataPersistable
    {
        public SelectableTypes currentTurn;
        public List<IActionable> allActionables = new();
        public List<IMovable> allMovables = new();
        
        private void OnEnable()
        {
            RegisterEvents();
        }

        private void OnDisable()
        {
            UnRegisterEvents();
        }

        private void RegisterEvents()
        {
            EventSenderController.onBeginTurn += OnBeginTurn;
            EventSenderController.onEndTurn += OnEndTurn;
            EventSenderController.unitSelected += UnitSelected;
            EventSenderController.unitDeselected += DeselectUnit;
            EventSenderController.onNavReady += NavsReady;
            EventSenderController.initiateDamagableDeath += OnDamageableDeath;
            EventSenderController.onUnitActionPointsChange += OnUnitActionPointsChange;
        }

        private void OnDamageableDeath(AttackAction attackaction)
        {
            var unit = attackaction.damageable.GetUnit();
            var moveable = unit.GetMovable();
            var actionable = unit.GetActionable();
            if(allMovables.Contains(moveable))
                allMovables.Remove(moveable);
            
        }
        
        private void UnRegisterEvents()
        {
            EventSenderController.onBeginTurn -= OnBeginTurn;
            EventSenderController.onEndTurn -= OnEndTurn;
            EventSenderController.unitSelected -= UnitSelected;
            EventSenderController.unitDeselected -= DeselectUnit;
            EventSenderController.onNavReady -= NavsReady;
            EventSenderController.initiateDamagableDeath -= OnDamageableDeath;
            EventSenderController.onUnitActionPointsChange -= OnUnitActionPointsChange;
        }

        private void NavsReady()
        {
            RefreshAP(LevelManagementController.instance.startingTurn);
            foreach (var actionable in allActionables.Where(a => a.GetUnitType() == LevelManagementController.instance.startingTurn))
                actionable.RefreshActionPoints();
            EventSenderController.BeginTurn(LevelManagementController.instance.startingTurn);
        }
        
        private void OnBeginTurn(SelectableTypes type)
        {
            currentTurn = type;
            RefreshAP(currentTurn);
            Debug.Log("begin turn : " + currentTurn);
        }

        private void OnEndTurn()
        {
            switch (currentTurn)
            {
                case SelectableTypes.Player:
                    EventSenderController.BeginTurn(SelectableTypes.AI);
                    break;
                case SelectableTypes.AI:
                    EventSenderController.BeginTurn(SelectableTypes.Player);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private void RefreshAP(SelectableTypes type)
        {
            foreach (var actionable in allActionables.Where(a => a.GetUnitType() == type))
                actionable.RefreshActionPoints();
        }
        
        public bool IsCurrentTurn(IActionable request)
        {
            return currentTurn == request.GetUnitType();
        }

        public void RegisterActionable(IActionable actionable)
        {
            allActionables.Add(actionable);
        }

        private void UnitSelected(ISelectable unit)
        {
            switch (unit.GetSelectableType())
            {
                case SelectableTypes.Player:
                    break;
                case SelectableTypes.AI:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DeselectUnit()
        {
        }

        private void OnUnitActionPointsChange(IActionable actionable, int previousActionPoints)
        {
            if (currentTurn != actionable.GetUnitType()) return;

            if (actionable.currentActionPoints == 0) {
                EventSenderController.EnterTopdownView();

                if (CurrentTurnHasActionPoints()) {
                    UnitCommonController.instance.SelectPlayerUnitWithAPDirection(UnitIndexDirection.Next);
                } else {
                    EventSenderController.PlayerOutOfActionPoints();
                }
            }
        }
        

        public void OnSave(GameData gameData)
        {
            gameData.currentTurn = currentTurn;
        }

        public void OnLoad(GameData gameData)
        {
            
        }
        
        private bool CurrentTurnHasActionPoints()
        {
            return allActionables.Where(actionable => currentTurn == actionable.GetUnitType())
                .Any(actionable => actionable.currentActionPoints > 0);
        }
    }
}