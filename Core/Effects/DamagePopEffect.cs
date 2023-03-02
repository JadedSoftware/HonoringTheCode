using TMPro;
using UnityEngine;

namespace Core.Effects
{
    /// <summary>
    /// Shows text on damage
    /// </summary>
    public class DamagePopEffect : MonoBehaviour
    {
        public TextMeshPro textMeshPro;

        private void Update()
        {
            var moveYSpeed = 20;
            transform.position += new Vector3(0, moveYSpeed, 0) * Time.deltaTime;
        }

        private void OnEnable()
        {
            if (textMeshPro == null)
                textMeshPro = GetComponentInChildren<TextMeshPro>();
        }

        public void SetText(string text)
        {
            textMeshPro.text = text;
        }
    }
}