using Configs.Impl;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class ConfigsInstaller : MonoInstaller
    {
        [SerializeField] private CarUISmoothingParameters _carUISmoothingParameters;
        [SerializeField] private CarSkidSmokeParameters _carCarSkidSmokeParameters;
        [SerializeField] private CarLightsParameters _carLightsParameters;
        [Space]
        [SerializeField] private GameSelectionParameters _gameSelectionParameters;
        [SerializeField] private CarCatalogParameters _carCatalogParameters;
        [SerializeField] private MapCatalogParameters _mapCatalogParameters;
        [SerializeField] private OpponentCatalogParameters _opponentCatalogParameters;
        [SerializeField] private TrainingTimeScoreParameters _trainingTimeScoreParameters;

        public override void InstallBindings()
        {
            Container.Bind<CarUISmoothingParameters>().FromInstance(_carUISmoothingParameters).AsSingle();
            Container.Bind<CarSkidSmokeParameters>().FromInstance(_carCarSkidSmokeParameters).AsSingle();
            Container.Bind<CarLightsParameters>().FromInstance(_carLightsParameters).AsSingle();
            
            Container.Bind<GameSelectionParameters>().FromInstance(_gameSelectionParameters).AsSingle();
            Container.Bind<CarCatalogParameters>().FromInstance(_carCatalogParameters).AsSingle();
            Container.Bind<MapCatalogParameters>().FromInstance(_mapCatalogParameters).AsSingle();
            Container.Bind<OpponentCatalogParameters>().FromInstance(_opponentCatalogParameters).AsSingle();
            Container.Bind<TrainingTimeScoreParameters>().FromInstance(_trainingTimeScoreParameters).AsSingle();
        }
    }
}