using Configs.Impl;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class CarConfigInstaller : MonoInstaller
    {
        [SerializeField] private CarPreset _carPreset;
        [SerializeField] private CarMovementParameters _carMovementParameters;
        
        public override void InstallBindings()
        {
            Container.Bind<CarPreset>().FromInstance(_carPreset).AsSingle();
            Container.Bind<CarMovementParameters>().FromInstance(_carMovementParameters).AsSingle();
        }
    }
}