using System;
using System.Collections.Generic;
using System.Linq;
using Core.AI;
using Core.Unit.AI.Goal.GoalActions;
using UnityEngine;

namespace Core.Unit.AI.Goal
{
    public static partial class GoalPlanner
    {
        public static List<int> navStops = new();
        public static List<int> endNavIndexes = new();
        public static AiGoalContainer CalculateGoal(AiActionPlan actionPlan)
        {
            var goal = actionPlan.aiUnit.goals.OrderByDescending(x => x.goalWeight).FirstOrDefault();
            if (goal == null)
            {
                goal =   new ()
                {
                    goalType = AiGoalTypes.DamagePlayer,
                    goalWeight = 1
                };
                actionPlan.aiUnit.goals.Add(goal);
            }

            return goal.goalType switch
            {
                AiGoalTypes.DamagePlayer => MoveTowardsPlayer(actionPlan),
                AiGoalTypes.KillPlayer => MoveTowardsPlayer(actionPlan),
                AiGoalTypes.ExposePlayer => MoveTowardsPlayer(actionPlan),
                AiGoalTypes.PinPlayer => MoveTowardsPlayer(actionPlan),
                AiGoalTypes.FlankPlayer => FindFlankPosition(actionPlan),
                AiGoalTypes.MoveToHighground => FindHighground(actionPlan),
                AiGoalTypes.DefendTeammate => MoveTowardsTeammate(actionPlan),
                AiGoalTypes.DefendSelf => MoveTowardsCover(actionPlan),
                AiGoalTypes.HealTeammate => MoveTowardsTeammate(actionPlan),
                AiGoalTypes.HealSelf => RegenerateEnergy(actionPlan),
                AiGoalTypes.BuffSelf => RegenerateEnergy(actionPlan),
                AiGoalTypes.BuffTeammate => RegenerateEnergy(actionPlan),
                AiGoalTypes.AllOutAttack => MoveTowardsPlayer(actionPlan),
                AiGoalTypes.AttackAtDistance => MoveTowardsPlayer(actionPlan),
                AiGoalTypes.AttackFromCover => MoveTowardsCover(actionPlan),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        public static void RecalculateMove(MoveGoalAction moveGoal, AiActionPlan actionPlan)
        {
            var currentInvalid = moveGoal.endNav;
            if (moveGoal.subActionParent != null)
            {
                switch (moveGoal.subActionParent)
                {
                    case AttackGoalAction parentAttackAction:
                        if (parentAttackAction.validNavPaths.Count > 1)
                        {
                            var newValidPaths = parentAttackAction.validNavPaths;
                            if (newValidPaths.ContainsKey(currentInvalid))
                            {
                                newValidPaths.Remove(currentInvalid);
                                parentAttackAction.validNavPaths = newValidPaths;
                            }
                        }
                        else
                        {
                            //flag for inability to find new location;
                        }
                        break;
                }
            }
        }

        public static KeyValuePair<INavigable, Stack<int>> BestPath(Dictionary<INavigable, Stack<int>> validNavPaths)
        {
            var lowestCostPath = validNavPaths.OrderBy(x => x.Value.Count).FirstOrDefault();
            var endIndex = lowestCostPath.Value.Last();
            if(endNavIndexes.Contains(endIndex))
            {
                if (validNavPaths.Count <= 1)
                {
                    //todo compare and recalculate goal 
                    Debug.Log("Invalid Move Path Generated");
                    return lowestCostPath;
                }
                validNavPaths.Remove(lowestCostPath.Key);
                BestPath(validNavPaths);
            }
            return lowestCostPath;
        }

        private static AiGoalContainer MoveTowardsPlayer(AiActionPlan actionPlan)
        {
            throw new NotImplementedException();
        }

        private static AiGoalContainer MoveTowardsTeammate(AiActionPlan actionPlan)
        {
            throw new NotImplementedException();
        }

        private static AiGoalContainer FindFlankPosition(AiActionPlan actionPlan)
        {
            throw new NotImplementedException();
        }

        private static AiGoalContainer FindHighground(AiActionPlan actionPlan)
        {
            throw new NotImplementedException();
        }

        private static AiGoalContainer MoveTowardsCover(AiActionPlan actionPlan)
        {
            throw new NotImplementedException();
        }

        private static AiGoalContainer RegenerateEnergy(AiActionPlan actionPlan)
        {
            throw new NotImplementedException();
        }


    }
}