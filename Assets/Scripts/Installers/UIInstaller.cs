using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Installers
{
    public class UIInstaller : MonoInstaller
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private EventSystem _eventSystem;
        
        public override void InstallBindings()
        {
            Container.Bind<Canvas>().FromInstance(_canvas).AsSingle();
            Container.Bind<EventSystem>().FromInstance(_eventSystem).AsSingle();
        }
    }
}