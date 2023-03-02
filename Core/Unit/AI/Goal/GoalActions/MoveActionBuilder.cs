using System.Collections.Generic;
using Core.AI;

namespace Core.Unit.AI.Goal.GoalActions
{
    public static class MoveActionBuilder
    {
        public static List<INavigable> CalculateMoveRange(AIUnit actionPlanAIUnit, INavigable lastPos, int remainingPoints)
        {
            var navs = new List<INavigable>();
            var navsInRange =
                NavPathJobs.instance.NavsInRange(lastPos.GetNavIndex(), remainingPoints / actionPlanAIUnit.moveCost);
            foreach (var i in navsInRange)
            {
                navs.Add(NavigableController.instance.GetNavigable(i));   
            }

            navsInRange.Dispose();
            return navs;
        }

        public static Stack<int> CalculateMovePath(INavigable lastPos, INavigable endNav)
        {
            return NavPathJobs.instance.FindPath(lastPos.GetNavIndex(), endNav.GetNavIndex());
        }
    }
}