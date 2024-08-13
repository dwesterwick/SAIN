using RootMotion.FinalIK;
using SAIN.Attributes;
using SAIN.Helpers;
using System.Threading.Tasks;
using System;
using UnityEngine.UIElements.Experimental;
using System.Collections.Generic;

namespace SAIN.Preset.GlobalSettings
{
    public class VisionConeSettings : SAINSettingsBase<VisionConeSettings>, ISAINSettings
    {
        [NameAndDescription("Nighttime Minimum Visible Angle",
            "The lowest a bot's vision angle can be as night falls when unassisted by flashlights or NVGS.")]
        [MinMax(10, 180)]
        [Advanced]
        public float Visible_Angle_Minimum = 30;

        [NameAndDescription("Nighttime Minimum Visible Angle for Known Enemy",
            "The lowest a bot's vision angle can be as night falls when unassisted by flashlights or NVGS, if their enemy is known to them (previously seen or heard)")]
        [MinMax(10, 180)]
        [Advanced]
        public float Visible_Angle_Minimum_KnownEnemy = 30;
    }

    public class LookSettings : SAINSettingsBase<LookSettings>, ISAINSettings
    {
        [Name("Vision Speed Settings")]
        public VisionSpeedSettings VisionSpeed = new VisionSpeedSettings();

        [Name("Vision Distance Settings")]
        public VisionDistanceSettings VisionDistance = new VisionDistanceSettings();

        [Name("Vision Cone Settings")]
        public VisionConeSettings VisionCone = new VisionConeSettings();

        [Name("Not Looking At Bot Settings")]
        public NotLookingSettings NotLooking = new NotLookingSettings();

        [Name("No Bush ESP")]
        public NoBushESPSettings NoBushESP = new NoBushESPSettings();

        [Name("Time Settings")]
        public TimeSettings Time = new TimeSettings();

        [Name("Flashlights and NVGs Settings")]
        public LightNVGSettings Light = new LightNVGSettings();

        public override void Init(List<ISAINSettings> list)
        {
            VisionSpeed.Init(list);
            list.Add(VisionSpeed);
            list.Add(VisionDistance);
            list.Add(VisionCone);
            list.Add(NotLooking);
            list.Add(NoBushESP);
            list.Add(Time);
            list.Add(Light);
        }
    }
}