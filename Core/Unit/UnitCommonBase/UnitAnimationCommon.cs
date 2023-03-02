using Animancer;
using System.Collections;
using System.Collections.Generic;
using Animation;
using Core.Unit.Interfaces;
using UnityEngine;

public partial class UnitCommon : IAnimate
{
    public AnimationContainer animContainer { get; set; }
    public AnimancerComponent animancer { get; set; }

    [SerializeField] public ClipTransition _RifleIdle;
    [SerializeField] public ClipTransition _SwordIdle;
    [SerializeReference] public ITransition _Move;
    [SerializeField] public ClipTransition _Sprint;
    [SerializeReference] public ITransition _SprintStop;
    public MixerState<Vector2> _MoveState;
    public MixerState<Vector2> _SprintStopState;

    public AnimancerState rifleIdleState;
    public AnimancerState swordIdleState;
    public AnimancerState moveState;
    public AnimancerState sprintState;
    public AnimancerState sprintStopState;

    public bool isMoving;
    private void ConfigureAnimations()
    {
        animancer = GetComponentInChildren<AnimancerComponent>();
        animContainer = modelContainer.animContainer;

        if (transform.root.gameObject.name.Equals("Players"))
        {
            var state = animancer.States.GetOrCreate(_Move);
            _MoveState = (MixerState<Vector2>)state;

            var _sprintStopState = animancer.States.GetOrCreate(_SprintStop);
            _SprintStopState = (MixerState<Vector2>)_sprintStopState;
        }
    }

    public void PlayRifleIdleAnimation()
    {
        rifleIdleState = animancer.Play(_RifleIdle);
        //idleState.Events.OnEnd = EndIdleState;
    }

    public void PlaySwordIdleAnimation()
    {
        swordIdleState = animancer.Play(_SwordIdle);
        //idleState.Events.OnEnd = EndIdleState;
    }

    public void HandleMovementAnimation(Vector3 direction)
    {
        if (_MoveState.IsActive)
        {
            _MoveState.Parameter = new Vector2(
                Vector3.Dot(Vector3.right, direction),
                Vector3.Dot(Vector3.forward, direction));

            _MoveState.Speed = 1;
        }
        else
        {
            _MoveState.Parameter = default;
            _MoveState.Speed = 0;
        }
    }
    public void HandleSprintStopAnimation(Vector3 direction)
    {
        if (_SprintStopState.IsActive)
        {
            _SprintStopState.Parameter = new Vector2(
                Vector3.Dot(Vector3.right, direction),
                Vector3.Dot(Vector3.forward, direction));

            _SprintStopState.Speed = 1;

            _SprintStopState.Events.OnEnd = OnEventEndPlayIdleAnimation;
            isMoving = false;
        }
        else
        {
            _SprintStopState.Parameter = default;
            _SprintStopState.Speed = 0;
        }
    }
    public void OnEventEndPlayIdleAnimation()
    {
        //handle state change, instead of playing animations, so animations are tied to states rather than events, more modular long run
        //switch (idleAnimState)
        //{
        //    case IdleAnimState.Sword:
        //        animancer.Play(_SwordIdle);
        //        break;
        //    case IdleAnimState.Rifle:
        //        animancer.Play(_RifleIdle);
        //        break;
        //    default:
        //        break;
        //}
        animancer.Play(_RifleIdle);
    }
    public void OnEventEndPlayMoveAnimation()
    {
        moveState = animancer.Play(_MoveState);
        moveState.Events.NormalizedEndTime = 0.1f;
        moveState.Events.OnEnd = OnEventEndPlaySprintAnimation;
    }
    public void OnEventEndPlaySprintAnimation()
    {
        sprintState = animancer.Play(_Sprint);
        isMoving = true;
    }
    public void PlaySprintStopState()
    {
        animancer.Play(_SprintStopState);
    }
}

