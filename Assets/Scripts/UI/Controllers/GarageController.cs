using Configs.Impl;
using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using TMPro;
using UI.Views;
using UI.Window;
using UniRx;
using UnityEngine;
using Zenject;

namespace UI.Controllers
{
    public class GarageController : AUiController<GarageView>
    {
        private readonly ILocalWindowsService _localWindowsService;

        private CarPresetParameters _pendingCarPreset;
        private CarParameters _pendingCarParameters;
        
        private Sprite _pendingCarPreview;
        private int _pendingCarIndex = -1;
        private bool _isReady;
        
        [Inject] private CarCatalog _carCatalog;
        [Inject] private GameSelectionParameters _gameSelectionParameters;

        public GarageController(ILocalWindowsService localWindowsService)
        {
            _localWindowsService = localWindowsService;
        }

        public override void Initialize()
        {
            _isReady = _carCatalog != null && _gameSelectionParameters != null;
            if (!_isReady)
                return;

            for (var i = 0; i < View.CarButtons.Count; i++)
            {
                var buttonIndex = i;
                View.CarButtons[buttonIndex].OnClickAsObservable()
                    .Subscribe(_ => OnCarButtonClick(buttonIndex)).AddTo(View);
            }

            View.ConfirmButton.OnClickAsObservable().Subscribe(_ => OnConfirmButtonClick()).AddTo(View);
            View.BackButton.OnClickAsObservable().Subscribe(_ => OnBackButtonClick()).AddTo(View);
        }

        protected override void OnOpen()
        {
            if (!_isReady)
                return;

            InitializeCarButtonLabels();
            EnsureDefaultSelection();
            PreparePendingCarSelection();
            RefreshCarButtons();
            UpdateCarPresentation();
        }

        private void InitializeCarButtonLabels()
        {
            var carCount = _carCatalog.Cars.Count;
            for (var index = 0; index < View.CarButtons.Count && index < carCount; index++)
            {
                var label = View.CarButtons[index].GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                    label.text = _carCatalog.Cars[index].DisplayName;
            }
        }

        private void EnsureDefaultSelection()
        {
            if (_gameSelectionParameters.SelectedCar != null || _carCatalog.Cars.Count <= 0)
                return;
            
            var entry = _carCatalog.Cars[0];
            _gameSelectionParameters.SetSelectedCar(entry.Preset, entry.Parameters, 0);
        }

        private void PreparePendingCarSelection()
        {
            if (_carCatalog.Cars.Count == 0)
                return;

            var clamped = Mathf.Clamp(_gameSelectionParameters.SelectedCarIndex, 0, _carCatalog.Cars.Count - 1);
            ApplyPendingCar(clamped);
        }

        private void RefreshCarButtons()
        {
            var selectedIndex = GetCarSelectionIndexForButtons();
            var carCount = _carCatalog != null ? _carCatalog.Cars.Count : 0;

            for (var index = 0; index < View.CarButtons.Count; index++)
            {
                var button = View.CarButtons[index];
                var isValid = index < carCount;

                button.gameObject.SetActive(isValid);
                if (!isValid)
                    continue;

                var selected = selectedIndex == index;
                button.interactable = !selected;
            }
        }

        private int GetCarSelectionIndexForButtons()
        {
            if (_pendingCarIndex >= 0)
                return _pendingCarIndex;

            return _gameSelectionParameters != null ? _gameSelectionParameters.SelectedCarIndex : -1;
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
            if (View.CarPresentation == null)
                return;

            View.CarPresentation.sprite = _pendingCarPreview;
            View.CarPresentation.enabled = View.CarPresentation.sprite != null;
        }

        private void OnCarButtonClick(int index)
        {
            if (index < 0 || index >= _carCatalog.Cars.Count)
                return;

            ApplyPendingCar(index);
            RefreshCarButtons();
        }

        private void OnConfirmButtonClick()
        {
            if (_pendingCarIndex < 0)
                return;

            _gameSelectionParameters.SetSelectedCar(_pendingCarPreset, _pendingCarParameters, _pendingCarIndex);
            RefreshCarButtons();
            _localWindowsService.OpenWindow<MainMenuWindow>();
        }

        private void OnBackButtonClick() => _localWindowsService.OpenWindow<MainMenuWindow>();
    }
}
