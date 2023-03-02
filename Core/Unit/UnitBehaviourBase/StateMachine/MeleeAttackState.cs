using Core.Unit.StateMachine;

public class MeleeAttackState : UnitStateCommon
{
    private IWarrior warrior;

    public MeleeAttackState(IWarrior unitWarrior)
    {
        warrior = unitWarrior;
    }

    public override void OnEnterState()
    {
        //PlayAnimation(unit.attackAnimation, false);
    }
}
