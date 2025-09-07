using Configs.Impl;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class CarConfigInstaller : MonoInstaller
    {
        [SerializeField] private CarPreset _carPreset;
        
        public override void InstallBindings()
        {
            Container.Bind<CarPreset>().FromInstance(_carPreset).AsSingle();
        }
    }
}