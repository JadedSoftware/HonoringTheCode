using UnityEngine;
/// <summary>
/// Data for the different Ranged weapons
/// </summary>
[CreateAssetMenu(fileName = "WeaponObjects", menuName = "WeaponScriptableObjects/RangedWeapon", order = 1)]
public class WarriorRangedWeapon : WarriorWeaponSO
{
    public int currentClipCapacity;
    public WarriorAmmoScriptable ammo;
    public float range;
}