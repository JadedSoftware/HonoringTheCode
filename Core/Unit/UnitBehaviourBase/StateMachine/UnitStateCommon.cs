using Animancer;
using Animancer.FSM;
using Core.Unit.Movement;
using KinematicCharacterController;
using UnityEngine;

namespace Core.Unit.StateMachine
{
    /// <summary>
    /// Base state for the unit, all unit states are derived from this component
    /// </summary>
    public abstract class UnitStateCommon : StateBehaviour, IOwnedState<UnitStateCommon>, IPrioritizable
    {
        public UnitCommon unit;
        public UnitBehaviourCommon unitBehaviour;
        private IMovable _movable;
        protected Quaternion rotationBeforeClimbing;
        public MoveMode moveMode { get; set; }
        protected KinematicCharacterMotor motor => unit.motor;
        protected Vector3 direction;

        public virtual void Awake()
        {
        }

        public virtual void OnEnable()
        {
        }

        public StateMachine<UnitStateCommon> OwnerStateMachine => unitBehaviour.StateMachine;

        public override bool CanEnterState => true;
        public override bool CanExitState => true;

        public override void OnEnterState()
        {
        }

        public override void OnExitState()
        {
        }

        public virtual float Priority { get; }

        public void PlayAnimation(AnimationClip _animationClip, bool _isLooping)
        {
           unit.animancer.Play(_animationClip);
        }
        
        public void EnableFreeMove()
        {
            moveMode = MoveMode.MoveFreely;
            motor.SetGroundSolvingActivation(false);
            motor.SetMovementCollisionsSolvingActivation(false);
        }

        public void EndFreeMove()
        {
            moveMode = MoveMode.Default;
            motor.SetGroundSolvingActivation(true);
            motor.SetMovementCollisionsSolvingActivation(true);
        }
    }
}
    
