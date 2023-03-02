using Core.Unit;

namespace Core.Helpers
{
    public enum TargetFormattingTypes
    {
        Standard,
        Replace
    }
    /// <summary>
    /// Provides information about Unit Targeting objects.
    /// </summary>
    public static class TargetHelpers
    {
        private static readonly string headTarget = "Head Target";
        private static readonly string chestTarget = "Chest Target";
        private static readonly string rightArmTarget = "Right Arm Target";
        private static readonly string rightHandTarget = "Right Hand Target";
        private static readonly string rightLegTarget = "Right Leg Target";
        private static readonly string rightFootTarget = "Right Foot Target";
        private static readonly string leftArmTarget = "Left Arm Target";
        private static readonly string leftHandTarget = "Left Hand Target";
        private static readonly string leftLegTarget = "Left Leg Target";
        private static readonly string leftFootTarget = "Left Foot Target";

        public static TargetingObjectType AssignTargetType(string targetHelper)
        {
            targetHelper = targetHelper.ToUpper();
            var objectType = TargetingObjectType.None;
            if (targetHelper == headTarget.ToUpper()) objectType = TargetingObjectType.Head;
            if (targetHelper == chestTarget.ToUpper()) objectType = TargetingObjectType.Chest;
            if (targetHelper == rightArmTarget.ToUpper()) objectType = TargetingObjectType.RightArm;
            if (targetHelper == rightHandTarget.ToUpper()) objectType = TargetingObjectType.RightHand;
            if (targetHelper == rightLegTarget.ToUpper()) objectType = TargetingObjectType.RightLeg;
            if (targetHelper == rightFootTarget.ToUpper()) objectType = TargetingObjectType.RightFoot;
            if (targetHelper == leftArmTarget.ToUpper()) objectType = TargetingObjectType.LeftArm;
            if (targetHelper == leftHandTarget.ToUpper()) objectType = TargetingObjectType.LeftHand;
            if (targetHelper == leftLegTarget.ToUpper()) objectType = TargetingObjectType.LeftLeg;
            if (targetHelper == leftFootTarget.ToUpper()) objectType = TargetingObjectType.LeftFoot;
            return objectType;
        }

        public static string TargetTypeName(TargetingObjectType objectType, TargetFormattingTypes formatting)
        {
            var objectName = objectType switch
            {
                TargetingObjectType.None => "None",
                TargetingObjectType.Head => headTarget,
                TargetingObjectType.Chest => chestTarget,
                TargetingObjectType.RightArm => rightArmTarget,
                TargetingObjectType.RightHand => rightHandTarget,
                TargetingObjectType.LeftArm => leftArmTarget,
                TargetingObjectType.LeftHand => leftHandTarget,
                TargetingObjectType.RightLeg => rightLegTarget,
                TargetingObjectType.RightFoot => rightFootTarget,
                TargetingObjectType.LeftLeg => leftLegTarget,
                TargetingObjectType.LeftFoot => leftFootTarget,
                _ => "None"
            };
            var formattedName = formatting switch
            {
                TargetFormattingTypes.Standard => objectName.ToUpper(),
                TargetFormattingTypes.Replace => objectName.Replace("Target", "").ToUpper().Trim(),
                _ => objectName
            };
            return formattedName;
        }
    }
}