using Core.GameManagement.Interfaces;
using Core.Interfaces;
using Core.Unit;
using Core.Unit.Targeting;
using Core.Unit.Warrior;
using UnityEngine;

/// <summary>
/// When an object can take damage
/// </summary>
public interface IDamageable : IHookshotable, IRaycastable
{
    void RegisterDamageable(IDamageable damageable, bool isActive);
    TargetingType TargetingType { get; }
    float GetHealthPercent();
    void TakeDamage(AttackAction attackAction);
    void DamageableDeathInitiated(AttackAction attackAction);
    void RegisterDamageableTarget(TargetingObject targetingObject);
    Vector3 DamageTextPosition();
    void ConfigureHealthUI(TargetingHealthUI newHealthUi);
    Vector3 GetPosition();
    UnitCommon GetUnit();
    public float currentHealth { get; set; }
}

public enum DamageType
{
    Melee,
    Ranged,
    Explosive
}