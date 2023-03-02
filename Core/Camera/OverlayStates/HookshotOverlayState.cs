namespace Core.Camera.OverlayStates
{
    public class HookshotOverlayState : CameraOverlayStateCommon
    {
        public override void Init()
        {
            base.Init();
            overlayState = CameraOverlayStates.Hookshot;
        }
    }
}