using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Helpers;
using UnityEngine;

namespace Core.Unit.Targeting
{
    /// <summary>
    /// Container for the various health bars and text when targeting a body part (TargetingObject)
    /// </summary>
    public class TargetingHealthUI : MonoBehaviour
    {
        public List<TargetingHealthBar> healthBars = new();
        public TargetingObject currentTargetingObject;
        private TargetingHealthBar activeHealthBar;
        public IDamageable damageable;
        private bool isActive;

        public void Create()
        {
            healthBars = GetComponentsInChildren<TargetingHealthBar>().ToList();
            foreach (var healthBar in healthBars)
                healthBar.textMesh.text =
                    TargetHelpers.TargetTypeName(healthBar.targetingObjectType, TargetFormattingTypes.Replace);
        }

        public void DisableTargetingUI()
        {
            isActive = false;
            gameObject.SetActive(false);
        }

        public void EnableTargetingUi()
        {
            gameObject.SetActive(true);
            transform.position = damageable.GetPosition();
            isActive = true;
            StartCoroutine(LookAtCamera());
        }

        private IEnumerator LookAtCamera()
        {
            while (isActive)
            {
                transform.LookAt(CameraController.instance.currentCinemachine.transform);
                yield return new WaitForEndOfFrame();
            }
        }

        public void ActivateTargetingHealthbar(TargetingObject targetingObject)
        {
            activeHealthBar = healthBars.Find(x => x.targetingObjectType == targetingObject.targetObjectType);
            activeHealthBar.Activate();
        }

        public void ChangeTargetingHealthbar(TargetingObject targetingObject)
        {
            activeHealthBar.Deactivate();
            activeHealthBar = healthBars.Find(x => x.targetingObjectType == targetingObject.targetObjectType);
            activeHealthBar.Activate();
        }

        public void DeactivateHealthBar()
        {
            activeHealthBar.Deactivate();
            activeHealthBar = null;
        }
    }
}