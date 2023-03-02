using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.GameManagement
{
    /// <summary>
    /// Contains a list of all warriors in a scene
    /// </summary>
    public class BattleController : MonoBehaviour
    {
        [HideInInspector]
        public Dictionary<IDamageable, bool> allDamageables;
        [HideInInspector]
        public Dictionary<IWarrior, bool> allWarriors;
        
        private static BattleController _instance;
        public static BattleController instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(BattleController)) as BattleController;

                return _instance;
            }
            set => _instance = value;
        }

        void RegisterDamageable(IDamageable damageable, bool isActive)
        {
            if(!allDamageables.ContainsKey(damageable))
            {
                allDamageables.Add(damageable, isActive);
            }
        }
        
        void UnRegisterDamageable(IDamageable damageable, bool isActive)
        {
            if(allDamageables.ContainsKey(damageable))
            {
                allDamageables[damageable] = isActive;
            }
        }
        void RegisterWarrior(IWarrior warrior, bool isActive)
        {
            if(!allWarriors.ContainsKey(warrior))
            {
                allWarriors.Add(warrior, isActive);
            }
        }
        
        void UnRRegisterWarrior(IWarrior warrior, bool isActive)
        {
            if(allWarriors.ContainsKey(warrior))
            {
                allWarriors[warrior] = isActive;
            }
        }
    }
}