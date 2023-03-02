using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.GameManagement.Interfaces;

namespace Core.Unit.Warrior
{
    public class MoveAction : IAction
    {
        public Stack<int> navPath;
        public int moveCost;
        public int endNavIndex => navPath.Last();
        public INavigable endNav => NavigableController.instance.GetNavigable(endNavIndex);
        public int actionPointCost => navPath.Count * moveCost;
        public ActionTypes actionType => ActionTypes.Move;
        public void OnExecute()
        {
            
        }
    }
}