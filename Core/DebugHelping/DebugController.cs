using System;
using System.Collections.Generic;
using System.Linq;
using Core.DebugHelping;
using Core.GameManagement;
using UnityEngine;
using EventSenderController = Core.GameManagement.EventSenders.EventSenderController;

/// <summary>
/// Handles various debug situations like showing the links between navigation objects
/// and shows the navIndex of navigation objects
/// </summary>
public partial class DebugController : MonoBehaviour
{
    private static DebugController _instance;
    public bool isDebugEnabled;
    public bool isDebugLedges;
    public bool isDebugCells;
    public bool isDebugGrapples;
    public bool isDebugIndex;

    public BearingDebugger bearingDebugger;

    [Range(1, 100)] public int nodeDebugRadius = 3;
    public NavNodeCanvas navNodeCanvas;
    public List<NavNodeCanvas> navNodeCanvases = new();
    private readonly List<BearingDebugger> bearingDebuggers = new();

    private bool hasStateChanged;
    private GridController gridController => GridController.instance;

    public static DebugController instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType(typeof(DebugController)) as DebugController;

            return _instance;
        }
        set => _instance = value;
    }

    private void Start()
    {
        hasStateChanged = isDebugIndex;
    }

    private void Update()
    {
        foreach (var debugger in bearingDebuggers)
            debugger.transform.LookAt(CameraController.instance.mainCamera.transform);
    }

    private void OnEnable()
    {
        RegisterEvents(true);
    }

    private void OnDisable()
    {
        RegisterEvents(false);
    }

    private void OnDrawGizmos()
    {
        if (!isDebugEnabled) return;

        foreach (var navCell in gridController.allNavCells)
        foreach (var grapple in navCell.linkedNavigables.Where(a => a.GetNavType() == NavigableTypes.GrapplePoint))
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(grapple.GetPosition(), new Vector3(.5f, .5f, .5f));
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(navCell.GetPosition(), grapple.GetPosition());
        }

        if (isDebugGrapples)
            foreach (var point in gridController.allGrapplePoints)
            foreach (var linkedNavigable in point.GetLinkedNavigables())
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(point.GetPosition(), linkedNavigable.GetPosition());
                if (linkedNavigable.GetNavType() == NavigableTypes.NavCell)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawCube(linkedNavigable.GetPosition(), new Vector3(.5f, .5f, .5f));
                }
            }

        if (isDebugCells)
        {
            foreach (var navCell in gridController.allNavCells.Where(a => a.offGridLink.Count > 0))
            foreach (var ledge in navCell.linkedLedges)
            foreach (var link in navCell.offGridLink)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(ledge.GetPosition(), link.GetPosition());
            }

            foreach (var navCell in gridController.allNavCells)
            foreach (var gridLink in navCell.offGridLink)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(navCell.GetPosition(), gridLink.GetPosition());
            }


            foreach (var point in gridController.allNavCells.Where(a => a.canBeHighlighted))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(point.GetPosition(), .2f);
            }
        }

        if (isDebugLedges)
            foreach (var point in gridController.allNavLedges)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(point.GetPosition(), .2f);

                Gizmos.color = Color.green;
                foreach (var linkedNavigable in point.linkedNavigables)
                    Gizmos.DrawLine(point.GetPosition(), linkedNavigable.GetPosition());
            }


        if (!isDebugCells || !isDebugLedges || !isDebugGrapples) return;
        foreach (var navigable in gridController.allNavigables)
        foreach (var neighbor in navigable.GetLinkedNavigables())
            switch (neighbor.GetNavType())
            {
                case NavigableTypes.NavCell:
                    if (neighbor is NavCell navCell)
                        if (navCell.IsTraversable())
                        {
                            Gizmos.color = Color.cyan;
                            Gizmos.DrawLine(navigable.GetPosition(), neighbor.GetPosition());
                        }

                    break;
                case NavigableTypes.Ledge:
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(navigable.GetPosition(), neighbor.GetPosition());
                    break;
                case NavigableTypes.GrapplePoint:
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(navigable.GetPosition(), neighbor.GetPosition());
                    break;
                default:
                    Debug.Log(neighbor.GetNavType());
                    throw new ArgumentOutOfRangeException();
            }
    }

    private void RegisterEvents(bool b)
    {
        switch (b)
        {
            case true:
                EventSenderController.onNavEstablished += NavLinksEstablished;
                break;
            case false:
                EventSenderController.onNavEstablished -= NavLinksEstablished;
                break;
        }
    }

    private void NavLinksEstablished()
    {
        foreach (var navigable in gridController.allNavigables)
        {
            var navCanvas = Instantiate(navNodeCanvas, navigable.GetPosition(), Quaternion.identity,
                gridController.transform);
            navCanvas.transform.SetParent(transform);
            navCanvas.SetNavText(navigable.GetNavIndex().ToString());
            switch (navigable.GetNavType())
            {
                case NavigableTypes.NavCell:
                    navCanvas.SetNavColor(Color.white);
                    if (navigable is NavCell navCell)
                    {
                        navCanvas.SetCellText(navCell.cell.index.ToString());
                        navCanvas.SetCellColor(Color.red);
                    }

                    break;
                case NavigableTypes.Ledge:
                    navCanvas.SetNavColor(Color.red);
                    break;
                case NavigableTypes.GrapplePoint:
                    navCanvas.SetNavColor(Color.magenta);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            navNodeCanvases.Add(navCanvas);
            navCanvas.gameObject.SetActive(false);
            navigable.NavNodeCanvas = navCanvas;
        }
    }

    public void DebugNavIndex()
    {
        var debugNavigables = gridController.allNavigables;
        var duplicates = debugNavigables.GroupBy(z => z.GetNavIndex()).Any(g => g.Count() > 1);
        Debug.Log(duplicates);
        var query
            = debugNavigables.GroupBy(x => x.GetNavIndex())
                .Where(g => g.Count() > 1);
        foreach (var nav in query) Debug.Log(nav.Key);
    }

    public void CreateBearingDebugger(Vector3 pos, string text, UnitCommon unit)
    {
        BearingDebugger bearingDebug;
        if (unit.bearingDebugger == null)
        {
            bearingDebug = Instantiate(bearingDebugger, pos, Quaternion.identity);
            bearingDebuggers.Add(bearingDebug);
            bearingDebug.transform.SetParent(transform);
            unit.bearingDebugger = bearingDebug;
        }
        else
        {
            bearingDebug = unit.bearingDebugger;
        }

        bearingDebug.SetText(text);
    }

    public void DrawQuads(List<BestTarget> list, Color color)
    {
        foreach (var key in list.Select(bestTarget => bestTarget.unit))
            instance.DrawDebug(DrawDebugTypes.Sphere, color, 10, false,
                key.motor.transform.position + key.motor.Capsule.center, 1, null);
    }

    public void DebugBearings(List<BestTarget> list)
    {
        foreach (var target in list)
            instance.CreateBearingDebugger(target.unit.motor.transform.position,
                target.bearing.ToString(), target.unit);
    }

    public void Log(string logMessage)
    {
        Debug.Log(logMessage);
    }
}

public enum DebugTypes
{
    Grid,
    NavObjects,
    NavLinks,
    NavCells,
    NavLedges,
    NavGrapples
}

[Serializable]
public struct DebugHelper
{
}