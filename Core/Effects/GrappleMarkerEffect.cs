using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for making the effect that indicates a spot on a cliff is moveable.
/// </summary>
public class GrappleMarkerEffect : MonoBehaviour
{
    public ParticleSystem shockwave;
    public ParticleSystem marker;
    public ParticleSystem circle;
    public ParticleSystem lightBeam;
    public ParticleSystem spot;
    public List<ParticleSystem> allEffects;
    private List<ParticleSystem> mainEffects;
    public INavigable grapplePoint { get; set; }

    public void OnEnable()
    {
        allEffects.Add(shockwave);
        allEffects.Add(marker);
        allEffects.Add(circle);
        allEffects.Add(lightBeam);
        allEffects.Add(spot);
        StopAllEffect();

        mainEffects = allEffects;
        mainEffects.Remove(lightBeam);
        SetColors(mainEffects, GridController.instance.gridBorderColor);

        var lightBeamMain = lightBeam.main;
        lightBeamMain.startColor = NavigableController.instance.cellHighlightColor;
    }

    private void SetColors(List<ParticleSystem> effectList, Color effectColor)
    {
        foreach (var effect in effectList)
        {
            var effectMain = effect.main;
            effectMain.startColor = effectColor;
        }
    }

    public void EnableGrappleHighlight()
    {
        PlayGrappleEffect(GrappleEffectsEnum.Shockwave);
        PlayGrappleEffect(GrappleEffectsEnum.Light);
    }

    public void DisableGrappleHighlight()
    {
        StopGrappleEffect(GrappleEffectsEnum.Shockwave);
        StopGrappleEffect(GrappleEffectsEnum.Light);
    }

    public void StopAllEffect()
    {
        foreach (var effect in allEffects) effect.Stop();
    }

    public void GrappleMovableEffect()
    {
        PlayGrappleEffect(GrappleEffectsEnum.Circle);
        PlayGrappleEffect(GrappleEffectsEnum.Marker);
        PlayGrappleEffect(GrappleEffectsEnum.Spot);
    }

    private void PlayAllEffects()
    {
        foreach (var effect in allEffects) effect.Play();
    }

    private void PlayGrappleEffect(GrappleEffectsEnum effectsEnum)
    {
        switch (effectsEnum)
        {
            case GrappleEffectsEnum.Shockwave:
                shockwave.Play();
                break;
            case GrappleEffectsEnum.Marker:
                marker.Play();
                break;
            case GrappleEffectsEnum.Circle:
                circle.Play();
                break;
            case GrappleEffectsEnum.Light:
                lightBeam.Play();
                break;
            case GrappleEffectsEnum.Spot:
                spot.Stop();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(effectsEnum), effectsEnum, null);
        }
    }

    private void StopGrappleEffect(GrappleEffectsEnum effectsEnum)
    {
        switch (effectsEnum)
        {
            case GrappleEffectsEnum.Shockwave:
                shockwave.Stop();
                break;
            case GrappleEffectsEnum.Marker:
                marker.Stop();
                break;
            case GrappleEffectsEnum.Circle:
                circle.Stop();
                break;
            case GrappleEffectsEnum.Light:
                lightBeam.Stop();
                break;
            case GrappleEffectsEnum.Spot:
                spot.Stop();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(effectsEnum), effectsEnum, null);
        }
    }
}

public enum GrappleEffectsEnum
{
    Shockwave,
    Marker,
    Circle,
    Light,
    Spot
}