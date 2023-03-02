using System;
using UnityEngine;

/// <summary>
/// Provides reference to the games layers and tags.
/// </summary>
public static class UserTags
{
    public static string cameraControlerTag = "CameraController";
    public static string obstacleTag = "Obstacle";
    public static string gridTag = "Grid";
    public static string terrainTag = "Terrain";
    public static string ledgeTag = "Ledge";
    public static string offGridTag = "OffGrid";
    public static string gridPointHolderTag = "GridPointHolder";
    public static string grapplePointTag = "GrapplePoint";
    public static string gridPointTag = "GridPoint";
    public static string sensorTag = "Sensor";
    public static string triggerTag = "Trigger";
    public static string aiTag = "AI";
    public static string clothTag = "Cloth";
    public static string treeTag = "Tree";
    public static string uiTag = "UI";
}

[Flags]
public enum LayersEnum
{
    Default = 0,
    TransparentFX = 1,
    IgnoreRaycast = 2,
    Terrain = 3,
    Water = 4,
    UI = 5,
    Unit = 6,
    NavCell = 7,
    TaskAtlas = 8,
    Obstacle = 9,
    Ledge = 10,
    GrapplePoint = 11,
    Sensor = 12,
    GridCreationObject = 13,
    Damage = 14,
    Ragdoll = 15,
    Cloth = 16,
    Target = 17
}

public static class LayerMaskHelper
{
    public static LayerMask terrainLayerMask = 1 << (int) LayersEnum.Terrain;
    public static LayerMask obstacleLayerMask = 1 << (int) LayersEnum.Obstacle;
    public static LayerMask ledgeLayerMask = 1 << (int) LayersEnum.Ledge;
    public static LayerMask cellLayerMask = 1 << (int) LayersEnum.NavCell;
    public static LayerMask grappleLayerMask = 1 << (int) LayersEnum.GrapplePoint;
    public static LayerMask gridCreationMask = 1 << (int) LayersEnum.GridCreationObject;
    public static LayerMask unitLayerMask = 1 << (int) LayersEnum.Unit;
    public static LayerMask SensorLayerMask = 1 << (int) LayersEnum.Sensor;
    public static LayerMask UiLayerMask = 1 << (int) LayersEnum.UI;
    public static LayerMask TargetLayerMask = 1 << (int) LayersEnum.Target;


    public static LayerMask navigablesLayerMask = cellLayerMask + ledgeLayerMask + grappleLayerMask;
    public static LayerMask moveLayerMask = terrainLayerMask + obstacleLayerMask + navigablesLayerMask;
    public static LayerMask levelLayerMask = terrainLayerMask + obstacleLayerMask;
    public static LayerMask linkLayerMask = levelLayerMask + navigablesLayerMask;

    public static LayerMask selectableMask =
        terrainLayerMask + obstacleLayerMask + unitLayerMask + grappleLayerMask + UiLayerMask;

    public static LayerMask attackVisionMask = unitLayerMask + terrainLayerMask + obstacleLayerMask;
}