namespace Core.Camera.OverlayStates
{
    public class GrenadeOverlayState : CameraOverlayStateCommon
    {
        public override void Init()
        {
            base.Init();
            overlayState = CameraOverlayStates.Grenade;
        }
    }
}