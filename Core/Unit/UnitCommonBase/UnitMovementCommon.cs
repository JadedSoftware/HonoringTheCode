using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Data;
using Core.GameManagement;
using Core.Helpers;
using Core.Unit.Movement;
using Core.Unit.StateMachine;
using Core.Unit.StateMachine.enums;
using KinematicCharacterController;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using EventSenderController = Core.GameManagement.EventSenders.EventSenderController;

public abstract partial class UnitCommon : IMovable
{
    [Header("Movement")] [Range(1, 10)] public float moveSpeed = 7;
    public int moveCost = 2;
    [Range(1, 20)] public float rotationSpeed = 10;
    public float steeringRayAngle = 360;
    public float steeringRayRange = 2;
    public Vector3 currentPos;
    
    private INavigable currentNavPoint;
    
    public int currentNavIndex;
    
    private bool hasMoveOverride;
    private bool isAtDestination;
    private bool isTarget;

    private Quaternion m_rotation;
    private Vector3 m_velocity;

    [HideInInspector] public Vector3 movePosTemp;

    public IMovable movable;
    private List<INavigable> movePath;

    private int startIndex = -1;
    
    [Header("Steering Adjusters")] private readonly int numberSteeringRays = 8;
    private Vector3 offsetLocation = Vector3.zero;

    public KinematicCharacterMotor motor { get; set; }
    
    private void ConfigureMovable()
    {
        motor = GetComponent<KinematicCharacterMotor>();
        motor.CharacterController = this;
        movable = this;
        RegisterMovable(this, true);
    }

    public void RegisterMovable(IMovable movable, bool isActive)
    {
        if (unitControl != null)
            unitControl.RegisterMovable(movable, isActive);
    }

    public void UnRegisterMovable(IMovable movable, bool isActive)
    {
        throw new NotImplementedException();
    }

    private void MoveableStartPosition()
    {
        if (!DataPersistenceController.instance.isNewGame)
        {
            motor.SetPosition(navController.GetNavigable(startIndex).GetPosition());   
        }
        currentNavPoint ??= TryFindNavigable();
        startIndex = currentNavPoint.GetNavIndex();
        currentNavPoint = navController.GetNavigable(startIndex);
        if (currentNavPoint == null) return;
        var navPath = new Stack<int>(new[] {startIndex});
        StartCoroutine(MovePath(navPath, currentNavPoint));
    }
    
    public IMovable GetMovable()
    {
        return movable;
    }

    public int GetMoveDistance()
    {
        return currentActionPoints / GetPointsRequired(ActionTypes.Move); //C# throws away the remainder
    }

    public int PredictMoveDistance(int actionPoints)
    {
        return (currentActionPoints - actionPoints) / GetPointsRequired(ActionTypes.Move);
    }

