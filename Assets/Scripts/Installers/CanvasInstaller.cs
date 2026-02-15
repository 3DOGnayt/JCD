using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Installers
{
    [CreateAssetMenu(menuName = "Game/UI/" + nameof(CanvasInstaller), fileName = nameof(CanvasInstaller), order = 0)]
    public class CanvasInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private EventSystem _eventSystem;

        public override void InstallBindings()
        {
            var canvasInstance = Container.InstantiatePrefabForComponent<Canvas>(_canvas);
            var eventSystemInstance = Container.InstantiatePrefabForComponent<EventSystem>(_eventSystem);
            
            Container.Bind<Canvas>().FromInstance(canvasInstance).AsSingle();
            Container.Bind<EventSystem>().FromInstance(eventSystemInstance).AsSingle();
        }
    }
}