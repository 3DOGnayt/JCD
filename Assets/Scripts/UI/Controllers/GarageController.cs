using Configs.Impl;
using Data.Enums;
using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using Services;
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
        private readonly IAudioService _audioService;
        
        private CarCatalogParameters _carCatalogParameters;
        private CarSelectionParameters _carSelectionParameters;

        private CarPresetParameters _pendingCarPreset;
        private CarParameters _pendingCarParameters;
        
        private Sprite _pendingCarPreview;
        private int _pendingCarIndex = -1;
        private bool _isReady;

        [Inject]
        public void Construct(
            CarCatalogParameters carCatalogParameters,
            CarSelectionParameters carSelectionParameters
        )
        {
            _carCatalogParameters = carCatalogParameters;
            _carSelectionParameters = carSelectionParameters;
        }

        public GarageController(
            ILocalWindowsService localWindowsService,
            IAudioService audioService
        )
        {
            _localWindowsService = localWindowsService;
            _audioService = audioService;
        }

        public override void Initialize()
        {
            _isReady = _carCatalogParameters != null && _carSelectionParameters != null;
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
            var carCount = _carCatalogParameters.Cars.Count;
            for (var index = 0; index < View.CarButtons.Count && index < carCount; index++)
            {
                var label = View.CarButtons[index].GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                    label.text = _carCatalogParameters.Cars[index].DisplayName;
            }
        }

        private void EnsureDefaultSelection()
        {
            if (_carSelectionParameters.SelectedCar != null || _carCatalogParameters.Cars.Count <= 0)
                return;
            
            var entry = _carCatalogParameters.Cars[0];
            _carSelectionParameters.SetSelectedCar(entry.Preset, entry.Parameters, 0);
        }

        private void PreparePendingCarSelection()
        {
            if (_carCatalogParameters.Cars.Count == 0)
                return;

            var clamped = Mathf.Clamp(
                _carSelectionParameters.SelectedCarIndex, 0, _carCatalogParameters.Cars.Count - 1);
            
            ApplyPendingCar(clamped);
        }

        private void RefreshCarButtons()
        {
            var selectedIndex = GetCarSelectionIndexForButtons();
            var carCount = _carCatalogParameters != null ? _carCatalogParameters.Cars.Count : 0;

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

            return _carSelectionParameters != null ? _carSelectionParameters.SelectedCarIndex : -1;
        }

        private void ApplyPendingCar(int index)
        {
            var entry = _carCatalogParameters.Cars[index];
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
            if (index < 0 || index >= _carCatalogParameters.Cars.Count)
                return;

            _audioService.PlayUiAudio(EAudioType.Ui, EAudioSubType.MenuButtonSelect);
            
            ApplyPendingCar(index);
            RefreshCarButtons();
        }

        private void OnConfirmButtonClick()
        {
            if (_pendingCarIndex < 0)
                return;

            _audioService.PlayUiAudio(EAudioType.Ui, EAudioSubType.MenuButtonConfirm);
            
            _carSelectionParameters.SetSelectedCar(_pendingCarPreset, _pendingCarParameters, _pendingCarIndex);
            RefreshCarButtons();
            _localWindowsService.OpenWindow<MainMenuWindow>();
        }

        private void OnBackButtonClick()
        {
            _audioService.PlayUiAudio(EAudioType.Ui, EAudioSubType.MenuButtonBack);
            
            _localWindowsService.OpenWindow<MainMenuWindow>();
        }
    }
}