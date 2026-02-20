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
        [SerializeField] private CarSelectionParameters _carSelectionParameters;
        [SerializeField] private MapSelectionParameters _mapSelectionParameters;
        [SerializeField] private GameModeSelectionParameters _gameModeSelectionParameters;
        [SerializeField] private OpponentSelectionParameters _opponentSelectionParameters;
        [SerializeField] private AudioSelectionParameters _audioSelectionParameters;
        [Space]
        [SerializeField] private CarCatalogParameters _carCatalogParameters;
        [SerializeField] private MapCatalogParameters _mapCatalogParameters;
        [SerializeField] private OpponentCatalogParameters _opponentCatalogParameters;
        [SerializeField] private TrainingTimeScoreParameters _trainingTimeScoreParameters;

        public override void InstallBindings()
        {
            Container.Bind<CarUISmoothingParameters>().FromInstance(_carUISmoothingParameters).AsSingle();
            Container.Bind<CarSkidSmokeParameters>().FromInstance(_carCarSkidSmokeParameters).AsSingle();
            Container.Bind<CarLightsParameters>().FromInstance(_carLightsParameters).AsSingle();
            
            Container.Bind<CarSelectionParameters>().FromInstance(_carSelectionParameters).AsSingle();
            Container.Bind<MapSelectionParameters>().FromInstance(_mapSelectionParameters).AsSingle();
            Container.Bind<GameModeSelectionParameters>().FromInstance(_gameModeSelectionParameters).AsSingle();
            Container.Bind<OpponentSelectionParameters>().FromInstance(_opponentSelectionParameters).AsSingle();
            Container.Bind<AudioSelectionParameters>().FromInstance(_audioSelectionParameters).AsSingle();

            var gameSelectionParameters = ScriptableObject.CreateInstance<GameSelectionParameters>();
            
            gameSelectionParameters.SetSources(_carSelectionParameters, _mapSelectionParameters,
                _gameModeSelectionParameters, _opponentSelectionParameters, _audioSelectionParameters);
            
            Container.Bind<GameSelectionParameters>().FromInstance(gameSelectionParameters).AsSingle();
            
            Container.Bind<CarCatalogParameters>().FromInstance(_carCatalogParameters).AsSingle();
            Container.Bind<MapCatalogParameters>().FromInstance(_mapCatalogParameters).AsSingle();
            Container.Bind<OpponentCatalogParameters>().FromInstance(_opponentCatalogParameters).AsSingle();
            Container.Bind<TrainingTimeScoreParameters>().FromInstance(_trainingTimeScoreParameters).AsSingle();
        }
    }
}