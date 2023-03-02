
using Core.Effects;
using Core.GameManagement.Interfaces;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// An movable object that isn't a hex cell -- Ledges and Grapplepoints.
/// </summary>
public abstract class NavPoint : NavigableObject, IHookshotable
{
    public GridCellVertex originVertex;
    public HookshotEffect hookshotEffect { get; set; }
    public Vector3 GetHookshotPosition() => GetPosition();
}
