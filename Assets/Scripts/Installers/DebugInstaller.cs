using Helpers;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class DebugInstaller : MonoInstaller
    {
        [SerializeField] private RaceHudVisibilityController _visibilityController;

        public override void InstallBindings()
        {
            var canvas = Container.Resolve<Canvas>();
            
            var raceHud = Container.InstantiatePrefabForComponent<RaceHudVisibilityController>(_visibilityController, canvas.transform);
            Container.Bind<RaceHudVisibilityController>().FromInstance(raceHud).AsSingle();
        }
    }
}