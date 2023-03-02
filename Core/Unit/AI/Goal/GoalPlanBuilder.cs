using System;
using System.Collections.Generic;
using System.Linq;
using Core.AI;
using Core.GameManagement.Interfaces;
using UnityEditor;

namespace Core.Unit.AI.Goal
{
    public static partial class GoalPlanner
    {
        public static void BuildPlan(AiActionPlan actionPlan, AiGoalContainer goal)
        {
            switch (goal.goalType)
            {
                case AiGoalTypes.DamagePlayer:
                    CreateDamagePlan(actionPlan, goal);
                    break;
                case AiGoalTypes.KillPlayer:
                    break;
                case AiGoalTypes.ExposePlayer:
                    break;
                case AiGoalTypes.PinPlayer:
                    break;
                case AiGoalTypes.FlankPlayer:
                    break;
                case AiGoalTypes.MoveToHighground:
                    break;
                case AiGoalTypes.DefendTeammate:
                    break;
                case AiGoalTypes.DefendSelf:
                    break;
                case AiGoalTypes.HealTeammate:
                    break;
                case AiGoalTypes.HealSelf:
                    break;
                case AiGoalTypes.BuffSelf:
                    break;
                case AiGoalTypes.BuffTeammate:
                    break;
                case AiGoalTypes.AllOutAttack:
                    break;
                case AiGoalTypes.AttackAtDistance:
                    break;
                case AiGoalTypes.AttackFromCover:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void CreateDamagePlan(AiActionPlan actionPlan, AiGoalContainer goal)
        {
            List<AttackGoalAction> potentialTargets = new();
            foreach (var playerUnit in actionPlan.worldState.playerInfo.Where(x => x.unit.GetUnitType() == SelectableTypes.Player))       
            {
                foreach (var weaponObject in actionPlan.aiUnit.weaponObjects)
                {
                    if (AttackActionBuilder.BuildAttackAction(actionPlan, weaponObject, playerUnit, out var attackActions))
                    {
                        potentialTargets.Add(attackActions);
                    }
                }
            }
            
            if (potentialTargets.Count > 0)
            {
                foreach (var action in potentialTargets.Select(target => new WeightedAction{goal = goal, action = target, weight = AttackActionBuilder.CalculationActionWeight(goal, target)}))
                {
                    actionPlan.actionsWeighted.Add(action);
                }
            }
        }

        private static void RecalcDamagePlan(AiActionPlan aiActionPlan, IDamageable invalidDamageable)
        {
            //todo
        }
    }
}