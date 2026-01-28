using KoboldUi.Services.WindowsService.Impl;
using KoboldUi.Utils;
using UI.Canvas.MenuSelection;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Installers
{
    public class UIInstaller : MonoInstaller
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private EventSystem _eventSystem;
        [SerializeField] private MenuSelectionWindow _menuSelectionWindowPrefab;
        [SerializeField] private MenuSelectionTransitionsConfig _menuSelectionTransitions;
        
        public override void InstallBindings()
        {
            Container.Bind<Canvas>().FromInstance(_canvas).AsSingle();
            Container.Bind<EventSystem>().FromInstance(_eventSystem).AsSingle();
            Container.Bind<MenuSelectionTransitionsConfig>().FromInstance(_menuSelectionTransitions).AsSingle();
            Container.BindInterfacesTo<LocalWindowsService>().AsSingle().NonLazy();
            Container.BindWindowFromPrefab(_canvas, _menuSelectionWindowPrefab);
            Container.BindInterfacesTo<MenuSelectionBootstrap>().AsSingle().NonLazy();
        }
    }
}
