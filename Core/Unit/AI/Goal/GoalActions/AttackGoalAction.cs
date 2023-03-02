using System.Collections.Generic;
using System.Linq;
using Core.AI;
using Core.GameManagement.Interfaces;
using Core.Unit.StateMachine.enums;
using Core.Unit.Targeting;
using Core.Unit.Warrior;
using JetBrains.Annotations;

namespace Core.Unit.AI.Goal
{
    public class AttackGoalAction : GoalActionCommon
    {
        public AttackAction attackAction;
        public Dictionary<INavigable, Stack<int>> validNavPaths;
        public Dictionary<INavigable, Stack<int>> validAdditionalPaths;
        public int totalActionCost => TotalActionCost();
        public float totalDamage;


        public AttackGoalAction(AiActionPlan actionPlan, OrderTypes orderType, IWarrior aiUnit, IDamageable target, WarriorWeaponSO warriorWeaponSo, [CanBeNull] TargetingObject targetingObject) : base(aiUnit, orderType, actionPlan)
        {
            attackAction = new(aiUnit, target, targetingObject);
        }

        public AttackGoalAction(AiActionPlan actionPlan, GoalActionCommon subAction, OrderTypes orderType, IWarrior aiUnit, IDamageable target,
            WarriorWeaponSO warriorWeaponSo, [CanBeNull] TargetingObject targetingObject) : base(aiUnit, orderType, actionPlan)
        {
            actionType = ActionTypes.Attack;
            subActions.Add(subAction);
            attackAction = new(aiUnit, target, targetingObject);
            totalDamage = warriorWeaponSo.damage;
        }
        

        public override void OnExecute()
        {
            
        }
        
        private int TotalActionCost()
        {
            int totalCost = 0;
            totalCost += actionPointCost;
            totalCost += subActions.Sum(subAction => subAction.actionPointCost);
            return totalCost;
        }

    }
}