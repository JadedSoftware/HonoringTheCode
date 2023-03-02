using System.Collections.Generic;
using Core;
using Core.GameManagement;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using EventSenderController = Core.GameManagement.EventSenders.EventSenderController;

/// <summary>
/// Handles the A* algorithm to find navigation
/// </summary>
public class NavPathJobs : MonoSingleton<NavPathJobs>
{
    private NativeArray<int> navNeighbors;
    private NativeArray<int> navNeighCount;
    private NativeArray<int> navObjIndex;

    private NativeArray<float3> navObjPos;
    private NativeArray<bool> navTraversable;
    private NativeArray<NavigableTypes> navTypes;
    private NativeArray<int> neighborStartIndex;

    private GridController gridController => GridController.instance;

    private void OnEnable()
    {
        RegisterEvents();
    }

    private void OnDisable()
    {
        UnRegisterEvents();
        navObjIndex.Dispose();
        navObjPos.Dispose();
        neighborStartIndex.Dispose();
        navNeighbors.Dispose();
        navNeighCount.Dispose();
        navTraversable.Dispose();
        navTypes.Dispose();
    }

    private void RegisterEvents()
    {
        EventSenderController.onNavEstablished += BuildNavNodes;
        EventSenderController.unitMovementRequested += MovementRequested;
    }

    private void UnRegisterEvents()
    {
        EventSenderController.onNavEstablished -= BuildNavNodes;
        EventSenderController.unitMovementRequested -= MovementRequested;
    }

    private void MovementRequested(IMovable unit, INavigable endNavigable)
    {
    }

    public void BuildNavNodes()
    {
        var navsByIndex = gridController.allNavigables;
        navObjIndex = new NativeArray<int>(navsByIndex.Count, Allocator.Persistent);
        navObjPos = new NativeArray<float3>(navsByIndex.Count, Allocator.Persistent);
        neighborStartIndex = new NativeArray<int>(navsByIndex.Count, Allocator.Persistent);
        navNeighbors = new NativeArray<int>(30000, Allocator.Persistent);
        navNeighCount = new NativeArray<int>(navsByIndex.Count, Allocator.Persistent);
        navTraversable = new NativeArray<bool>(navNeighCount.Length, Allocator.Persistent);
        navTypes = new NativeArray<NavigableTypes>(navsByIndex.Count, Allocator.Persistent);
        var neighArrayIndex = 0;
        var count = 0;
        foreach (var navigable in navsByIndex)
        {
            var navIndex = navigable.GetNavIndex();
            navObjIndex[navIndex] = navIndex;
            navObjPos[navIndex] = navigable.GetPosition();
            navTypes[navIndex] = navigable.GetNavType();
            navNeighCount[navIndex] = navigable.GetLinkedNavigables().Count;
            navTraversable[navIndex] = navigable.IsTraversable();
            neighborStartIndex[navIndex] = neighArrayIndex;
            foreach (var navNeighbor in navigable.GetLinkedNavigables())
            {
                navNeighbors[count] = navNeighbor.GetNavIndex();
                count++;
            }

            neighArrayIndex += navigable.GetLinkedNavigables().Count;
        }

        EventSenderController.NavPathBuilt();
    }

    public Stack<int> FindPath(int startIndex, int endIndex)
    {
        var calculatedPath = new NativeList<int>(Allocator.TempJob);
        FindPathJob findPathJob = new()
        {
            startIndex = startIndex,
            endIndex = endIndex,
            navObjIndex = navObjIndex,
            navObjPos = navObjPos,
            navObjNeighbors = navNeighbors,
            navNeighborCount = navNeighCount,
            navTraversable = navTraversable,
            neighborStartIndex = neighborStartIndex,
            calculatedPath = calculatedPath
        };
        findPathJob.Run();
        var pathList = new Stack<int>();
        foreach (var node in calculatedPath) pathList.Push(node);
        calculatedPath.Dispose();
        return pathList;
    }

    public NativeList<int> NavsInRange(int startIndex, int depth)
    {
        // var time = Time.realtimeSinceStartup;
        var navsInRange = new NativeList<int>(Allocator.TempJob);
        NavsRangeJob navRangeJob = new()
        {
            startIndex = startIndex,
            depth = depth,
            navObjIndex = navObjIndex,
            navObjPos = navObjPos,
            navObjNeighbors = navNeighbors,
            navNeighborCount = navNeighCount,
            neighborStartIndex = neighborStartIndex,
            navTraversable = navTraversable,
            navNodeTypes = navTypes,
            navsInRange = navsInRange
        };
        navRangeJob.Run();
        // Debug.Log("Navs in Range : " + navsInRange.Length);
        // Debug.Log("Navs in range execute time : " + (Time.realtimeSinceStartup - time)* 1000f);
        return navsInRange;
    }

