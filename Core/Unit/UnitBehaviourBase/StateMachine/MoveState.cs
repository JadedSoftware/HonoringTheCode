using System;
using Core.Unit.StateMachine;
using KinematicCharacterController;
using UnityEngine;

namespace Core.Unit.Movement
{
    public enum MoveMode
    {
        Default,
        MoveFreely,
        Hanging,
        LookAt,
    }
    /// <summary>
    /// State for when the unit is moving
    /// </summary>

    public class MoveState : UnitStateCommon
    {        

        public override void OnEnable()
        {
            base.OnEnable();
            moveMode = MoveMode.Default;
        }

        public override void OnEnterState()
        {
            base.OnEnterState();
            moveMode = unit.GetCurrentNavigable().GetNavType() switch
            {
                NavigableTypes.NavCell => MoveMode.Default,
                NavigableTypes.Ledge => MoveMode.Hanging,
                NavigableTypes.GrapplePoint => MoveMode.MoveFreely,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public void Update()
        {
            if (transform.root.gameObject.name.Equals("Players"))
            {
                direction = unit.movePosTemp - unit.motor.Transform.position;
                unit.HandleMovementAnimation(direction);

                if (unit.isMoving)
                {
                    if(Vector3.Distance(unit.movePosTemp,unit.motor.Transform.position) < 0.5f)
                    {
                        unit.animancer.Play(unit._SprintStopState);
                        unit.HandleSprintStopAnimation(direction);
                    }
                }
            }
        }
    }
}