using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core.AI;
using Core.GameManagement.Interfaces;
using Core.Unit.AI.Goal.GoalActions;
using Core.Unit.StateMachine.enums;
using Core.Unit.Warrior;

namespace Core.Unit.AI.Goal
{
    public static class AttackActionBuilder
    {
        public static bool BuildAttackAction(AiActionPlan actionPlan, WarriorWeaponSO warriorWeaponSo,
            UnitInfo playerUnit,
            out AttackGoalAction attackAction)
        {
            //todo 
            // if (currentWeapon != warriorWeaponSo) 
            // { Add cost of switching to estimate });
            List<INavigable> navsInRange = new();
            Dictionary<INavigable, Stack<int>> validNavPaths = new();
            switch (warriorWeaponSo)
            {
                case WarriorRangedWeapon rangedWeapon:
                    navsInRange.AddRange(actionPlan.worldState.movableNavs
                        .Where(nav =>
                            Vector3.Distance(playerUnit.unit.GetPosition(), nav.GetPosition()) < rangedWeapon.range));
                    if (navsInRange.Count == 0)
                    {
                        attackAction = null;
                        return false;
                    }

                    break;
                case WarriorMeleeWeapon meleeWeapon:
                    navsInRange.AddRange(playerUnit.unit.GetCurrentNavigable().GetLinkedNavigables()
                        .Where(nav => actionPlan.worldState.movableNavs.Contains(nav)));
                    if (navsInRange.Count == 0)
                    {
                        attackAction = null;
                        return false;
                    }

                    var valid = false;
                    var removedNavs = new List<INavigable>();
                    foreach (var nav in navsInRange)
                    {
                        var navMoveCost =
                            NavPathJobs.instance.FindPath(actionPlan.aiUnit.currentNavIndex, nav.GetNavIndex());
                        if (navMoveCost.Count * actionPlan.aiUnit.moveCost + meleeWeapon.actionPointCost <=
                            actionPlan.aiUnit.currentActionPoints)
                        {
                            validNavPaths.Add(nav, navMoveCost);
                            valid = true;
                        }
                        else
                        {
                            removedNavs.Add(nav);
                        }
                    }

                    if (!valid)
                    {
                        attackAction = null;
                        return false;
                    }

                    foreach (var nav in removedNavs)
                    {
                        navsInRange.Remove(nav);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var lowestCostPath = GoalPlanner.BestPath(validNavPaths);
            var moveAction = new MoveGoalAction(actionPlan, OrderTypes.BeforeAction, actionPlan.aiUnit,
                lowestCostPath.Value);
            var lowestNavCost = lowestCostPath.Value.Count;
            attackAction = new(actionPlan, moveAction, OrderTypes.PrimaryAction, actionPlan.aiUnit, playerUnit.unit,
                warriorWeaponSo, null)
            {
                minActionCost = warriorWeaponSo.actionPointCost + lowestNavCost,
                validNavPaths = validNavPaths,
            };
            moveAction.subActionParent = attackAction;
            attackAction.executionRound = 1;
            moveAction.executionRound = 1;
            var remainingPoints = actionPlan.aiUnit.currentActionPoints - attackAction.totalActionCost;
            CalculateAdditionalAttacks(actionPlan, attackAction, warriorWeaponSo, remainingPoints);
            return true;
        }

        private static void CalculateAdditionalAttacks(AiActionPlan actionPlan, AttackGoalAction attackAction,
            WarriorWeaponSO warriorWeaponSo, int remainingPoints)
        {
            while (true)
            {
                if (remainingPoints >= warriorWeaponSo.actionPointCost)
                {
                    if (attackAction.attackAction.damageable.currentHealth > attackAction.totalDamage)
                    {
                        var newAttackAction = new AttackGoalAction(actionPlan, OrderTypes.AfterAction,
                            attackAction.unit,
                            attackAction.attackAction.damageable, warriorWeaponSo, null);
                        newAttackAction.subActionParent = attackAction;
                        newAttackAction.executionRound = attackAction.primaryActionRounds + 1;
                        attackAction.primaryActionRounds++; 
                        attackAction.subActions.Add(newAttackAction);
                        attackAction.totalDamage += warriorWeaponSo.damage;
                        remainingPoints -= warriorWeaponSo.actionPointCost;
                        continue;
                    }

                    var lastPos = actionPlan.aiUnit.GetCurrentNavigable();
                   
                    if (attackAction.subActions.Count > 0)  // if the unit moved before attacking
                    {
                        var moveActions = attackAction.subActions.Where(x => x.actionType == ActionTypes.Move);
                        var lastMove = moveActions.Last();
                        if (lastMove is MoveGoalAction moveAction)
                        {
                            lastPos = moveAction.endNav;
                        }
                    }

                    var possibleMoves =
                        MoveActionBuilder.CalculateMoveRange(actionPlan.aiUnit, lastPos, remainingPoints);
                    var unitsInRange = (from nav in possibleMoves.Where(x => x.IsOccupied())
                        where nav.GetOccupiedMovable().GetUnitType() == SelectableTypes.Player
                        select nav.GetOccupiedMovable().GetUnit()).ToList();
                    var closestUnits = unitsInRange.OrderBy(x =>
                        Vector3.Distance(x.currentPos, lastPos.GetPosition())).ToList();
                    var possiblePaths = (from unit in closestUnits
                        from linkedNavigable in unit.GetCurrentNavigable().GetLinkedNavigables()
                        select MoveActionBuilder.CalculateMovePath(lastPos, linkedNavigable)).ToList();
                    foreach (var possiblePath in possiblePaths.Where(possiblePath =>
                                 possiblePath.Count * actionPlan.aiUnit.moveCost <= remainingPoints))
                    {
                        if (warriorWeaponSo.attackType == AttackType.Melee)
                        {
                            if (possiblePath.Count * actionPlan.aiUnit.moveCost +
                                warriorWeaponSo.actionPointCost <=
                                remainingPoints)
                            {
                                var nav = NavigableController.instance.GetNavigable(possiblePath.Last());
                                attackAction.validAdditionalPaths.Add(nav, possiblePath);
                            }
                            //TODO calculate next best action based on available remaining sub actions;
                        }
                    }

                    if (remainingPoints >= attackAction.unit.moveCost)
                    {
                        attackAction.isStillMovable = true; 
                    }

                    break;
                }
            }
        }

        public static float CalculationActionWeight(AiGoalContainer goal, GoalActionCommon action)
        {
            switch (action.actionType)
            {
                case ActionTypes.Move:
                    break;
                case ActionTypes.Attack:
                    if (action is AttackGoalAction attackGoalAction)
                        if (goal.goalType == AiGoalTypes.DamagePlayer)
                        {
                            return (goal.goalWeight * 3 * attackGoalAction.attackAction.damage) -
                                   (attackGoalAction.unit.currentActionPoints - attackGoalAction.totalActionCost);
                        }
                        else
                        {
                            return (goal.goalWeight * attackGoalAction.attackAction.damage) -
                                   (attackGoalAction.unit.currentActionPoints - attackGoalAction.totalActionCost);
                        }

                    break;
                case ActionTypes.Reload:
                    break;
                case ActionTypes.Guard:
                    break;
                case ActionTypes.Special:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return 0;
        }
    }
}