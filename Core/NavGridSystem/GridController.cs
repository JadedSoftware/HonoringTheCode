using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.GameManagement;
using TGS;
using Unity.VisualScripting;
using UnityEngine;
using EventSenderController = Core.GameManagement.EventSenders.EventSenderController;

/// <summary>
/// Handles creation of the navigation grid.
/// This script needs to always run first as many other scripts depend on the grid being configured first
/// </summary>
public class GridController : MonoSingleton<GridController>
{
    public Color gridBorderColor;
    private readonly HashSet<Cell> allTerrainCells = new();
    private readonly HashSet<Vector3> hitPosList = new();
    public HashSet<NavGrapplePoint> allGrapplePoints = new();
    public HashSet<NavCell> allNavCells = new();
    public HashSet<INavigable> allNavigables = new();
    public HashSet<NavLedge> allNavLedges = new();
    private RaycastHit grappleHit;
    private bool isCullingCell = true;
    private bool isCullingGrapple = true;
    private bool isCullingLedge = true;
    private bool isLinkingCells = true;
    private bool isLinkingGrapples = true;
    private bool isLinkingLedges = true;
    private GridPointController gridPointController => GridPointController.instance;
    private NavigableController navController => NavigableController.instance;

    private void Start()
    {
        ConfigureGrid();
        GrapplePointDetection();
        CullNavPoints();
        StartCoroutine(ConfigNavLinks());
        StartCoroutine(CreateNavigableList());
    }

    private void OnEnable()
    {
        allNavCells.Clear();
    }

    private void CullNavPoints()
    {
        StartCoroutine(CullNavCells());
        StartCoroutine(CullTerrainLedges());
        StartCoroutine(CullLedge());
        StartCoroutine(CullGrapplePoints(allGrapplePoints));
    }

    private IEnumerator ConfigNavLinks()
    {
        while (isCullingGrapple && isCullingLedge && isCullingCell) yield return new WaitForEndOfFrame();
        LinkNavLedges();
        LinkGrapplePoints();
    }


    private IEnumerator CreateNavigableList()
    {
        while (isCullingGrapple && isCullingLedge && isCullingCell
               && isLinkingCells && isLinkingLedges && isLinkingGrapples)
            yield return new WaitForEndOfFrame();

        EventSenderController.GridConfigured();

        var startIndex = 0;

        foreach (var navCell in allNavCells)
        {
            navCell.navIndex = startIndex++;
            allNavigables.Add(navCell);
        }

        foreach (var grapplePoint in allGrapplePoints)
        {
            grapplePoint.grappleMarkerEffect = Instantiate(EffectsController.instance.grappleMarkerEffect,
                grapplePoint.transform, true);
            grapplePoint.grappleMarkerEffect.transform.position =
                grapplePoint.GetPosition() + grapplePoint.hitNormal * .25f;
            grapplePoint.grappleMarkerEffect.transform.rotation = Quaternion.LookRotation(grapplePoint.hitNormal);
            grapplePoint.grappleMarkerEffect.grapplePoint = grapplePoint;
            grapplePoint.grappleMarkerEffect.StopAllEffect();
            grapplePoint.navIndex = startIndex++;
            allNavigables.Add(grapplePoint);
        }

        foreach (var navLedge in allNavLedges)
        {
            navLedge.navIndex = startIndex++;
            allNavigables.Add(navLedge);
        }

        foreach (var navigable in allNavigables)
        {
            foreach (var neighbor in navigable.GetLinkedNavigables()) neighbor.AddNeighbor(navigable);
            navigable.SetLinkedNavigables();
        }

        foreach (var navigable in allNavigables)
        {
            foreach (var neighbor in navigable.GetLinkedNavigables()) neighbor.AddNeighbor(navigable);
            navigable.SetLinkedNavigables();
        }

        yield return new WaitForEndOfFrame();
        allNavigables.OrderBy(a => a.GetNavIndex());
        EventSenderController.NavlinksEstablished();
    }

    private Vector3 RayGetStart(Vector3 startPos, Vector3 dir)
    {
        if (Physics.Raycast(startPos, Vector3.down, out var hit, 100, LayerMaskHelper.levelLayerMask))
            return hit.collider.gameObject.layer == (int) LayersEnum.Terrain
                ? hit.point
                : RayGetStart(startPos - dir, dir);

        return startPos;
    }

