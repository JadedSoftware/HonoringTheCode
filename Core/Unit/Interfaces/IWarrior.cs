using Core.Unit.Warrior;

/// <summary>
/// When an object can attack
/// </summary>
public interface IWarrior
{
    WarriorWeaponSO GetCurrentWeapon();
    void OnAttackInitiated(AttackAction attackAction);
    void OnAttackPerformed(AttackAction attackAction);
    void OnAttackCompleted(AttackAction attackAction);
    void RegisterWarrior(IWarrior warrior, bool isActive);
    void UnRegisterWarriorable(IWarrior warrior, bool isActive);

    void WeaponSwap(WarriorWeaponSO newWeapon);

    int AttackActionCost();
    public int currentActionPoints { get; set; }
}

public enum AttackType
{
    Range,
    Melee,
    Special
}

public enum WeaponType
{
    Rifle,
    Sword,
    Claws,
    Psychic
}

public enum AmmoType
{
    Normal,
    ArmorPiercing
}