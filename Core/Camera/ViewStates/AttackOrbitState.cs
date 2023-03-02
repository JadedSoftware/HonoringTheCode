using System.Collections;
using Cinemachine;
using Core.GameManagement;
using Core.Unit.Targeting;
using UnityEngine;
using EventSenderController = Core.GameManagement.EventSenders.EventSenderController;

namespace Core.Camera
{
    public class AttackOrbitState : CameraViewStateCommon
    {
        private CinemachineOrbitalTransposer camTransposer;
        protected override void Awake()
        {
            base.Awake();
            camTransposer = virtualCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();
        }

        public override void OnEnterState()
        {
            var selectedUnit = GameManagementController.instance.GetCurrentUnit();
            //freeLook.transform.position = selectedUnit.AttackCamTransform().position;
            virtualCamera.Follow = camControl.aimCamObj.transform;
            virtualCamera.LookAt = selectedUnit.AttackCamTransform();
            virtualCamera.transform.position = selectedUnit.AttackCamTransform().position;
            camTransposer.m_FollowOffset.y = 0;
            base.OnEnterState();
            viewEnabled = true;
            StartCoroutine(RayForTarget());
        }

        public override void OnExitState()
        {
            viewEnabled = false;
            base.OnExitState();
        }
        public void OffsetY(float i)
        {
            camTransposer.m_FollowOffset.y = i;
        }
        public TargetingObject PassTargetingObject()
        {
            var targetingObject = currentTargetingObject;
            EventSenderController.DisengageTargetingObject();
            currentTargetingObject = null;
            return targetingObject;
        }
    }
}