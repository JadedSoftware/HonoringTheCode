using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data container for the locations of where weapons are holstered and active
/// </summary>
public class ModelWeaponSlots : MonoBehaviour
{
    [SerializeField] public List<WeaponRefPosition> weaponRefPositions;
}

[Serializable]
public struct WeaponRefPosition
{
    public WeaponType weaponType;
    [SerializeField] public List<WeaponPosType> actionRefPos;
    [SerializeField] public List<WeaponPosType> idleRefPos;
}

[Serializable]
public struct WeaponPosType
{
    public RefPosType refPosType;
    public Transform refPos;
}

public enum RefPosType
{
    RightHandAction,
    LeftHandAction,
    BackIdleHolster,
    LeftHipHolster,
    RightHipHolster
}