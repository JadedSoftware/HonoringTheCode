using System;
using System.Collections.Generic;
using System.Linq;
using Core.GameManagement;
using Core.Unit.Warrior;
using UnityEngine;
using EventSenderController = Core.GameManagement.EventSenders.EventSenderController;

public enum UnitWarroirClass
{
    Soldier,
    Mage,
    Hero,
    Berserker
}

public abstract partial class UnitCommon : IWarrior
{
    [Header("Warrior Settings")]
    [SerializeField] public List<WarriorWeaponSO> weaponObjects;

    /*[HideInInspector]*/ public WarriorWeaponSO currentWeaponObject;
    [HideInInspector] public WarriorRangedWeapon currentRangedWeapon;
    [HideInInspector] public WarriorMeleeWeapon currentMeleeWeapon;
    [HideInInspector] public GameObject oldWeaponPrefab;

    [HideInInspector] public WarriorAmmoScriptable currentAmmo;

    public UnitWarroirClass unitWarroirClass;
    private WeaponModelCommon currentWeaponModel;
    private GameObject currentWeaponPrefab;

    
    [SerializeField] private AttackCameraTransform attackCameraPos;


    private IkContainer ikContainer;

    public IWarrior warrior;

    [Header("Weapon")] private ModelWeaponSlots weaponSlots;
    
    private void ConfigureWarrior()
    {
        if (currentWeaponObject == null)
        {
            currentWeaponObject = weaponObjects.FirstOrDefault();
        }
        ikContainer = GetComponentInChildren<IkContainer>();
        warrior = GetComponent<IWarrior>();
        weaponSlots = GetComponentInChildren<ModelWeaponSlots>();
        attackCameraPos = GetComponentInChildren<AttackCameraTransform>();
        ConfigureWeapon(currentWeaponObject);
    }

    public void RegisterWarrior(IWarrior warrior, bool isActive)
    {
        //todo
    }

    public void UnRegisterWarriorable(IWarrior warrior, bool isActive)
    {
        
    }
    public IWarrior GetWarrior()
    {
        return warrior;
    }

    public WarriorWeaponSO GetCurrentWeapon()
    {
        return currentWeaponObject;
    }

    public void OnAttackInitiated(AttackAction attackAction)
    {
        if (attackAction.warrior == warrior)
            EventSenderController.AttackPerformed(attackAction);
    }

    public virtual void OnAttackPerformed(AttackAction attackAction)
    {
        if (attackAction.damageable == damageable)
        { 
            CalculateDamage(attackAction);
            EventSenderController.AttackCompleted(attackAction);
        }

        if (attackAction.warrior == warrior)
        {
            ConsumeActionPoints(GetPointsRequired(ActionTypes.Attack));
            RefreshMovableNavs();
        }
    }

    public void OnAttackCompleted(AttackAction attackAction)
    {
    }
    
    public int AttackActionCost()
    {
        return GetPointsRequired(ActionTypes.Attack);
    }
    
    public void ConfigureWeapon(WarriorWeaponSO weaponSo)
    {
        switch (weaponSo.weaponType)
        {
            case WeaponType.Rifle:
                if (weaponSo is WarriorRangedWeapon warriorRangedWeapon)
                {
                    unitBehaviour.SetState(Core.Unit.StateMachine.enums.UnitStateTypes.IdleRanged);
                    ConfigureRangedWeapon(warriorRangedWeapon);
                }
                break;
            case WeaponType.Sword:
                if (weaponSo is WarriorMeleeWeapon warriorMeleeWeapon)
                {
                    unitBehaviour.SetState(Core.Unit.StateMachine.enums.UnitStateTypes.IdleMelee);
                    ConfigureSword(warriorMeleeWeapon);
                }
                break;
            case WeaponType.Claws:
                ConfigureMelee(weaponSo);
                break;
            case WeaponType.Psychic:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    private void ConfigureSword(WarriorMeleeWeapon warriorMeleeWeapon)
    {
        print(warriorMeleeWeapon);
        currentWeaponObject = warriorMeleeWeapon;

        print(currentWeaponObject);
        var refPositions = weaponSlots.weaponRefPositions.First(a => a.weaponType == currentWeaponObject.weaponType);
        var actionRefPos = refPositions.actionRefPos.FirstOrDefault();
        var swordRefPos = actionRefPos.refPos;
        currentWeaponPrefab = Instantiate(currentWeaponObject.prefab, swordRefPos, false);
        oldWeaponPrefab = currentWeaponPrefab;
        ikContainer.ConfigureWeaponIk(currentWeaponModel = currentWeaponPrefab.GetComponent<WeaponModelCommon>());
    }

    private void ConfigureMelee(WarriorWeaponSO weaponSo)
    {

    }

    private void ConfigureRangedWeapon(WarriorRangedWeapon warriorRangedWeapon)
    {
        currentWeaponObject = currentRangedWeapon;
        currentRangedWeapon = warriorRangedWeapon;
        currentAmmo = currentRangedWeapon.ammo;
        var refPositions = weaponSlots.weaponRefPositions.First(a => a.weaponType == currentRangedWeapon.weaponType);
        var actionRefPos = refPositions.actionRefPos.FirstOrDefault();
        var rifleRefPos = actionRefPos.refPos;
        currentWeaponPrefab = Instantiate(currentRangedWeapon.prefab, rifleRefPos, false);
        oldWeaponPrefab = currentWeaponPrefab;
        ikContainer.ConfigureWeaponIk(currentWeaponModel = currentWeaponPrefab.GetComponent<WeaponModelCommon>());
    }
    
    public float AttackRange()
    {
        return currentRangedWeapon.range;
    }

    public void DeactivateOldWeapon()
    {
        if(oldWeaponPrefab != null)
        {
            oldWeaponPrefab.SetActive(false);
        }
    }

    public void WeaponSwap(WarriorWeaponSO newWeapon)
    {
        //throw new NotImplementedException();
    }
}