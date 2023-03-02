using System;
using System.Collections.Generic;
using System.Linq;
using Micosmo.SensorToolkit;
using UnityEngine;

public class RaySensorContainer : MonoBehaviour
{
    [SerializeField] private List<UnitRaySensor> unitRaySensors;
    public GameObject groundPos;
    public GameObject midPos;
    public GameObject topPos;
    public UnitCommon unit;

    private void OnEnable()
    {
        SortSensors();
    }

    public void SortSensors()
    {
        foreach (var raySensor in groundPos.GetComponentsInChildren<RaySensor>())
        {
            var newSensor = new UnitRaySensor();
            var o = raySensor.gameObject;
            newSensor.sensorGameObject = o;
            newSensor.sensor = raySensor;
            newSensor.sensorDir = NameToEnum(o);
            newSensor.sensorHeight = SensorHeight.Ground;
            newSensor.sensorType = SensorType.Ray;
            unitRaySensors.Add(newSensor);
        }

        foreach (var raySensor in midPos.GetComponentsInChildren<RaySensor>())
        {
            var newSensor = new UnitRaySensor();
            var o = raySensor.gameObject;
            newSensor.sensorGameObject = o;
            newSensor.sensor = raySensor;
            newSensor.sensorDir = NameToEnum(o);
            newSensor.sensorHeight = SensorHeight.Mid;
            newSensor.sensorType = SensorType.Ray;
            unitRaySensors.Add(newSensor);
        }

        foreach (var raySensor in topPos.GetComponentsInChildren<RaySensor>())
        {
            var newSensor = new UnitRaySensor();
            var o = raySensor.gameObject;
            newSensor.sensorGameObject = o;
            newSensor.sensor = raySensor;
            newSensor.sensorDir = NameToEnum(o);
            newSensor.sensorHeight = SensorHeight.Top;
            newSensor.sensorType = SensorType.Ray;
            unitRaySensors.Add(newSensor);
        }

        foreach (var unitRaySensor in unitRaySensors)
        {
            var raySensor = unitRaySensor;
            raySensor.sensorGameObject.tag = UserTags.sensorTag;
            raySensor.sensorGameObject.layer = (int) LayersEnum.Sensor;
            raySensor.sensor.PulseMode = PulseRoutine.Modes.Manual;
            raySensor.sensor.Length = 10;
            raySensor.sensor.IgnoreList.Add(unit.gameObject);
        }
    }

    public void PulseAllSensors()
    {
        foreach (var unitRaySensor in unitRaySensors) unitRaySensor.sensor.Pulse();
    }

    public void PulseSensorHeight(SensorHeight height)
    {
        foreach (var raySensor in unitRaySensors.Where(a => a.sensorHeight == height)) raySensor.sensor.Pulse();
    }

    public List<GameObject> GetAllDetected()
    {
        var detectedObjects = new List<GameObject>();
        foreach (var sensor in unitRaySensors) detectedObjects.AddRange(sensor.sensor.GetDetectionsByDistance());

        return detectedObjects;
    }

    public List<GameObject> GetDetected(Sensor sensor)
    {
        return sensor.GetDetectionsByDistance();
    }

    public SensorDirection NameToEnum(GameObject go)
    {
        switch (go.name)
        {
            case "N":
                return SensorDirection.N;
            case "NE":
                return SensorDirection.NE;
            case "E":
                return SensorDirection.E;
            case "SE":
                return SensorDirection.SE;
            case "S":
                return SensorDirection.S;
            case "SW":
                return SensorDirection.SW;
            case "W":
                return SensorDirection.W;
            case "NW":
                return SensorDirection.NW;
            default: return SensorDirection.A;
        }
    }
}

[Serializable]
public struct UnitRaySensor
{
    public GameObject sensorGameObject;
    public RaySensor sensor;
    public SensorDirection sensorDir;
    public SensorHeight sensorHeight;
    public SensorType sensorType;
}

public enum SensorDirection
{
    N = 0,
    NW = 1,
    W = 2,
    SW = 3,
    S = 4,
    SE = 5,
    E = 6,
    NE = 7,
    A = 8
}

public enum SensorHeight
{
    Ground,
    Mid,
    Top
}