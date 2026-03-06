using Data.Enums;
using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using Services;
using Configs.Impl;
using UI.Views;
using UniRx;
using UnityEngine.UI;
using Zenject;

namespace UI.Controllers
{
    public class SettingsController : AUiController<SettingsView>
    {
        private readonly ILocalWindowsService _localWindowsService;
        private readonly IAudioService _audioService;
        private readonly IDataService _dataService;
        
        [Inject] private AudioSelectionParameters _audioSelectionParameters;

        public SettingsController(
            ILocalWindowsService localWindowsService,
            IAudioService audioService,
            IDataService dataService
        )
        {
            _localWindowsService = localWindowsService;
            _audioService = audioService;
            _dataService = dataService;
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
                .Subscribe(value => OnVolumeChanged(type, value))
                .AddTo(View);

            _audioService.SetAudioVolume(type, slider.value);
        }

        private void ApplySavedVolumes()
        {
            var fallback = _audioSelectionParameters != null ? _audioSelectionParameters.VolumeSetup : null;
            var settings = _dataService.LoadAudioVolumes(fallback);

            if (View.MasterVolume != null)
                View.MasterVolume.SetValueWithoutNotify(settings.Master);
            if (View.SfxVolume != null)
                View.SfxVolume.SetValueWithoutNotify(settings.Sfx);
            if (View.MusicVolume != null)
                View.MusicVolume.SetValueWithoutNotify(settings.Music);
            if (View.UiVolume != null)
                View.UiVolume.SetValueWithoutNotify(settings.Ui);

            if (_audioSelectionParameters != null)
            {
                _audioSelectionParameters.SetMasterVolume(settings.Master);
                _audioSelectionParameters.SetSfxVolume(settings.Sfx);
                _audioSelectionParameters.SetMusicVolume(settings.Music);
                _audioSelectionParameters.SetUiVolume(settings.Ui);
            }
        }

        private void OnVolumeChanged(EAudioType type, float value)
        {
            _audioService.SetAudioVolume(type, value);
            UpdateAudioSelection(type, value);
            _dataService.SaveAudioVolume(type, value);
        }

        private void UpdateAudioSelection(EAudioType type, float value)
        {
            if (_audioSelectionParameters == null)
                return;

            switch (type)
            {
                case EAudioType.Master:
                    _audioSelectionParameters.SetMasterVolume(value);
                    break;
                case EAudioType.Sfx:
                    _audioSelectionParameters.SetSfxVolume(value);
                    break;
                case EAudioType.Music:
                    _audioSelectionParameters.SetMusicVolume(value);
                    break;
                case EAudioType.Ui:
                    _audioSelectionParameters.SetUiVolume(value);
                    break;
            }
        }

        private void OnCloseButtonClick()
        {
            _audioService.PlayUiAudio(EAudioType.Ui, EAudioSubType.MenuButtonBack);
            
            _localWindowsService.CloseWindow();
        }
    }
}