using KoboldUi.Windows;
using UI.Canvas;
using UI.Canvas.MenuSelection.Views;
using UnityEngine;
using Zenject;

namespace UI.Canvas.MenuSelection
{
    public class MenuSelectionWindow : AWindow
    {
        [Header("Views")]
        [SerializeField] private MainMenuView _mainMenuView;
        [SerializeField] private MapSelectionView _mapSelectionView;
        [SerializeField] private CarSelectionView _carSelectionView;
        [SerializeField] private RaceSelectionView _raceSelectionView;
        [SerializeField] private OpponentSelectionView _opponentSelectionView;
        [SerializeField] private SettingsPanelController _settingsPanelController;

        [Header("Transitions")]
        [SerializeField] private MenuSelectionTransitionsConfig _transitions;

        private bool _isReady;

        public MenuSelectionFlow Flow { get; private set; }
        public MainMenuView MainMenuView => _mainMenuView;
        public MapSelectionView MapSelectionView => _mapSelectionView;
        public CarSelectionView CarSelectionView => _carSelectionView;
        public RaceSelectionView RaceSelectionView => _raceSelectionView;
        public OpponentSelectionView OpponentSelectionView => _opponentSelectionView;
        public SettingsPanelController SettingsPanelController => _settingsPanelController;

        public float PanelSlideDuration => _transitions.PanelSlideDuration;
        public AnimationCurve PanelSlideEase => _transitions.PanelSlideEase;
        public float MenuLeftShift => _transitions.MenuLeftShift;
        public float MapSlideOffset => _transitions.MapSlideOffset;
        public float CarSlideOffset => _transitions.CarSlideOffset;
        public float RaceSlideOffset => _transitions.RaceSlideOffset;
        public float OpponentSlideOffset => _transitions.OpponentSlideOffset;
        public float OpponentPanelDelay => _transitions.OpponentPanelDelay;
        public float RacePanelDelay => _transitions.RacePanelDelay;

        [Inject]
        private void Construct(MenuSelectionTransitionsConfig transitions)
        {
            if (_transitions == null)
                _transitions = transitions;
        }

        private void Awake()
        {
            _isReady = ValidateReferences();
            if (!_isReady)
                return;

            Flow = new MenuSelectionFlow(this);
            Flow.Initialize();

            _settingsPanelController.Closed += HandleSettingsClosed;
        }

        private void OnDestroy()
        {
            if (_settingsPanelController != null)
                _settingsPanelController.Closed -= HandleSettingsClosed;
        }

        protected override void AddControllers()
        {
            if (!_isReady)
                return;

            AddController<Controllers.MainMenuController, MainMenuView>(_mainMenuView);
            AddController<Controllers.MapSelectionController, MapSelectionView>(_mapSelectionView);
            AddController<Controllers.CarSelectionController, CarSelectionView>(_carSelectionView);
            AddController<Controllers.OpponentSelectionController, OpponentSelectionView>(_opponentSelectionView);
            AddController<Controllers.RaceSelectionController, RaceSelectionView>(_raceSelectionView);
        }

        private bool ValidateReferences()
        {
            var isValid = true;

            isValid &= ValidateReference("MainMenuView", _mainMenuView != null && _mainMenuView.IsValid);
            isValid &= ValidateReference("MapSelectionView", _mapSelectionView != null && _mapSelectionView.IsValid);
            isValid &= ValidateReference("CarSelectionView", _carSelectionView != null && _carSelectionView.IsValid);
            isValid &= ValidateReference("RaceSelectionView", _raceSelectionView != null && _raceSelectionView.IsValid);
            isValid &= ValidateReference("OpponentSelectionView", _opponentSelectionView != null && _opponentSelectionView.IsValid);
            isValid &= ValidateReference("SettingsPanelController", _settingsPanelController != null);
            isValid &= ValidateReference("TransitionsConfig", _transitions != null);

            return isValid;
        }

        private bool ValidateReference(string label, bool isValid)
        {
            if (!isValid)
                Debug.LogError($"[MenuSelection] {label} is missing required references.");

            return isValid;
        }

        private void HandleSettingsClosed()
        {
            if (Flow != null)
                Flow.SetMenuButtonsInteractable(true);
        }
    }
}
