using Core.Unit.Actions;
using UnityEngine;

namespace Core.Unit.Specials
{
    [CreateAssetMenu(fileName = "SpecialMoves", menuName = "SpecialMovesScriptableObjects/Hookshot", order = 1)]
    public class SpecialHookshot : SpecialMovesCommon
    {
        public bool canAttackEnemies;
    }
}