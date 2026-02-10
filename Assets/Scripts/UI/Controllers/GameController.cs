using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using UI.Views;
using UI.Window;

namespace UI.Controllers
{
    public class GameController : AUiController<GameView>
    {
        private readonly ILocalWindowsService _localWindowsService;

        public GameController(ILocalWindowsService localWindowsService)
        {
            _localWindowsService = localWindowsService;
        }

        public override void Initialize()
        {
            
        }

        protected override void OnOpen()
        {
            _localWindowsService.OpenWindow<GameStartEndWindow>();
        }
    }
}