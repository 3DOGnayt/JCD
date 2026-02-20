using Data.Enums;
using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using Services;
using UI.Views;
using UI.Window;
using UniRx;
using UnityEngine;

namespace UI.Controllers
{
    public class MainMenuController : AUiController<MainMenuView>
    {
        private readonly ILocalWindowsService _localWindowsService;
        private readonly IAudioService _audioService;
        
        public MainMenuController(
            ILocalWindowsService localWindowsService,
            IAudioService audioService
        )
        {
            _localWindowsService = localWindowsService;
            _audioService = audioService;
        }

        public override void Initialize()
        {
            View.StartButton.OnClickAsObservable().Subscribe(_ => OnStartButtonClick()).AddTo(View);
            View.GarageButton.OnClickAsObservable().Subscribe(_ => OnGarageButtonClick()).AddTo(View);
            View.SettingsButton.OnClickAsObservable().Subscribe(_ => OnSettingsButtonClick()).AddTo(View);
            View.ExitButton.OnClickAsObservable().Subscribe(_ => OnExitButtonClick()).AddTo(View);
        }

        protected override void OnOpen()
        {
            _audioService.PlayMusicAudio(EAudioType.Music, EAudioSubType.MenuBack, 0.3f);// need audio catalog parameters
        }

        private void OnStartButtonClick()
        {
            _audioService.PlayUiAudio(EAudioType.Ui, EAudioSubType.MenuButtonSelect);
            
            _localWindowsService.OpenWindow<MapWindow>();
        }

        private void OnGarageButtonClick()
        {
            _audioService.PlayUiAudio(EAudioType.Ui, EAudioSubType.MenuButtonSelect);

            _localWindowsService.OpenWindow<GarageWindow>();
        }

        private void OnSettingsButtonClick()
        {
            _audioService.PlayUiAudio(EAudioType.Ui, EAudioSubType.MenuButtonSelect);
            
            _localWindowsService.OpenWindow<SettingsWindow>();
        }

        private void OnExitButtonClick()
        {
            _audioService.PlayUiAudio(EAudioType.Ui, EAudioSubType.MenuButtonBack);
            
            Application.Quit();
        }
    }
}