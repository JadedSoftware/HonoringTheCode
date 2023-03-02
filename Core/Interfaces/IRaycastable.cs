using UnityEngine;

namespace Core.Interfaces
{
    public interface IRaycastable
    {
        Collider GetCollider();
        LayersEnum GetLayer();
    }
}