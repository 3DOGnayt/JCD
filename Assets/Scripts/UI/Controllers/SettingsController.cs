using Data.Enums;
using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using Services;
using Configs.Impl;
using UI.Views;
using UI.Window;
using UniRx;
using UnityEngine.UI;
using Zenject;

namespace UI.Controllers
{
    public class SettingsController : AUiController<SettingsView>
    {
        private readonly ILocalWindowsService _localWindowsService;
        private readonly IAudioService _audioService;
        
        [Inject] private AudioSelectionParameters _audioSelectionParameters;

        public SettingsController(ILocalWindowsService localWindowsService, IAudioService audioService)
        {
            _localWindowsService = localWindowsService;
            _audioService = audioService;
        }

        public override void Initialize()
        {
            ApplySavedVolumes();
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

        private void ApplySavedVolumes()
        {
            if (_audioSelectionParameters == null || _audioSelectionParameters.VolumeSetup == null)
                return;

            var settings = _audioSelectionParameters.VolumeSetup;
            if (View.MasterVolume != null)
                View.MasterVolume.SetValueWithoutNotify(settings.Master);
            if (View.SfxVolume != null)
                View.SfxVolume.SetValueWithoutNotify(settings.Sfx);
            if (View.MusicVolume != null)
                View.MusicVolume.SetValueWithoutNotify(settings.Music);
            if (View.UiVolume != null)
                View.UiVolume.SetValueWithoutNotify(settings.Ui);
        }

        private void OnCloseButtonClick()
        {
            _audioService.PlayUiAudio(EAudioType.Ui, EAudioSubType.MenuButtonBack);
            
            _localWindowsService.CloseWindow();
        }
    }
}