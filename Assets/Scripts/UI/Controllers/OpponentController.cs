using Configs.Impl;
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
    public class OpponentController : AUiController<OpponentView>
    {
        private const int LEFT_SLIDE_600 = -600;
        
        private readonly ILocalWindowsService _localWindowsService;
        private readonly IGameSessionService _gameSessionService;

        private string _pendingOpponentName;
        private Sprite _pendingOpponentPreview;
        private float _pendingOpponentDifficulty;
        private int _pendingOpponentIndex = -1;
        private bool _isReady;

        private OpponentCatalogParameters _opponentCatalogParameters;
        private OpponentSelectionParameters _opponentSelectionParameters;
        
        [Inject]
        public void Construct(
            OpponentCatalogParameters opponentCatalogParameters,
            OpponentSelectionParameters opponentSelectionParameters
        )
        {
            _opponentCatalogParameters = opponentCatalogParameters;
            _opponentSelectionParameters = opponentSelectionParameters;
        }

        public OpponentController(
            ILocalWindowsService localWindowsService,
            IGameSessionService gameSessionService
        )
        {
            _localWindowsService = localWindowsService;
            _gameSessionService = gameSessionService;
        }

        public override void Initialize()
        {
            _isReady = _opponentCatalogParameters != null && _opponentSelectionParameters != null;
            if (!_isReady)
                return;

            for (var i = 0; i < View.OpponentButtons.Count; i++)
            {
                var buttonIndex = i;
                View.OpponentButtons[buttonIndex].OnClickAsObservable()
                    .Subscribe(_ => OnOpponentButtonClick(buttonIndex)).AddTo(View);
            }

            View.ConfirmButton.OnClickAsObservable().Subscribe(_ => OnConfirmButtonClick()).AddTo(View);
            View.BackButton.OnClickAsObservable().Subscribe(_ => OnBackButtonClick()).AddTo(View);
        }

        protected override void OnOpen()
        {
            if (!_isReady)
                return;

            InitializeOpponentButtonLabels();
            EnsureDefaultSelection();
            PreparePendingOpponentSelection();
            RefreshOpponentButtons();
            UpdateOpponentPresentation();
        }

        private void InitializeOpponentButtonLabels()
        {
            var opponentCount = _opponentCatalogParameters.Opponents.Count;
            for (var index = 0; index < View.OpponentButtons.Count && index < opponentCount; index++)
            {
                var label = View.OpponentButtonsText[index];
                label.text = _opponentCatalogParameters.Opponents[index].Base.DisplayName;
            }
        }

        private void EnsureDefaultSelection()
        {
            if (!string.IsNullOrEmpty(_opponentSelectionParameters.SelectedOpponentName) || _opponentCatalogParameters.Opponents.Count <= 0)
                return;

            var entry = _opponentCatalogParameters.Opponents[0];
            _opponentSelectionParameters.SetSelectedOpponent(entry.Base.DisplayName, entry.Base.Difficulty, 0);
        }

        private void PreparePendingOpponentSelection()
        {
            if (_opponentCatalogParameters.Opponents.Count == 0)
                return;

            var clamped = Mathf.Clamp(
                _opponentSelectionParameters.SelectedOpponentIndex,
                0, _opponentCatalogParameters.Opponents.Count - 1);
            
            ApplyPendingOpponent(clamped);
        }

        private void RefreshOpponentButtons()
        {
            var selectedIndex = GetOpponentSelectionIndexForButtons();
            var opponentCount = _opponentCatalogParameters != null ? _opponentCatalogParameters.Opponents.Count : 0;

            for (var index = 0; index < View.OpponentButtons.Count; index++)
            {
                var button = View.OpponentButtons[index];
                var isValid = index < opponentCount;

                button.gameObject.SetActive(isValid);
                if (!isValid)
                    continue;

                var selected = selectedIndex == index;
                button.interactable = !selected;
            }
        }

        private int GetOpponentSelectionIndexForButtons()
        {
            if (_pendingOpponentIndex >= 0)
                return _pendingOpponentIndex;

            return _opponentSelectionParameters != null ? _opponentSelectionParameters.SelectedOpponentIndex : -1;
        }

        private void ApplyPendingOpponent(int index)
        {
            var entry = _opponentCatalogParameters.Opponents[index];
            _pendingOpponentIndex = index;
            _pendingOpponentName = entry.Base.DisplayName;
            _pendingOpponentPreview = entry.Base.Preview;
            _pendingOpponentDifficulty = entry.Base.Difficulty;
            UpdateOpponentPresentation();
        }

        private void UpdateOpponentPresentation()
        {
            if (View.OpponentPresentation != null)
            {
                View.OpponentPresentation.sprite = _pendingOpponentPreview;
                View.OpponentPresentation.enabled = View.OpponentPresentation.sprite != null;
            }

            if (View.OpponentDifficulty != null)
                View.OpponentDifficulty.fillAmount = Mathf.Clamp01(_pendingOpponentDifficulty);
        }

        private void OnOpponentButtonClick(int index)
        {
            if (index < 0 || index >= _opponentCatalogParameters.Opponents.Count)
                return;

            ApplyPendingOpponent(index);
            RefreshOpponentButtons();
        }

        private void OnConfirmButtonClick()
        {
            if (_pendingOpponentIndex < 0)
                return;

            _opponentSelectionParameters.SetSelectedOpponent(
                _pendingOpponentName, _pendingOpponentDifficulty, _pendingOpponentIndex);
            
            RefreshOpponentButtons();
            _localWindowsService.CloseToWindow<MainMenuWindow>();
            _gameSessionService?.BeginGame();
        }

        private void OnBackButtonClick()
        {
            _localWindowsService.AnimateWindow<MainMenuWindow>(Vector2.right * LEFT_SLIDE_600);
            _localWindowsService.AnimateWindow<MapWindow>(Vector2.right * LEFT_SLIDE_600);
            _localWindowsService.AnimateWindow<GameModWindow>(Vector2.zero);
            
            _localWindowsService.CloseWindow();
        }
    }
}