    private Tuple<bool, Vector3> MoveAwayRay(Vector3 startPos, Vector3 dir)
    {
        if (Physics.Raycast(startPos, Vector3.down, out var downHit, 1, LayerMaskHelper.levelLayerMask))
        {
            var downHitLayer = downHit.collider.gameObject.layer;
            switch (downHitLayer)
            {
                case (int) LayersEnum.Terrain:
                    return new Tuple<bool, Vector3>(true, startPos);
                case (int) LayersEnum.Obstacle:
                    var newStartPos = startPos - dir;
                    return MoveAwayRay(newStartPos, dir);
            }
        }

        return new Tuple<bool, Vector3>(false, startPos);
    }

    #region ConfigGrid

    private void ConfigureGrid()
    {
        foreach (var gridSystem in TerrainGridSystem.grids)
        {
            allTerrainCells.AddRange(gridSystem.cells);
            gridSystem.showTerritoriesOuterBorders = false;
            gridSystem.showCells = false;
            gridSystem.highlightEffect = HIGHLIGHT_EFFECT.None;
            gridSystem.cellBorderColor = gridBorderColor;

            foreach (var gridCell in gridSystem.cells) GridConfigCells(gridSystem, gridCell);

            if (gridSystem.gameObject.CompareTag("Terrain"))
            {
                gridSystem.blockingMask = LayerMaskHelper.obstacleLayerMask;
                ScanTerrainLedges(gridSystem);
            }

            if (gridSystem.gameObject.CompareTag("Obstacle"))
                ObstacleLedgeDetection(gridSystem);
        }
    }

    private void GridConfigCells(TerrainGridSystem gridSystem, Cell gridCell)
    {
        var navCell = new GameObject().AddComponent<NavCell>();
        gridCell.visible = false;
        var cellPos = gridSystem.CellGetPosition(gridCell);
        var isAboveSomething = false;
        //var hasLedges = false;
        gridCell.canCross = false;
        if (Physics.Raycast(cellPos, Vector3.down, out var hit, .5f, LayerMaskHelper.levelLayerMask))
        {
            var angle = Vector3.Angle(hit.normal, Vector3.up);
            navCell.hit = hit;
            if (angle <= 60)
            {
                navCell.isAngled = false;
                var insideColliders = Physics.OverlapSphere(cellPos, .25f, LayerMaskHelper.obstacleLayerMask);
                if (insideColliders.Length == 0)
                {
                    gridCell.canCross = true;
                    cellPos = hit.point;
                    isAboveSomething = true;
                }
                else
                {
                    navCell.isNearObstacle = true;
                    var smallObstacle =
                        Physics.OverlapSphere(cellPos + Vector3.up, .25f, LayerMaskHelper.obstacleLayerMask);
                    if (smallObstacle.Length == 0)
                    {
                        if (Physics.Raycast(cellPos + Vector3.up, Vector3.down, out var hitDown))
                        {
                            navCell.isCenterCovered = true;
                            gridCell.canCross = true;
                            cellPos = hitDown.point;
                            isAboveSomething = true;
                        }
                    }
                    else
                    {
                        gridCell.canCross = false;
                        navCell.canBeHighlighted = false;
                    }
                }
            }
            else if (angle > 60)
            {
                gridCell.canCross = false;
                navCell.isAngled = true;
                navCell.isTraversable = false;
                cellPos = hit.point;
                isAboveSomething = true;
                navCell.canBeHighlighted = false;
            }
        }
        else
        {
            Debug.DrawRay(cellPos, Vector3.down, Color.red, 120);
        }

        navCell.navPosition = cellPos;
        CreateNavCell(gridSystem, navCell, gridCell, isAboveSomething);
    }

    private void ScanTerrainLedges(TerrainGridSystem gridSystem)
    {
        var angledCells = allNavCells.Where(a => a.isAngled);
        foreach (var navCell in angledCells)
        {
            hitPosList.Clear();
            var ledge = FindTerrainLedge(navCell.hit.normal, navCell.GetPosition(), navCell.cell);
            var gridCellVertices = navCell.verticesList.OrderBy(a => a.vertPos.y);
            CreateLedge(navCell.hit, LedgeType.TerrainLedge, gridCellVertices.LastOrDefault());
        }
    }

