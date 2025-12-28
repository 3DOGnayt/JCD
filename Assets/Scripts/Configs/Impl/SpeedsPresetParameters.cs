using System.Collections.Generic;
using Data.Helpers;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/" + nameof(SpeedsPresetParameters), fileName = nameof(SpeedsPresetParameters))]
    public class SpeedsPresetParameters : ScriptableObject, ISpeedsPreset
    {
        [SerializeField] private List<CarSpeedSetup> _carSpeedSettings;

        public List<CarSpeedSetup> CarSpeedSettings => _carSpeedSettings;
    }
}