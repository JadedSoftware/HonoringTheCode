using Animancer;
using Animation;

namespace Core.Unit.Interfaces
{
    public interface IAnimate
    {
        public AnimancerComponent animancer { get; set; }
        public AnimationContainer animContainer { get; set; }
    }
}