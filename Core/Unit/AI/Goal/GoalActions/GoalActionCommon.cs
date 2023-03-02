using System.Collections.Generic;
using Core.AI;
using Core.GameManagement.Interfaces;
using Core.Unit.Interfaces;
using Core.Unit.StateMachine.enums;
using UnityEngine;

namespace Core.Unit.AI.Goal
{
    public abstract class GoalActionCommon : IAiAction
    {
        public UnitCommon unit;
        private AiActionPlan actionPlan;
        public Dictionary<GoalActionCommon, int> allActionWeights = new();
        public List<GoalActionCommon> subActions = new();
        public IAction subActionParent;
        public int minActionCost;
        public ActionPlanEstimation estimate;
        public OrderTypes orderType;
        public bool isStillMovable = false;
        public int executionRound;
        public int totalGoalPriority;
        public int primaryActionRounds;
        public int actionPointCost
        {
            get => minActionCost;
        }
        public ActionTypes actionType { get; set; }
        public bool flagForRecalculation = false;

        protected GoalActionCommon(UnitCommon _unit, OrderTypes orderType, AiActionPlan actionPlan)
        {
            unit = _unit;
            this.orderType = orderType;
            this.actionPlan = actionPlan;
            if (orderType == OrderTypes.PrimaryAction)
            {
                primaryActionRounds = 1;
            }
        }
        
        protected GoalActionCommon(IWarrior _unit, OrderTypes orderType, AiActionPlan actionPlan)
        {
            unit = _unit as UnitCommon;
            this.orderType = orderType;
            this.actionPlan = actionPlan;
        }
        

        public abstract void OnExecute();
    }
}