using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.Common;
using System.Linq;
using Core.GameManagement.Interfaces;
using Core.Unit.AI.Goal;
using Core.Unit.AI.Goal.GoalActions;

namespace Core.AI
{
    public static class ActionEstimation
    {
        public static List<int> endNavs = new();
        public static Dictionary<GoalActionCommon, ActionPlanEstimation> goalBuildingEstimations = new();
        public static List<ActionPlanEstimation> commitedActionEstimates = new();
        public static List<ActionPlanEstimation> EstimateActionResults(List<AiActionPlan> allActionPlans)
        {
            foreach (var actionPlan in allActionPlans)
            {
                foreach (var action in actionPlan.selectedActions)
                {
                    var aiEstimate = commitedActionEstimates.First(x => x.unitCommon == actionPlan.aiUnit);
                    ActionEstimateCalculation(action.action, aiEstimate);
                }   
            }
            return commitedActionEstimates;
        }

        public static void ActionEstimateCalculation(IAction action, ActionPlanEstimation aiEstimate)
        {
            switch (action)
            {
                case AttackGoalAction attackGoalAction:
                    var targetEstimate = commitedActionEstimates.First(x => ReferenceEquals(x.unitCommon, attackGoalAction.attackAction.damageable));
                    aiEstimate.actionPoints -= action.actionPointCost;
                    var damage = attackGoalAction.attackAction.weapon.damage; 
                    aiEstimate.totalDamage += damage;
                    targetEstimate.sheilds -= damage;
                    if (targetEstimate.sheilds < 0)
                    {
                        targetEstimate.health += targetEstimate.sheilds;
                        targetEstimate.sheilds = 0;
                    }

                    foreach (var subAction in attackGoalAction.subActions)
                    {
                        ActionEstimateCalculation(subAction, aiEstimate);
                    }
                    break;
                case MoveGoalAction moveGoalAction:
                    aiEstimate.actionPoints -= moveGoalAction.minActionCost;
                    endNavs.Add(moveGoalAction.endNavIndex);
                    foreach (var subAction in moveGoalAction.subActions)
                    {
                        ActionEstimateCalculation(subAction, aiEstimate);
                    }
                    break;
            }
        }

        public static void CreateEstimate(UnitCommon unit)
        {
            commitedActionEstimates.Add(new(unit, unit.maxActionPoints, unit.currentHealth, unit.currentShields));
        }

        public static ActionPlanEstimation GetEstimate(UnitCommon unit)
        {
            return commitedActionEstimates.First(x => x.unitCommon == unit);
        }

        public static ActionPlanEstimation GetEstimate(IDamageable attackActionDamageable)
        {
            if (attackActionDamageable is UnitCommon unit)
            {
                return commitedActionEstimates.First(x => x.unitCommon == unit);
            }
            return null;
        }
    }
}