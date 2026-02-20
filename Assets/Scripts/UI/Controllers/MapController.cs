using System.Collections.Generic;
using Configs.Impl;
using Data.Enums;
using DG.Tweening;
using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using Services;
using Tools;
using UI.Views;
using UI.Window;
using UniRx;
using UnityEngine;
using TMPro;
using Zenject;

namespace UI.Controllers
{
    public class MapController : AUiController<MapView>
    {
        private const int LEFT_SLIDE_600 = -600;
        
        private readonly ILocalWindowsService _localWindowsService;
        private readonly IAudioService _audioService;

        private MapCatalogParameters _mapCatalogParameters;
        private AudioCatalogParameters _audioCatalogParameters;
        private MapSelectionParameters _mapSelectionParameters;
        private AudioSelectionParameters _audioSelectionParameters;

        private Tween _presentationTween;
        private Tween _delayTween;

        private GameObject _pendingMapPrefab;
        private Sprite _pendingMapPreview;
        private int _pendingMapIndex = -1;
        private bool _isReady;
        private float _delaySlideAnimationOnView = 0.2f;
        private readonly List<EAudioSubType> _musicOptions = new();
        
        [Inject]
        public void Construct(
            MapCatalogParameters mapCatalogParameters,
            AudioCatalogParameters audioCatalogParameters,
            MapSelectionParameters mapSelectionParameters,
            AudioSelectionParameters audioSelectionParameters
        )
        {
            _mapCatalogParameters = mapCatalogParameters;
            _audioCatalogParameters = audioCatalogParameters;
            _mapSelectionParameters = mapSelectionParameters;
            _audioSelectionParameters = audioSelectionParameters;
        }
        
        public MapController(ILocalWindowsService localWindowsService, IAudioService audioService)
        {
            _localWindowsService = localWindowsService;
            _audioService = audioService;
        }

        public override void Initialize()
        {
            _isReady = _mapCatalogParameters != null && _mapSelectionParameters != null;
            if (!_isReady)
                return;

            for (var i = 0; i < View.MapButtons.Count; i++)
            {
                var buttonIndex = i;
                View.MapButtons[buttonIndex].OnClickAsObservable()
                    .Subscribe(_ => OnMapButtonClick(buttonIndex)).AddTo(View);
            }

            View.ConfirmButton.OnClickAsObservable().Subscribe(_ => OnConfirmButtonClick()).AddTo(View);
            View.BackButton.OnClickAsObservable().Subscribe(_ => OnBackButtonClick()).AddTo(View);

            InitializeMusicDropdown();
        }

        protected override void OnOpen()
        {
            if (!_isReady)
                return;

            InitializeMapButtonLabels();
            EnsureDefaultSelection();
            PreparePendingMapSelection();
            RefreshMapButtons();
            UpdateMapPresentation();
            RefreshMusicDropdownSelection();
            
            _presentationTween?.Kill();

            _presentationTween = DOVirtual.DelayedCall(View.PresentationDelay, () => SetPresentationState(true))
                .SetUpdate(true).SetLink(View.gameObject);
        }

        private void InitializeMapButtonLabels()
        {
            var mapCount = _mapCatalogParameters.Maps.Count;
            for (var index = 0; index < View.MapButtons.Count && index < mapCount; index++)
            {
                var label = View.MapButtonsText[index];
                label.text = _mapCatalogParameters.Maps[index].EMap.ToString();
            }
        }

        private void EnsureDefaultSelection()
        {
            if (_mapSelectionParameters.SelectedMapPrefab != null || _mapCatalogParameters.Maps.Count <= 0)
                return;

            var entry = _mapCatalogParameters.Maps[0];
            _mapSelectionParameters.SetSelectedMap(entry.Prefab, 0, entry.EMap, entry.SelectionCount, entry.LapCount);
        }

        private void PreparePendingMapSelection()
        {
            if (_mapCatalogParameters.Maps.Count == 0)
                return;

            var clamped = Mathf.Clamp(_mapSelectionParameters.SelectedMapIndex, 0, _mapCatalogParameters.Maps.Count - 1);
            ApplyPendingMap(clamped);
        }

        private void RefreshMapButtons()
        {
            var selectedIndex = GetMapSelectionIndexForButtons();
            var mapCount = _mapCatalogParameters != null ? _mapCatalogParameters.Maps.Count : 0;

            for (var index = 0; index < View.MapButtons.Count; index++)
            {
                var button = View.MapButtons[index];
                var isValid = index < mapCount;

                button.gameObject.SetActive(isValid);
                if (!isValid)
                    continue;

                var selected = selectedIndex == index;
                button.interactable = !selected;
            }
        }

        private int GetMapSelectionIndexForButtons()
        {
            if (_pendingMapIndex >= 0)
                return _pendingMapIndex;

            return _mapSelectionParameters != null ? _mapSelectionParameters.SelectedMapIndex : -1;
        }

        private void ApplyPendingMap(int index)
        {
            var entry = _mapCatalogParameters.Maps[index];
            _pendingMapIndex = index;
            _pendingMapPrefab = entry.Prefab;
            _pendingMapPreview = entry.Preview;
            UpdateMapPresentation();
        }

        private void UpdateMapPresentation()
        {
            if (View.MapPresentation == null)
                return;

            View.MapPresentation.sprite = _pendingMapPreview;
            View.MapPresentation.enabled = View.MapPresentation.sprite != null;
        }
        
        private void OnMapButtonClick(int index)
        {
            if (index < 0 || index >= _mapCatalogParameters.Maps.Count)
                return;

            _audioService.PlayUiAudio(EAudioType.Ui, EAudioSubType.MenuButtonSelect);
            
            ApplyPendingMap(index);
            RefreshMapButtons();
        }

        private void OnConfirmButtonClick()
        {
            if (_pendingMapIndex < 0)
                return;

            _audioService.PlayUiAudio(EAudioType.Ui, EAudioSubType.MenuButtonConfirm);
            
            var entry = _mapCatalogParameters.Maps[_pendingMapIndex];
            _mapSelectionParameters.SetSelectedMap(
                _pendingMapPrefab, _pendingMapIndex, entry.EMap, entry.SelectionCount, entry.LapCount);
            
            RefreshMapButtons();
            
            _localWindowsService.AnimateWindow<MainMenuWindow>(Vector2.right * LEFT_SLIDE_600);
            _localWindowsService.AnimateWindow<MapWindow>(Vector2.right * LEFT_SLIDE_600);
            
            SetPresentationState(false);

            _delayTween?.Kill();
            _delayTween = DOVirtual
                .DelayedCall(_delaySlideAnimationOnView, () => _localWindowsService.OpenWindow<GameModWindow>())
                .SetUpdate(true).SetLink(View.gameObject);
        }

        private void SetPresentationState(bool isActive)
        {
            View.ConfirmButton.gameObject.SetActive(isActive);
            View.BackButton.gameObject.SetActive(isActive);
            View.MapPresentation.gameObject.SetActive(isActive);
            View.MusicList.gameObject.SetActive(isActive);
        }

        private void OnBackButtonClick()
        {
            _audioService.PlayUiAudio(EAudioType.Ui, EAudioSubType.MenuButtonBack);
            
            _localWindowsService.CloseWindow();
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
        } //TODO: replace in audio controller

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