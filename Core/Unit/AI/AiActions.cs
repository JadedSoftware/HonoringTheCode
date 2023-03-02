using System.Collections.Generic;
using Core.Unit.Actions;
using Core.Unit.Specials;

namespace Core.AI
{
    public class AiActions
    {
        public ActionTypes actionType;
        public UnitCommon target;
        public INavigable navPoint;
        public Stack<int> navPath;
        public SpecialMovesCommon specialMove;
    }
}