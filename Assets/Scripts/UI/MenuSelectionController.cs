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

        [Header("Buttons")]
        [SerializeField] private Button _openMapPanelButton;
        [SerializeField] private Button _openCarPanelButton;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _exitButton;

        [Header("Selection Buttons")]
        [SerializeField] private List<Button> _mapButtons = new List<Button>();
        [SerializeField] private List<Button> _carButtons = new List<Button>();

        [Header("Data")]
        [SerializeField] private MapCatalog _mapCatalog;
        [SerializeField] private CarCatalog _carCatalog;
        [SerializeField] private GameSelectionParameters _gameSelectionParameters;

        [Inject] private SignalBus _signalBus;

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
                _startButton.onClick.AddListener(StartRace);

            if (_exitButton != null)
                _exitButton.onClick.AddListener(Application.Quit);
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

                var selected = _gameSelectionParameters != null && _gameSelectionParameters.SelectedMapIndex == index;
                button.interactable = !selected;

                button.onClick.RemoveAllListeners();
                var captured = index;
                button.onClick.AddListener(() => SelectMap(captured));
            }
        }

        private void RefreshCarButtons()
        {
            var carCount = _carCatalog != null ? _carCatalog.Cars.Count : 0;
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

                var selected = _gameSelectionParameters != null && _gameSelectionParameters.SelectedCarIndex == index;
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

            var entry = _mapCatalog.Maps[index];
            _gameSelectionParameters.SetSelectedMap(entry.Prefab, index);
            RefreshMapButtons();
            ShowMainPanel();
        }

        private void SelectCar(int index)
        {
            if (_carCatalog == null || _gameSelectionParameters == null)
                return;

            if (index < 0 || index >= _carCatalog.Cars.Count)
                return;

            var entry = _carCatalog.Cars[index];
            _gameSelectionParameters.SetSelectedCar(entry.Preset, index);
            RefreshCarButtons();
            ShowMainPanel();
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
                _gameSelectionParameters.SetSelectedCar(_carCatalog.Cars[0].Preset, 0);

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
        }

        private void ShowMapPanel()
        {
            if (_mainPanel != null)
                _mainPanel.SetActive(false);
            if (_mapPanel != null)
                _mapPanel.SetActive(true);
            if (_carPanel != null)
                _carPanel.SetActive(false);
        }

        private void ShowCarPanel()
        {
            if (_mainPanel != null)
                _mainPanel.SetActive(false);
            if (_mapPanel != null)
                _mapPanel.SetActive(false);
            if (_carPanel != null)
                _carPanel.SetActive(true);
        }
    }
}
