using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Data;
using Core.Effects;
using Core.GameManagement;
using Core.GameManagement.Interfaces;
using Core.Unit;
using Core.Unit.Interfaces;
using Core.Unit.Targeting;
using Core.Unit.Warrior;
using UnityEngine;
using EventSenderController = Core.GameManagement.EventSenders.EventSenderController;

public abstract partial class UnitCommon : IDamageable, IShields
{
    public float healthMax = 10;
    public TargetingHealthUI healthUI;
    public TargetingType targetingType;
    private TargetingObject currentTargetedObject;
    protected IDamageable damageable;
    private List<TargetingObject> damageableTargets = new();
    public HookshotEffect hookshotEffect { get; set; }
    public Vector3 GetHookshotPosition() => GetPosition();

    [SerializeField] private float maxShieldAmount;
    public float maxShields
    {
        get => maxShieldAmount;
        set { }
    }
    public float currentShields { get; set; }
    
    [Header("Damageable")] [SerializeField]
    protected float health;
    public float currentHealth
    {
        get => health;
        set => health = value;
    }
    public TargetingType TargetingType => targetingType;
    public bool isAlive;
    private IShields shieldsImplementation;

    private void ConfigureDamageable()
    {
        if (DataPersistenceController.instance.isNewGame)
        {
            isAlive = true;
            health = healthMax;
        }
        damageable = GetComponent<IDamageable>();
        damageableTargets = GetComponentsInChildren<TargetingObject>().ToList();
        currentShields = maxShields;
        RegisterDamageable(damageable, true);
    }

    public void RegisterDamageable(IDamageable damageable, bool isActive)
    {
        //todo
    }
    
    public void RegisterDamageableTarget(TargetingObject targetingObject)
    {
        damageableTargets.Add(targetingObject);
    }
    
    private void LinkDamageableTarget(TargetingHealthBar healthBar)
    {
        var targetable = damageableTargets.Find(x => x.targetObjectType == healthBar.targetingObjectType);
        healthBar.targetingObject = targetable;
        targetable.healthBar = healthBar;
    }
    
    public void ConfigureHealthUI(TargetingHealthUI newHealthUi)
    {
        newHealthUi.damageable = this;
        healthUI = newHealthUi;
        healthUI.transform.SetParent(transform);
        foreach (var healthBar in healthUI.healthBars) LinkDamageableTarget(healthBar);
    } 

    public float GetHealthPercent()
    {
        return health / healthMax;
    }
    public void DamageableDeathInitiated(AttackAction attackAction)
    {
        if (ReferenceEquals(attackAction.warrior, this))
        {
            isTarget = false;
        }
        if (ReferenceEquals(attackAction.damageable, this))
        {
            isAlive = false;
            isTarget = false;
            RegisterDamageable(this, false);
            StopAllCoroutines();
            StartCoroutine(DestroyDamageable(attackAction));
        }
    }

    private IEnumerator DestroyDamageable(AttackAction attackAction)
    {
        bool isEndFrame = true;
        while (isEndFrame)
        {
            isEndFrame = false;
            yield return new WaitForEndOfFrame();
        }
        EventSenderController.PerformDamageableDeath(attackAction);
        gameObject.SetActive(false);
    }

    public virtual void TakeDamage(AttackAction attackAction)
    {
        health -= attackAction.damage;
        if (health <= 0)
            EventSenderController.InitiateDamagableDeath(attackAction);
    }

    public Vector3 DamageTextPosition()
    {
        return motor.transform.position + motor.Capsule.height * Vector3.up * 1.5f;
    }
    
    private void CalculateDamage(AttackAction attackAction)
    {
        // todo critical damage
        attackAction.damage = attackAction.CriticalAttack();
        TakeDamage(attackAction);
    }


    private void EngageTargetingObject(TargetingObject target)
    {
        //throw new NotImplementedException();
    }

    private void ChangeTargetingObject(TargetingObject target)
    {
        //throw new NotImplementedException();
    }

    private void DisengageTargetingObject()
    {
        //throw new NotImplementedException();
    }

    private void DamageableDeathPerformed(AttackAction attackAction)
    {
        //throw new System.NotImplementedException();
    }

    public Collider GetCollider()
    {
        return motor.Capsule;
    }

    public LayersEnum GetLayer()
    {
        return (LayersEnum)gameObject.layer;
    }
}
