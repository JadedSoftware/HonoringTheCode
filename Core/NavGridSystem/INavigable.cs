using System.Collections.Generic;
using Core.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Defines what makes an object navigable
/// </summary>
public interface INavigable : IRaycastable
{
    public NavNodeCanvas NavNodeCanvas { get; set; }
    public void SetCanvasActive(bool status);
    public void RegisterNavigable();
    public int GetNavIndex();
    public Vector3 GetPosition();
    public NavigableTypes GetNavType();
    public void AddNeighbor(INavigable navigable);
    public bool IsOccupied();
    public bool IsTraversable();
    public void SetOccupiedMovable(IMovable movable);
    public IMovable GetMovable();
    public void SetLinkedNavigables();
    public HashSet<INavigable> GetLinkedNavigables();
    public void NavlinksEstablished();
    public void ActivateMovableEffect();
    public IMovable GetOccupiedMovable();
}

public enum NavigableTypes
{
    NavCell,
    Ledge,
    GrapplePoint
}