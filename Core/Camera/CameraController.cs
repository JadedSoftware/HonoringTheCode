using System;
using System.Collections;
using System.Collections.Generic;
using Animancer.FSM;
using Cinemachine;
using Core.Camera;
using Core.GameManagement;
using Core.UI;
using Core.Unit;
using Core.Unit.Warrior;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using EventSenderController = Core.GameManagement.EventSenders.EventSenderController;

public enum CameraViewStates
{
    Topdown,
    Attack,
    AttackOrbit,
    EnemyTurn
}
public class CameraController : MonoBehaviour, IEvents, InputControls.ICameraMovementActions
{
  
    public Transform lookAtObject;
    [Range(2, 100)] public float movementSpeed;

    [Range(2, 200)] public float rotationSpeed;
    [Range(1, 75)] public float orbitSpeedX = 45;
    [Range(1, 75)] public float orbitDampiningX = 1;
    [Range(1, 75)] public float yawSpeedY = 1;
    [Range(1, 75)] public float yawDampiningY = 1;
    [Range(-10, 0)] public float yawMin = -5;
    [Range(1, 25)] public float yawMax = 15;

    [Range(2, 30)] public float zoomSpeed;
    public float zoomMulitplier = 2;

    public CinemachineVirtualCamera currentCinemachine;
    public Camera mainCamera;
    public bool isInputDisabled;
    public bool hasMovement;

    public StateMachine<CameraViewStates, CameraViewStateCommon> cameraStateMachine;
    public TopdownCameraViewState topdownCameraViewState;
    public AttackCameraViewState attackViewState;
    public AttackOrbitState attackOrbitState;
    
    [HideInInspector] public AimCameraObject aimCamObj;
    private Vector2 mousePosScreen;
    private readonly float yZoomMax = 70;
    private readonly float yZoomMin = 5;
    private CinemachineOrbitalTransposer currentCamTransposer;
    private Vector3 goalZoom;
    private bool hasAction;
    private bool hasRotation;
    private bool hasUnitSelected;

    public InputControls inputControls;

    private bool isChangingTarget;
    private bool isOrbitView;
    private bool isHookshotView;

    private bool isShiftModified;
    private InputAction moveAction;
    private PhysicsRaycaster physicsRaycaster;
    private InputAction rotateAction;

    private ISelectable currentSelectable;
    private CinemachineCollider topDownCollider;

    private InputAction zoomAction;

    private CameraOverlayController camOverlayController;

