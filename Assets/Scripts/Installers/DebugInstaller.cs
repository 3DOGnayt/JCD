using UI.Helpers;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class DebugInstaller : MonoInstaller
    {
        [SerializeField] private RaceHudVisibilityController _visibilityController;
        //[SerializeField] private CarParametersView _carParametersView;
        //[SerializeField] private ResetCar _resetCar;

        public override void InstallBindings()
        {
            var canvas = Container.Resolve<Canvas>();
            
            var raceHud = Container.InstantiatePrefabForComponent<RaceHudVisibilityController>(_visibilityController, canvas.transform);
            
            //var carParametersView = Container.InstantiatePrefabForComponent<CarParametersView>(_carParametersView);
            //var resetCar = Container.InstantiatePrefabForComponent<ResetCar>(_resetCar);

            Container.Bind<RaceHudVisibilityController>().FromInstance(raceHud).AsSingle();
            //Container.Bind<CarParametersView>().FromInstance(carParametersView).AsSingle();
            //Container.Bind<ResetCar>().FromInstance(resetCar).AsSingle();
        }
    }
}