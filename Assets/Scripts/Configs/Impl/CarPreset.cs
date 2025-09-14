using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/CarPreset", fileName = "CarPreset")]
    public class CarPreset : ScriptableObject, ICarPreset
    {
        [SerializeField] private GameObject _car;
        [Header("GAME CAR PARAMETERS")]
        [SerializeField] private CarSetup _carSetup;
        [Header("CAR PARAMETERS")]
        [Space]
        [SerializeField] private CarParameters _carParameters;
        [Space]
        [Header("WHEELS")]
        [Space]
        [SerializeField] private WheelParameters _frontWheelParameters;
        [Space]
        [SerializeField] private WheelParameters _backWheelParameters;
        [Space]
        [SerializeField] private List<WheelInfo> _wheelInfos;

        public GameObject Car => _car;
        public CarSetup CarSetup => _carSetup;
        public CarParameters CarParameters => _carParameters;
        public WheelParameters FrontWheelParameters => _frontWheelParameters;
        public WheelParameters BackWheelParameters => _backWheelParameters;
        public List<WheelInfo> WheelInfos => _wheelInfos;
    }
}