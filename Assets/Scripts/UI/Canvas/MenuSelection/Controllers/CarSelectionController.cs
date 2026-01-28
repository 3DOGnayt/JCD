using Configs.Impl;
using KoboldUi.Element.Controller;
using TMPro;
using UnityEngine;
using UI.Canvas.MenuSelection.Views;
using Zenject;

namespace UI.Canvas.MenuSelection.Controllers
{
    public class CarSelectionController : AUiController<CarSelectionView>
    {
        [Inject] private MenuSelectionWindow _window;
        [Inject] private CarCatalog _carCatalog;
        [Inject] private GameSelectionParameters _gameSelectionParameters;

        private bool _isReady;
        private int _pendingCarIndex = -1;
        private CarPresetParameters _pendingCarPreset;
        private CarParameters _pendingCarParameters;
        private Sprite _pendingCarPreview;

        public override void Initialize()
        {
            if (_window == null
                || View == null
                || !View.IsValid
                || _carCatalog == null
                || _gameSelectionParameters == null)
            {
                Debug.LogError("[MenuSelection] CarSelectionController is missing required references.");
                return;
            }

            _isReady = true;

            MenuSelectionButtonUtility.BuildButtons(View.CarButtons, _carCatalog.Cars.Count);

            View.BackButton.onClick.AddListener(HandleBackFromCar);
            View.ConfirmButton.onClick.AddListener(HandleConfirmCar);

            _window.Flow.CarPanelShown += HandleCarPanelShown;

            EnsureDefaultSelection();
            RefreshCarButtons();
            UpdateCarPresentation();
        }

        private void HandleCarPanelShown()
        {
            PreparePendingCarSelection();
            RefreshCarButtons();
            UpdateCarPresentation();
        }

        private void HandleBackFromCar()
        {
            if (!_isReady)
                return;

            _window.Flow.ShowMainPanelFromCar();
        }

        private void HandleConfirmCar()
        {
            if (!_isReady || _pendingCarIndex < 0)
                return;

            _gameSelectionParameters.SetSelectedCar(_pendingCarPreset, _pendingCarParameters, _pendingCarIndex);
            RefreshCarButtons();
            _window.Flow.ShowMainPanelFromCar();
        }

        private void SelectCar(int index)
        {
            if (!_isReady)
                return;

            if (index < 0 || index >= _carCatalog.Cars.Count)
                return;

            ApplyPendingCar(index);
            RefreshCarButtons();
        }

        private void EnsureDefaultSelection()
        {
            if (_carCatalog.Cars.Count == 0)
                return;

            if (_gameSelectionParameters.SelectedCar == null)
            {
                var entry = _carCatalog.Cars[0];
                _gameSelectionParameters.SetSelectedCar(entry.Preset, entry.Parameters, 0);
            }
        }

        private void PreparePendingCarSelection()
        {
            if (_carCatalog.Cars.Count == 0)
                return;

            var clamped = Mathf.Clamp(_gameSelectionParameters.SelectedCarIndex, 0, _carCatalog.Cars.Count - 1);
            ApplyPendingCar(clamped);
        }

        private void ApplyPendingCar(int index)
        {
            var entry = _carCatalog.Cars[index];
            _pendingCarIndex = index;
            _pendingCarPreset = entry.Preset;
            _pendingCarParameters = entry.Parameters;
            _pendingCarPreview = entry.Preview;
            UpdateCarPresentation();
        }

        private void UpdateCarPresentation()
        {
            View.SetPresentation(_pendingCarPreview);
        }

        private void RefreshCarButtons()
        {
            var carCount = _carCatalog.Cars.Count;
            var selectedIndex = GetCarSelectionIndexForButtons();

            for (var index = 0; index < View.CarButtons.Count; index++)
            {
                var button = View.CarButtons[index];
                var isValid = index < carCount;

                button.gameObject.SetActive(isValid);
                if (!isValid)
                    continue;

                var label = button.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                    label.text = _carCatalog.Cars[index].DisplayName;

                var selected = selectedIndex == index;
                button.interactable = !selected;

                button.onClick.RemoveAllListeners();
                var captured = index;
                button.onClick.AddListener(() => SelectCar(captured));
            }
        }

        private int GetCarSelectionIndexForButtons()
        {
            if (_pendingCarIndex >= 0)
                return _pendingCarIndex;

            return _gameSelectionParameters.SelectedCarIndex;
        }
    }
}
