using System;
using System.Linq;
using RootMotion.FinalIK;
using UnityEngine;
/// <summary>
/// Handles IK type events
/// </summary>

public class IkContainer : MonoBehaviour
{
    private AimIK aimIk;
    private FullBodyBipedIK fbbik;
    private GrounderFBBIK grounderFbbik;

    private void OnEnable()
    {
        SetIkComponents();
        foreach (var value in (IkType[]) Enum.GetValues(typeof(IkType))) SetIkWeights(1, value);
    }

    public void SetIkActive(bool isActive, IkType ikType)
    {
        switch (ikType)
        {
            case IkType.FBBIK:
                fbbik.enabled = isActive;
                break;
            case IkType.AimIk:
                aimIk.enabled = isActive;
                break;
            case IkType.GrounderIk:
                grounderFbbik.enabled = isActive;
                break;
        }
    }

    private void SetIkComponents()
    {
        if (fbbik == null) fbbik = GetComponentInChildren<FullBodyBipedIK>();
        if (aimIk == null) aimIk = GetComponentInChildren<AimIK>();
        if (grounderFbbik == null) grounderFbbik = GetComponentInChildren<GrounderFBBIK>();
    }

    public void SetAimTransform(Transform aimTransform)
    {
        if (aimIk == null)
            SetIkComponents();
        aimIk.solver.transform = aimTransform;
    }

    public void SetAimTarget(Transform target)
    {
        aimIk.solver.target = target;
    }

    public void SetIkWeights(int amount, IkType ikType)
    {
        switch (ikType)
        {
            case IkType.FBBIK:
                fbbik.solver.IKPositionWeight = amount;
                break;
            case IkType.AimIk:
                aimIk.solver.IKPositionWeight = amount;
                break;
            case IkType.GrounderIk:
                grounderFbbik.weight = amount;
                break;
            default:
                return;
        }
    }

    public void ConfigureWeaponIk(WeaponModelCommon currentWeaponModel)
    {
        SetIkActive(true, IkType.FBBIK);
        foreach (var weaponModelPosition in currentWeaponModel.weaponModelPositions.Where(weaponModelPosition =>
                     weaponModelPosition.isIkActive))
            switch (weaponModelPosition.weaponPositionType)
            {
                case WeaponPositionType.RightGrip:
                    SetFbbikTarget(weaponModelPosition.position, FbbikEffectorTypes.RightHand, 1, 1);
                    break;
                case WeaponPositionType.LeftGrip:
                    SetFbbikTarget(weaponModelPosition.position, FbbikEffectorTypes.LeftHand, 1, 1);
                    break;
                case WeaponPositionType.FirePoint:
                    SetAimTransform(weaponModelPosition.position);
                    break;
                case WeaponPositionType.ReloadPoint:
                    break;
                case WeaponPositionType.StockPoint:
                    break;
                case WeaponPositionType.AimSightPoint:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        SetIkWeights(0, IkType.AimIk);
    }

    private void SetFbbikTarget(Transform position, FbbikEffectorTypes effectorTypes, float posWeight, float rotWeight)
    {
        switch (effectorTypes)
        {
            case FbbikEffectorTypes.Body:
                fbbik.solver.bodyEffector.target = position;
                fbbik.solver.bodyEffector.positionWeight = posWeight;
                fbbik.solver.bodyEffector.rotationWeight = rotWeight;
                fbbik.solver.bodyEffector.maintainRelativePositionWeight = 1;
                break;
            case FbbikEffectorTypes.LeftHand:
                fbbik.solver.leftHandEffector.target = position;
                fbbik.solver.leftHandEffector.positionWeight = posWeight;
                fbbik.solver.leftHandEffector.rotationWeight = rotWeight;
                fbbik.solver.leftHandEffector.maintainRelativePositionWeight = 1;
                break;
            case FbbikEffectorTypes.RightHand:
                fbbik.solver.rightHandEffector.target = position;
                fbbik.solver.rightHandEffector.positionWeight = posWeight;
                fbbik.solver.rightHandEffector.rotationWeight = rotWeight;
                fbbik.solver.rightHandEffector.maintainRelativePositionWeight = 1;
                break;
            case FbbikEffectorTypes.LeftFoot:
                fbbik.solver.leftFootEffector.target = position;
                fbbik.solver.leftFootEffector.positionWeight = posWeight;
                fbbik.solver.leftFootEffector.rotationWeight = rotWeight;
                fbbik.solver.leftFootEffector.maintainRelativePositionWeight = 1;
                break;
            case FbbikEffectorTypes.RightFoot:
                fbbik.solver.rightFootEffector.target = position;
                fbbik.solver.rightFootEffector.positionWeight = posWeight;
                fbbik.solver.rightFootEffector.rotationWeight = rotWeight;
                fbbik.solver.rightFootEffector.maintainRelativePositionWeight = 1;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(effectorTypes), effectorTypes, null);
        }
    }
}

public enum IkType
{
    FBBIK,
    AimIk,
    GrounderIk
}

public enum FbbikEffectorTypes
{
    Body,
    LeftHand,
    RightHand,
    LeftFoot,
    RightFoot
}