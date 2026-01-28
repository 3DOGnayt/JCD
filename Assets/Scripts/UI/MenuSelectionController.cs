using System.Collections;
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
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _oponentPanel;

        [Header("Buttons")]
        [SerializeField] private Button _openCarPanelButton;
        [SerializeField] private Button _openSettingsButton;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _exitButton;

        [Header("Back Buttons")]
        [SerializeField] private Button _backFromMapButton;
        [SerializeField] private Button _backFromCarButton;
        [SerializeField] private Button _backFromRaceButton;
        [SerializeField] private Button _backFromOpponentButton;

        [Header("Confirm Buttons")]
        [SerializeField] private Button _confirmMapButton;
        [SerializeField] private Button _confirmCarButton;
        [SerializeField] private Button _confirmRaceButton;
        [SerializeField] private Button _confirmOpponentButton;
        [SerializeField] private GameObject _opponentThings;

        [Header("Selection Buttons")]
        [SerializeField] private List<Button> _mapButtons = new List<Button>();
        [SerializeField] private List<Button> _carButtons = new List<Button>();

        [Header("Presentation")]
        [SerializeField] private Image _mapPresentation;
        [SerializeField] private Image _carPresentation;

        [Header("Transitions")]
        [SerializeField] private float _panelSlideDuration = 0.25f;
        [SerializeField] private AnimationCurve _panelSlideEase;
        [SerializeField] private float _menuLeftShift = 450f;
        [SerializeField] private float _mapSlideOffset = 600f;
        [SerializeField] private float _carSlideOffset = 1100f;
        [SerializeField] private float _raceSlideOffset = 600f;
        [SerializeField] private float _oponentSlideOffset = 600f;
        [SerializeField] private float _oponentPanelDelay = 0.25f;
        [SerializeField] private float _mapPresentationDelay = 0.1f;
        [SerializeField] private float _racePanelDelay = 0.25f;
        [SerializeField] private float _oponentThingsDelay = 0.1f;

        [Header("Data")]
        [SerializeField] private MapCatalog _mapCatalog;
        [SerializeField] private CarCatalog _carCatalog;
        [SerializeField] private GameSelectionParameters _gameSelectionParameters;

        [SerializeField] private SettingsPanelController _settingsPanelController;

        [Inject] private SignalBus _signalBus;

        private RectTransform _mainPanelTransform;
        private RectTransform _mapPanelTransform;
        private RectTransform _carPanelTransform;
        private RectTransform _racePanelTransform;
        private RectTransform _oponentPanelTransform;

        private Vector2 _mainPanelBasePosition;
        private Vector2 _mapPanelBasePosition;
        private Vector2 _carPanelBasePosition;
        private Vector2 _racePanelBasePosition;
        private Vector2 _oponentPanelBasePosition;

        private Coroutine _mainPanelSlideRoutine;
        private Coroutine _mapPanelSlideRoutine;
        private Coroutine _carPanelSlideRoutine;
        private Coroutine _racePanelSlideRoutine;
        private Coroutine _oponentPanelSlideRoutine;
        private Coroutine _returnFromRaceRoutine;
        private Coroutine _oponentPanelEntryRoutine;
        private Coroutine _racePanelEntryRoutine;
        private Coroutine _mapPanelReturnRoutine;
        private Coroutine _mapPresentationRoutine;
        private Coroutine _confirmMapRoutine;
        private Coroutine _oponentThingsRoutine;

        private int _pendingMapIndex = -1;
        private GameObject _pendingMapPrefab;
        private Sprite _pendingMapPreview;

        private int _pendingCarIndex = -1;
        private CarPresetParameters _pendingCarPreset;
        private CarParameters _pendingCarParameters;
        private Sprite _pendingCarPreview;

        private void Awake()
        {
            CachePanelTransforms();
            WireMainButtons();
            EnsureDefaultSelection();
            BuildButtons(_mapButtons, _mapCatalog != null ? _mapCatalog.Maps.Count : 0);
            BuildButtons(_carButtons, _carCatalog != null ? _carCatalog.Cars.Count : 0);
            RefreshButtons();
            ShowMainPanel();

            if (_settingsPanelController != null)
                _settingsPanelController.Closed += HandleSettingsClosed;
        }

        private void OnDestroy()
        {
            if (_settingsPanelController != null)
                _settingsPanelController.Closed -= HandleSettingsClosed;
        }

        private void CachePanelTransforms()
        {
            if (_mainPanelTransform == null && _mainPanel != null)
                _mainPanelTransform = _mainPanel.GetComponent<RectTransform>();
            if (_mapPanelTransform == null && _mapPanel != null)
                _mapPanelTransform = _mapPanel.GetComponent<RectTransform>();
            if (_carPanelTransform == null && _carPanel != null)
                _carPanelTransform = _carPanel.GetComponent<RectTransform>();
            if (_racePanelTransform == null && _racePanel != null)
                _racePanelTransform = _racePanel.GetComponent<RectTransform>();
            if (_oponentPanelTransform == null && _oponentPanel != null)
                _oponentPanelTransform = _oponentPanel.GetComponent<RectTransform>();

            if (_mainPanelTransform != null)
                _mainPanelBasePosition = _mainPanelTransform.anchoredPosition;
            if (_mapPanelTransform != null)
                _mapPanelBasePosition = _mapPanelTransform.anchoredPosition;
            if (_carPanelTransform != null)
                _carPanelBasePosition = _carPanelTransform.anchoredPosition;
            if (_racePanelTransform != null)
                _racePanelBasePosition = _racePanelTransform.anchoredPosition;
            if (_oponentPanelTransform != null)
                _oponentPanelBasePosition = _oponentPanelTransform.anchoredPosition;

            if (_settingsPanelController == null && _settingsPanel != null)
                _settingsPanelController = _settingsPanel.GetComponent<SettingsPanelController>();
        }

        private void WireMainButtons()
        {
            if (_openCarPanelButton != null)
                _openCarPanelButton.onClick.AddListener(ShowCarPanel);

            if (_openSettingsButton != null)
                _openSettingsButton.onClick.AddListener(ShowSettingsPanel);

            if (_startButton != null)
                _startButton.onClick.AddListener(ShowMapPanel);

            if (_exitButton != null)
                _exitButton.onClick.AddListener(Application.Quit);

            if (_backFromMapButton != null)
                _backFromMapButton.onClick.AddListener(ShowMainPanelFromMap);

            if (_backFromCarButton != null)
                _backFromCarButton.onClick.AddListener(ShowMainPanelFromCar);

            if (_backFromRaceButton != null)
                _backFromRaceButton.onClick.AddListener(ShowOpponentPanelFromRace);
            if (_backFromOpponentButton != null)
                _backFromOpponentButton.onClick.AddListener(ShowMapPanelFromOpponent);

            if (_confirmMapButton != null)
                _confirmMapButton.onClick.AddListener(ConfirmMapSelection);

            if (_confirmCarButton != null)
                _confirmCarButton.onClick.AddListener(ConfirmCarSelection);

            if (_confirmRaceButton != null)
                _confirmRaceButton.onClick.AddListener(ConfirmRaceSelection);
            if (_confirmOpponentButton != null)
                _confirmOpponentButton.onClick.AddListener(ConfirmOpponentSelection);
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
            if (_settingsPanel != null)
                _settingsPanel.SetActive(false);
            if (_oponentPanel != null)
                _oponentPanel.SetActive(false);

            ResetPanelPositions();
            SetMenuButtonsInteractable(true);
        }

        private void ShowMainPanelFromMap()
        {
            if (_mainPanel != null)
                _mainPanel.SetActive(true);
            if (_carPanel != null)
                _carPanel.SetActive(false);
            if (_racePanel != null)
                _racePanel.SetActive(false);
            if (_settingsPanel != null)
                _settingsPanel.SetActive(false);
            if (_oponentPanel != null)
                _oponentPanel.SetActive(false);

            SetMenuButtonsInteractable(true);
            if (_mapPanelTransform != null)
            {
                SlidePanel(_mapPanelTransform, _mapPanelBasePosition - new Vector2(0f, _mapSlideOffset),
                    ref _mapPanelSlideRoutine, true);
            }
            else if (_mapPanel != null)
            {
                _mapPanel.SetActive(false);
            }

            if (_mapPresentation != null)
                _mapPresentation.gameObject.SetActive(false);
            if (_confirmMapButton != null)
                _confirmMapButton.gameObject.SetActive(false);
            if (_backFromMapButton != null)
                _backFromMapButton.gameObject.SetActive(false);
        }

        private void ShowMainPanelFromCar()
        {
            if (_mainPanel != null)
                _mainPanel.SetActive(true);
            if (_mapPanel != null)
                _mapPanel.SetActive(false);
            if (_racePanel != null)
                _racePanel.SetActive(false);
            if (_settingsPanel != null)
                _settingsPanel.SetActive(false);
            if (_oponentPanel != null)
                _oponentPanel.SetActive(false);

            SetMenuButtonsInteractable(true);
            if (_carPanelTransform != null)
            {
                SlidePanel(_carPanelTransform, _carPanelBasePosition + new Vector2(0f, _carSlideOffset),
                    ref _carPanelSlideRoutine, true);
            }
            else if (_carPanel != null)
            {
                _carPanel.SetActive(false);
            }
        }

        private void ShowMapPanel()
        {
            PreparePendingMapSelection();
            if (_mainPanel != null)
                _mainPanel.SetActive(true);
            if (_mapPanel != null)
                _mapPanel.SetActive(true);
            if (_carPanel != null)
                _carPanel.SetActive(false);
            if (_racePanel != null)
                _racePanel.SetActive(false);
            if (_settingsPanel != null)
                _settingsPanel.SetActive(false);
            if (_oponentPanel != null)
                _oponentPanel.SetActive(false);

            ResetPanelPositions();
            SetMenuButtonsInteractable(false);
            if (_mapPanelTransform != null)
            {
                _mapPanelTransform.anchoredPosition = _mapPanelBasePosition - new Vector2(0f, _mapSlideOffset);
                SlidePanel(_mapPanelTransform, _mapPanelBasePosition, ref _mapPanelSlideRoutine);
            }
            if (_mapPresentation != null)
            {
                _mapPresentation.gameObject.SetActive(false);
                ShowMapPresentationDelayed();
            }
            if (_confirmMapButton != null)
            {
                _confirmMapButton.gameObject.SetActive(false);
                ShowConfirmMapDelayed();
            }
            if (_backFromMapButton != null)
                _backFromMapButton.gameObject.SetActive(false);
            RefreshMapButtons();
            UpdateMapPresentation();
        }

        private void ShowCarPanel()
        {
            PreparePendingCarSelection();
            if (_mainPanel != null)
                _mainPanel.SetActive(true);
            if (_mapPanel != null)
                _mapPanel.SetActive(false);
            if (_carPanel != null)
                _carPanel.SetActive(true);
            if (_racePanel != null)
                _racePanel.SetActive(false);
            if (_settingsPanel != null)
                _settingsPanel.SetActive(false);
            if (_oponentPanel != null)
                _oponentPanel.SetActive(false);

            SetMenuButtonsInteractable(false);
            if (_carPanelTransform != null)
            {
                _carPanelTransform.anchoredPosition = _carPanelBasePosition + new Vector2(0f, _carSlideOffset);
                SlidePanel(_carPanelTransform, _carPanelBasePosition, ref _carPanelSlideRoutine);
            }

            RefreshCarButtons();
            UpdateCarPresentation();
        }

        private void ConfirmMapSelection()
        {
            if (_gameSelectionParameters == null || _pendingMapIndex < 0)
                return;

            _gameSelectionParameters.SetSelectedMap(_pendingMapPrefab, _pendingMapIndex);
            RefreshMapButtons();
            ShowOpponentPanel();
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

        private void ConfirmOpponentSelection()
        {
            ShowRacePanelFromOpponent();
        }

        private void ShowOpponentPanel()
        {
            if (_mainPanel != null)
                _mainPanel.SetActive(true);
            if (_mapPanel != null)
                _mapPanel.SetActive(true);
            if (_carPanel != null)
                _carPanel.SetActive(false);
            if (_racePanel != null)
                _racePanel.SetActive(false);
            if (_settingsPanel != null)
                _settingsPanel.SetActive(false);
            if (_oponentPanel != null)
                _oponentPanel.SetActive(true);

            SetMenuButtonsInteractable(false);
            if (_mapPresentation != null)
                _mapPresentation.gameObject.SetActive(false);
            if (_confirmMapButton != null)
                _confirmMapButton.gameObject.SetActive(false);
            if (_backFromMapButton != null)
                _backFromMapButton.gameObject.SetActive(false);

            SlidePanel(_mainPanelTransform, _mainPanelBasePosition + new Vector2(-_menuLeftShift, 0f),
                ref _mainPanelSlideRoutine);
            if (_mapPanelTransform != null)
            {
                SlidePanel(_mapPanelTransform, _mapPanelBasePosition + new Vector2(-_menuLeftShift, 0f),
                    ref _mapPanelSlideRoutine);
            }

            if (_oponentPanel != null)
                _oponentPanel.SetActive(false);

            if (_oponentPanelEntryRoutine != null)
                StopCoroutine(_oponentPanelEntryRoutine);

            _oponentPanelEntryRoutine = StartCoroutine(ShowOpponentAfterMapSlide());
        }

        private void ShowRacePanel()
        {
            if (_mainPanel != null)
                _mainPanel.SetActive(true);
            if (_mapPanel != null)
                _mapPanel.SetActive(true);
            if (_carPanel != null)
                _carPanel.SetActive(false);
            if (_racePanel != null)
                _racePanel.SetActive(true);
            if (_settingsPanel != null)
                _settingsPanel.SetActive(false);
            if (_oponentPanel != null)
                _oponentPanel.SetActive(false);

            SetMenuButtonsInteractable(false);
            if (_mapPresentation != null)
                _mapPresentation.gameObject.SetActive(false);
            if (_confirmMapButton != null)
                _confirmMapButton.gameObject.SetActive(false);

            SlidePanel(_mainPanelTransform, _mainPanelBasePosition + new Vector2(-_menuLeftShift, 0f),
                ref _mainPanelSlideRoutine);
            SlidePanel(_mapPanelTransform, _mapPanelBasePosition + new Vector2(-_menuLeftShift, 0f),
                ref _mapPanelSlideRoutine);
            SlidePanel(_oponentPanelTransform, _oponentPanelBasePosition + new Vector2(-_menuLeftShift, 0f),
                ref _oponentPanelSlideRoutine, true);

            if (_racePanelTransform != null)
            {
                _racePanelTransform.anchoredPosition = _racePanelBasePosition + new Vector2(_raceSlideOffset, 0f);
                SlidePanel(_racePanelTransform, _racePanelBasePosition, ref _racePanelSlideRoutine);
            }
        }

        private void ShowRacePanelFromOpponent()
        {
            if (_mainPanel != null)
                _mainPanel.SetActive(true);
            if (_mapPanel != null)
                _mapPanel.SetActive(true);
            if (_carPanel != null)
                _carPanel.SetActive(false);
            if (_racePanel != null)
                _racePanel.SetActive(true);
            if (_settingsPanel != null)
                _settingsPanel.SetActive(false);
            if (_oponentPanel != null)
                _oponentPanel.SetActive(true);

            SetMenuButtonsInteractable(false);

            if (_opponentThings != null)
                _opponentThings.SetActive(false);
            if (_confirmOpponentButton != null)
                _confirmOpponentButton.gameObject.SetActive(false);
            if (_backFromOpponentButton != null)
                _backFromOpponentButton.gameObject.SetActive(false);

            if (_racePanelTransform != null)
                _racePanelTransform.anchoredPosition = _racePanelBasePosition + new Vector2(_raceSlideOffset, 0f);

            SlidePanel(_mainPanelTransform, _mainPanelBasePosition + new Vector2(-_menuLeftShift * 2f, 0f),
                ref _mainPanelSlideRoutine);
            SlidePanel(_mapPanelTransform, _mapPanelBasePosition + new Vector2(-_menuLeftShift * 2f, 0f),
                ref _mapPanelSlideRoutine);
            SlidePanel(_oponentPanelTransform, _oponentPanelBasePosition + new Vector2(-_menuLeftShift, 0f),
                ref _oponentPanelSlideRoutine);

            if (_racePanelEntryRoutine != null)
                StopCoroutine(_racePanelEntryRoutine);

            _racePanelEntryRoutine = StartCoroutine(ShowRaceAfterOpponentShift());
        }

        private void ShowMapPanelFromOpponent()
        {
            if (_mainPanel != null)
                _mainPanel.SetActive(true);
            if (_mapPanel != null)
                _mapPanel.SetActive(true);
            if (_carPanel != null)
                _carPanel.SetActive(false);
            if (_racePanel != null)
                _racePanel.SetActive(false);
            if (_settingsPanel != null)
                _settingsPanel.SetActive(false);

            SetMenuButtonsInteractable(false);
            if (_oponentPanelTransform != null)
            {
                SlidePanel(_oponentPanelTransform, _oponentPanelBasePosition + new Vector2(0f, _oponentSlideOffset),
                    ref _oponentPanelSlideRoutine, true);
            }
            else if (_oponentPanel != null)
            {
                _oponentPanel.SetActive(false);
            }

            if (_mapPanelReturnRoutine != null)
                StopCoroutine(_mapPanelReturnRoutine);

            _mapPanelReturnRoutine = StartCoroutine(ReturnMapAfterOpponentExit());
        }

        private IEnumerator ShowOpponentAfterMapSlide()
        {
            var delay = Mathf.Max(0f, _oponentPanelDelay);
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            if (_oponentPanelTransform == null)
                yield break;

            if (_oponentPanel != null)
                _oponentPanel.SetActive(true);
            _oponentPanelTransform.anchoredPosition = _oponentPanelBasePosition + new Vector2(0f, _oponentSlideOffset);
            SlidePanel(_oponentPanelTransform, _oponentPanelBasePosition, ref _oponentPanelSlideRoutine);
        }

        private IEnumerator ReturnMapAfterOpponentExit()
        {
            yield return new WaitForSeconds(_panelSlideDuration);

            SlidePanel(_mainPanelTransform, _mainPanelBasePosition, ref _mainPanelSlideRoutine);
            if (_mapPanelTransform != null)
            {
                SlidePanel(_mapPanelTransform, _mapPanelBasePosition, ref _mapPanelSlideRoutine);
            }

            if (_mapPresentation != null)
            {
                _mapPresentation.gameObject.SetActive(false);
                ShowMapPresentationDelayed();
            }
            if (_confirmMapButton != null)
            {
                _confirmMapButton.gameObject.SetActive(false);
                ShowConfirmMapDelayed();
            }
            if (_backFromMapButton != null)
                _backFromMapButton.gameObject.SetActive(false);
        }

        private IEnumerator ShowRaceAfterOpponentShift()
        {
            var delay = Mathf.Max(0f, _racePanelDelay);
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            if (_racePanelTransform == null)
                yield break;

            SlidePanel(_racePanelTransform, _racePanelBasePosition, ref _racePanelSlideRoutine);
        }

        private void ShowOpponentPanelFromRace()
        {
            if (_mainPanel != null)
                _mainPanel.SetActive(true);
            if (_mapPanel != null)
                _mapPanel.SetActive(true);
            if (_carPanel != null)
                _carPanel.SetActive(false);
            if (_settingsPanel != null)
                _settingsPanel.SetActive(false);
            if (_oponentPanel != null)
                _oponentPanel.SetActive(true);

            SetMenuButtonsInteractable(false);
            if (_mapPresentation != null)
                _mapPresentation.gameObject.SetActive(false);
            if (_confirmMapButton != null)
                _confirmMapButton.gameObject.SetActive(false);
            if (_backFromMapButton != null)
                _backFromMapButton.gameObject.SetActive(false);

            if (_returnFromRaceRoutine != null)
                StopCoroutine(_returnFromRaceRoutine);

            _returnFromRaceRoutine = StartCoroutine(ReturnFromRaceRoutine());
        }

        private IEnumerator ReturnFromRaceRoutine()
        {
            if (_racePanelTransform != null)
            {
                SlidePanel(_racePanelTransform, _racePanelBasePosition + new Vector2(_raceSlideOffset, 0f),
                    ref _racePanelSlideRoutine, true);
            }
            else if (_racePanel != null)
            {
                _racePanel.SetActive(false);
            }

            yield return new WaitForSeconds(_panelSlideDuration);

            SlidePanel(_mainPanelTransform, _mainPanelBasePosition + new Vector2(-_menuLeftShift, 0f),
                ref _mainPanelSlideRoutine);
            SlidePanel(_mapPanelTransform, _mapPanelBasePosition + new Vector2(-_menuLeftShift, 0f),
                ref _mapPanelSlideRoutine);
            SlidePanel(_oponentPanelTransform, _oponentPanelBasePosition, ref _oponentPanelSlideRoutine);

            ShowOpponentThingsDelayed();
        }

        private void ShowMapPresentationDelayed()
        {
            if (_mapPresentationRoutine != null)
                StopCoroutine(_mapPresentationRoutine);

            _mapPresentationRoutine = StartCoroutine(ShowMapPresentationAfterDelay());
        }

        private IEnumerator ShowMapPresentationAfterDelay()
        {
            var delay = Mathf.Max(0f, _mapPresentationDelay);
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            if (_mapPresentation != null)
                _mapPresentation.gameObject.SetActive(true);
        }

        private void ShowConfirmMapDelayed()
        {
            if (_confirmMapRoutine != null)
                StopCoroutine(_confirmMapRoutine);

            _confirmMapRoutine = StartCoroutine(ShowConfirmMapAfterDelay());
        }

        private IEnumerator ShowConfirmMapAfterDelay()
        {
            var delay = Mathf.Max(0f, _mapPresentationDelay);
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            if (_confirmMapButton != null)
                _confirmMapButton.gameObject.SetActive(true);
            if (_backFromMapButton != null)
                _backFromMapButton.gameObject.SetActive(true);
        }

        private void ShowOpponentThingsDelayed()
        {
            if (_oponentThingsRoutine != null)
                StopCoroutine(_oponentThingsRoutine);

            _oponentThingsRoutine = StartCoroutine(ShowOpponentThingsAfterDelay());
        }

        private IEnumerator ShowOpponentThingsAfterDelay()
        {
            var delay = Mathf.Max(0f, _oponentThingsDelay);
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            if (_opponentThings != null)
                _opponentThings.SetActive(true);
            if (_confirmOpponentButton != null)
                _confirmOpponentButton.gameObject.SetActive(true);
            if (_backFromOpponentButton != null)
                _backFromOpponentButton.gameObject.SetActive(true);
        }

        private void ShowSettingsPanel()
        {
            if (_mainPanel != null)
                _mainPanel.SetActive(true);
            if (_mapPanel != null)
                _mapPanel.SetActive(false);
            if (_carPanel != null)
                _carPanel.SetActive(false);
            if (_racePanel != null)
                _racePanel.SetActive(false);
            if (_settingsPanelController != null)
                _settingsPanelController.OpenSettings();
            if (_oponentPanel != null)
                _oponentPanel.SetActive(false);

            ResetPanelPositions();
            SetMenuButtonsInteractable(false);
        }

        private void HandleSettingsClosed()
        {
            SetMenuButtonsInteractable(true);
        }

        private void SetMenuButtonsInteractable(bool isInteractable)
        {
            SetButtonInteractable(_openCarPanelButton, isInteractable);
            SetButtonInteractable(_openSettingsButton, isInteractable);
            SetButtonInteractable(_startButton, isInteractable);
            SetButtonInteractable(_exitButton, isInteractable);
        }

        private void SetButtonInteractable(Button button, bool isInteractable)
        {
            if (button != null)
                button.interactable = isInteractable;
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

        private void ResetPanelPositions()
        {
            SetPanelPosition(_mainPanelTransform, _mainPanelBasePosition, ref _mainPanelSlideRoutine);
            SetPanelPosition(_mapPanelTransform, _mapPanelBasePosition, ref _mapPanelSlideRoutine);
            SetPanelPosition(_carPanelTransform, _carPanelBasePosition, ref _carPanelSlideRoutine);
            SetPanelPosition(_racePanelTransform, _racePanelBasePosition, ref _racePanelSlideRoutine);
            SetPanelPosition(_oponentPanelTransform, _oponentPanelBasePosition, ref _oponentPanelSlideRoutine);
        }

        private void SetPanelPosition(RectTransform rect, Vector2 position, ref Coroutine routine)
        {
            if (rect == null)
                return;

            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }

            rect.anchoredPosition = position;
        }

        private void SlidePanel(RectTransform rect, Vector2 target, ref Coroutine routine, bool deactivateOnComplete = false)
        {
            if (rect == null)
                return;

            if (routine != null)
                StopCoroutine(routine);

            routine = StartCoroutine(SlidePanelRoutine(rect, rect.anchoredPosition, target, deactivateOnComplete));
        }

        private IEnumerator SlidePanelRoutine(RectTransform rect, Vector2 from, Vector2 to, bool deactivateOnComplete)
        {
            var duration = Mathf.Max(0.01f, _panelSlideDuration);
            var ease = _panelSlideEase != null && _panelSlideEase.length > 0
                ? _panelSlideEase
                : AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var eased = ease.Evaluate(t);
                rect.anchoredPosition = Vector2.LerpUnclamped(from, to, eased);
                yield return null;
            }

            rect.anchoredPosition = to;

            if (deactivateOnComplete)
                rect.gameObject.SetActive(false);
        }
    }
}
