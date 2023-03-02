using System.Collections.Generic;
using Core.GameManagement.Interfaces;
using Core.Unit.Specials;
using Core.Unit.Warrior;

namespace Core.GameManagement.EventSenders
{
    public static partial class EventSenderController
    {
        
        //------------ Actions ---------------//
        
        public delegate void OnMovementRequested(IMovable unit, INavigable endNavigable);

        public static event OnMovementRequested unitMovementRequested;


        public delegate void OnMovementInitiated(IMovable unit, Stack<int> navPath, INavigable endNavigable);

        public static event OnMovementInitiated unitMovementInitiated;


        public delegate void OnMovementComplete(IMovable unit);

        public static event OnMovementComplete unitMovementComplete;


        public delegate void OnInitiateAttack(AttackAction attackAction);

        public static event OnInitiateAttack onAttackIntiated;


        public delegate void OnPerformAttack(AttackAction attackAction);

        public static event OnPerformAttack onAttackPerformed;


        public delegate void OnCompletedAttack(AttackAction attackAction);

        public static event OnCompletedAttack onAttackCompleted;


        public delegate void OnPerformDeath(AttackAction attackAction);

        public static event OnPerformDeath performDamageableDeath;


        public delegate void InitiateDamageableDeath(AttackAction attackAction);

        public static event InitiateDamageableDeath initiateDamagableDeath;
    

        public delegate void OnPulseSensors();

        public static event OnPulseSensors pulseAllSensors;

        public delegate void OnHookshotRequested();

        public static event OnHookshotRequested onHookshotRequested;

        public delegate void OnHookshotPerformed(HookshotAction hookshotAction);

        public static event OnHookshotPerformed onHookshotPerformed;
        
        public delegate void OnHookshotCompleted(HookshotAction hookshotAction);

        public static event OnHookshotCompleted onHookshotCompleted;

        public delegate void OnWeaponSwap(UnitCommon unit, WarriorWeaponSO newWeapon);

        public static event OnWeaponSwap onWeaponSwap;


        
        //-------------- Static Methods ------------//

        public static void UnitMovementRequested(IMovable unit, INavigable endNavPoint)
        {
            ScheduleEvent(() => unitMovementRequested?.Invoke(unit, endNavPoint));
        }

        public static void UnitMovementPerformed(IMovable unit, Stack<int> navPath, INavigable endNavPoint)
        {
            ScheduleEvent(() => unitMovementInitiated?.Invoke(unit, navPath, endNavPoint));
        }

        public static void UnitMovementComplete(IMovable unit)
        {
            ScheduleEvent(() => unitMovementComplete?.Invoke(unit));
        }
        
        public static void PerformDamageableDeath(AttackAction attackAction)
        {
            ScheduleEvent(() => performDamageableDeath?.Invoke(attackAction));
        }

        public static void InitiateDamagableDeath(AttackAction attackAction)
        {
            ScheduleEvent(() => initiateDamagableDeath?.Invoke(attackAction));
        }

        public static void AttackInitiated(AttackAction attackAction)
        {
            ScheduleEvent(() => onAttackIntiated?.Invoke(attackAction));
        }

        public static void AttackPerformed(AttackAction attackAction)
        {
            ScheduleEvent(() => onAttackPerformed?.Invoke(attackAction));
        }

        public static void AttackCompleted(AttackAction attackAction)
        {
            ScheduleEvent(() => onAttackCompleted?.Invoke(attackAction));
        }
    
        public static void PulseAllSensors()
        {
            ScheduleEvent(() => pulseAllSensors?.Invoke());
        }

        public static void HookshotAttackRequested()
        {
            ScheduleEvent(() => onHookshotRequested?.Invoke());
        }

        public static void HookshotPerformed(HookshotAction hookshotAction)
        {
            ScheduleEvent(() => onHookshotPerformed?.Invoke(hookshotAction));
        }

        public static void HookshotCompleted(HookshotAction hookshotAction)
        {
            ScheduleEvent(() => onHookshotCompleted?.Invoke(hookshotAction));
        }

        public static void WeaponSwap(UnitCommon unit, WarriorWeaponSO newWeapon)
        {
            ScheduleEvent(() => {
                DebugController.print(newWeapon);
                onWeaponSwap?.Invoke(unit, newWeapon);
            });
        }
    }
}