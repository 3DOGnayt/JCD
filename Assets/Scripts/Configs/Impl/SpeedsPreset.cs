using System.Collections.Generic;
using Configs.Helpers;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/SpeedsPreset", fileName = "SpeedsPreset")]
    public class SpeedsPreset : ScriptableObject, ISpeedsPreset
    {
        [SerializeField] private List<CarSpeedSettings> _carSpeedSettings;

        public List<CarSpeedSettings> CarSpeedSettings => _carSpeedSettings;
    }
}