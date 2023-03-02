using System.Collections;
using System.Collections.Generic;
using Core.GameManagement;
using Core.Unit;
using Core.Unit.Targeting;
using Unity.VisualScripting;
using UnityEngine;
using EventSenderController = Core.GameManagement.EventSenders.EventSenderController;

namespace Core.Camera
{
    public class AttackCameraViewState : CameraViewStateCommon
    {
        protected override void Awake()
        {
            base.Awake();
            attackLookObject = new GameObject
            {
                transform =
                {
                    parent = camControl.transform,
                    position = transform.position
                },
                name = "AttackViewObject"
            };
            unitTargetingObjects = new List<TargetingObject>();
            virtualCamera.LookAt = attackLookObject.transform;
        }

        private void Start()
        {
            EventSenderController.unitSelected += UnitSelected;
        }

        private void OnDestroy()
        {
            EventSenderController.unitSelected -= UnitSelected;
        }

        private void UnitSelected(ISelectable unit)
        {
            virtualCamera.transform.position = unit.GetPosition();
        }

        public override void OnEnterState()
        {
            isChangeingTarget = true;
            UnitCommon bestPossibleEnemy;
            var selectedUnit = GameManagementController.instance.GetCurrentUnit();
            virtualCamera.transform.position = selectedUnit.AttackCamTransform().position;
            virtualCamera.Follow = selectedUnit.AttackCamTransform();
            viewState = CameraViewStates.Attack;
            UnitsInView = new List<UnitCommon>();
            viewEnabled = true;
            if (camControl.cameraStateMachine.PreviousKey == CameraViewStates.AttackOrbit)
            {
                var passedTargetingObject = camControl.attackOrbitState.PassTargetingObject();
                currentTargetingObject = passedTargetingObject;
                bestPossibleEnemy = passedTargetingObject.damageable.GetUnit();
            }
            else
            {
                bestPossibleEnemy = TargetingController.instance.AttackViewTarget(selectedUnit);
                attackLookObject.transform.position =
                    bestPossibleEnemy.motor.transform.position + bestPossibleEnemy.motor.Capsule.center;
            }
            StartCoroutine(LookAtEnemy(selectedUnit, bestPossibleEnemy));
            StartCoroutine(RayForTarget());
            base.OnEnterState();
        }

        public override void OnExitState()
        {
            currentTargetingObject = null;
            if (camControl.cameraStateMachine.NextKey != CameraViewStates.AttackOrbit)
            {
                viewEnabled = false;
                EventSenderController.ExitAttackView();
                EventSenderController.DisengageTargetingObject();
                UnitsInView.Clear();
            }

            StopAllCoroutines();
            base.OnExitState();
        }
    }
}