using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using UI.Views;

namespace UI.Controllers
{
    public class TrainingOpponentController : AUiController<TrainingOpponentView>
    {
        private readonly ILocalWindowsService _localWindowsService;

        public TrainingOpponentController(ILocalWindowsService localWindowsService)
        {
            _localWindowsService = localWindowsService;
        }

        public override void Initialize()
        {
            //
        }
    }
}