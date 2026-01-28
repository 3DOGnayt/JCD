using Configs.Impl;
using KoboldUi.Element.Controller;
using TMPro;
using UnityEngine;
using UI.Canvas.MenuSelection.Views;
using Zenject;

namespace UI.Canvas.MenuSelection.Controllers
{
    public class MapSelectionController : AUiController<MapSelectionView>
    {
        [Inject] private MenuSelectionWindow _window;
        [Inject] private MapCatalog _mapCatalog;
        [Inject] private GameSelectionParameters _gameSelectionParameters;

        private bool _isReady;
        private int _pendingMapIndex = -1;
        private GameObject _pendingMapPrefab;
        private Sprite _pendingMapPreview;

        public override void Initialize()
        {
            if (_window == null
                || View == null
                || !View.IsValid
                || _mapCatalog == null
                || _gameSelectionParameters == null)
            {
                Debug.LogError("[MenuSelection] MapSelectionController is missing required references.");
                return;
            }

            _isReady = true;

            MenuSelectionButtonUtility.BuildButtons(View.MapButtons, _mapCatalog.Maps.Count);

            View.BackButton.onClick.AddListener(HandleBackFromMap);
            View.ConfirmButton.onClick.AddListener(HandleConfirmMap);

            _window.Flow.MapPanelShown += HandleMapPanelShown;
            _window.Flow.MapPanelHidden += HandleMapPanelHidden;

            EnsureDefaultSelection();
            RefreshMapButtons();
            UpdateMapPresentation();
        }

        private void HandleMapPanelShown()
        {
            PreparePendingMapSelection();
            View.SetPresentationVisible(false);
            View.SetConfirmBackVisible(false);
            View.ShowPresentationDelayed();
            View.ShowConfirmBackDelayed();
            RefreshMapButtons();
            UpdateMapPresentation();
        }

        private void HandleMapPanelHidden()
        {
            View.SetPresentationVisible(false);
            View.SetConfirmBackVisible(false);
        }

        private void HandleBackFromMap()
        {
            if (!_isReady)
                return;

            _window.Flow.ShowMainPanelFromMap();
        }

        private void HandleConfirmMap()
        {
            if (!_isReady || _pendingMapIndex < 0)
                return;

            _gameSelectionParameters.SetSelectedMap(_pendingMapPrefab, _pendingMapIndex);
            RefreshMapButtons();
            _window.Flow.ShowOpponentPanel();
        }

        private void SelectMap(int index)
        {
            if (!_isReady)
                return;

            if (index < 0 || index >= _mapCatalog.Maps.Count)
                return;

            ApplyPendingMap(index);
            RefreshMapButtons();
        }

        private void EnsureDefaultSelection()
        {
            if (_mapCatalog.Maps.Count == 0)
                return;

            if (_gameSelectionParameters.SelectedMapPrefab == null)
                _gameSelectionParameters.SetSelectedMap(_mapCatalog.Maps[0].Prefab, 0);
        }

        private void PreparePendingMapSelection()
        {
            if (_mapCatalog.Maps.Count == 0)
                return;

            var clamped = Mathf.Clamp(_gameSelectionParameters.SelectedMapIndex, 0, _mapCatalog.Maps.Count - 1);
            ApplyPendingMap(clamped);
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
            View.SetPresentation(_pendingMapPreview);
        }

        private void RefreshMapButtons()
        {
            var mapCount = _mapCatalog.Maps.Count;
            var selectedIndex = GetMapSelectionIndexForButtons();

            for (var index = 0; index < View.MapButtons.Count; index++)
            {
                var button = View.MapButtons[index];
                var isValid = index < mapCount;

                button.gameObject.SetActive(isValid);
                if (!isValid)
                    continue;

                var label = button.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                    label.text = _mapCatalog.Maps[index].DisplayName;

                var selected = selectedIndex == index;
                button.interactable = !selected;

                button.onClick.RemoveAllListeners();
                var captured = index;
                button.onClick.AddListener(() => SelectMap(captured));
            }
        }

        private int GetMapSelectionIndexForButtons()
        {
            if (_pendingMapIndex >= 0)
                return _pendingMapIndex;

            return _gameSelectionParameters.SelectedMapIndex;
        }
    }
}
