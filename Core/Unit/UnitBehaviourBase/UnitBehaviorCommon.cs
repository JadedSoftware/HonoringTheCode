using System;
using System.Collections.Generic;
using Animancer.FSM;
using Core.Unit.Movement;
using Core.Unit.StateMachine;
using Core.Unit.StateMachine.enums;
using UnityEngine;

/// <summary>
/// Handles changing unit StateMachine states
/// </summary>

[RequireComponent(typeof(UnitCommon))]
public abstract class UnitBehaviourCommon : MonoBehaviour
{
    public List<UnitStateTypes> characterBehavioursList =
        new() {UnitStateTypes.Idle,UnitStateTypes.IdleRanged, UnitStateTypes.IdleMelee, UnitStateTypes.Attack, UnitStateTypes.Move};

    private GameObject behaviourGameObject;

    public StateMachine<UnitStateTypes, UnitStateCommon> StateMachine { get; private set; }

    private UnitCommon unit { get; set; }
    public IdleState idleState => StateMachine[UnitStateTypes.Idle] as IdleState;
    public RifleIdleState idleRifleState => StateMachine[UnitStateTypes.IdleRanged] as RifleIdleState;
    public MeleeIdleState idleMeleeState => StateMachine[UnitStateTypes.IdleMelee] as MeleeIdleState;
    public MoveState moveState => StateMachine[UnitStateTypes.Move] as MoveState;
    public AttackState attackState => StateMachine[UnitStateTypes.Attack] as AttackState;

    public virtual void Awake()
    {
        StateMachine = new StateMachine<UnitStateTypes, UnitStateCommon>();
        unit = GetComponent<UnitCommon>();
        behaviourGameObject = new GameObject
        {
            transform =
            {
                parent = transform
            },
            name = "Behaviour Holder"
        };
    }

    public virtual void OnEnable()
    {
        unit = GetComponent<UnitCommon>();
        ConfigureStates();
    }

    private void ConfigureStates()
    {
        StateMachine = new StateMachine<UnitStateTypes, UnitStateCommon>();
        foreach (var state in characterBehavioursList)
        {
            UnitStateCommon newState;
            switch (state)
            {
                case UnitStateTypes.Idle:
                    newState = behaviourGameObject.AddComponent<IdleState>();
                    break;
                case UnitStateTypes.IdleMelee:
                    newState = behaviourGameObject.AddComponent<MeleeIdleState>();
                    break;
                case UnitStateTypes.IdleRanged:
                    newState = behaviourGameObject.AddComponent<RifleIdleState>();
                    break;
                case UnitStateTypes.Move:
                    newState = behaviourGameObject.AddComponent<MoveState>();
                    break;
                case UnitStateTypes.Attack:
                    newState = behaviourGameObject.AddComponent<AttackState>();
                    break;
                
                case UnitStateTypes.Guard:
                case UnitStateTypes.Cover:
                case UnitStateTypes.Overwatch:
                case UnitStateTypes.Special:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            newState.unit = unit;
            newState.unitBehaviour = this;
            StateMachine.Add(state, newState);
        }
    }

    public void SetState(UnitStateTypes stateType)
    {
        StateMachine.ForceSetState(stateType);
    }

    public void SetMoveMode(MoveMode moveMode)
    {
        moveState.moveMode = moveMode;
    }
}