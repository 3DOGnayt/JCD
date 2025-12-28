using System.Collections.Generic;
using Data.Helpers;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/" + nameof(CarSpeedsPresetParameters), fileName = nameof(CarSpeedsPresetParameters))]
    public class CarSpeedsPresetParameters : ScriptableObject, ISpeedsPreset
    {
        [SerializeField] private List<CarSpeedSetup> _carSpeedSettings;

        public List<CarSpeedSetup> CarSpeedSettings => _carSpeedSettings;
    }
}