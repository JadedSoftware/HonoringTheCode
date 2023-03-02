namespace Core.Camera.OverlayStates
{
    public class MeleeAttackOverlayState : CameraOverlayStateCommon
    {
        public override void Init()
        {
            base.Init();
            overlayState = CameraOverlayStates.MeleeAttack;
        }
    }
}