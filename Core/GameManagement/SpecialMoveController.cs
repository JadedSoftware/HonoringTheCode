using System;
using System.Collections.Generic;
using System.Linq;
using Core.Camera;
using Core.GameManagement.EventSenders;
using Core.GameManagement.Interfaces;
using Core.Unit.Specials;
using UnityEngine;

namespace Core.GameManagement
{
    public class SpecialMoveController : MonoSingleton<SpecialMoveController>
    {
        public List<IHookshotable> hookShotTargets;
        private Queue<ActionContainer> cachedActions;
        private IHookshotable activeHookshotable;
        private int chainCount;
        public override void Init()
        {
            hookShotTargets = new();
            cachedActions = new();
            chainCount = 0;
            RegisterEvents();
        }

        private void OnDisable()
        {
            UnregisterEvents();
        }
        private void RegisterEvents()
        {
            EventSenderController.hookshotsAvailable += HookshotsAvailable;
            EventSenderController.hookshotActive += HookshotActive;
            EventSenderController.hookshotDeactive += HookshotDeactive;
            EventSenderController.onOverlayChanged += OverlayChanged;
            EventSenderController.onHookshotRequested += HookshotRequested;
            EventSenderController.onHookshotCompleted += HookshotCompleted;
        }
        
        private void UnregisterEvents()
        {
            EventSenderController.hookshotsAvailable -= HookshotsAvailable;
            EventSenderController.hookshotActive -= HookshotActive;
            EventSenderController.hookshotDeactive -= HookshotDeactive;
            EventSenderController.onOverlayChanged -= OverlayChanged;
            EventSenderController.onHookshotRequested -= HookshotRequested;
            EventSenderController.onHookshotCompleted -= HookshotCompleted;
        }
        private void OverlayChanged(CameraOverlayStates overlayState)
        {
            if (overlayState != CameraOverlayStates.Hookshot)
            {
                ClearHookshots();
                ClearCachedActions();
            }
        }

        private void ClearHookshots()
        {
            foreach (var hookshotable in hookShotTargets)
            {
                hookshotable.hookshotEffect.DisengageEffect();
            }
            hookShotTargets.Clear();
        }


        private void HookshotsAvailable(List<IHookshotable> hookshots, Vector3 startPos)
        {
            foreach (var hookshot in hookshots)
            {
                AddHookshotPoint(hookshot, startPos);
            }
        }
        
        
        private void HookshotActive(IHookshotable hookshotable)
        {
            activeHookshotable = hookshotable;
        }

        private void HookshotDeactive()
        {
            activeHookshotable = null;
        }

        private void AddHookshotPoint(IHookshotable hookshotPoint, Vector3 startPos)
        {
            hookShotTargets.Add(hookshotPoint);
            EffectsController.instance.HighlightHookshotPoint(hookshotPoint, startPos);
        }
        
        private void HookshotRequested()
        {
           
            if (activeHookshotable != null)
            {
                var selectedUnit = GameManagementController.instance.GetCurrentUnit();
                var hookshotData =
                    selectedUnit.specialMovesList.FirstOrDefault(x => x.specialType == SpecialTypes.Hookshot);
                if (UnitCommonController.instance.CheckActionPoints(selectedUnit, hookshotData))
                {
                    var hookshot = hookshotData as SpecialHookshot;
                    var hookshotAction = new HookshotAction(selectedUnit, activeHookshotable, hookshot);
                    if (hookshotData.chainableTimes > 0 && chainCount < hookshotData.chainableTimes)
                    {  
                        CacheAction(hookshotAction);
                        ClearHookshots();
                        var startPos = activeHookshotable.GetHookshotPosition() + Vector3.up * 3;
                        var hookshots =
                            NavigableController.instance.AvailableHookshots(startPos, hookshot);
                        EventSenderController.HookshotTargetsAvailable(hookshots, 
                            startPos);
                        HookshotDeactive();
                        return;
                    }
                    if (cachedActions.Count > 0)
                    {
                        chainCount--;
                        cachedActions.Enqueue(hookshotAction);
                        hookshotAction = cachedActions.Dequeue() as HookshotAction;
                    }
                    EventSenderController.HookshotPerformed(hookshotAction);
                }
            }
        }
        
        private void HookshotCompleted(HookshotAction hookshotaction)
        {
            switch (cachedActions.Count)
            {
                case > 0:
                {
                    var hookshotAction = cachedActions.Dequeue() as HookshotAction;
                    EventSenderController.HookshotPerformed(hookshotAction);
                    chainCount--;
                    break;
                }
                case 0:
                    EventSenderController.OverlayChanged(CameraOverlayStates.Movement);
                    break;
            }
        }

        private void CacheAction(ActionContainer action)
        {
            chainCount++;
            cachedActions.Enqueue(action);
        }
        
        private void ClearCachedActions()
        {
            chainCount = 0;
            cachedActions.Clear();
        }
    }
}