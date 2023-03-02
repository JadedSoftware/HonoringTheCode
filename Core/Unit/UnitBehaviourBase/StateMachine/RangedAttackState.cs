using Core.Unit.StateMachine;

public class RangedAttackState : UnitStateCommon
{
    private IWarrior warrior;

    public RangedAttackState(IWarrior unitWarrior)
    {
        warrior = unitWarrior;
    }

    public override void OnEnterState()
    {
        //PlayAnimation(unit.attackAnimation, false);
    }
}
