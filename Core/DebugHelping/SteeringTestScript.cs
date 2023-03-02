using TMPro;
using Unity.Mathematics;
using UnityEngine;

[ExecuteInEditMode]
public class SteeringTestScript : MonoBehaviour
{
    public int numberSteeringRays = 1;
    public float steeringRayRange = 2;
    [Range(1, 1000)] public int steeringXAngle;

    public Canvas textCanvas;
    public TextMeshProUGUI text;
    public float angle;
    private readonly float steeringRayAngle = 360;

    private void Update()
    {
        text.text = angle.ToString();
    }

    private void OnDrawGizmos()
    {
        for (var i = 0; i < numberSteeringRays; i++)
        {
            var rotation = transform.rotation;
            var rayAngle =
                Quaternion.AngleAxis(i / (float) numberSteeringRays * steeringRayAngle,
                    transform.up) * quaternion.Euler(steeringXAngle, 0, 0);
            var direction = rotation * rayAngle * transform.up;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, direction * steeringRayRange);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, Vector3.up);

            angle = Vector3.Angle(transform.up, direction);
        }
    }
}