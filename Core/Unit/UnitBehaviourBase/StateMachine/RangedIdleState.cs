using Core.Unit.StateMachine;
public class RifleIdleState : UnitStateCommon
{
    public RifleIdleState()
    {
    }

    public override void OnEnterState()
    {
        if (transform.root.gameObject.name.Equals("Players"))
        {
            print("enter");
            unit.PlayRifleIdleAnimation();
        }
    }
}
