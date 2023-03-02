using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main container for attack components,
/// Ranged and Melee attacks derive from this class
/// </summary>
public class WarriorAttack : ScriptableObject
{
    public string attackName;
    public AttackType attackType;
}
