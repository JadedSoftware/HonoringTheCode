using System;
using Core.Unit.StateMachine;
using Core.Unit.StateMachine.enums;
using KinematicCharacterController;
using UnityEngine;
/// <summary>
/// Handles the Kinematic Character Controller functions of the unit:
/// https://docs.google.com/document/d/1qT71uGaUO4UmK1NbW9-UsrX1G-0dWWQ2JQlKWBH0dIw/edit
/// </summary>
[RequireComponent(typeof(KinematicCharacterMotor))]
public partial class UnitCommon : ICharacterController
{
    #region KCC controls

    // ---------------------- KCC ------------------- //
    private Vector3 characterColliderPoint;
    private Vector3 collisionPoint;
    private Vector3 collisionNormal;
    private Vector3 moveOffset;

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        var rotation = motor.transform.rotation;
        switch (unitBehaviour.StateMachine.CurrentKey)
        {
            case UnitStateTypes.Idle:
                currentRotation = Quaternion.Slerp(motor.transform.rotation, m_rotation, deltaTime * rotationSpeed);
                currentRotation.x = rotation.x;
                currentRotation.z = rotation.z;
                break;
            case UnitStateTypes.IdleMelee:
                break;
            case UnitStateTypes.IdleRanged:
                break;
            case UnitStateTypes.Move:
                if (!isAtDestination)
                    currentRotation = Quaternion.Slerp(motor.transform.rotation, m_rotation, deltaTime * rotationSpeed);
                currentRotation.x = rotation.x;
                currentRotation.z = rotation.z;
                break;
            case UnitStateTypes.Guard:
                break;
            case UnitStateTypes.Overwatch:
                break;
            case UnitStateTypes.Attack:
                break;
            case UnitStateTypes.Cover:
                break;
            case UnitStateTypes.Special:
                break;
           
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (isHookshot)
        {
            currentVelocity = m_velocity.normalized * (moveSpeed * 3);
            return;
        }
        
        if (!isAtDestination)
            currentVelocity = m_velocity.normalized * moveSpeed;
        else
            currentVelocity = Vector3.zero;
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
    }

    public void PostGroundingUpdate(float deltaTime)
    {
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        var collGameObject = coll.gameObject;
        return collGameObject.layer is (int) LayersEnum.Terrain or (int) LayersEnum.Obstacle;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport)
    {
        //isGrounded = hitStabilityReport.IsStable;
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport)
    {
        if (hitCollider.gameObject.layer == (int) LayersEnum.Obstacle)
        {
            Debug.Log("Avoid Collision");
            CollisionAvoidance(hitPoint);
        }
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        Vector3 atCharacterPosition,
        Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }

    #endregion
}