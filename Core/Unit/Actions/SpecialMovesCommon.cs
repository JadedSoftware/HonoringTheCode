using Core.GameManagement.Interfaces;
using Core.Unit.Specials;
using Core.Unit.Specials.Interfaces;
using UnityEngine;

namespace Core.Unit.Actions
{
    public abstract class SpecialMovesCommon : ScriptableObject, IChainable, IAction
    {
        public SpecialTypes specialType;
        [SerializeField] private int requiredActionPoints;
        public float range;
        public float damage;
        public int coolDown;
        [SerializeField] private int chainableCount;
        public int chainableTimes => chainableCount;
        public int actionPointCost => requiredActionPoints;
        public ActionTypes actionType => ActionTypes.Special;
        public void OnExecute()
        {
            
        }
    }
}