using Core.Effects.Interfaces;
using UnityEngine;

public abstract class EffectsCommon : MonoBehaviour, IEffects
{
    private Color color;
    private Gradient gradient;

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    public void SetGradient(Gradient _gradient)
    {
        gradient = _gradient;
    }

    public void SetColor(Color _color)
    {
        color = _color;
    }
}