    private Vector3 FindTerrainLedge(Vector3 normal, Vector3 hitPos, Cell cell)
    {
        var newrayPos = normal * .1f + hitPos;
        var newDir = new Vector3(hitPos.x, newrayPos.y, hitPos.z) - newrayPos;
        var newRay = new Ray(newrayPos, newDir);
        if (Physics.Raycast(newRay, out var hit, 10, LayerMaskHelper.levelLayerMask))
        {
            var downRay = new Ray(hit.point + Vector3.up, Vector3.down);
            if (Physics.Raycast(downRay, out var hitDown, 10, LayerMaskHelper.levelLayerMask))
            {
                hitPosList.Add(hit.point);
                FindTerrainLedge(hitDown.normal, hitDown.point, cell);
            }
            else
            {
                return hit.point;
            }
        }

        return hitPosList.Count == 0 ? cell.navCell.GetPosition() : hitPosList.Last();
    }

    private void ObstacleLedgeDetection(TerrainGridSystem gridSystem)
    {
        var vertsOverLedge = VertexConfig(gridSystem);
        foreach (var vertex in vertsOverLedge)
        {
            vertex.navCell.hasFloatingVert = true;
            var vertRayOrigin = vertex.vertPos + Vector3.down * .5f;
            var vertRayDir = vertex.navCell.GetPosition() + Vector3.down * .5f - vertRayOrigin;
            var ray = new Ray(vertRayOrigin, vertRayDir);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 3.5f, LayerMaskHelper.levelLayerMask))
                CreateLedge(hit, LedgeType.ObstacleLedge, vertex);
        }
    }

    private List<GridCellVertex> VertexConfig(TerrainGridSystem gridSystem)
    {
        // used to cull cells with no verts over movable object
        var cellCompletelyOff = new List<Cell>();
        // used to detect grapple points
        var vertsOverLedge = new List<GridCellVertex>();
        foreach (var cell in gridSystem.cells)
        {
            var hitCount = 0;
            foreach (var vertex in cell.navCell.verticesList)
            {
                var ray = new Ray(vertex.vertPos, Vector3.down);
                if (Physics.Raycast(ray, .5f, LayerMaskHelper.levelLayerMask))
                {
                    hitCount++;
                    var results = new Collider[10];
                    results = Physics.OverlapCapsule(vertex.vertPos + Vector3.up * .1f,
                        vertex.vertPos + Vector3.up * 3,
                        .25f,
                        LayerMaskHelper.levelLayerMask);
                    if (results.Length > 0)
                    {
                        // todo find half cover; Raycast from other verts on cell towards object and work upward. 
                        vertex.cover = VertCoverType.Full;
                        vertex.navCell.hasVertCovered = true;
                    }
                    else
                    {
                        vertex.cover = VertCoverType.None;
                    }
                }
                else
                {
                    vertex.isOffLedge = true;
                    vertsOverLedge.Add(vertex);
                }
            }

            if (hitCount < cell.navCell.verticesList.Count) cell.navCell.isOverLedge = true;

            if (hitCount == 0) cellCompletelyOff.Add(cell);
        }

        foreach (var cell in cellCompletelyOff)
        {
            foreach (var vertex in cell.navCell.verticesList) vertsOverLedge.Remove(vertex);

            CullCell(cell.navCell);
        }

        return vertsOverLedge;
    }

    private void GrapplePointDetection()
    {
        foreach (var navCell in allNavCells.Where(gridCell => gridCell.hasFloatingVert))
        foreach (var vertex in navCell.verticesList.Where(s => s.isOffLedge))
            //fire a ray at all other vertex in cell
        foreach (var otherVertex in navCell.verticesList)
        {
            if (otherVertex == vertex) continue;
            var targetPos = otherVertex.vertPos;
            var dir = targetPos - vertex.vertPos;
            var rayStartPos = RayGetStart(vertex.vertPos, dir);
            var vertexList = new List<Vector3>();
            var grapplePointHits = FindGrapplePoints(rayStartPos, targetPos, vertexList);
            foreach (var point in grapplePointHits)
                CreateGrapplePoint(vertex, point, rayStartPos, dir, grapplePointHits, grappleHit);
        }
    }

    private List<Vector3> FindGrapplePoints(Vector3 startPos, Vector3 targetPos, List<Vector3> vertexList)
    {
        var verticleSpacingIncrement = 3;
        var dir = new Vector3(targetPos.x, startPos.y, targetPos.z) - startPos;
        RaycastHit hit;
        dir = new Vector3(targetPos.x, startPos.y, targetPos.z) - startPos;
        var ray = new Ray(startPos, dir * 10);
        if (Physics.Raycast(ray, out hit, 5, LayerMaskHelper.obstacleLayerMask))
        {
            vertexList.Add(hit.point);
            grappleHit = hit;
            return FindGrapplePoints(startPos + Vector3.up * verticleSpacingIncrement, hit.point, vertexList);
        }

        return vertexList;
    }

    #endregion ConfigGrid

    #region ConfigLinks

    private void BetweenGridLinks()
    {
        foreach (var navCell in allNavCells.Where(gridCell => gridCell.hasFloatingVert))
        {
            var offGridCells = new List<NavCell>();
            foreach (var vertex in navCell.verticesList.Where(s => s.isOffLedge))
            {
                var ray = new Ray(vertex.vertPos, Vector3.down);
                RaycastHit hit;
                //Debug.DrawRay(vertex.vertPos, Vector3.down * 30, Color.blue, 60);
                var radius = 3;
                if (Physics.Raycast(ray, out hit, 30, LayerMaskHelper.terrainLayerMask))
                {
                    var gridSystem = TerrainGridSystem.GetGridAt(hit.point);
                    if (!gridSystem) continue;
                    if (Physics.Raycast(ray, out var secondHit, 30, LayerMaskHelper.levelLayerMask))
                        if (secondHit.collider.gameObject.layer != (int) LayersEnum.Terrain)
                            radius = 5;
                    Collider[] gridColliders;
                    gridColliders = Physics.OverlapSphere(hit.point, radius, LayerMaskHelper.cellLayerMask);
                    foreach (var cellCollider in gridColliders)
                    {
                        var cell = allNavCells.First(a => a.sphereCollider == cellCollider);
                        var dir = cellCollider.bounds.center - vertex.vertPos;
                        if (Physics.Raycast(vertex.vertPos, dir, out var cellHit, 30,
                                LayerMaskHelper.linkLayerMask))
                        {
                            if (cellHit.collider != cellCollider) continue;
                            offGridCells.Add(cell);
                        }
                    }
                }
            }

            foreach (var cell in offGridCells.Distinct())
            {
                navCell.offGridLink.Add(cell);
                cell.offGridLink.Add(navCell);
            }
        }

        isLinkingCells = false;
    }

    private void LinkNavLedges()
    {
        isLinkingLedges = false;
    }

    private void LinkGrapplePoints()
    {
        foreach (var grapplePoint in allGrapplePoints)
        {
            Collider[] navColliders;
            navColliders = Physics.OverlapSphere(grapplePoint.GetPosition(), 5, LayerMaskHelper.navigablesLayerMask);
            foreach (var navCollider in navColliders)
            {
                var navigable = navCollider.GetComponent<INavigable>();
                if (ReferenceEquals(navigable, grapplePoint)) continue;
                var rayOrigin = grapplePoint.GetPosition() - grapplePoint.rayDir;
                var dir = navigable.GetPosition() - rayOrigin;
                if (Physics.Raycast(rayOrigin, dir, out var hit, 10,
                        LayerMaskHelper.linkLayerMask))
                    if (hit.collider == navCollider)
                        grapplePoint.AddNeighbor(navigable);
            }
        }

        isLinkingGrapples = false;
    }

    #endregion ConfigLinks

    #region CullNavs

    private void CullCell(NavCell navCell)
    {
        allNavCells.Remove(navCell);
        Destroy(navCell);
    }

    private IEnumerator CullNavCells()
    {
        yield return new WaitForEndOfFrame();
    }

    private IEnumerator CullLedge()
    {
        yield return new WaitForFixedUpdate();
        var cullList = new List<NavLedge>();
        foreach (var ledge in allNavLedges.Where(ledge => ledge.ledgeType == LedgeType.ObstacleLedge))
        {
            if (cullList.Contains(ledge)) continue;
            var ledgeDist = Vector3.Distance(ledge.transform.position, ledge.originVertex.vertPos);
            Collider[] ledgeColliders;
            ledgeColliders = Physics.OverlapSphere(ledge.transform.position, ledge.sphereCollider.radius,
                LayerMaskHelper.ledgeLayerMask);
            foreach (var ledgeCollider in ledgeColliders)
            {
                var otherLedge = ledgeCollider.GetComponent<NavLedge>();
                if (otherLedge == ledge || cullList.Contains(otherLedge)) continue;
                var otherLedgeDist =
                    Vector3.Distance(otherLedge.transform.position, otherLedge.originVertex.vertPos);
                if (ledgeDist <= otherLedgeDist) cullList.Add(otherLedge);
                if (ledgeDist > otherLedgeDist) cullList.Add(ledge);
            }
        }

        foreach (var ledge in cullList)
        {
            var linkedNavCells = allNavCells.Where(a => a.linkedLedges.Contains(ledge));
            foreach (var variNavCell in linkedNavCells) variNavCell.linkedLedges.Remove(ledge);
            allNavLedges.Remove(ledge);
            Destroy(ledge.gameObject);
        }

        isCullingLedge = false;
    }

    private IEnumerator CullTerrainLedges()
    {
        var cullList = new List<NavLedge>();
        yield return new WaitForFixedUpdate();
        foreach (var ledge in allNavLedges.Where(ledge => ledge.ledgeType == LedgeType.TerrainLedge))
        {
            Collider[] ledgeColliders;
            ledgeColliders = Physics.OverlapSphere(ledge.transform.position, ledge.sphereCollider.radius,
                LayerMaskHelper.ledgeLayerMask);
            if (ledgeColliders.Any(
                    ledgeCollider => ledge.sphereCollider.bounds.center.y < ledgeCollider.bounds.center.y))
                cullList.Add(ledge);
        }

        foreach (var ledge in cullList)
        {
            allNavLedges.Remove(ledge);
            Destroy(ledge.gameObject);
        }

        isCullingCell = false;
    }

    private IEnumerator CullGrapplePoints(HashSet<NavGrapplePoint> grapplePointsCulling)
    {
        yield return new WaitForFixedUpdate();
        var cullList = new List<NavGrapplePoint>();
        foreach (var grapplePoint in grapplePointsCulling)
        {
            if (cullList.Contains(grapplePoint))
                continue;
            Collider[] grapplePointColliders;
            grapplePointColliders = Physics.OverlapSphere(grapplePoint.transform.position,
                grapplePoint.sphereCollider.radius,
                LayerMaskHelper.ledgeLayerMask + LayerMaskHelper.terrainLayerMask + LayerMaskHelper.cellLayerMask +
                LayerMaskHelper.gridCreationMask);
            if (grapplePointColliders.Length > 0)
            {
                cullList.Add(grapplePoint);
                continue;
            }

            grapplePointColliders = Physics.OverlapSphere(grapplePoint.transform.position,
                grapplePoint.sphereCollider.radius,
                LayerMaskHelper.grappleLayerMask);
            var dist1 = Vector3.Distance(grapplePoint.navPosition, grapplePoint.originVertex.vertPos);
            foreach (var grapplePointCollider in grapplePointColliders)
            {
                var grapplePointObject = grapplePointCollider.gameObject.GetComponent<NavGrapplePoint>();
                var dist2 = Vector3.Distance(grapplePointObject.navPosition, grapplePointObject.originVertex.vertPos);
                if (dist1 < dist2) cullList.Add(grapplePointObject);

                if (dist2 < dist1) cullList.Add(grapplePoint);
            }
        }

        foreach (var grapplePoint in cullList)
        {
            allGrapplePoints.Remove(grapplePoint);
            foreach (var cell in allNavCells.Where(a => a.linkedGrapplePoints.Count > 0))
                cell.linkedGrapplePoints.Remove(grapplePoint);

            Destroy(grapplePoint.gameObject);
        }

        isCullingGrapple = false;
    }

    #endregion CullNavs

    #region CreateNavs

    private void CreateLedge(RaycastHit hit, LedgeType ledgeType, GridCellVertex vertex)
    {
        var newNavPos = hit.point;
        var newGO = new GameObject
        {
            transform =
            {
                position = newNavPos,
                name = "Ledge: " + newNavPos,
            },
            layer = (int) LayersEnum.Ledge,
            tag = UserTags.ledgeTag
        };
        var sphereCollider = newGO.AddComponent<SphereCollider>();
        sphereCollider.radius = .5f;
        sphereCollider.isTrigger = true;
        var navLedge = newGO.AddComponent<NavLedge>();
        switch (ledgeType)
        {
            case LedgeType.TerrainLedge:
                newGO.transform.SetParent(gridPointController.terrainLedgeHolder.transform);
                break;
            case LedgeType.ObstacleLedge:
                newGO.transform.SetParent(gridPointController.obstacleLedgeHolder.transform);
                break;
        }

        navLedge.hit = hit;
        navLedge.sphereCollider = sphereCollider;
        navLedge.ledgeType = ledgeType;
        navLedge.originVertex = vertex;
        navLedge.originVertex.navCell.linkedLedges.Add(navLedge);
        navLedge.navPosition = newNavPos;
        navLedge.navType = NavigableTypes.Ledge;
        navLedge.isTraversable = true;
        allNavLedges.Add(navLedge);
    }

    private void CreateGrapplePoint(GridCellVertex vertex, Vector3 newNavPos, Vector3 rayStartPos, Vector3 rayDir,
        List<Vector3> grapplePointHits, RaycastHit grappleRayHit)
    {
        var hitNormal = grappleRayHit.normal;
        var newGO = new GameObject();
        var navGrapplePoint = newGO.AddComponent<NavGrapplePoint>();
        newGO.transform.position = newNavPos;
        newGO.layer = LayerMask.NameToLayer("GrapplePoint");
        newGO.name = "GrapplePoint: " + newNavPos;
        newGO.tag = UserTags.grapplePointTag;
        newGO.transform.SetParent(gridPointController.navGrappleHolder.transform);
        var sphereCollider = newGO.AddComponent<SphereCollider>();
        sphereCollider.radius = .75f;
        sphereCollider.isTrigger = true;
        navGrapplePoint.sphereCollider = sphereCollider;
        navGrapplePoint.navPosition = newNavPos;
        navGrapplePoint.originVertex = vertex;
        navGrapplePoint.rayStartPos = rayStartPos;
        navGrapplePoint.rayDir = rayDir;
        navGrapplePoint.grapplePointHits = grapplePointHits;
        navGrapplePoint.navType = NavigableTypes.GrapplePoint;
        navGrapplePoint.isTraversable = true;
        navGrapplePoint.hitNormal = hitNormal;
        navGrapplePoint.hit = grappleRayHit;
        vertex.navCell.linkedGrapplePoints.Add(navGrapplePoint);
        navGrapplePoint.linkedCells.Add(navGrapplePoint.originVertex.navCell);
        allGrapplePoints.Add(navGrapplePoint);
    }

    private void CreateNavCell(TerrainGridSystem gridSystem, NavCell navCell, Cell cell, bool isAboveSomething)
    {
        navCell.transform.position = navCell.navPosition;
        navCell.name = "Cell: " + navCell.navPosition;
        navCell.gameObject.layer = (int) LayersEnum.NavCell;
        navCell.tag = UserTags.gridPointTag;
        navCell.isOverLedge = !isAboveSomething;
        navCell.gridSystem = gridSystem;
        navCell.navIndex = cell.index;
        navCell.gridSystem = gridSystem;
        if (isAboveSomething)
        {
            var sphereCollider = navCell.AddComponent<SphereCollider>();
            sphereCollider.radius = .25f;
            sphereCollider.isTrigger = true;
            navCell.sphereCollider = sphereCollider;
            navCell.tag = "Grid";
            navCell.transform.SetParent(gridPointController.navCellHolder.transform);
        }
        else
        {
            navCell.isTraversable = false;
            navCell.canBeHighlighted = false;
            navCell.tag = "OffGrid";
            navCell.transform.SetParent(gridPointController.offGridHolder.transform);
        }

        navCell.navType = NavigableTypes.NavCell;
        navCell.cell = cell;
        cell.navCell = navCell;
        GetVertices(gridSystem, cell, navCell);
        allNavCells.Add(navCell);
    }

    private static void GetVertices(TerrainGridSystem gridSystem, Cell cell, NavCell navCell)
    {
        var vertexCount = gridSystem.CellGetVertexCount(cell.index);
        for (var i = 0; i < vertexCount; i++)
        {
            var vertexPos = gridSystem.CellGetVertexPosition(cell.index, i);
            var vertexObject = new GameObject().AddComponent<GridCellVertex>();
            vertexObject.index = i;
            vertexObject.vertPos = vertexPos;
            vertexObject.navCell = navCell;
            vertexObject.transform.SetParent(navCell.transform);
            vertexObject.vertEdgePos = (VertEdgePosition) i;
            vertexObject.name = "Vertex : " + vertexObject.vertEdgePos;
            navCell.verticesList.Add(vertexObject);
        }
    }

    #endregion CreateNavs
}