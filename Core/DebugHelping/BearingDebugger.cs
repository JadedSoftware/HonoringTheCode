using TMPro;
using UnityEngine;

namespace Core.DebugHelping
{
    /// <summary>
    /// debug helper for determining angle between player and enemy
    /// </summary>
    public class BearingDebugger : MonoBehaviour
    {
        public TextMeshProUGUI textMesh;

        public void SetText(string newText)
        {
            textMesh.text = newText;
        }
    }
}