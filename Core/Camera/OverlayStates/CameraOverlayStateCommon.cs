using Animancer.FSM;

namespace Core.Camera.OverlayStates
{
    public class CameraOverlayStateCommon : StateBehaviour, IOwnedState<CameraViewStateCommon>
    {
        public CameraOverlayStates overlayState;
        public override bool CanEnterState => true;
        public override bool CanExitState => true;
        public override void OnEnterState()
        {

        }
        public override void OnExitState()
        {
        }
        
        public virtual void Init()
        {
        }
        public StateMachine<CameraViewStateCommon> OwnerStateMachine { get; }
    }
}