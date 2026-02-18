using Configs.Impl;
using Data.Enums;
using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using Services;
using UI.Views;
using UI.Window;
using UniRx;
using UnityEngine;
using Zenject;

namespace UI.Controllers
{
    public class MainMenuController : AUiController<MainMenuView>
    {
        private readonly ILocalWindowsService _localWindowsService;
        private readonly IAudioService _audioService;

        private AudioCatalogParameters _audioCatalogParameters;

        [Inject]
        public void Construct(AudioCatalogParameters audioCatalogParameters)
        {
            _audioCatalogParameters = audioCatalogParameters;
        }

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
            var audioSettings = _audioCatalogParameters.AudioSetups;
            for (var i = 0; i < audioSettings.Count; i++)
            {
                if (audioSettings[i].AudioType == EAudioType.Music)
                {
                    _audioService.PlaySfx2D(
                        audioSettings[i].AudioSettings.AudioClip,
                        audioSettings[i].AudioSettings.Volume);
                    
                    return;
                }
            }
        }

        private void OnStartButtonClick() => _localWindowsService.OpenWindow<MapWindow>();
        private void OnGarageButtonClick()=> _localWindowsService.OpenWindow<GarageWindow>();
        private void OnSettingsButtonClick() => _localWindowsService.OpenWindow<SettingsWindow>();
        private void OnExitButtonClick() => Application.Quit();
    }
}