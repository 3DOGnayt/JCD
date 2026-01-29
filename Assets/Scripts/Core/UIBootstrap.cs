using System;
using KoboldUi.Services.WindowsService;
using UI.Canvas.Window;
using UniRx;
using Zenject;

namespace Core
{
    public class UIBootstrap : IInitializable, IDisposable
    {
        private readonly CompositeDisposable _compositeDisposable = new();

        private readonly ILocalWindowsService _localWindowsService;

        public UIBootstrap(ILocalWindowsService localWindowsService)
        {
            _localWindowsService = localWindowsService;
        }

        public void Initialize()
        {
            OpenMainMenu();
        }

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
        }

        private void OpenMainMenu()
        {
            _localWindowsService.OpenWindow<MainMenuWindow>();
            _compositeDisposable.Dispose();
        }
    }
}