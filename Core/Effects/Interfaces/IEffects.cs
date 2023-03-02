using UnityEngine;

namespace Core.Effects.Interfaces
{
    public interface IEffects
    {
        void SetPosition(Vector3 _pos);
        void SetGradient(Gradient _gradient);
        void SetColor(Color _color);
    }
}