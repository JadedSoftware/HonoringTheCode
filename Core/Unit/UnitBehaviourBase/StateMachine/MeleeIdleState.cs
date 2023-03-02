using Core.Unit.StateMachine;

public class MeleeIdleState : UnitStateCommon
{
    public override void OnEnterState()
    {
        if (transform.root.gameObject.name.Equals("Players"))
        {
            unit.PlaySwordIdleAnimation();
        }
    }
}
