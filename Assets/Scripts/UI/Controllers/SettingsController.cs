using Data.Enums;
using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using Services;
using UI.Views;
using UI.Window;
using UniRx;
using UnityEngine.UI;

namespace UI.Controllers
{
    public class SettingsController : AUiController<SettingsView>
    {
        private readonly ILocalWindowsService _localWindowsService;
        private readonly IAudioService _audioService;

        public SettingsController(ILocalWindowsService localWindowsService, IAudioService audioService)
        {
            _localWindowsService = localWindowsService;
            _audioService = audioService;
        }

        public override void Initialize()
        {
            BindVolumeSlider(View.MasterVolume, EAudioType.Master);
            BindVolumeSlider(View.SfxVolume, EAudioType.Sfx);
            BindVolumeSlider(View.MusicVolume, EAudioType.Music);
            BindVolumeSlider(View.UiVolume, EAudioType.Ui);

            View.CloseButton.OnClickAsObservable().Subscribe(_ => OnCloseButtonClick()).AddTo(View);
        }

        private void BindVolumeSlider(Slider slider, EAudioType type)
        {
            if (slider == null)
                return;

            slider.onValueChanged.AsObservable()
                .Subscribe(value => _audioService.SetAudioVolume(type, value))
                .AddTo(View);

            _audioService.SetAudioVolume(type, slider.value);
        }

        private void OnCloseButtonClick()
        {
            _audioService.PlayUiAudio(EAudioType.Ui, EAudioSubType.MenuButtonBack);
            
            _localWindowsService.OpenWindow<MainMenuWindow>();
        }
    }
}