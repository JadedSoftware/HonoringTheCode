namespace Core.Camera
{
    /// <summary>
    /// Standard topdown camera view state
    /// </summary>
    public class TopdownCameraViewState : CameraViewStateCommon
    {
        protected override void Awake()
        {
            viewState = CameraViewStates.Topdown;
            base.Awake();
        }

        protected override void LateUpdate()
        {
            camControl.ZoomTopdown();
        }
    }
}