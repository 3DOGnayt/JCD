using System.Collections.Generic;
using Configs.Impl;
using Data;
using Services;
using UnityEngine;
using Zenject;

namespace Core
{
    public class CarView : MonoBehaviour
    {
        [SerializeField] private CarPreset _carPreset;
        [Space]
        [SerializeField] private CarSetup _carSetup;
        [Space]
        public List<WheelInfo> _wheelInfos;

        private IInputService _inputService;

        [Inject]
        public void Construct(IInputService inputService)
        {
            _inputService = inputService;
        }

        private void FixedUpdate()
        {
            _inputService.ApplySpeed_Test(_carSetup, _wheelInfos);
        }
    }
}