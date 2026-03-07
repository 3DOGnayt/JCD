using System.Collections.Generic;
using Configs.Impl;
using Data.Enums;
using KoboldUi.Element.Controller;
using Services;
using UI.Views;
using UniRx;
using TMPro;
using Zenject;

namespace UI.Controllers
{
    public class AudioMenuController : AUiController<MapView>
    {
        private readonly IAudioService _audioService;
        private readonly List<EAudioSubType> _musicOptions = new();

        private AudioCatalogParameters _audioCatalogParameters;
        private AudioSelectionParameters _audioSelectionParameters;

        [Inject]
        public void Construct(
            AudioCatalogParameters audioCatalogParameters,
            AudioSelectionParameters audioSelectionParameters
        )
        {
            _audioCatalogParameters = audioCatalogParameters;
            _audioSelectionParameters = audioSelectionParameters;
        }

        public AudioMenuController(IAudioService audioService)
        {
            _audioService = audioService;
        }

        public override void Initialize()
        {
            InitializeMusicDropdown();
        }

        protected override void OnOpen()
        {
            RefreshMusicDropdownSelection();
        }

        private void InitializeMusicDropdown()
        {
            if (View.MusicList == null)
                return;

            BuildMusicOptions();
            View.MusicList.ClearOptions();

            var options = new List<TMP_Dropdown.OptionData>(_musicOptions.Count);
            for (var i = 0; i < _musicOptions.Count; i++)
                options.Add(new TMP_Dropdown.OptionData(_musicOptions[i].ToString()));

            View.MusicList.AddOptions(options);
            View.MusicList.interactable = _musicOptions.Count > 0;

            View.MusicList.onValueChanged.AsObservable()
                .Subscribe(OnMusicDropdownChanged).AddTo(View);
        }

        private void BuildMusicOptions()
        {
            _musicOptions.Clear();
            if (_audioCatalogParameters == null)
                return;

            var setups = _audioCatalogParameters.AudioSetups;
            for (var i = 0; i < setups.Count; i++)
            {
                var setup = setups[i];
                if (setup.AudioType != EAudioType.Music)
                    continue;

                var entries = setup.AudioSettingsEntry;
                if (entries == null)
                    continue;

                for (var k = 0; k < entries.Count; k++)
                {
                    var subType = entries[k].AudioSubType;
                    if (_musicOptions.Contains(subType))
                        continue;

                    _musicOptions.Add(subType);
                }
            }
        }

        private void EnsureDefaultMusicSelection()
        {
            if (_musicOptions.Count == 0 || _audioSelectionParameters == null)
                return;

            var selectedSubType = _audioSelectionParameters.SelectedMusicSubType;
            if (selectedSubType != EAudioSubType.None && _musicOptions.Contains(selectedSubType))
                return;

            _audioSelectionParameters.SetSelectedMusic(_musicOptions[0], 0);
        }

        private int GetMusicSelectionIndex()
        {
            if (_audioSelectionParameters == null || _musicOptions.Count == 0)
                return 0;

            var index = _audioSelectionParameters.SelectedMusicIndex;
            var selectedSubType = _audioSelectionParameters.SelectedMusicSubType;
            if (index >= 0 && index < _musicOptions.Count && _musicOptions[index] == selectedSubType)
                return index;

            var fallbackIndex = _musicOptions.IndexOf(selectedSubType);
            return fallbackIndex >= 0 ? fallbackIndex : 0;
        }

        private void RefreshMusicDropdownSelection()
        {
            if (View.MusicList == null || _musicOptions.Count == 0)
                return;

            EnsureDefaultMusicSelection();
            View.MusicList.SetValueWithoutNotify(GetMusicSelectionIndex());
        }

        private void OnMusicDropdownChanged(int index)
        {
            if (index < 0 || index >= _musicOptions.Count || _audioSelectionParameters == null)
                return;

            _audioSelectionParameters.SetSelectedMusic(_musicOptions[index], index);
            _audioService.PlayUiAudio(EAudioType.Ui, EAudioSubType.MenuButtonSelect);
        }
    }
}