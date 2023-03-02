using System.Collections.Generic;
using System.Linq;
using Core.AI;
using Core.Unit.StateMachine.enums;
using Core.Unit.Warrior;

namespace Core.Unit.AI.Goal.GoalActions
{
    public class MoveGoalAction : GoalActionCommon
    {
        public MoveAction moveAction;
        public int endNavIndex => moveAction.endNavIndex;
        public INavigable endNav => moveAction.endNav;
        public MoveGoalAction(AiActionPlan actionPlan, OrderTypes orderType, AIUnit aiUnit, Stack<int> navPath) : base(aiUnit, orderType, actionPlan)
        {
            moveAction = new()
            {
                navPath = navPath,
                moveCost = aiUnit.moveCost,
            };
            GoalPlanner.navStops.Add(moveAction.endNavIndex);
        }

        public override void OnExecute()
        {
            GoalPlanner.navStops.Remove(moveAction.endNavIndex);
        }
    }
}