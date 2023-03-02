using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moveable spot on a cliff or something that isn't the terrain
/// </summary>
public class NavGrapplePoint : NavPoint
{
    public List<Vector3> grapplePointHits;
    public Vector3 rayStartPos;
    public Vector3 rayDir;
    public Vector3 hitNormal;
    public GrappleMarkerEffect grappleMarkerEffect;
    private Vector3 effectPos;

    public override void ActivateMovableEffect()
    {
        switch (CameraController.instance.cameraStateMachine.CurrentKey)
        {
            case CameraViewStates.Topdown:
                grappleMarkerEffect.GrappleMovableEffect();
                break;
            case CameraViewStates.Attack:
            case CameraViewStates.AttackOrbit:
            case CameraViewStates.EnemyTurn:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
     
    }
}