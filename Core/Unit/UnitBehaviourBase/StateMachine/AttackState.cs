using Core.Unit.StateMachine;

/// <summary>
/// State for when the unit is attacking
/// </summary>
public class AttackState : UnitStateCommon
{
    private IWarrior warrior;

    public AttackState(IWarrior unitWarrior)
    {
        warrior = unitWarrior;
    }

    public override void OnEnterState()
    {
        //PlayAnimation(unit.attackAnimation, false);
    }
}