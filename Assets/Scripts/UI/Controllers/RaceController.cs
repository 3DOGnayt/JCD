using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using UI.Views;
using UI.Window;
using UniRx;

namespace UI.Controllers
{
    public class RaceController : AUiController<RaceView>
    {
        private readonly ILocalWindowsService _localWindowsService;

        public RaceController(ILocalWindowsService localWindowsService)
        {
            _localWindowsService = localWindowsService;
        }

        public override void Initialize()
        {
            View.TrainingButton.OnClickAsObservable().Subscribe(_ => OnTrainingButtonClick()).AddTo(View);
            View.StoryButton.OnClickAsObservable().Subscribe(_ => OnStoryButtonClick()).AddTo(View);
            View.ConfirmButton.OnClickAsObservable().Subscribe(_ => OnConfirmButtonClick()).AddTo(View);
            View.BackButton.OnClickAsObservable().Subscribe(_ => OnBackButtonClick()).AddTo(View);
        }

        private void OnTrainingButtonClick()
        {
            //change game mod
        }

        private void OnStoryButtonClick()
        {
            //change game mod
        }

        private void OnConfirmButtonClick() => _localWindowsService.OpenWindow<OpponentWindow>();

        private void OnBackButtonClick() => _localWindowsService.CloseWindow();
    }
}