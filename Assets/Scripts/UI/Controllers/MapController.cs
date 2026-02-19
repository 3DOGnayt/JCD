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
using Zenject;

namespace UI.Controllers
{
    public class MapController : AUiController<MapView>
    {
        private const int LEFT_SLIDE_600 = -600;
        
        private readonly ILocalWindowsService _localWindowsService;
        private readonly IAudioService _audioService;

        private MapCatalogParameters _mapCatalogParameters;
        private GameSelectionParameters _gameSelectionParameters;

        private Tween _presentationTween;
        private Tween _delayTween;

        private GameObject _pendingMapPrefab;
        private Sprite _pendingMapPreview;
        private int _pendingMapIndex = -1;
        private bool _isReady;
        private float _delaySlideAnimationOnView = 0.2f;
        
        [Inject]
        public void Construct(
            MapCatalogParameters mapCatalogParameters,
            GameSelectionParameters gameSelectionParameters
        )
        {
            _mapCatalogParameters = mapCatalogParameters;
            _gameSelectionParameters = gameSelectionParameters;
        }
        
        public MapController(ILocalWindowsService localWindowsService, IAudioService audioService)
        {
            _localWindowsService = localWindowsService;
            _audioService = audioService;
        }

        public override void Initialize()
        {
            _isReady = _mapCatalogParameters != null && _gameSelectionParameters != null;
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
            if (_gameSelectionParameters.SelectedMapPrefab != null || _mapCatalogParameters.Maps.Count <= 0)
                return;

            var entry = _mapCatalogParameters.Maps[0];
            _gameSelectionParameters.SetSelectedMap(entry.Prefab, 0, entry.EMap, entry.SelectionCount, entry.LapCount);
        }

        private void PreparePendingMapSelection()
        {
            if (_mapCatalogParameters.Maps.Count == 0)
                return;

            var clamped = Mathf.Clamp(_gameSelectionParameters.SelectedMapIndex, 0, _mapCatalogParameters.Maps.Count - 1);
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

            return _gameSelectionParameters != null ? _gameSelectionParameters.SelectedMapIndex : -1;
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
            _gameSelectionParameters.SetSelectedMap(
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
        }

        private void OnBackButtonClick()
        {
            _audioService.PlayUiAudio(EAudioType.Ui, EAudioSubType.MenuButtonBack);
            
            _localWindowsService.CloseWindow();
        }
    }
}