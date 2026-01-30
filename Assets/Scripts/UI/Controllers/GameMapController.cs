using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using UI.Views;

namespace UI.Controllers
{
    public class GameMapController : AUiController<GameMapView>
    {
        private readonly ILocalWindowsService _localWindowsService;

        public GameMapController(ILocalWindowsService localWindowsService)
        {
            _localWindowsService = localWindowsService;
        }

        public override void Initialize()
        {
            //
        }
    }
}