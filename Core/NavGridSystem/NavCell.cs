using System;
using System.Collections.Generic;
using System.Linq;
using TGS;
using UnityEngine;

/// <summary>
/// Basic HexCell object
/// </summary>
public class NavCell : NavigableObject
{
    public bool isAngled;
    public bool isOverLedge;
    public bool isCenterCovered;
    public bool isNearObstacle;
    public bool hasVertCovered;
    public bool hasFloatingVert;
    public TerrainGridSystem gridSystem;
    public Cell cell;
    public HashSet<NavCell> offGridLink = new();
    public HashSet<GridCellVertex> verticesList = new();

    public HashSet<GridCellVertex> vertsOverLedge;

    public int GetCellIndex()
    {
        return cell.index;
    }

    public override void AddNeighbor(INavigable navigable)
    {
        switch (navigable.GetNavType())
        {
            case NavigableTypes.NavCell:
                if (navigable is NavCell navCell)
                {
                    if (navCell.cell.neighbours.Contains(navCell.cell)) return;
                    if (!offGridLink.Contains(navCell))
                        offGridLink.Add(navCell);
                }

                break;
            case NavigableTypes.Ledge:
                if (navigable is NavLedge navLedge)
                    linkedLedges.Add(navLedge);
                break;
            case NavigableTypes.GrapplePoint:
                if (navigable is NavGrapplePoint navGrapple)
                    if (!linkedGrapplePoints.Contains(navGrapple))
                        linkedGrapplePoints.Add(navGrapple);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override void SetLinkedNavigables()
    {
        var cells = cell.neighbours;
        foreach (var navigable in cells.Select(gridCell => gridCell.navCell).Cast<INavigable>().ToList())
            linkedNavigables.Add(navigable);
        foreach (var navCell in offGridLink) linkedNavigables.Add(navCell);
        foreach (var linkedGrapplePoint in linkedGrapplePoints) linkedNavigables.Add(linkedGrapplePoint);
        foreach (var navLedge in GridController.instance.allNavLedges.Where(a => a.originVertex.navCell == this))
            linkedNavigables.Add(navLedge);
    }
}

namespace TGS
{
    public partial class Cell : AdminEntity
    {
        public NavCell navCell;
    }
}