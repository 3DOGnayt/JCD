using KoboldUi.Services.WindowsService;
using Zenject;

namespace UI.Canvas.MenuSelection
{
    public class MenuSelectionBootstrap : IInitializable
    {
        private readonly ILocalWindowsService _windowsService;

        public MenuSelectionBootstrap(ILocalWindowsService windowsService)
        {
            _windowsService = windowsService;
        }

        public void Initialize()
        {
            _windowsService.OpenWindow<MenuSelectionWindow>();
        }
    }
}
