namespace Core.Camera.OverlayStates
{
    public class MovementOverlayState : CameraOverlayStateCommon
    {
        public override void Init()
        {
            base.Init();
            overlayState = CameraOverlayStates.Movement;
        }
    }
}