    public INavigable GetCurrentNavigable()
    {
        return currentNavPoint;
    }
    public virtual void SetMovementVelocity(Vector3 pos)
    {
        var dir = pos - motor.Transform.position;
        if (hasMoveOverride)
        {
            m_velocity = dir;
            m_rotation = Quaternion.LookRotation(dir);
            Debug.Log("move to override");
            return;
        }

        switch (unitBehaviour.moveState.moveMode)
        {
            case MoveMode.Default:
                m_velocity = new Vector3(dir.x, 0, dir.z);
                m_rotation = Quaternion.LookRotation(dir);
                break;
            case MoveMode.MoveFreely:
                m_velocity = dir;
                m_rotation = Quaternion.LookRotation(dir);
                break;
            case MoveMode.Hanging:
                break;
            case MoveMode.LookAt:
                m_rotation = Quaternion.LookRotation(dir);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void RotateTowardsCamera()
    {
        var cameraForward = CameraController.instance.CameraPlanarForward() * 3 + motor.transform.position;
        SetRotation(cameraForward);
    }
    

    private INavigable TryFindNavigable()
    {
        return navController.GetNavigable(NavPathJobs.instance.FindClosestNav(motor.transform.position));
    }

    public virtual void SetRotation(Vector3 pos)
    {
        var dir = pos - motor.Transform.position;
        m_rotation = Quaternion.LookRotation(dir, motor.CharacterUp);
    }

    public virtual void AttackViewTarget(UnitCommon selectedUnit)
    {
        isTarget = true;
        StartCoroutine(LookAtTarget(selectedUnit));
    }

    public void TurnOffTarget()
    {
        isTarget = false;
        SetHighlight(false, Color.clear);
    }

    private IEnumerator LookAtTarget(UnitCommon lookAtUnit)
    {
        while (isTarget)
        {
            SetRotation(lookAtUnit.motor.transform.position);
            yield return new WaitForEndOfFrame();
        }
    }

    protected virtual void UnitMovementRequested(IMovable unit, INavigable endnavigable)
    {
        if (ReferenceEquals(unit, this))
        {
            //do something
        }
        else
        {
            // if the unit is requesting movement in a space that another unit is overwatching requested space
            // poerform overwatch attack
        }
    }

    protected virtual void UnitMovementInitiated(IMovable unit, Stack<int> navPath, INavigable navEndPoint)
    {
        if (!ReferenceEquals(unit, this)) return;
        PerformAction(ActionTypes.Move, navPath.Count);
        if (navEndPoint != currentNavPoint)
            currentNavPoint.SetOccupiedMovable(null);
        StartCoroutine(MovePath(navPath, navEndPoint));
    }

    private void UnitMovementComplete(IMovable unit)
    {
        if (ReferenceEquals(unit, this))
        {
            unitBehaviour.StateMachine.TrySetState(UnitStateTypes.Idle);
            currentPos = motor.transform.position;
        }
    }
    protected virtual IEnumerator MovePath(Stack<int> navPath, INavigable endNavPoint)
    {
        unitBehaviour.SetState(UnitStateTypes.Move);
        var dir = endNavPoint.GetPosition() - motor.transform.position;
        isAtDestination = false;
        var previousNavPoint = GetCurrentNavigable();

        foreach (var navPoint in navPath.Select(navPointIndex => navController.GetNavigable(navPointIndex)))
        {
            UpdateNavigable(navPoint);
            var movePos = navPoint.GetPosition();
            movePosTemp = endNavPoint.GetPosition();
            switch (navPoint.GetNavType())
            {
                case NavigableTypes.NavCell:
                    if (previousNavPoint.GetNavType() is NavigableTypes.GrapplePoint or NavigableTypes.Ledge)
                        if (unitBehaviour.moveState.moveMode != MoveMode.Default && isGrounded)
                            unitBehaviour.moveState.EndFreeMove();

                    break;
                case NavigableTypes.Ledge:
                    break;
                case NavigableTypes.GrapplePoint:
                    if (previousNavPoint.GetNavType() == NavigableTypes.NavCell)
                        if (unitBehaviour.moveState.moveMode != MoveMode.MoveFreely)
                            unitBehaviour.moveState.EnableFreeMove();

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            while (DistanceToNavigable(navPoint) > .5f)
            {
                while (hasMoveOverride)
                {
                    if (Vector3.Distance(motor.transform.position, offsetLocation) > .5f)
                        SetMovementVelocity(offsetLocation);
                    else
                        hasMoveOverride = false;

                    yield return new WaitForEndOfFrame();
                }

                SetMovementVelocity(movePos);
                yield return new WaitForEndOfFrame();
            }

            previousNavPoint = navPoint;
        }

        if (DebugController.instance.isDebugEnabled) Debug.Log("At Destination : " + endNavPoint.GetNavIndex());
        UpdateNavigable(endNavPoint);
        isAtDestination = true;
        unitBehaviour.SetState(UnitStateTypes.Idle);
        m_velocity = Vector3.zero;
        EventSenderController.UnitMovementComplete(this);
    }

    private float DistanceToNavigable(INavigable navPoint)
    {
        return Vector3.Distance(motor.transform.position, navPoint.GetPosition());
    }

    private void CollisionAvoidance(Vector3 collisionPoint)
    {
        if (hasMoveOverride)
            return;

        var time = Time.realtimeSinceStartup;
        //todo deteremine where collision is based on character motor. 
        var motorPos = motor.transform.position;
        var centerPos = motor.Capsule.center + motorPos;
        var rotation = transform.rotation;
        List<Vector3> possibleMoves = new();

        var rayAngles = currentNavPoint.GetPosition().y > centerPos.y
            ? RayAngleHelper.ForwardAngles
            : RayAngleHelper.BehindAngles;
        for (var i = 0; i < numberSteeringRays; i++)
            possibleMoves.AddRange(from angle in rayAngles
                select Quaternion.AngleAxis(i / (float) numberSteeringRays * steeringRayAngle,
                           motor.CharacterUp)
                       * quaternion.Euler((int) angle, 0, 0)
                into rayAngle
                select rotation * rayAngle * motor.CharacterUp
                into direction
                let ray = new Ray(centerPos, direction)
                where !Physics.Raycast(ray, steeringRayRange, LayerMaskHelper.levelLayerMask)
                select centerPos + direction * steeringRayRange);

        Debug.Log("Find Avoid Ledge execute time : " + (Time.realtimeSinceStartup - time) * 1000f);

        Vector3 closestPoint = new();
        var closestMoves = possibleMoves.OrderBy(x => Vector3.Distance(x, currentNavPoint.GetPosition())).ToList();
        foreach (var pos in closestMoves.Where(pos => !Physics.CapsuleCast(motor.transform.position,
                     motor.transform.position + new Vector3(0, motor.Capsule.height + .5f, 0),
                     motor.Capsule.radius + .5f,
                     pos, steeringRayRange,
                     LayerMaskHelper.levelLayerMask)))
        {
            closestPoint = pos;
            break;
        }

        Debug.Log("Avoid Ledge execute time : " + (Time.realtimeSinceStartup - time) * 1000f);

        unitBehaviour.moveState.EnableFreeMove();
        offsetLocation = closestPoint;
        hasMoveOverride = true;
    }


}