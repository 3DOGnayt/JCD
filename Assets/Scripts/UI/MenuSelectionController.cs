using System.Collections.Generic;
using Configs.Impl;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class MenuSelectionController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _mapPanel;
        [SerializeField] private GameObject _carPanel;
        [SerializeField] private GameObject _racePanel;

        [Header("Buttons")]
        [SerializeField] private Button _openMapPanelButton;
        [SerializeField] private Button _openCarPanelButton;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _exitButton;

        [Header("Confirm Buttons")]
        [SerializeField] private Button _confirmMapButton;
        [SerializeField] private Button _confirmCarButton;
        [SerializeField] private Button _confirmRaceButton;

        [Header("Selection Buttons")]
        [SerializeField] private List<Button> _mapButtons = new List<Button>();
        [SerializeField] private List<Button> _carButtons = new List<Button>();

        [Header("Presentation")]
        [SerializeField] private Image _mapPresentation;
        [SerializeField] private Image _carPresentation;

        [Header("Data")]
        [SerializeField] private MapCatalog _mapCatalog;
        [SerializeField] private CarCatalog _carCatalog;
        [SerializeField] private GameSelectionParameters _gameSelectionParameters;

        [Inject] private SignalBus _signalBus;

        private int _pendingMapIndex = -1;
        private GameObject _pendingMapPrefab;
        private Sprite _pendingMapPreview;

        private int _pendingCarIndex = -1;
        private CarPresetParameters _pendingCarPreset;
        private CarParameters _pendingCarParameters;
        private Sprite _pendingCarPreview;

        private void Awake()
        {
            WireMainButtons();
            EnsureDefaultSelection();
            BuildButtons(_mapButtons, _mapCatalog != null ? _mapCatalog.Maps.Count : 0);
            BuildButtons(_carButtons, _carCatalog != null ? _carCatalog.Cars.Count : 0);
            RefreshButtons();
            ShowMainPanel();
        }

        private void WireMainButtons()
        {
            if (_openMapPanelButton != null)
                _openMapPanelButton.onClick.AddListener(ShowMapPanel);

            if (_openCarPanelButton != null)
                _openCarPanelButton.onClick.AddListener(ShowCarPanel);

            if (_startButton != null)
                _startButton.onClick.AddListener(ShowMapPanel);

            if (_exitButton != null)
                _exitButton.onClick.AddListener(Application.Quit);

            if (_confirmMapButton != null)
                _confirmMapButton.onClick.AddListener(ConfirmMapSelection);

            if (_confirmCarButton != null)
                _confirmCarButton.onClick.AddListener(ConfirmCarSelection);

            if (_confirmRaceButton != null)
                _confirmRaceButton.onClick.AddListener(ConfirmRaceSelection);
        }

        private void BuildButtons(List<Button> buttons, int needed)
        {
            if (buttons == null || buttons.Count == 0 || needed <= buttons.Count)
                return;

            var template = buttons[0];
            var templateRect = template.transform as RectTransform;
            if (templateRect == null)
                return;

            var parent = template.transform.parent;
            var step = -130f;

            if (buttons.Count > 1)
            {
                var secondRect = buttons[1].transform as RectTransform;
                if (secondRect != null)
                    step = secondRect.anchoredPosition.y - templateRect.anchoredPosition.y;
            }

            for (var index = buttons.Count; index < needed; index++)
            {
                var instance = Instantiate(template, parent);
                instance.name = $"{template.name}_{index + 1}";

                var instanceRect = instance.transform as RectTransform;
                if (instanceRect != null)
                {
                    instanceRect.anchoredPosition = new Vector2(
                        templateRect.anchoredPosition.x,
                        templateRect.anchoredPosition.y + step * index);
                }

                buttons.Add(instance);
            }
        }

        private void RefreshButtons()
        {
            RefreshMapButtons();
            RefreshCarButtons();
        }

        private void RefreshMapButtons()
        {
            var mapCount = _mapCatalog != null ? _mapCatalog.Maps.Count : 0;
            var selectedIndex = GetMapSelectionIndexForButtons();
            for (var index = 0; index < _mapButtons.Count; index++)
            {
                var button = _mapButtons[index];
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

        private void RefreshCarButtons()
        {
            var carCount = _carCatalog != null ? _carCatalog.Cars.Count : 0;
            var selectedIndex = GetCarSelectionIndexForButtons();
            for (var index = 0; index < _carButtons.Count; index++)
            {
                var button = _carButtons[index];
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

        private void SelectMap(int index)
        {
            if (_mapCatalog == null || _gameSelectionParameters == null)
                return;

            if (index < 0 || index >= _mapCatalog.Maps.Count)
                return;

            ApplyPendingMap(index);
            RefreshMapButtons();
        }

        private void SelectCar(int index)
        {
            if (_carCatalog == null || _gameSelectionParameters == null)
                return;

            if (index < 0 || index >= _carCatalog.Cars.Count)
                return;

            ApplyPendingCar(index);
            RefreshCarButtons();
        }

        private void StartRace()
        {
            EnsureDefaultSelection();
            _signalBus.Fire(new StartRaceSignal());
            gameObject.SetActive(false);
        }

        private void EnsureDefaultSelection()
        {
            if (_gameSelectionParameters == null)
                return;

            if (_carCatalog != null && _gameSelectionParameters.SelectedCar == null && _carCatalog.Cars.Count > 0)
            {
                var entry = _carCatalog.Cars[0];
                _gameSelectionParameters.SetSelectedCar(entry.Preset, entry.Parameters, 0);
            }

            if (_mapCatalog != null && _gameSelectionParameters.SelectedMapPrefab == null && _mapCatalog.Maps.Count > 0)
                _gameSelectionParameters.SetSelectedMap(_mapCatalog.Maps[0].Prefab, 0);
        }

        private void ShowMainPanel()
        {
            if (_mainPanel != null)
                _mainPanel.SetActive(true);
            if (_mapPanel != null)
                _mapPanel.SetActive(false);
            if (_carPanel != null)
                _carPanel.SetActive(false);
            if (_racePanel != null)
                _racePanel.SetActive(false);
        }

        private void ShowMapPanel()
        {
            PreparePendingMapSelection();
            if (_mainPanel != null)
                _mainPanel.SetActive(false);
            if (_mapPanel != null)
                _mapPanel.SetActive(true);
            if (_carPanel != null)
                _carPanel.SetActive(false);
            if (_racePanel != null)
                _racePanel.SetActive(false);

            RefreshMapButtons();
            UpdateMapPresentation();
        }

        private void ShowCarPanel()
        {
            PreparePendingCarSelection();
            if (_mainPanel != null)
                _mainPanel.SetActive(false);
            if (_mapPanel != null)
                _mapPanel.SetActive(false);
            if (_carPanel != null)
                _carPanel.SetActive(true);
            if (_racePanel != null)
                _racePanel.SetActive(false);

            RefreshCarButtons();
            UpdateCarPresentation();
        }

        private void ConfirmMapSelection()
        {
            if (_gameSelectionParameters == null || _pendingMapIndex < 0)
                return;

            _gameSelectionParameters.SetSelectedMap(_pendingMapPrefab, _pendingMapIndex);
            RefreshMapButtons();
            ShowRacePanel();
        }

        private void ConfirmCarSelection()
        {
            if (_gameSelectionParameters == null || _pendingCarIndex < 0)
                return;

            _gameSelectionParameters.SetSelectedCar(_pendingCarPreset, _pendingCarParameters, _pendingCarIndex);
            RefreshCarButtons();
            ShowMainPanel();
        }

        private void ConfirmRaceSelection()
        {
            StartRace();
        }

        private void ShowRacePanel()
        {
            if (_mainPanel != null)
                _mainPanel.SetActive(false);
            if (_mapPanel != null)
                _mapPanel.SetActive(false);
            if (_carPanel != null)
                _carPanel.SetActive(false);
            if (_racePanel != null)
                _racePanel.SetActive(true);
        }

        private void PreparePendingMapSelection()
        {
            if (_mapCatalog == null || _gameSelectionParameters == null || _mapCatalog.Maps.Count == 0)
                return;

            var clamped = Mathf.Clamp(_gameSelectionParameters.SelectedMapIndex, 0, _mapCatalog.Maps.Count - 1);
            ApplyPendingMap(clamped);
        }

        private void PreparePendingCarSelection()
        {
            if (_carCatalog == null || _gameSelectionParameters == null || _carCatalog.Cars.Count == 0)
                return;

            var clamped = Mathf.Clamp(_gameSelectionParameters.SelectedCarIndex, 0, _carCatalog.Cars.Count - 1);
            ApplyPendingCar(clamped);
        }

        private void ApplyPendingMap(int index)
        {
            var entry = _mapCatalog.Maps[index];
            _pendingMapIndex = index;
            _pendingMapPrefab = entry.Prefab;
            _pendingMapPreview = entry.Preview;
            UpdateMapPresentation();
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

        private void UpdateMapPresentation()
        {
            if (_mapPresentation == null)
                return;

            _mapPresentation.sprite = _pendingMapPreview;
            _mapPresentation.enabled = _mapPresentation.sprite != null;
        }

        private void UpdateCarPresentation()
        {
            if (_carPresentation == null)
                return;

            _carPresentation.sprite = _pendingCarPreview;
            _carPresentation.enabled = _carPresentation.sprite != null;
        }

        private int GetMapSelectionIndexForButtons()
        {
            if (_pendingMapIndex >= 0)
                return _pendingMapIndex;

            return _gameSelectionParameters != null ? _gameSelectionParameters.SelectedMapIndex : -1;
        }

        private int GetCarSelectionIndexForButtons()
        {
            if (_pendingCarIndex >= 0)
                return _pendingCarIndex;

            return _gameSelectionParameters != null ? _gameSelectionParameters.SelectedCarIndex : -1;
        }
    }
}
