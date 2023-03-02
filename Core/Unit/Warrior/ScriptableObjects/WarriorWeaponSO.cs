using Core.GameManagement.Interfaces;
using UnityEngine;
/// <summary>
/// Main container for weapon components
/// Ranged and Melee weapons derive from this class
/// </summary>
public class WarriorWeaponSO : ScriptableObject, IAction
{
    // Start is called before the first frame update
    public string weaponName;
    public GameObject prefab;
    public AttackType attackType;
    public WeaponType weaponType;
    public int damage;
    public int actionPointsConsumed;
    public int actionPointCost => actionPointsConsumed;

    public int SwapCost;

    public ActionTypes actionType => ActionTypes.Attack;
    public void OnExecute()
    {
        
    }
} 