    private static CameraController _instance;
    public static CameraController instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType(typeof(CameraController)) as CameraController;
            return _instance;
        }
        set => _instance = value;
    }

    private void Awake()
    {
        instance = this;
        inputControls = new InputControls();
        moveAction = inputControls.CameraMovement.Move;
        rotateAction = inputControls.CameraMovement.Rotate;
        zoomAction = inputControls.CameraMovement.Zoom;
        var mTransform = transform;
        aimCamObj = new GameObject
        {
            transform =
            {
                name = "AimCamObject",
                parent = mTransform,
                position = mTransform.position
            }
        }.AddComponent<AimCameraObject>();
        inputControls.CameraMovement.SetCallbacks(this);
        
        camOverlayController = new GameObject
        {
            transform =
            {
                name = "CameraOverlayController",
                parent = transform,
                position = transform.position
            }
        }.AddComponent<CameraOverlayController>();
        camOverlayController.OnCreate();
    }

    private void Start()
    {
        physicsRaycaster = mainCamera.AddComponent<PhysicsRaycaster>();
        physicsRaycaster.eventMask = LayerMaskHelper.selectableMask;
        ConfigureStateMachine();
    }

    private void OnEnable()
    {
        inputControls.Enable();
        currentCamTransposer = currentCinemachine.GetCinemachineComponent<CinemachineOrbitalTransposer>();
        topDownCollider = currentCinemachine.GetComponent<CinemachineCollider>();
        topDownCollider.enabled = false;
        goalZoom = currentCamTransposer.m_FollowOffset;
        RegisterEvents();
    }

    private void OnDisable()
    {
        inputControls.Disable();
        UnRegisterEvents();
    }

    public void RegisterEvents()
    {
        EventSenderController.unitSelected += UnitSelected;
        EventSenderController.unitDeselected += UnitDeselected;
        EventSenderController.unitMovementInitiated += UnitMovementInitiated;
        EventSenderController.unitMovementComplete += UnitMovementComplete;
        EventSenderController.enterAttackView += EnterAttackView;
        EventSenderController.enterTopdownView += EnterTopdownView;
        EventSenderController.initiateDamagableDeath += DamageableDeathInitiated;
        EventSenderController.performDamageableDeath += DamgeableDeathPerformed;
        EventSenderController.onOverlayChanged += OnChangeOverlay;
    }

    public void UnRegisterEvents()
    {
        EventSenderController.unitSelected -= UnitSelected;
        EventSenderController.unitDeselected -= UnitDeselected;
        EventSenderController.unitMovementInitiated -= UnitMovementInitiated;
        EventSenderController.unitMovementComplete -= UnitMovementComplete;
        EventSenderController.enterAttackView -= EnterAttackView;
        EventSenderController.enterTopdownView -= EnterTopdownView;
        EventSenderController.initiateDamagableDeath -= DamageableDeathInitiated;
        EventSenderController.performDamageableDeath -= DamgeableDeathPerformed;
        EventSenderController.onOverlayChanged -= OnChangeOverlay;
    }

    private void ConfigureStateMachine()
    {
        cameraStateMachine = new StateMachine<CameraViewStates, CameraViewStateCommon>();
        if (topdownCameraViewState == null) topdownCameraViewState = GetComponentInChildren<TopdownCameraViewState>();

        if (attackViewState == null) attackViewState = GetComponentInChildren<AttackCameraViewState>();

        if (attackOrbitState == null) attackOrbitState = GetComponentInChildren<AttackOrbitState>();
        
        cameraStateMachine.Add(CameraViewStates.Topdown, topdownCameraViewState);
        cameraStateMachine.Add(CameraViewStates.Attack, attackViewState);
        cameraStateMachine.Add(CameraViewStates.AttackOrbit, attackOrbitState);        

        SetState(CameraViewStates.Topdown);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (isInputDisabled) return;
        switch (cameraStateMachine.CurrentKey)
        {
            case CameraViewStates.Topdown:
                if (context.started)
                {
                    hasMovement = true;
                    StartCoroutine(MoveTopdownCamera());
                }

                if (context.canceled)
                {
                    hasMovement = false;
                    StopCoroutine(MoveTopdownCamera());
                }

                break;
            case CameraViewStates.Attack:
                if (isChangingTarget) return;
                StartCoroutine(ChangeTargetPerformed());
                break;
            case CameraViewStates.EnemyTurn:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        switch (cameraStateMachine.CurrentKey)
        {
            case CameraViewStates.Topdown:
                break;
            case CameraViewStates.Attack:
                break;
            case CameraViewStates.EnemyTurn:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        if (isInputDisabled) return;
        switch (cameraStateMachine.CurrentKey)
        {
            case CameraViewStates.Topdown:
                if (context.started)
                {
                    hasRotation = true;
                    StartCoroutine(RotateTopdown());
                }

                if (context.canceled)
                {
                    hasRotation = false;
                    StopCoroutine(RotateTopdown());
                }

                break;
            case CameraViewStates.Attack:
                break;
            case CameraViewStates.EnemyTurn:
                break;
            case CameraViewStates.AttackOrbit:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void OnZoom(InputAction.CallbackContext context)
    {
        if (isInputDisabled) return;
        switch (cameraStateMachine.CurrentKey)
        {
            case CameraViewStates.Topdown:
                //TopDown State LateUpdate
                break;
            case CameraViewStates.Attack:
                break;
            case CameraViewStates.EnemyTurn:
                break;
            case CameraViewStates.AttackOrbit:
            default:
                return;
        }
    }

    public void OnResetPosition(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }
    
    private void EnterTopdownView()
    {
        SetState(CameraViewStates.Topdown);
    }

    private void EnterAttackView()
    {
        SetState(CameraOverlayController.instance.currentOverlayState == CameraOverlayStates.Hookshot ? 
            CameraViewStates.AttackOrbit : CameraViewStates.Attack);
    }

    public void OnTargetingObjectModifier(InputAction.CallbackContext context)
    {
        if (context.started) isShiftModified = true;

        if (context.canceled) isShiftModified = false;
    }

    public void OnAttackOrbit(InputAction.CallbackContext context)
    {
        if (cameraStateMachine.CurrentKey == CameraViewStates.Topdown)
            return;
        if (!context.performed) return;

        if (!isOrbitView)
        {
            var currentSelectedUnit = GameManagementController.instance.GetCurrentUnit();
            aimCamObj.transform.position = currentSelectedUnit.AttackCamTransform().position +
                                           currentSelectedUnit.motor.CharacterForward * 3;
            attackOrbitState.currentTargetingObject = attackViewState.currentTargetingObject;
            cameraStateMachine.ForceSetState(CameraViewStates.AttackOrbit);
            attackOrbitState.OffsetY(0);
            isOrbitView = true;
            StartCoroutine(OrbitView());
        }
        else
        {
            isOrbitView = false;
            cameraStateMachine.ForceSetState(CameraViewStates.Attack);
        }
    }
    
    
    private IEnumerator MoveTopdownCamera()
    {
        while (hasMovement)
        {
            yield return new WaitForEndOfFrameUnit();
            var moveValue = moveAction.ReadValue<Vector2>();
            moveValue *= movementSpeed * Time.deltaTime;
            lookAtObject.Translate(moveValue.x, 0, moveValue.y);
        }
    }

    private IEnumerator RotateTopdown()
    {
        while (hasRotation)
        {
            yield return new WaitForEndOfFrameUnit();
            var rotateValue = rotateAction.ReadValue<float>();
            rotateValue *= rotationSpeed * Time.deltaTime;
            lookAtObject.Rotate(0, rotateValue, 0);
        }
    }

    private IEnumerator OrbitView()
    {
        var eulerAngles = aimCamObj.transform.eulerAngles;
        var desiredRot = eulerAngles.y;
        var desirderYaw = aimCamObj.transform.position.y;
        while (isOrbitView)
        {
            mousePosScreen = MouseScreenPosition();
            var orbitRotation = mousePosScreen.x switch
            {
                <= .35f => -1f,
                >= .65f => 1f,
                _ => 0
            };
            var yawRotation = mousePosScreen.y switch
            {
                <= .35f => 1f,
                >= .65f => -1f,
                _ => 0
            };
            orbitRotation *= orbitSpeedX * Time.deltaTime;
            yawRotation *= yawSpeedY * Time.deltaTime;
            desiredRot += orbitRotation;
            desirderYaw = Mathf.Lerp(desirderYaw, desirderYaw + yawRotation, Time.deltaTime * yawDampiningY);
            desirderYaw = Math.Clamp(desirderYaw, -4, 17);
            attackOrbitState.OffsetY(desirderYaw);
            var desiredRotQ = Quaternion.Euler(transform.eulerAngles.x, desiredRot, transform.eulerAngles.z);
            aimCamObj.transform.rotation = Quaternion.Lerp(aimCamObj.transform.rotation, desiredRotQ,
                Time.deltaTime * orbitDampiningX);
            currentSelectable.GetMovable().RotateTowardsCamera();
            yield return new WaitForEndOfFrame();
        }
    }

    public Vector2 MouseScreenPosition()
    {
        return mainCamera.ScreenToViewportPoint(Mouse.current.position.ReadValue());
    }

    private void UnitSelected(ISelectable unit)
    {
        currentSelectable = unit;
        if (topDownCollider != null) StartCoroutine(EnableCamCollider());
        StartCoroutine(MoveLookTarget(unit.GetPosition()));
    }

    private void UnitDeselected()
    {
        currentSelectable = null;
        StopAllCoroutines();
    }

    private IEnumerator EnableCamCollider()
    {
        while (Vector3.Distance(lookAtObject.transform.position, currentSelectable.GetPosition()) < .2f)
        {
            topDownCollider.enabled = true;
            yield return new WaitForEndOfFrame();
        }

        topDownCollider.enabled = false;
    }

    private void UnitMovementInitiated(IMovable unit, Stack<int> navpath, INavigable endnavigable)
    {
        isInputDisabled = true;
        StartCoroutine(MoveWithTarget(unit));
    }

    private void UnitMovementComplete(IMovable unit)
    {
        isInputDisabled = false;
    }


    private IEnumerator MoveWithTarget(IMovable unit)
    {
        while (isInputDisabled)
        {
            if (Vector3.Distance(lookAtObject.transform.position, unit.GetPosition()) > 1f)
            {
                var movePos = Vector3.Lerp(lookAtObject.transform.position, unit.GetPosition(),
                    movementSpeed / 10 * Time.deltaTime);
                lookAtObject.transform.position = movePos;
            }

            lookAtObject.transform.position = unit.GetPosition();
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator MoveLookTarget(Vector3 getPosition)
    {
        isInputDisabled = true;
        while (Vector3.Distance(lookAtObject.transform.position, getPosition) > 1f)
        {
            var movePos = Vector3.Lerp(lookAtObject.transform.position, getPosition,
                movementSpeed / 10 * Time.deltaTime);
            lookAtObject.transform.position = movePos;
            yield return new WaitForEndOfFrame();
        }

        lookAtObject.transform.position = getPosition;
        isInputDisabled = false;
    }

    public void ZoomTopdown()
    {
        var zoomValue = -zoomAction.ReadValue<float>();
        if (zoomValue != 0)
        {
            zoomValue *= zoomMulitplier;
            var newYOffset = Mathf.Clamp(goalZoom.y + zoomValue, yZoomMin, yZoomMax);
            goalZoom = new Vector3(goalZoom.x, newYOffset, goalZoom.z);
        }

        var dist = Vector3.Distance(goalZoom, currentCamTransposer.m_FollowOffset);
        if (dist > .1f)
        {
            float newZoomY;
            if (currentCamTransposer.m_FollowOffset.y > goalZoom.y)
                newZoomY = currentCamTransposer.m_FollowOffset.y - zoomSpeed * Time.deltaTime;
            else
                newZoomY = currentCamTransposer.m_FollowOffset.y + zoomSpeed * Time.deltaTime;
            currentCamTransposer.m_FollowOffset.y = newZoomY;
        }
    }

    private void ResetCameraPosition(InputAction.CallbackContext ctx)
    {
        // vCamTransform.position = vCamReset.transform.position;
        // vCamTransform.rotation = vCamReset.transform.rotation;
    }

    public Vector3 CameraPlanarForward()
    {
        var cameraPlanarDirection = Vector3
            .ProjectOnPlane(mainCamera.transform.rotation * Vector3.forward,
                Vector3.up)
            .normalized;
        if (cameraPlanarDirection.sqrMagnitude == 0f)
            cameraPlanarDirection = Vector3
                .ProjectOnPlane(mainCamera.transform.rotation * Vector3.up, Vector3.up)
                .normalized;
        return cameraPlanarDirection;
    }

    private IEnumerator ChangeTargetPerformed()
    {
        isInputDisabled = true;
        yield return new WaitForSeconds(.1f);
        var tempInput = moveAction.ReadValue<Vector2>();
        isInputDisabled = false;
        var input = new Vector2((float) Math.Round(tempInput.x), (float) Math.Round(tempInput.y));
        if (input == Vector2.zero) yield break;
        CompassDirections direction;
        if (input == new Vector2(0, 1))
            direction = CompassDirections.N;
        else if (input == new Vector2(0, -1))
            direction = CompassDirections.S;
        else if (input == new Vector2(-1, 0))
            direction = CompassDirections.W;
        else if (input == new Vector2(1, 0))
            direction = CompassDirections.E;
        else if (input == new Vector2(1, 1))
            direction = CompassDirections.NE;
        else if (input == new Vector2(-1, 1))
            direction = CompassDirections.NW;
        else if (input == new Vector2(-1, -1))
            direction = CompassDirections.SW;
        else if (input == new Vector2(1, -1))
            direction = CompassDirections.SE;
        else
            direction = CompassDirections.None;

        if (isShiftModified)
        {
            if (direction == CompassDirections.None) yield break;
            StartCoroutine(ChangeTargetingObject(direction));
            yield break;
        }

        StartCoroutine(ChangeTarget(direction));
    }

    private IEnumerator ChangeTarget(CompassDirections direction)
    {
        isChangingTarget = true;
        var selectedUnit = GameManagementController.instance.GetCurrentUnit();
        var currentEnemy = attackViewState.currentTarget;
        var bestTarget = TargetingController.instance.ChangeAttackViewTarget(selectedUnit, currentEnemy, direction);
        attackViewState.ChangeUnitTarget(selectedUnit, bestTarget, true);
        yield return new WaitForSeconds(.75f);
        isChangingTarget = false;
    }

    private IEnumerator ChangeTargetingObject(CompassDirections direction)
    {
        isChangingTarget = true;
        var nextTargetingObject =
            TargetingController.instance.NextTargetingObject(attackViewState.currentTargetingObject, direction);
        attackViewState.ChangeTargetingObject(nextTargetingObject);
        yield return new WaitForSeconds(.5f);
        isChangingTarget = false;
    }


    private void DamageableDeathInitiated(AttackAction attackaction)
    {
        // todo
    }

    private void DamgeableDeathPerformed(AttackAction attackaction)
    {
        if (UnitCommonController.instance.allAiUnits.Count == 0)
        {
            cameraStateMachine.ForceSetState(CameraViewStates.Topdown);
            return;
        }
        switch (cameraStateMachine.CurrentKey)
        {
            case CameraViewStates.Topdown:
                break;
            case CameraViewStates.Attack:
                attackViewState.ResetView(attackaction.damageable);
                break;
            case CameraViewStates.AttackOrbit:
                break;
            case CameraViewStates.EnemyTurn:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public Ray GetMouseRay()
    {
        return mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
    }

    public void SetState(CameraViewStates newState)
    {
        cameraStateMachine.ForceSetState(newState);
    }

    public CameraViewStates ViewState()
    {
        return cameraStateMachine.CurrentKey;
    }

    private void OnChangeOverlay(CameraOverlayStates overlayState)
    {
        if (NavigableController.instance.selectedNavigable != null)
        {
            EventSenderController.MouseExitNavigable(NavigableController.instance.selectedNavigable);
            RaycastController.instance.lastHit = null;
        }
        cameraStateMachine.CurrentState.ChangeOverlay(overlayState);
    }
}