using System.Collections.Generic;
using Micosmo.SensorToolkit;
using UnityEngine;

public interface ISensor
{
    void SortSensors();
    void PulseAllSensors();
    void PulseSensorType(SensorType pulseSensorType);

    List<GameObject> GetAllDetected();

    List<GameObject> GetDetected(Sensor sensor);
}