using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Indicates that a hex is a moveable location 
/// </summary>
public class HexEffect : EffectsCommon
{
    [SerializeField] public List<HexParticleEffects> hexParticleEffects;
}

[Serializable]
public struct HexParticleEffects
{
    public string name;
    public GameObject effectGO;
    public ParticleSystem mainParticleSystem;
    public ParticleSystem hexParticleSystem;
    public ParticleSystem poleParticleSystem;
    public ParticleSystem starsParticleSystem;
}