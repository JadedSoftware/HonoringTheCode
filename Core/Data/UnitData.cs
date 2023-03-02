using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Data
{
    /// <summary>
    /// An object for storing unit save data
    /// </summary>
    [Serializable]
    public class UnitData
    {
        public string guid;
        public string unitName;
        public float health;
        public int navIndex;
        public Vector3 pos;
        public List<WarriorWeaponSO> weapons;
        public WarriorWeaponSO currentWeapon;
        public WarriorAmmoScriptable currentAmmo;

        public UnitData(string guid, string unitName, float health, int navIndex, Vector3 pos, List<WarriorWeaponSO> weapons, WarriorWeaponSO currentWeapon, WarriorAmmoScriptable currentAmmo)
        {
            this.guid = guid;
            this.unitName = unitName;
            this.health = health;
            this.navIndex = navIndex;
            this.pos = pos;
            this.weapons = weapons;
            this.currentWeapon = currentWeapon;
            this.currentAmmo = currentAmmo;
        }
        
    }
}