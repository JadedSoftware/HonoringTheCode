using System;
using System.Linq;
using Core.Data;
using Core.GameManagement.EventSenders;
using Core.Interfaces;
using Core.Unit.Model;
using UnityEngine;

public enum SelectableTypes
{
    Player,
    AI
}
/// <summary>
/// Main container for the Unit
/// Handles registering for events
/// Saving and Loading calls
/// Configuring State machine
/// </summary>
[Serializable, RequireComponent(typeof(UnitBehaviourCommon))]
public abstract partial class UnitCommon : MonoBehaviour, IEvents, IDataPersistable
{
    private string guid;
    [Header("Unit Config")] public string unitName;
    
    public GameObject modelPrefab;
    [SerializeField] protected SelectableTypes selectableType;

    public GameObject placeholder;
    private GameObject currentModel;
    private ModelContainer modelContainer;

    private bool isGrounded;
    private SensorContainer sensorContainer;

    [HideInInspector] public UnitBehaviourCommon unitBehaviour;
    [HideInInspector] public int unitIndex { get; private set; }

    public Rigidbody rigidBody => GetComponent<Rigidbody>();
    protected GameManagementController gameManagementContoller => GameManagementController.instance;
    private NavigableController navController => NavigableController.instance;
    private UnitCommonController unitControl => UnitCommonController.instance;

    protected virtual void Awake()
    {        
        for (var i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.TryGetComponent(out ModelContainer model))
            {
                currentModel = model.gameObject;
                break;
            }

            Destroy(placeholder);
        }

