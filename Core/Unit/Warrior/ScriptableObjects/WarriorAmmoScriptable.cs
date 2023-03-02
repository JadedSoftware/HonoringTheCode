using UnityEngine;
/// <summary>
/// Data for the different ammo types of weapons
/// </summary>
[CreateAssetMenu(fileName = "AmmoObjects", menuName = "WeaponScriptableObjects/Ammo", order = 2)]
public class WarriorAmmoScriptable : ScriptableObject
{
    public GameObject prefab;
    public AmmoType ammoType;
    public int maxClipCapactiy;
    public int bulletPerShot;
}