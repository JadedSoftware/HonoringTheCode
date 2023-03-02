using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Animancer.FSM;
using Cinemachine;
using Core.Camera;
using Core.GameManagement;
using Core.Unit;
using Core.Unit.Targeting;
using Unity.VisualScripting;
using UnityEngine;
using EventSenderController = Core.GameManagement.EventSenders.EventSenderController;

/// <summary>
/// Camera state machine base
/// All camera states derive from this component
/// </summary>
public abstract class CameraViewStateCommon : StateBehaviour, IOwnedState<CameraViewStateCommon>
{
    public CinemachineVirtualCamera virtualCamera;
    protected CameraController camControl;
    protected CameraViewStates viewState;
    protected bool viewEnabled;
    protected bool isAimMoving;
    protected bool isChangeingTarget;
    public TargetingObject currentTargetingObject;
    protected List<TargetingObject> unitTargetingObjects;
    public GameObject attackLookObject;
    [Range(0.01f, 10f)] public float moveSpeed = 2;
    private IDamageable overDamagable;
    private float overDamageableCount => TargetingController.instance.overDamageableCount;
    private float rayWaitTime => TargetingController.instance.rayWaitTime;
    private InputControls inputControls;
    private StateMachine<CameraViewStates, CameraViewStateCommon> stateMachine => CameraController.instance.cameraStateMachine;
    protected List<UnitCommon> UnitsInView;
    public UnitCommon selectedUnit { get; set; }
    
    public UnitCommon currentTarget { get; protected set; }
    public StateMachine<CameraViewStateCommon> OwnerStateMachine => CameraController.instance.cameraStateMachine;
    protected virtual void Awake()
    {
        camControl = CameraController.instance;
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    protected virtual void LateUpdate()
    {
       
    }

    public override bool CanEnterState => true;
    public override bool CanExitState => true;

    public override void OnEnterState()
    {
        camControl.currentCinemachine = virtualCamera;
        virtualCamera.Priority = 10;
        enabled = true;
    }

    public override void OnExitState()
    {
        StopAllCoroutines(); 
        virtualCamera.Priority = 1;
        enabled = false; 
    }
    
    protected IEnumerator RayForTarget()
    {
        var count = 0f;
        while (viewEnabled)
        {
            if (isChangeingTarget)
                yield return new WaitForSeconds(rayWaitTime);
            var ray = camControl.GetMouseRay();
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, LayerMaskHelper.TargetLayerMask))
            {
                var hitObject = hit.collider.gameObject.GetComponent<TargetingObject>();
                if (hitObject == null)
                    yield return new WaitForSeconds(.1f);
                var hitDamageable = hitObject.damageable;
                var hitUnit = hitDamageable.GetUnit();
                if (hitUnit == currentTarget)
                {
                    if (currentTargetingObject == null)
                    {
                        yield return new WaitForSeconds(rayWaitTime);
                    }
                    else
                    {
                        if (currentTargetingObject.gameObject != hit.collider.gameObject)
                            ChangeTargetingObject(hitObject);
                    }
                }
                else
                {
                    if (overDamagable != hitDamageable)
                    {
                        count = 0;
                        overDamagable = hitDamageable;
                    }
                    else
                    {
                        if (count >= overDamageableCount)
                        {
                            TargetingController.instance.lastDirection = CompassDirections.None;
                            count = 0;
                            ChangeUnitTarget(GameManagementController.instance.GetCurrentUnit()
                                , hitUnit, false);
                        }
                        else
                        {
                            count += rayWaitTime;
                        }
                    }
                }
            }
            yield return new WaitForSeconds(rayWaitTime);
        }
    }

    private void EngageTargetObjects(TargetingObject targetObject)
    {
        if (stateMachine.CurrentKey == CameraViewStates.Attack)
            virtualCamera.LookAt = currentTargetingObject.transform;
        EventSenderController.EngageTargetingObject(targetObject);
    }

    protected internal void ChangeTargetingObject(TargetingObject targetingObject)
    {
        if (stateMachine.CurrentKey == CameraViewStates.Attack)
            virtualCamera.LookAt = targetingObject.transform;
        currentTargetingObject = targetingObject;
        EventSenderController.ChangeTargetingObject(targetingObject);
    }

    protected internal void ChangeUnitTarget(UnitCommon selectedUnit, UnitCommon bestPossibleEnemy, bool isManualChange)
    {
        StartCoroutine(ChangingUnitTarget(selectedUnit, bestPossibleEnemy, isManualChange));
    }

    protected IEnumerator ChangingUnitTarget(UnitCommon selectedUnit, UnitCommon bestPossibleEnemy,
        bool isManualChange)
    { 
        isChangeingTarget = true;
        var waitTime = isManualChange ? .5f : 0;
        EventSenderController.DisengageTargetingObject();
        if (currentTarget != null) 
            currentTarget.TurnOffTarget();
        currentTargetingObject = null; 
        StopCoroutine(LookAtEnemy(selectedUnit, bestPossibleEnemy));
        yield return new WaitForSeconds(waitTime);
        StartCoroutine(LookAtEnemy(selectedUnit, bestPossibleEnemy));
    }

    protected IEnumerator LookAtEnemy(UnitCommon selectedUnit, UnitCommon bestPossibleEnemy)
    {
        currentTarget = bestPossibleEnemy;
        EventSenderController.UnitIsTargeted(currentTarget);
        currentTarget.AttackViewTarget(selectedUnit);
        var targetCenter = currentTarget.motor.transform.position + currentTarget.motor.Capsule.center;
        isAimMoving = true;
        unitTargetingObjects = TargetingController.instance.UnitTargetingObjects(bestPossibleEnemy);
        if (currentTargetingObject == null)
            currentTargetingObject = unitTargetingObjects.Find(x => x.targetObjectType == TargetingObjectType.Head);
        EventSenderController.EngageTargetingObject(currentTargetingObject);
        if (stateMachine.CurrentKey == CameraViewStates.Attack)
        {
            selectedUnit.AttackViewTarget(currentTarget);
            while (isAimMoving)
            {
                yield return new WaitForEndOfFrameUnit();
                if (Vector3.Distance(attackLookObject.transform.position, targetCenter) > .5f)
                {
                    attackLookObject.transform.position = Vector3.Slerp(attackLookObject.transform.position,
                        targetCenter,
                        Time.deltaTime * moveSpeed);
                }
                else
                {
                    attackLookObject.transform.position = targetCenter;
                    isAimMoving = false;
                }
            }
            virtualCamera.LookAt = currentTargetingObject.transform;
        }
        isChangeingTarget = false;
    }
    
    public void ResetView(IDamageable deadDamageable)
    {   
        var selectedUnit = GameManagementController.instance.GetCurrentUnit();
        UnitCommon bestPossibleEnemy;
        bestPossibleEnemy = TargetingController.instance.ResetOnDeath(selectedUnit, deadDamageable);
        ChangeUnitTarget(selectedUnit, bestPossibleEnemy, true);
    }

    public void ChangeOverlay(CameraOverlayStates cameraOverlayStates)
    {
        StopAllCoroutines();
        if (cameraOverlayStates != CameraOverlayStates.Hookshot)
            StartCoroutine(RayForTarget());
    }
}