    public int FindClosestNav(Vector3 startPos)
    {
        var time = Time.realtimeSinceStartup;
        var closestPoint = new NativeArray<int>(1, Allocator.TempJob);
        ClosestNavJob closestNavJob = new()
        {
            startPos = startPos,
            navObjIndex = navObjIndex,
            navObjPos = navObjPos,
            navTraversable = navTraversable,
            closestPoint = closestPoint
        };
        closestNavJob.Run();
        if (DebugController.instance.isDebugEnabled)
        {
            Debug.Log("Closest Nav : " + closestPoint[0]);
            Debug.Log("Closest Nav execute time : " + (Time.realtimeSinceStartup - time) * 1000f);
        }

        var point = closestPoint[0];
        closestPoint.Dispose();
        return point;
    }

    private static float CalculateDistance(float3 startPos, float3 endPos)
    {
        return Vector3.Distance(startPos, endPos);
    }

    public struct NavNodeJobs
    {
        public int index;
        public float3 position;

        public float gCost;
        public float hCost;
        public float fCost;

        public bool isTraversable;
        public int cameFromIndex;
        public int distanceFromStart;

        public NavigableTypes navType;

        public float CalculateFCost()
        {
            return gCost + hCost;
        }
    }

    [BurstCompile]
    public struct NavsRangeJob : IJob
    {
        public int startIndex;
        public int depth;
        [ReadOnly] public NativeArray<int> navObjIndex;
        [ReadOnly] public NativeArray<float3> navObjPos;
        [ReadOnly] public NativeArray<int> navObjNeighbors;
        [ReadOnly] public NativeArray<int> navNeighborCount;
        [ReadOnly] public NativeArray<int> neighborStartIndex;
        [ReadOnly] public NativeArray<bool> navTraversable;
        [ReadOnly] public NativeArray<NavigableTypes> navNodeTypes;
        public NativeList<int> navsInRange;

        public void Execute()
        {
            var navPathNodes = new NativeArray<NavNodeJobs>(navObjPos.Length, Allocator.Temp);

            for (var i = 0; i < navObjIndex.Length; i++)
            {
                var newPathNode = new NavNodeJobs
                {
                    index = navObjIndex[i]
                };
                newPathNode.position = navObjPos[i];
                newPathNode.isTraversable = navTraversable[i];
                newPathNode.cameFromIndex = -1;
                newPathNode.distanceFromStart = -1;
                navPathNodes[newPathNode.index] = newPathNode;
            }

            var checkedList = new NativeList<int>(Allocator.Temp);
            var currentLayer = new NativeList<int>(Allocator.Temp);
            var previousLayer = new NativeList<int>(Allocator.Temp);
            var neighborsToAdd = new NativeList<int>(Allocator.Temp);
            currentLayer.Add(startIndex);
            var startNode = navPathNodes[startIndex];
            startNode.distanceFromStart = 0;
            navPathNodes[startIndex] = startNode;
            for (var currentDepth = 0; currentDepth < depth - 1; currentDepth++)
            {
                for (var index = 0; index < currentLayer.Length; index++)
                {
                    var i = currentLayer[index];
                    if (!navPathNodes[i].isTraversable) continue;
                    if (previousLayer.Contains(i)) continue;
                    var neighbors = GetNeighbors(i);

                    for (var index1 = 0; index1 < neighbors.Length; index1++)
                    {
                        var neighbor = neighbors[index1];
                        if (!navPathNodes[neighbor].isTraversable) continue;
                        if (!currentLayer.Contains(neighbor))
                        {
                            neighborsToAdd.Add(neighbor);
                            if (!navsInRange.Contains(neighbor))
                                navsInRange.Add(neighbor);
                        }
                    }

                    if (!checkedList.Contains(i))
                    {
                        checkedList.Add(i);
                        previousLayer.Add(i);
                    }
                }

                currentLayer.Clear();
                for (var i = 0; i < neighborsToAdd.Length; i++) currentLayer.Add(neighborsToAdd[i]);
                neighborsToAdd.Clear();
            }

            checkedList.Dispose();
            currentLayer.Dispose();
            previousLayer.Dispose();
            neighborsToAdd.Dispose();
        }

        private NativeArray<int> GetNeighbors(int navObjectIndex)
        {
            var startIndex = neighborStartIndex[navObjectIndex];
            var neighborCount = navNeighborCount[navObjectIndex];

            var neighbors = new NativeArray<int>(neighborCount, Allocator.Temp);
            for (var i = 0; i < neighborCount; i++)
            {
                neighbors[i] = navObjNeighbors[startIndex];
                startIndex++;
            }

            return neighbors;
        }
    }


    [BurstCompile]
    private struct FindPathJob : IJob
    {
        public int startIndex;
        public int endIndex;
        [ReadOnly] public NativeArray<int> navObjIndex;
        [ReadOnly] public NativeArray<float3> navObjPos;
        [ReadOnly] public NativeArray<int> navObjNeighbors;
        [ReadOnly] public NativeArray<int> navNeighborCount;
        [ReadOnly] public NativeArray<int> neighborStartIndex;
        [ReadOnly] public NativeArray<bool> navTraversable;
        [WriteOnly] public NativeList<int> calculatedPath;

