using Configs.Impl;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class CarConfigInstaller : MonoInstaller
    {
        [SerializeField] private CarUISmoothingParameters _carUISmoothingParameters;
        [SerializeField] private CarSkidSmokeParameters _carCarSkidSmokeParameters;
        [SerializeField] private CarLightsParameters _carLightsParameters;
        [Space]
        [SerializeField] private GameSelectionParameters _gameSelectionParameters;
        [SerializeField] private CarCatalog _carCatalog;
        [SerializeField] private MapCatalog _mapCatalog;
        [SerializeField] private OpponentCatalog _opponentCatalog;

        public override void InstallBindings()
        {
            Container.Bind<CarUISmoothingParameters>().FromInstance(_carUISmoothingParameters).AsSingle();
            Container.Bind<CarSkidSmokeParameters>().FromInstance(_carCarSkidSmokeParameters).AsSingle();
            Container.Bind<CarLightsParameters>().FromInstance(_carLightsParameters).AsSingle();
            
            Container.Bind<GameSelectionParameters>().FromInstance(_gameSelectionParameters).AsSingle();
            Container.Bind<CarCatalog>().FromInstance(_carCatalog).AsSingle();
            Container.Bind<MapCatalog>().FromInstance(_mapCatalog).AsSingle();
            Container.Bind<OpponentCatalog>().FromInstance(_opponentCatalog).AsSingle();
        }
    }
}
