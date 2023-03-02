using System;
using System.Collections.Generic;
using System.Linq;
using Micosmo.SensorToolkit;
using UnityEngine;

public class SensorContainer : MonoBehaviour, ISensor
{
    [SerializeField] private List<UnitSensor> unitSensors;
    [SerializeField] private SteeringSensor steeringSensor;
    public UnitCommon unit;
    private readonly List<Sensor> pulseSensors = new();
    private ArcSensor arcSensor;
    private TriggerSensor fovSensor;
    private LOSSensor losSensor;
    private RangeSensor rangeSensor;
    private List<Rigidbody> rigidbodies;
    private TriggerSensor triggerSensor;
    private RaySensor unitRaySensor;
    private Rigidbody unitRigidbody;

    // public RaySensorController raySensorController { get; private set; }

    private void OnEnable()
    {
        unit = GetComponentInParent<UnitCommon>();
        rigidbodies = GetComponentsInChildren<Rigidbody>().ToList();
        if (rigidbodies.Count > 0)
            foreach (var rb in rigidbodies)
            {
                rb.isKinematic = true;
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }

        //raySensorController = GetComponentInChildren<RaySensorController>();
        //raySensorController.unit = unit;
        //ConfigureSteeringSensor();
        SortSensors();
        ConfigureLOS();
    }

    public void SortSensors()
    {
        foreach (var sensor in unitSensors)
        {
            sensor.sensorGameObject.layer = (int) LayersEnum.Sensor;
            switch (sensor.sensorType)
            {
                case SensorType.Arc:
                    arcSensor = sensor.sensorGameObject.GetComponent<ArcSensor>();
                    sensor.sensorGameObject.tag = UserTags.sensorTag;
                    arcSensor.PulseMode = PulseRoutine.Modes.Manual;
                    arcSensor.IgnoreList.Add(unit.gameObject);
                    arcSensor.DetectsOnLayers = LayerMaskHelper.selectableMask;
                    pulseSensors.Add(arcSensor);
                    break;
                case SensorType.Ray:
                    unitRaySensor = sensor.sensorGameObject.GetComponent<RaySensor>();
                    sensor.sensorGameObject.tag = UserTags.sensorTag;
                    unitRaySensor.PulseMode = PulseRoutine.Modes.Manual;
                    unitRaySensor.IgnoreList.Add(unit.gameObject);
                    pulseSensors.Add(unitRaySensor);
                    break;
                case SensorType.Range:
                    rangeSensor = sensor.sensorGameObject.GetComponent<RangeSensor>();
                    sensor.sensorGameObject.tag = UserTags.sensorTag;
                    rangeSensor.PulseMode = PulseRoutine.Modes.Manual;
                    //rangeSensor.IgnoreList.Add(unit.gameObject);
                    rangeSensor.DetectsOnLayers = LayerMaskHelper.selectableMask;
                    rangeSensor.Shape = RangeSensor.Shapes.Sphere;
                    pulseSensors.Add(rangeSensor);
                    break;
                case SensorType.Trigger:
                    sensor.sensorGameObject.tag = UserTags.triggerTag;
                    triggerSensor = sensor.sensorGameObject.GetComponent<TriggerSensor>();
                    triggerSensor.DetectionMode = DetectionModes.Colliders;
                    triggerSensor.IgnoreList.Add(unit.gameObject);
                    triggerSensor.OnSignalAdded += TriggerSignalDetected;
                    triggerSensor.OnSignalLost += TriggerSignalLost;
                    break;
                case SensorType.LOS:
                    losSensor = sensor.sensorGameObject.GetComponent<LOSSensor>();
                    sensor.sensorGameObject.tag = UserTags.sensorTag;
                    losSensor.PulseMode = PulseRoutine.Modes.Manual;
                    losSensor.BlocksLineOfSight = LayerMaskHelper.selectableMask;
                    losSensor.IgnoreTriggerColliders = true;
                    losSensor.NumberOfRays = 20;
                    pulseSensors.Add(losSensor);
                    break;
                case SensorType.FOV:
                    sensor.sensorGameObject.tag = UserTags.triggerTag;
                    fovSensor = sensor.sensorGameObject.GetComponent<TriggerSensor>();
                    fovSensor.DetectionMode = DetectionModes.Colliders;
                    fovSensor.IgnoreList.Add(unit.gameObject);
                    fovSensor.OnSignalAdded += FovSignalDetected;
                    fovSensor.OnSignalLost += FovSignalLost;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public void PulseAllSensors()
    {
        foreach (var sensor in pulseSensors) sensor.Pulse();
    }

    public void PulseSensorType(SensorType pulseSensorType)
    {
        foreach (var unitSensor in unitSensors.Where(a => a.sensorType == pulseSensorType)) unitSensor.sensor.Pulse();
    }

    public List<GameObject> GetAllDetected()
    {
        var detectedObjects = new List<GameObject>();
        foreach (var sensor in pulseSensors) detectedObjects.AddRange(sensor.GetDetectionsByDistance());

        return detectedObjects;
    }

    public List<GameObject> GetDetected(Sensor sensor)
    {
        return sensor.GetDetectionsByDistance();
    }

    private void ConfigureSteeringSensor()
    {
        if (steeringSensor == null)
            steeringSensor = GetComponent<SteeringSensor>();
        if (steeringSensor == null) steeringSensor = gameObject.AddComponent<SteeringSensor>();
        steeringSensor.IsSpherical = true;
        steeringSensor.Resolution = 3;
        steeringSensor.PulseMode = PulseRoutine.Modes.Manual;
        steeringSensor.Avoid.Sensors.Add(rangeSensor);
        steeringSensor.LocomotionMode = LocomotionMode.RigidBodyFlying;
        steeringSensor.RigidBody = unit.rigidBody;
    }

    public Vector3 GetSteeringVector()
    {
        steeringSensor.Pulse();
        var steeringVector = steeringSensor.GetSteeringVector();
        return new Vector3(steeringVector.x, 0, steeringVector.z);
    }

    private void ConfigureLOS()
    {
        losSensor.InputSensor = fovSensor;
    }

    private void FovSignalLost(Signal signal, Sensor sensor)
    {
    }

    private void FovSignalDetected(Signal signal, Sensor sensor)
    {
    }

    private void TriggerSignalLost(Signal signal, Sensor sensor)
    {
    }

    private void TriggerSignalDetected(Signal signal, Sensor sensor)
    {
    }

    public void PulseRayHeight(SensorHeight height)
    {
        // raySensorController.PulseSensorHeight(height);
    }
}

public enum SensorType
{
    Arc,
    Ray,
    Range,
    Trigger,
    LOS,
    FOV
}

[Serializable]
public struct UnitSensor
{
    public GameObject sensorGameObject;
    public Sensor sensor;
    public SensorType sensorType;
}