using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using UI.Views;

namespace UI.Controllers
{
    public class GameTimerController : AUiController<GameTimerView>
    {
        private readonly ILocalWindowsService _localWindowsService;

        public GameTimerController(ILocalWindowsService localWindowsService)
        {
            _localWindowsService = localWindowsService;
        }

        public override void Initialize()
        {
            //
        }
    }
}