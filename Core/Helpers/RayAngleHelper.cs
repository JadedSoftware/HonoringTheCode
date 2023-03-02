using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Helpers
{
    /// <summary>
    /// Provides reference to the bearing angles between units.
    /// Contains a list of angle directions, whether they are forward or down
    /// </summary>
    public static class RayAngleHelper
    {
        /// <summary>
        ///     Angles compared to Vector3.Up
        /// </summary>
        public enum RayAnglesEnum
        {
            FiftyFive = 1,
            OneHundredFifteen = 2,
            OneSeventy = 3,
            OneThirty = 4,
            SeventyFive = 5,
            Fifteen = 6,
            Fourty = 7,
            Onehundred = 8,
            OneFiftyFive = 9,
            OneFourtyFive = 10,
            Ninety = 11,
            Thirty = 12,
            OneEighty = 22,
            Zero = 44
        }

        public static List<RayAnglesEnum> ForwardAngles = new()
        {
            RayAnglesEnum.Zero, RayAnglesEnum.Fifteen, RayAnglesEnum.Thirty, RayAnglesEnum.Fourty,
            RayAnglesEnum.FiftyFive, RayAnglesEnum.SeventyFive, RayAnglesEnum.Ninety
        };

        public static List<RayAnglesEnum> BehindAngles = new()
        {
            RayAnglesEnum.Ninety, RayAnglesEnum.Onehundred, RayAnglesEnum.OneHundredFifteen, RayAnglesEnum.OneThirty,
            RayAnglesEnum.OneFourtyFive, RayAnglesEnum.OneFiftyFive, RayAnglesEnum.OneSeventy, RayAnglesEnum.OneEighty
        };

        public static List<RayAnglesEnum> AllAngels = new()
        {
            RayAnglesEnum.Zero, RayAnglesEnum.Fifteen, RayAnglesEnum.Thirty, RayAnglesEnum.Fourty,
            RayAnglesEnum.FiftyFive, RayAnglesEnum.SeventyFive, RayAnglesEnum.Ninety,
            RayAnglesEnum.Onehundred, RayAnglesEnum.OneHundredFifteen, RayAnglesEnum.OneThirty,
            RayAnglesEnum.OneFourtyFive, RayAnglesEnum.OneFiftyFive, RayAnglesEnum.OneSeventy, RayAnglesEnum.OneEighty
        };

        public static Color RayAngleColor(RayAnglesEnum angleEnum)
        {
            var angleColor = angleEnum switch
            {
                RayAnglesEnum.FiftyFive => new Color(255, 51, 51),
                RayAnglesEnum.OneHundredFifteen => new Color(255, 153, 51),
                RayAnglesEnum.OneSeventy => new Color(255, 255, 51),
                RayAnglesEnum.OneThirty => new Color(153, 255, 51),
                RayAnglesEnum.SeventyFive => new Color(51, 255, 51),
                RayAnglesEnum.Fifteen => new Color(51, 255, 153),
                RayAnglesEnum.Fourty => new Color(255, 195, 51),
                RayAnglesEnum.Onehundred => new Color(213, 255, 51),
                RayAnglesEnum.OneFiftyFive => new Color(111, 255, 51),
                RayAnglesEnum.OneFourtyFive => new Color(51, 255, 93),
                RayAnglesEnum.Ninety => new Color(51, 255, 195),
                RayAnglesEnum.Thirty => new Color(51, 213, 255),
                RayAnglesEnum.OneEighty => new Color(115, 51, 255),
                RayAnglesEnum.Zero => new Color(217, 51, 255),
                _ => throw new ArgumentOutOfRangeException(nameof(angleEnum), angleEnum, null)
            };
            return angleColor;
        }
    }
}