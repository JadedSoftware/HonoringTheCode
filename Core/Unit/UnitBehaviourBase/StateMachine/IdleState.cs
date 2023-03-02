using Core.Unit.StateMachine;
using UnityEngine;

/// <summary>
/// State for when the unit is idle
/// </summary>
public class IdleState : UnitStateCommon
{

    public override void OnEnterState()
    {
        if (transform.root.gameObject.name.Equals("Players"))
        {
            unit.PlayRifleIdleAnimation();
        }
    }
}