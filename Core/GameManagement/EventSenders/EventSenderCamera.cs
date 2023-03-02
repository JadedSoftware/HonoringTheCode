using Core.Camera;
using Core.Unit.Targeting;

namespace Core.GameManagement.EventSenders
{
    public static partial class EventSenderController
    {
        //------------ Camera ----------------//
        public delegate void OnTopdownView();

        public static event OnTopdownView enterTopdownView;

        public delegate void OnAttackView();

        public static event OnAttackView enterAttackView;

        public delegate void OffAttackView();

        public static event OffAttackView exitAttackView;

        public delegate void OnUnitTargeted(UnitCommon unitTarget);

        public static event OnUnitTargeted onUnitTargeted;

        public delegate void OnTargetingObject(TargetingObject target);

        public static event OnTargetingObject engageTargetingObject;

        public delegate void AlterTargetingObject(TargetingObject target);

        public static event AlterTargetingObject changeTargetingObject;

        public delegate void ExitTargetingObject();

        public static event ExitTargetingObject disengageTargetingObject;

        public delegate void OnOverlayChanged(CameraOverlayStates overlayState);

        public static event OnOverlayChanged onOverlayChanged;
        
        
        //------------ Static Methods ----------------//

        public static void EnterAttackView()
        {
            ScheduleEvent(() => enterAttackView?.Invoke());
        }

        public static void ExitAttackView()
        {
            ScheduleEvent(() => exitAttackView?.Invoke());
        }

        public static void UnitIsTargeted(UnitCommon currentTarget)
        {
            ScheduleEvent(() => onUnitTargeted?.Invoke(currentTarget));
        }

        public static void EnterTopdownView()
        {
            ScheduleEvent(() => enterTopdownView?.Invoke());
        }

        public static void EngageTargetingObject(TargetingObject currentTargetingObject)
        {
            ScheduleEvent(() => engageTargetingObject?.Invoke(currentTargetingObject));
        }

        public static void ChangeTargetingObject(TargetingObject currentTargetingObject)
        {
            ScheduleEvent(() => changeTargetingObject?.Invoke(currentTargetingObject));
        }

        public static void DisengageTargetingObject()
        {
            ScheduleEvent(() => disengageTargetingObject?.Invoke());
        }

        public static void OverlayChanged(CameraOverlayStates overlayState)
        {
            ScheduleEvent(() => onOverlayChanged?.Invoke(overlayState));
        }
    }
}