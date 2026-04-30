using System.Collections.Generic;
using Data.HelperClass;

namespace Configs.Impl
{
    public partial class CarSelectionParameters
    {
        public float GetSpeedMaxKmh() => GetSpeedMaxKmhInternal(_selectedCarSpeedsPresetParameters);
        public float GetSpeedMaxKmh(CarSpeedsPresetParameters speedsPreset) => GetSpeedMaxKmhInternal(speedsPreset);

        public float GetBackSpeedMaxKmh() => GetBackSpeedMaxKmhInternal(_selectedCarSpeedsPresetParameters);
        public float GetBackSpeedMaxKmh(CarSpeedsPresetParameters speedsPreset) => GetBackSpeedMaxKmhInternal(speedsPreset);

        public int GetGearCount() => GetGearCountInternal(_selectedCarSpeedsPresetParameters);
        public int GetGearCount(CarSpeedsPresetParameters speedsPreset) => GetGearCountInternal(speedsPreset);

        private static float GetSpeedMaxKmhInternal(CarSpeedsPresetParameters speedsPreset)
        {
            var speedMaxKmh = 0f;

            if (!TryGetSpeedSettings(speedsPreset, out var settings))
                return speedMaxKmh;

            for (var i = 0; i < settings.Count; i++)
            {
                var setting = settings[i];
                if ((int)setting.EGear <= 0)
                    continue;

                if (setting.SpeedLimit > speedMaxKmh)
                    speedMaxKmh = setting.SpeedLimit;
            }

            return speedMaxKmh;
        }

        private static float GetBackSpeedMaxKmhInternal(CarSpeedsPresetParameters speedsPreset)
        {
            var backSpeedMaxKmh = 0f;

            if (!TryGetSpeedSettings(speedsPreset, out var settings))
                return backSpeedMaxKmh;

            for (var i = 0; i < settings.Count; i++)
            {
                var setting = settings[i];
                if ((int)setting.EGear >= 0)
                    continue;

                if (setting.SpeedLimit > backSpeedMaxKmh)
                    backSpeedMaxKmh = setting.SpeedLimit;
            }

            return backSpeedMaxKmh;
        }

        private static int GetGearCountInternal(CarSpeedsPresetParameters speedsPreset)
        {
            var gearCount = 0;

            if (!TryGetSpeedSettings(speedsPreset, out var settings))
                return gearCount;

            for (var i = 0; i < settings.Count; i++)
            {
                var setting = settings[i];
                var gearValue = (int)setting.EGear;
                if (gearValue > gearCount)
                    gearCount = gearValue;
            }

            return gearCount;
        }

        private static bool TryGetSpeedSettings(CarSpeedsPresetParameters speedsPreset, out List<CarSpeedSetup> settings)
        {
            settings = null;

            if (speedsPreset == null)
                return false;

            settings = speedsPreset.CarSpeedSettings;
            return settings != null && settings.Count > 0;
        }
    }
}