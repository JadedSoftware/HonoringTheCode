using Core;
using UnityEngine;

/// <summary>
/// Keeps the scene hierarchy from getting cluttered with navigation objects.
/// </summary>
public class GridPointController : MonoSingleton<GridPointController>
{
    public GameObject offGridHolder;
    public GameObject navCellHolder;
    public GameObject obstacleLedgeHolder;
    public GameObject terrainLedgeHolder;
    public GameObject navGrappleHolder;
}