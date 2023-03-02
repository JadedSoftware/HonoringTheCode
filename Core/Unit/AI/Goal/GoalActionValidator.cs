using System;
using System.Collections.Generic;
using System.Linq;
using Core.AI;
using Core.GameManagement.Interfaces;
using Core.Unit.AI.Goal.GoalActions;

namespace Core.Unit.AI.Goal
{
    public static partial class GoalValidator
    {
        private static Dictionary<IAction, AiActionPlan> invalidActions = new();

        public static void ValidateActions(List<AiActionPlan> actionPlans)
        {
            foreach (var actionPlan in actionPlans)
            {
                foreach (var action in actionPlan.selectedActions)
                {
                    ValidateAction(action.action, actionPlan);
                }
                foreach (var action in invalidActions)
                {
                    switch (action.Key)
                    {
                        case MoveGoalAction moveGoal:
                            GoalPlanner.RecalculateMove(moveGoal, actionPlan);
                            break;
                    }
                }
            }
        }

        private static void ValidateAction(IAction action, AiActionPlan actionPlan)
        {
            switch (action)
            {
                case AttackGoalAction attackAction:
                    ValidateAttackAction(attackAction, actionPlan);
                    break;
                case MoveGoalAction moveAction:
                    ValidateMoveAction(moveAction, actionPlan);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void ValidateMoveAction(MoveGoalAction moveAction, AiActionPlan actionPlan)
        {
            var endNavDuplicates = ActionEstimation.endNavs.GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(x => x.Key);
            if (endNavDuplicates.Contains(moveAction.endNavIndex))
            {
                moveAction.flagForRecalculation = true;
                invalidActions.Add(moveAction, actionPlan);
            }
        }

        private static void ValidateAttackAction(AttackGoalAction attackAction, AiActionPlan actionPlan)
        {
            foreach (var subAction in attackAction.subActions)
            {
                ValidateAction(subAction, actionPlan);
            }
        }
    }
}