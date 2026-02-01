using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using UI.Views;

namespace UI.Controllers
{
    public class GamePauseController : AUiController<GamePauseView>
    {
        private readonly ILocalWindowsService _localWindowsService;

        public GamePauseController(ILocalWindowsService localWindowsService)
        {
            _localWindowsService = localWindowsService;
        }

        public override void Initialize()
        {
            //
        }
    }
}