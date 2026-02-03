using Configs.Impl;
using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using UI.Views;
using UI.Window;
using UniRx;
using UnityEngine;
using Zenject;

namespace UI.Controllers
{
    public class MapController : AUiController<MapView>
    {
        private readonly ILocalWindowsService _localWindowsService;

        private GameObject _pendingMapPrefab;
        private Sprite _pendingMapPreview;
        private int _pendingMapIndex = -1;
        private bool _isReady;

        [Inject] private MapCatalog _mapCatalog;
        [Inject] private GameSelectionParameters _gameSelectionParameters;

        public MapController(ILocalWindowsService localWindowsService)
        {
            _localWindowsService = localWindowsService;
        }

        public override void Initialize()
        {
            _isReady = _mapCatalog != null && _gameSelectionParameters != null;
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
        }

        private void InitializeMapButtonLabels()
        {
            var mapCount = _mapCatalog.Maps.Count;
            for (var index = 0; index < View.MapButtons.Count && index < mapCount; index++)
            {
                var label = View.MapButtonsText[index];
                label.text = _mapCatalog.Maps[index].DisplayName;
            }
        }

        private void EnsureDefaultSelection()
        {
            if (_gameSelectionParameters.SelectedMapPrefab != null || _mapCatalog.Maps.Count <= 0)
                return;

            var entry = _mapCatalog.Maps[0];
            _gameSelectionParameters.SetSelectedMap(entry.Prefab, 0);
        }

        private void PreparePendingMapSelection()
        {
            if (_mapCatalog.Maps.Count == 0)
                return;

            var clamped = Mathf.Clamp(_gameSelectionParameters.SelectedMapIndex, 0, _mapCatalog.Maps.Count - 1);
            ApplyPendingMap(clamped);
        }

        private void RefreshMapButtons()
        {
            var selectedIndex = GetMapSelectionIndexForButtons();
            var mapCount = _mapCatalog != null ? _mapCatalog.Maps.Count : 0;

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
            var entry = _mapCatalog.Maps[index];
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
            if (index < 0 || index >= _mapCatalog.Maps.Count)
                return;

            ApplyPendingMap(index);
            RefreshMapButtons();
        }

        private void OnConfirmButtonClick()
        {
            if (_pendingMapIndex < 0)
                return;

            _gameSelectionParameters.SetSelectedMap(_pendingMapPrefab, _pendingMapIndex);
            RefreshMapButtons();
            _localWindowsService.OpenWindow<OpponentWindow>();
        }

        private void OnBackButtonClick() => _localWindowsService.CloseWindow();
    }
}