        public void Execute()
        {
            var navPathNodes = new NativeArray<NavNodeJobs>(navObjPos.Length, Allocator.Temp);
            for (var i = 0; i < navObjPos.Length; i++)
            {
                var newPathNode = new NavNodeJobs
                {
                    index = navObjIndex[i]
                };
                newPathNode.gCost = newPathNode.index == startIndex ? 0 : 9999;
                newPathNode.position = navObjPos[i];
                newPathNode.hCost = Vector3.Distance(newPathNode.position, navObjPos[endIndex]);
                newPathNode.fCost = newPathNode.CalculateFCost();
                newPathNode.isTraversable = navTraversable[i];
                newPathNode.cameFromIndex = -1;
                navPathNodes[newPathNode.index] = newPathNode;
            }

            var startNode = navPathNodes[startIndex];
            var openList = new NativeList<int>(Allocator.Temp);
            var closedList = new NativeList<int>(Allocator.Temp);

            openList.Add(startNode.index);
            while (openList.Length > 0)
            {
                var currentNodeIndex = GetLowestF(openList, navPathNodes);
                var currentNode = navPathNodes[currentNodeIndex];
                if (currentNodeIndex == endIndex) break;
                for (var i = 0; i < openList.Length; i++)
                    if (openList[i] == currentNodeIndex)
                    {
                        openList.RemoveAtSwapBack(i);
                        break;
                    }

                closedList.Add(currentNodeIndex);

                var currentNavNeighbors = GetNeighbors(currentNodeIndex);
                for (var i = 0; i < currentNavNeighbors.Length; i++)
                {
                    var neighborNav = navPathNodes[currentNavNeighbors[i]];
                    if (closedList.Contains(neighborNav.index)) continue;
                    if (!neighborNav.isTraversable) continue;
                    var tentativeGCost = currentNode.gCost +
                                         CalculateDistance(currentNode.position, neighborNav.position);
                    if (tentativeGCost < neighborNav.gCost)
                    {
                        neighborNav.cameFromIndex = currentNodeIndex;
                        neighborNav.gCost = tentativeGCost;
                        neighborNav.CalculateFCost();
                        navPathNodes[neighborNav.index] = neighborNav;

                        if (!openList.Contains(neighborNav.index)) openList.Add(neighborNav.index);
                    }
                }
            }

            var endNode = navPathNodes[endIndex];
            if (endNode.cameFromIndex == -1)
            {
                Debug.Log("Jobs End node index -1");
            }
            else
            {
                var navPathArray = CalculatePath(navPathNodes, endNode);
                foreach (var i in navPathArray) calculatedPath.Add(i);
                navPathArray.Dispose();
            }

            openList.Dispose();
            closedList.Dispose();
            navPathNodes.Dispose();
        }


        private NativeArray<int> CalculatePath(NativeArray<NavNodeJobs> navPathNodes, NavNodeJobs endNode)
        {
            if (endNode.cameFromIndex == -1)
                return new NativeList<int>(Allocator.Temp);

            var path = new NativeList<int>(Allocator.Temp);
            path.Add(endNode.index);
            var currentNode = endNode;
            while (currentNode.cameFromIndex != -1)
            {
                var cameFromNode = navPathNodes[currentNode.cameFromIndex];
                path.Add(cameFromNode.index);
                currentNode = cameFromNode;
            }

            return path;
        }

        private int GetLowestF(NativeList<int> openList, NativeArray<NavNodeJobs> navPathNodes)
        {
            var lowestFCost = navPathNodes[openList[0]];
            for (var i = 1; i < openList.Length; i++)
            {
                var testPathNode = navPathNodes[openList[i]];
                if (testPathNode.fCost < lowestFCost.fCost) lowestFCost = testPathNode;
            }

            return lowestFCost.index;
        }

        private NativeArray<int> GetNeighbors(int navObjectIndex)
        {
            var startIndex = neighborStartIndex[navObjectIndex];
            var neighborCount = navNeighborCount[navObjectIndex];

            var neighbors = new NativeArray<int>(neighborCount, Allocator.Temp);
            for (var i = 0; i < neighborCount; i++)
            {
                neighbors[i] = navObjNeighbors[startIndex];
                startIndex++;
            }

            return neighbors;
        }
    }

    private struct ClosestNavJob : IJob
    {
        public float3 startPos;

        [ReadOnly] public NativeArray<int> navObjIndex;
        [ReadOnly] public NativeArray<float3> navObjPos;
        [ReadOnly] public NativeArray<bool> navTraversable;
        [WriteOnly] public NativeArray<int> closestPoint;

        public void Execute()
        {
            closestPoint[0] = -1;
            var closestDist = 100f;
            for (var i = 0; i < navObjIndex.Length; i++)
            {
                if (!navTraversable[i]) continue;

                var point = navObjPos[i];
                var dist = CalculateDistance(startPos, point);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestPoint[0] = navObjIndex[i];
                }
            }
        }
    }
}