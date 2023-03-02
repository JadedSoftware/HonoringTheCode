using System;
using System.Collections;
using Core.Camera;
using Core.GameManagement.EventSenders;
using Core.Interfaces;
using UnityEngine;

namespace Core.GameManagement
{
    /// <summary>
    /// Handles mouse to screen raycast to determine if a hex cell should be highlighted
    /// </summary>
    public class RaycastController : MonoSingleton<RaycastController>
    {
        public IRaycastable lastHit { get; set; }

        public override void Init()
        {
            RegisterEvents();
        }


        private void OnDisable()
        {
            UnRegisterEvents();
        }
        private void RegisterEvents()
        {
            EventSenderController.damagableUnitEnter += EnterDamageable;
            EventSenderController.damagableUnitExit += ExitDamageable;
        }


        private void UnRegisterEvents()
        {

            EventSenderController.damagableUnitEnter -= EnterDamageable;
            EventSenderController.damagableUnitExit -= ExitDamageable;
        }
        private void Update()
        {
            GameRaycast();
        }
        
        private void GameRaycast()
        {
            var ray = CameraController.instance.GetMouseRay();
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 1000,
                        LayerMaskHelper.navigablesLayerMask + LayerMaskHelper.levelLayerMask))
                {
                    //todo other navigable types
                    var layer = (LayersEnum) hit.collider.gameObject.layer;
                    if (lastHit != null)
                        if (layer != lastHit.GetLayer())
                        {
                            if (lastHit is INavigable navigable)
                            {
                                EventSenderController.MouseExitNavigable(navigable);
                                lastHit = null;
                            }

                            if (lastHit is UnitCommon)
                            {
                                lastHit = null;
                            }
                        }
                    switch (layer)
                    {
                        case LayersEnum.Terrain:
                            break;
                        case LayersEnum.NavCell:
                            break;
                        case LayersEnum.Obstacle:
                            break;
                        case LayersEnum.Unit:
                            if (CameraOverlayController.instance.currentOverlayState == CameraOverlayStates.Hookshot)
                                HookshotUnit(hit);
                            break;
                        case LayersEnum.Ledge:
                            if (CameraOverlayController.instance.currentOverlayState == CameraOverlayStates.Hookshot)
                                HookshotNavpoint(hit);
                            break;
                        case LayersEnum.GrapplePoint:
                            if (CameraOverlayController.instance.currentOverlayState == CameraOverlayStates.Hookshot)
                                HookshotNavpoint(hit);
                            else
                                MovementNavPoint(hit);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
        }

        private void HookshotUnit(RaycastHit hit)
        {
            var unit = hit.collider.gameObject.GetComponent<UnitCommon>();
            if (unit != null)
            {
                if((UnitCommon) lastHit != unit)
                    lastHit = unit;
            }
        }

        private void HookshotNavpoint(RaycastHit hit)
        {
            var navPoint = hit.collider.gameObject.GetComponent<NavPoint>();
            if (navPoint != null)
            {
                if ((NavPoint) lastHit != navPoint)
                {
                    if (lastHit is INavigable navigable)
                    {
                        EventSenderController.MouseExitNavigable(navigable);
                    }
                    EventSenderController.MouseOverNavigable(navPoint);
                }
                lastHit = navPoint;
            }
        }

        private void MovementNavPoint(RaycastHit hit)
        {
            var grapplePoint = hit.collider.gameObject.GetComponent<INavigable>();
            if (grapplePoint != null)
            {
                if (lastHit != grapplePoint)
                {
                    if (lastHit is INavigable navigable)
                    {
                        EventSenderController.MouseExitNavigable(navigable);
                    }

                    EventSenderController.MouseOverNavigable(grapplePoint);
                    lastHit = grapplePoint;
                }
            }
            else
            {
                if (lastHit is INavigable navigable)
                {
                    EventSenderController.MouseExitNavigable(navigable);
                }
                lastHit = null;
            }
        }
        
        private void EnterDamageable(UnitCommon unit)
        { 
           //lastHit = unit;
        }

        private void ExitDamageable(UnitCommon unit)
        {
            //lastHit = null;
        }
    }
}