using Core.GameManagement.Interfaces;
using Core.Unit.Targeting;
using JetBrains.Annotations;

namespace Core.Unit.Warrior
{
    /// <summary>
    /// Data container for attack actions, is passed by event to all objects
    /// </summary>
    public class AttackAction : IAction
    {
        public float damage;
        public IDamageable damageable;
        public TargetingObject targetObject;
        public bool isCritical;
        public IWarrior warrior;
        public WarriorWeaponSO weapon;
        public int actionCost;
        public AttackType attackType;
        public int actionPointCost { get; }
        public ActionTypes actionType
        {
            get => ActionTypes.Attack;
        }
        
        public AttackAction(IWarrior warrior, IDamageable targetedDamagebale, [CanBeNull] TargetingObject targetingObject)
        {
            this.warrior = warrior;
            damageable = targetedDamagebale;
            targetObject = targetingObject;
            weapon = warrior.GetCurrentWeapon();
            damage = weapon.damage;
        }

        public float CriticalAttack()
        {
            if (isCritical) return damage *= 2;
            return damage;
        }
        
        public void OnExecute()
        {
            
        }

    }
}