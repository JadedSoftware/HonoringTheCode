using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holder for a weapon, stores the specific areas of a weapon like the firing point, the handle, and the reload spot.
/// </summary>
public abstract class WeaponModelCommon : MonoBehaviour
{
    [SerializeField] public List<WeaponPositions> weaponModelPositions;
}

[Serializable]
public struct WeaponPositions
{
    public Transform position;
    public WeaponPositionType weaponPositionType;
    public bool isIkActive;
}

public enum WeaponPositionType
{
    RightGrip,
    LeftGrip,
    FirePoint,
    ReloadPoint,
    StockPoint,
    AimSightPoint
}