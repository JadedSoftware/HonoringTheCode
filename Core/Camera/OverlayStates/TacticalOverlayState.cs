namespace Core.Camera.OverlayStates
{
    public class TacticalOverlayState : CameraOverlayStateCommon
    {
        public override void Init()
        {
            base.Init();
            overlayState = CameraOverlayStates.Tactical;
        }
    }
}