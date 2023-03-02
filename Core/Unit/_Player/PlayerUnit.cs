using System.Linq;
using Core.Data;
using Core.GameManagement.EventSenders;
using Core.UI;
using UnityEngine;

/// <summary>
/// Implementation of UnitCommon for the player units
/// </summary>
public class PlayerUnit : UnitCommon
{
    [HideInInspector] public UnitHudElement hudElement;

    public override void OnEnable()
    {
        actionType = ActionTurnType.Player;
        selectableType = SelectableTypes.Player;
        
        base.OnEnable();

        weaponObjects.AddRange(Resources.LoadAll<WarriorWeaponSO>("Weapons"));
    }
}