using System;
using Drawing;
using Unity.Mathematics;
using UnityEngine;

public enum DrawDebugTypes
{
    Sphere,
    Ray,
    Cylinder
}
/// <summary>
/// helper class to draw lines and spheres in the game during play, or the scene.
/// </summary>

public partial class DebugController : MonoBehaviour
{
    public void DrawDebug(DrawDebugTypes types, Color color, float duration, bool isInGame, float3? pos,
        float? radius_length,
        Ray? ray)
    {
        var drawRadius = radius_length ?? 1;
        if (isInGame)
            DrawInGame(types, color, duration, pos, drawRadius, ray);
        else
            DrawInScene(types, color, duration, pos, drawRadius, ray);
    }

    private void DrawInScene(DrawDebugTypes types, Color color, float duration, float3? pos, float radius, Ray? ray)
    {
        using (Draw.WithDuration(duration))
        {
            using (Draw.WithColor(color))
            {
                switch (types)
                {
                    case DrawDebugTypes.Sphere:

                        if (pos != null)
                        {
                            var drawPos = (float3) pos;
                            Draw.WireSphere(drawPos, radius);
                        }

                        break;
                    case DrawDebugTypes.Ray:
                        if (ray != null)
                        {
                            var drawRay = (Ray) ray;
                            Draw.Ray(drawRay, radius);
                        }

                        break;
                    case DrawDebugTypes.Cylinder:
                        if (pos != null)
                        {
                            var drawPos = (float3) pos;
                            Draw.WireCylinder(drawPos, Vector3.up, 2, radius, color);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(types), types, null);
                }
            }
        }
    }

    private void DrawInGame(DrawDebugTypes types, Color color, float duration, float3? pos, float radius, Ray? ray)
    {
        using (Draw.ingame.WithDuration(duration))
        {
            using (Draw.ingame.WithColor(color))
            {
                switch (types)
                {
                    case DrawDebugTypes.Sphere:
                        if (pos != null)
                        {
                            var drawPos = (float3) pos;
                            Draw.ingame.WireSphere(drawPos, radius);
                        }

                        break;
                    case DrawDebugTypes.Ray:
                        if (ray != null)
                        {
                            var drawRay = (Ray) ray;
                            Draw.ingame.Ray(drawRay, radius);
                        }

                        break;
                    case DrawDebugTypes.Cylinder:
                        if (pos != null)
                        {
                            var drawPos = (float3) pos;
                            Draw.ingame.WireCylinder(drawPos, Vector3.up, 2, radius, color);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(types), types, null);
                }
            }
        }
    }

    public void DrawTargetCenter(Vector3 targetCenter)
    {
        if (isDebugEnabled)
            DrawDebug(DrawDebugTypes.Sphere, Color.blue, 5f, true, targetCenter, .5f, null);
    }
}