using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using UI.Views;

namespace UI.Controllers
{
    public class GameSpeedometerController : AUiController<GameSpeedometerView>
    {
        private readonly ILocalWindowsService _localWindowsService;

        public GameSpeedometerController(ILocalWindowsService localWindowsService)
        {
            _localWindowsService = localWindowsService;
        }

        public override void Initialize()
        {
            //
        }
    }
}