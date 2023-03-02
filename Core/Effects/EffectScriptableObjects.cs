using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds the effects and types
/// </summary>
[CreateAssetMenu(fileName = "EffectsScriptableObjects", menuName = "ScriptableObjects/EffectsObjects", order = 1)]
public class EffectScriptableObjects : ScriptableObject
{
    [SerializeField] public List<EffectColorGradient> effectsGradientColors;
    [SerializeField] public List<HexHighlightEffects> hexHighlightEffects;
}


[Serializable]
public struct HexHighlightEffects
{
    public GameObject vfxEffect;
    public HexEffect particleEffect;
    public EffectType effectType;
    public EffectActionType actionType;
}

[Serializable]
public struct EffectColorGradient
{
    public ColorType color;
    public Gradient gradient;
}

public enum EffectType
{
    Particle,
    VfxGraph
}

public enum EffectActionType
{
    CellHighlighted,
    CellSelected,
    UnitSelected,
    MoveEffect,
    AttackEffect,
    HitEffect
}

public enum HexHighlightType
{
    Regular,
    Extra,
    Mini,
    Round
}

[Serializable]
public enum ColorType
{
    Yellow,
    Green,
    Purple,
    Blue,
    Red,
    White
}

public static class PaletteColors
{
    private static Color[] colors =
    {
        new(0.125f, 0.08103875f, 0f),
        new(0.375f, 0.2431163f, 0f),
        new(0.625f, 0.4051937f, 0f),
        new(0.875f, 0.5672712f, 0f),
        new(1f, 0.6922712f, 0.125f),
        new(1f, 0.7801937f, 0.375f),
        new(1f, 0.8681163f, 0.625f),
        new(1f, 0.9560388f, 0.875f),
        new(0.09287384f, 0.09964179f, 0f),
        new(0.2786215f, 0.2989254f, 0f),
        new(0.4643692f, 0.498209f, 0f),
        new(0.6501169f, 0.6974925f, 0f),
        new(0.7751169f, 0.8224925f, 0.125f),
        new(0.8393692f, 0.8732089f, 0.375f),
        new(0.9036216f, 0.9239254f, 0.625f),
        new(0.9678738f, 0.9746418f, 0.875f),
        new(0.06074768f, 0.1182448f, 0f),
        new(0.182243f, 0.3547345f, 0f),
        new(0.3037384f, 0.5912241f, 0f),
        new(0.4252338f, 0.8277138f, 0f),
        new(0.5502338f, 0.9527138f, 0.125f),
        new(0.6787384f, 0.9662241f, 0.375f),
        new(0.807243f, 0.9797345f, 0.625f),
        new(0.9357477f, 0.9932448f, 0.875f),
        new(0.03037384f, 0.07772544f, 0.05664215f),
        new(0.09112152f, 0.2331763f, 0.1699265f),
        new(0.1518692f, 0.3886272f, 0.2832108f),
        new(0.2126169f, 0.5440781f, 0.3964951f),
        new(0.3376169f, 0.6690781f, 0.521495f),
        new(0.5268692f, 0.7636272f, 0.6582108f),
        new(0.7161216f, 0.8581764f, 0.7949265f),
        new(0.9053738f, 0.9527254f, 0.9316422f),
        new(0f, 0.03720605f, 0.1132843f),
        new(0f, 0.1116182f, 0.3398529f),
        new(0f, 0.1860303f, 0.5664215f),
        new(0f, 0.2604424f, 0.7929901f),
        new(0.125f, 0.3854424f, 0.9179901f),
        new(0.375f, 0.5610303f, 0.9414215f),
        new(0.625f, 0.7366182f, 0.9648529f),
        new(0.875f, 0.9122061f, 0.9882843f),
        new(0.0625f, 0.01860303f, 0.06476036f),
        new(0.1875f, 0.05580908f, 0.1942811f),
        new(0.3125f, 0.09301513f, 0.3238018f),
        new(0.4375f, 0.1302212f, 0.4533225f),
        new(0.5625f, 0.2552212f, 0.5783225f),
        new(0.6875f, 0.4680151f, 0.6988018f),
        new(0.8125f, 0.6808091f, 0.8192811f),
        new(0.9375f, 0.893603f, 0.9397603f),
        new(0.125f, 0f, 0.0162364f),
        new(0.375f, 0f, 0.04870921f),
        new(0.625f, 0f, 0.08118201f),
        new(0.875f, 0f, 0.1136548f),
        new(1f, 0.125f, 0.2386548f),
        new(1f, 0.375f, 0.456182f),
        new(1f, 0.625f, 0.6737092f),
        new(1f, 0.875f, 0.8912364f),
        new(0.125f, 0.04051938f, 0.008118201f),
        new(0.375f, 0.1215581f, 0.0243546f),
        new(0.625f, 0.2025969f, 0.04059101f),
        new(0.875f, 0.2836356f, 0.05682741f),
        new(1f, 0.4086356f, 0.1818274f),
        new(1f, 0.5775969f, 0.415591f),
        new(1f, 0.7465581f, 0.6493546f),
        new(1f, 0.9155194f, 0.8831182f)
    };
}