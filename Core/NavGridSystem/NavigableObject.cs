using System;
using System.Collections.Generic;
using System.Linq;
using Core.Data;
using Core.GameManagement;
using Core.Interfaces;
using UnityEngine;
using EventSenderController = Core.GameManagement.EventSenders.EventSenderController;

/// <summary>
/// Instance of an object the represents a movable location.
/// </summary>
[Serializable]
public abstract class NavigableObject : MonoBehaviour, INavigable, IDataPersistable
{
    public int navIndex;
    public Vector3 navPosition;
    public SphereCollider sphereCollider;
    public NavigableTypes navType;

    public bool isOccupied;
    public bool canBeHighlighted = true;
    public bool isTraversable = true;
    public HashSet<NavCell> linkedCells = new();
    public HashSet<NavGrapplePoint> linkedGrapplePoints = new();
    public HashSet<NavLedge> linkedLedges = new();
    public HashSet<INavigable> linkedNavigables = new();
    public RaycastHit hit;
    protected IMovable occupiedMovable;

    public virtual void OnEnable()
    {
        RegisterEvents();
    }

    public virtual void OnDisable()
    {
        UnRegisterEvents();
    }

    public NavNodeCanvas NavNodeCanvas { get; set; }


    public void SetCanvasActive(bool status)
    {
        if (DebugController.instance.isDebugEnabled) NavNodeCanvas.gameObject.SetActive(status);
    }

    public virtual void RegisterNavigable()
    {
        //gridPathFinder.RegisterNavigable(this);
    }

    public int GetNavIndex()
    {
        return navIndex;
    }

    public Vector3 GetPosition()
    {
        return navPosition;
    }

    public bool IsTraversable()
    {
        return isTraversable;
    }

    public virtual Collider GetCollider()
    {
        return sphereCollider;
    }
    public virtual void AddNeighbor(INavigable navigable)
    {
        switch (navigable.GetNavType())
        {
            case NavigableTypes.NavCell:
                if (navigable is NavCell navCell)
                    linkedCells.Add(navCell);
                break;
            case NavigableTypes.Ledge:
                if (navigable is NavLedge navLedge)
                    linkedLedges.Add(navLedge);
                break;
            case NavigableTypes.GrapplePoint:
                if (navigable is NavGrapplePoint navGrapple)
                    linkedGrapplePoints.Add(navGrapple);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (!linkedNavigables.Contains(navigable))
            linkedNavigables.Add(navigable);
    }

    public HashSet<INavigable> GetLinkedNavigables()
    {
        return linkedNavigables;
    }

    public virtual void SetLinkedNavigables()
    {
        foreach (var linkedNav in linkedCells.Where(linkedNav => !linkedNavigables.Contains(linkedNav)))
            linkedNavigables.Add(linkedNav);

        foreach (var linkedNav in linkedLedges.Where(linkedNav => !linkedNavigables.Contains(linkedNav)))
            linkedNavigables.Add(linkedNav);

        foreach (var linkedNav in linkedGrapplePoints.Where(linkedNav => !linkedNavigables.Contains(linkedNav)))
            linkedNavigables.Add(linkedNav);
    }

    public virtual NavigableTypes GetNavType()
    {
        return navType;
    }

    public virtual bool IsOccupied()
    {
        return occupiedMovable != null;
    }

    public virtual void SetOccupiedMovable(IMovable movable)
    {
        occupiedMovable = movable;
        if (occupiedMovable != null)
            isOccupied = true;
    }

    public virtual IMovable GetMovable()
    {
        return occupiedMovable;
    }

    public virtual void NavlinksEstablished()
    {
        foreach (var navCell in linkedCells)
        {
        }

        foreach (var navLedge in linkedLedges)
        {
        }

        foreach (var navGrapplePoint in linkedGrapplePoints)
        {
        }
    }

    public virtual void ActivateMovableEffect()
    {
    }

    public IMovable GetOccupiedMovable()
    {
        return occupiedMovable;
    }

    public LayersEnum GetLayer()
    {
        return (LayersEnum) gameObject.layer;
    }

    protected virtual void RegisterEvents()
    {
        EventSenderController.gridConfigured += OnGridConfigured;
        EventSenderController.onNavEstablished += NavlinksEstablished;
        EventSenderController.onSave += OnSave;
        EventSenderController.onLoad += OnLoad;
    }

    protected virtual void UnRegisterEvents()
    {
        EventSenderController.gridConfigured -= OnGridConfigured;
        EventSenderController.onNavEstablished -= NavlinksEstablished;
        EventSenderController.onSave -= OnSave;
        EventSenderController.onLoad -= OnLoad;
    }

    private void OnGridConfigured()
    {
        RegisterNavigable();
    }

    public void OnSave(GameData gameData)
    {

    }

    public void OnLoad(GameData gameData)
    {

    }
}