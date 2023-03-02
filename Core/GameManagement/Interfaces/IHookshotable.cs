using Core.Effects;
using UnityEngine;

namespace Core.GameManagement.Interfaces
{
    public interface IHookshotable
    {
        public HookshotEffect hookshotEffect { get; set; }
        Vector3 GetHookshotPosition();
    }
}