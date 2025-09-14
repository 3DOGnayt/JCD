using System.Collections.Generic;
using Configs.Impl;
using Data;
using UnityEngine;

namespace Core
{
    public class CarView : MonoBehaviour
    {
        [SerializeField] private CarPreset _carPreset;
        [Space]
        [SerializeField] private CarSetup _carSetup;
        [Space]
        [SerializeField] private List<WheelInfo> _wheelInfos;

        public CarPreset CarPreset => _carPreset;
        public CarSetup CarSetup => _carSetup;
        public List<WheelInfo> CarWheelInfos { get => _wheelInfos; set => _wheelInfos = value; }
    }
}