        if (currentModel == null) currentModel = Instantiate(modelPrefab, transform);
        modelContainer = currentModel.GetComponent<ModelContainer>();
        ConfigSensorController();
    }

    public virtual void OnEnable()
    {
        ConfigureAnimations();
        ConfigureStateMachine();
        ConfigureMovable();
        ConfigureWarrior();
        ConfigureActionable();
        ConfigureDamageable();
        SetHighlightEffect();
        RegisterUnit();
        if(DataPersistenceController.instance.isNewGame)
            RefreshActionPoints();
        RegisterEvents();
    }

    protected void OnDisable()
    {
        UnRegisterEvents();
    }

    public SelectableTypes GetUnitType()
    {
        return selectableType;
    }

    public void RegisterEvents()
    {
        EventSenderController.onNavReady += OnNavReady;
        EventSenderController.unitSelected += OnUnitSelected;
        EventSenderController.unitDeselected += OnUnitDeselect;
        EventSenderController.onNavReady += OnNavReady;
        EventSenderController.unitMovementRequested += UnitMovementRequested;
        EventSenderController.unitMovementInitiated += UnitMovementInitiated;
        EventSenderController.unitMovementComplete += UnitMovementComplete;
        EventSenderController.onAttackIntiated += OnAttackInitiated;
        EventSenderController.onAttackPerformed += OnAttackPerformed;
        EventSenderController.onAttackCompleted += OnAttackCompleted;
        EventSenderController.onHookshotPerformed += HookshotPerformed;
        EventSenderController.gridConfigured += OnGridConfigured;
        EventSenderController.initiateDamagableDeath += DamageableDeathInitiated;
        EventSenderController.performDamageableDeath += DamageableDeathPerformed;
        EventSenderController.onBeginTurn += OnBeginTurn;
        EventSenderController.onEndTurn += OnEndTurn;
        EventSenderController.enterAttackView += EnterAttackView;
        EventSenderController.exitAttackView += ExitAttackView;
        EventSenderController.enterTopdownView += EnterTopdownView;
        EventSenderController.onUnitTargeted += OnUnitTargeted;
        EventSenderController.engageTargetingObject += EngageTargetingObject;
        EventSenderController.changeTargetingObject += ChangeTargetingObject;
        EventSenderController.disengageTargetingObject += DisengageTargetingObject;
        EventSenderController.onSave += OnSave;
        EventSenderController.onLoad += OnLoad;
        EventSenderController.onWeaponSwap += OnWeaponSwap;
    }

    public void UnRegisterEvents()
    {
        EventSenderController.onNavReady -= OnNavReady;
        EventSenderController.unitSelected -= OnUnitSelected;
        EventSenderController.unitDeselected -= OnUnitDeselect;
        EventSenderController.onNavReady -= OnNavReady;
        EventSenderController.unitMovementRequested += UnitMovementRequested;
        EventSenderController.unitMovementInitiated -= UnitMovementInitiated;
        EventSenderController.unitMovementComplete -= UnitMovementComplete;
        EventSenderController.onAttackIntiated -= OnAttackInitiated;
        EventSenderController.onAttackPerformed -= OnAttackPerformed;
        EventSenderController.onAttackCompleted -= OnAttackCompleted;
        EventSenderController.gridConfigured -= OnGridConfigured;
        EventSenderController.initiateDamagableDeath -= DamageableDeathInitiated;
        EventSenderController.performDamageableDeath -= DamageableDeathPerformed;
        EventSenderController.onBeginTurn -= OnBeginTurn;
        EventSenderController.onEndTurn -= OnEndTurn;
        EventSenderController.enterAttackView -= EnterAttackView;
        EventSenderController.exitAttackView -= ExitAttackView;
        EventSenderController.enterTopdownView -= EnterTopdownView;
        EventSenderController.onUnitTargeted -= OnUnitTargeted;
        EventSenderController.engageTargetingObject -= EngageTargetingObject;
        EventSenderController.changeTargetingObject -= ChangeTargetingObject;
        EventSenderController.disengageTargetingObject -= DisengageTargetingObject;
        EventSenderController.onSave -= OnSave;
        EventSenderController.onLoad -= OnLoad;
        EventSenderController.onWeaponSwap -= OnWeaponSwap;
    }

    void OnNavReady()
    {
        MoveableStartPosition();
    }

    public void UpdateNavigable(INavigable navPoint)
    {
        currentNavPoint.SetOccupiedMovable(null);
        currentNavPoint = navPoint;
        currentNavIndex = currentNavPoint.GetNavIndex();
        currentNavPoint.SetOccupiedMovable(this);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public UnitCommon GetUnit()
    {
        return this;
    }

    private void ConfigSensorController()
    {
        sensorContainer = GetComponentInChildren<SensorContainer>();
        sensorContainer.unit = this;
    }


    private void ConfigureStateMachine()
    {
        unitBehaviour = GetComponent<UnitBehaviourCommon>();
    }

    private void OnGridConfigured()
    {
    }

    private void RegisterUnit()
    {
        unitIndex = UnitCommonController.instance.RegisterUnit(this);
    }

    public virtual void OnSave(GameData gameData)
    {
        gameData.persistedUnits.Add(this);
        var unitData = new UnitData(guid, unitName, health, currentNavIndex, currentPos, weaponObjects, GetCurrentWeapon(), currentAmmo);
        gameData.unitDataList.Add(unitData);
    }

    public virtual void OnLoad(GameData gameData)
    {
        startIndex = -1;
        var unitData = gameData.unitDataList.FirstOrDefault(x => x.unitName == unitName);
        if (unitData != null)
        {
            health = unitData.health;
            startIndex = unitData.navIndex;
            weaponObjects = unitData.weapons;
            currentWeaponObject = unitData.currentWeapon;
            currentAmmo = unitData.currentAmmo;
        }
    }

    public void OnWeaponSwap(UnitCommon unit, WarriorWeaponSO newWeapon)
    {
        if (transform.root.gameObject.name.Equals("Players"))
        {
            print(newWeapon);
            unit.DeactivateOldWeapon();
            unit.ConsumeActionPoints(newWeapon.SwapCost);
            unit.ConfigureWeapon(newWeapon);
        }
    }
}