using System.Collections.Generic;
using Data.HelperClass;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/SelectionParameters/" + nameof(CarSelectionParameters), fileName = nameof(CarSelectionParameters), order = 1)]
    public class CarSelectionParameters : ScriptableObject
    {
        [SerializeField] private CarPresetParameters _selectedCar;
        [SerializeField] private CarMovementParameters _selectedCarMovementParameters;
        [SerializeField] private CarSpeedsPresetParameters _selectedCarSpeedsPresetParameters;
        [SerializeField] private CarSlipParameters _selectedCarSlipParameters;
        [SerializeField] private CarEngineAudioParameters _selectedCarEngineAudioParameters;
        [SerializeField] private int _selectedCarIndex;

        public CarPresetParameters SelectedCar => _selectedCar;
        public CarMovementParameters MovementParameters => _selectedCarMovementParameters;
        public CarSpeedsPresetParameters CarSpeedsPresetParameters => _selectedCarSpeedsPresetParameters;
        public CarSlipParameters SlipParameters => _selectedCarSlipParameters;
        public CarEngineAudioParameters SelectedCarEngineAudioParameters => _selectedCarEngineAudioParameters;
        public int SelectedCarIndex => _selectedCarIndex;

        public float GetSpeedMaxKmh()
        {
            var speedMaxKmh = 0f;

            if (!TryGetSpeedSettings(out var settings))
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

        public float GetBackSpeedMaxKmh()
        {
            var backSpeedMaxKmh = 0f;

            if (!TryGetSpeedSettings(out var settings))
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

        public int GetGearCount()
        {
            var gearCount = 0;

            if (!TryGetSpeedSettings(out var settings))
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

        private bool TryGetSpeedSettings(out List<CarSpeedSetup> settings)
        {
            settings = null;

            if (_selectedCarSpeedsPresetParameters == null)
                return false;

            settings = _selectedCarSpeedsPresetParameters.CarSpeedSettings;
            return settings != null && settings.Count > 0;
        }

        public void SetSelectedCar(
            CarPresetParameters car,
            CarMovementParameters movementParameters,
            CarSpeedsPresetParameters speedsPresetParameters,
            CarSlipParameters slipParameters,
            CarEngineAudioParameters engineAudioParameters,
            int index
        )
        {
            _selectedCar = car;
            _selectedCarMovementParameters = movementParameters;
            _selectedCarSpeedsPresetParameters = speedsPresetParameters;
            _selectedCarSlipParameters = slipParameters;
            _selectedCarEngineAudioParameters = engineAudioParameters;
            _selectedCarIndex = index;
        }
    }
}