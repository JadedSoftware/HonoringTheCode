using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Unit.Targeting
{
    /// <summary>
    /// Container for the health bar that is shown when a body part (TargetingObject) is targeted
    /// </summary>
    [Serializable]
    public class TargetingHealthBar : MonoBehaviour
    {
        public TargetingObjectType targetingObjectType;
        public TargetingObject targetingObject;
        [HideInInspector] public Image healthBarImage;
        [HideInInspector] public TextMeshProUGUI textMesh;
        private LineRenderer lineRenderer;

        private void OnEnable()
        {
            healthBarImage = GetComponentInChildren<Image>();
            healthBarImage.rectTransform.sizeDelta = new Vector2(.5f, .06f);
            textMesh = GetComponent<TextMeshProUGUI>();
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.alignment = TextAlignmentOptions.Midline;
            textMesh.enableWordWrapping = false;
            lineRenderer = GetComponentInChildren<LineRenderer>();
            ConfigureLineRender();
            ChangeUiAlpha(.05f);
        }

        private void ConfigureLineRender()
        {
            if (lineRenderer == null) return;
            var children = lineRenderer.transform.Cast<Transform>().Where(child => null != child).ToList();
            lineRenderer.positionCount = children.Count;
            for (var i = 0; i < children.Count; i++) lineRenderer.SetPosition(i, children[i].position);
        }

        private void ChangeUiAlpha(float alphaValue)
        {
            textMesh.alpha = alphaValue;
            healthBarImage.color = new Color(healthBarImage.color.r, healthBarImage.color.g, healthBarImage.color.b,
                alphaValue);
        }

        public void Activate()
        {
            ChangeUiAlpha(1);
        }

        public void Deactivate()
        {
            ChangeUiAlpha(.05f);
        }
    }
}