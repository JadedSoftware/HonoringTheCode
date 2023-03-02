using System.Collections.Generic;
using System.Linq;
using Core.AI;
using UnityEngine;


namespace Core.Unit.AI.Goal
{
    public static partial class GoalPlanner
    {
        public static void EstablishAction(AiActionPlan actionPlan)
        {
            var highestGoalAction = actionPlan.actionsWeighted.OrderByDescending(x => x.weight).FirstOrDefault();
            actionPlan.selectedActions.Add(highestGoalAction);
        }

        private static void RandomizedActionSelection(AiActionPlan actionPlan)
        {
            int a = 0;
            var randomValue = Random.Range(.01f, 1.01f);
            foreach (var weightedAction in actionPlan.actionsWeighted.Where(x => x.goal == actionPlan.selectedGoal))
            {
                if (randomValue <= weightedAction.weight)
                {
                    actionPlan.selectedActions.Add(weightedAction);
                    return;
                }
                else
                {
                    a++;
                    randomValue -= weightedAction.weight;
                }
            }

            if (actionPlan.selectedActions.Count == 0)
            {
                actionPlan.selectedActions.Add(actionPlan.actionsWeighted.OrderByDescending(x => x.weight)
                    .FirstOrDefault());
            }
        }

        public static AiGoalContainer EstablishGoal(List<AiGoalContainer> actionPlanGoals)
        {
            return RandomizedGoalSelection(actionPlanGoals);
        }

        private static AiGoalContainer RandomizedGoalSelection(List<AiGoalContainer> actionPlanGoals)
        {
            int a = 0;
            var orderedGoals = actionPlanGoals.OrderByDescending(x => x.goalWeight).ToList();
            var randomValue = Random.Range(.01f, 1.01f);
            foreach (var goal in actionPlanGoals)
            {
                if (randomValue <= goal.goalWeight)
                {
                    return goal;
                }
                else
                {
                    a++;
                    randomValue -= goal.goalWeight;
                }
            }

            return orderedGoals.FirstOrDefault();
        }
    }
}