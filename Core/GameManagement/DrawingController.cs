using UnityEngine;

namespace Core.GameManagement
{
    public static class DrawingController 
    {
        public static void DrawRay(Vector3 startPosition, Vector3 endPosition, Color getColor, float duration)
        {
            var ray = new Ray(startPosition, endPosition - startPosition);
            DebugController.instance.DrawDebug(DrawDebugTypes.Ray, getColor, duration, true, null, Vector3.Distance(startPosition, endPosition), ray);
        }
    }
}