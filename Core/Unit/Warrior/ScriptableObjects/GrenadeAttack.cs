using Core.Unit.Targeting;
using Core.Unit.Warrior;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeAttack : AttackAction
{
    
    public GrenadeAttack(IWarrior warrior, IDamageable targetedDamagebale, [CanBeNull] TargetingObject targetingObject) : base(warrior, targetedDamagebale, targetingObject)
    {
    }
}
