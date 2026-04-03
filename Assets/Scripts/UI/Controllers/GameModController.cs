using System;
using Configs.Impl;
using DG.Tweening;
using Data.Enums;
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
    public class GameModController : AUiController<GameModView>
    {
        private const int LEFT_SLIDE_1200 = -1200;
        private const int LEFT_SLIDE_600 = -600;
        
        private readonly ILocalWindowsService _localWindowsService;
        private readonly IGameSessionService _gameSessionService;
        private readonly IAudioService _audioService;
        private readonly IDataService _dataService;
        
        private GameModeSelectionParameters _gameModeSelectionParameters;
        
        private Tween _presentationTween;
        private Tween _delayTween;
        private float _delaySlideAnimationOnView = 0.2f;

        [Inject]
        public void Construct(GameModeSelectionParameters gameModeSelectionParameters)
        {
            _gameModeSelectionParameters = gameModeSelectionParameters;
        }

        public GameModController(
            ILocalWindowsService localWindowsService,
            IGameSessionService gameSessionService,
            IAudioService audioService,
            IDataService dataService
        )
        {
            _localWindowsService = localWindowsService;
            _gameSessionService = gameSessionService;
            _audioService = audioService;
            _dataService = dataService;
        }

        public override void Initialize()
        {
            if (View.TrainingButton.interactable && View.StoryButton.interactable)
                ApplySavedSelection();

            View.TrainingButton.OnClickAsObservable().Subscribe(_ => OnTrainingButtonClick()).AddTo(View);
            View.StoryButton.OnClickAsObservable().Subscribe(_ => OnStoryButtonClick()).AddTo(View);
            View.ConfirmButton.OnClickAsObservable().Subscribe(_ => OnConfirmButtonClick()).AddTo(View);
            View.BackButton.OnClickAsObservable().Subscribe(_ => OnBackButtonClick()).AddTo(View);
        }
        
        private void ApplySavedSelection()
        {
            if (_gameModeSelectionParameters != null && _gameModeSelectionParameters.GameMod == EGameMod.None)
                RestoreSavedGameMode();

            switch (_gameModeSelectionParameters.GameMod)
            {
                case EGameMod.Story:
                    SetInteractableButtons(true);
                    break;
                case EGameMod.Training:
                    SetInteractableButtons(false);
                    break;
                case EGameMod.None:
                default:
                    SetInteractableButtons(false);
                    _gameModeSelectionParameters.SetSelectedGameMode(EGameMod.Training);
                    SaveGameModeSelection(EGameMod.Training);
                    break;
            }
        }

        protected override void OnOpen()
        {
            _presentationTween?.Kill();

            _presentationTween = DOVirtual
                .DelayedCall(View.PresentationDelay, () => SetPresentationState(true))
                .SetUpdate(true).SetLink(View.gameObject);
        }

        private void OnTrainingButtonClick()
        {
            _audioService.PlayUiAudio(EAudioType.Ui, EAudioSubType.MenuButtonSelect);
            
            SetInteractableButtons(false);
            _gameModeSelectionParameters.SetSelectedGameMode(EGameMod.Training);
            SaveGameModeSelection(EGameMod.Training);
        }

        private void OnStoryButtonClick()
        {
            _audioService.PlayUiAudio(EAudioType.Ui, EAudioSubType.MenuButtonSelect);
            
            SetInteractableButtons(true);
            _gameModeSelectionParameters.SetSelectedGameMode(EGameMod.Story);
            SaveGameModeSelection(EGameMod.Story);
        }

        private void SetInteractableButtons(bool isActive)
        {
            View.TrainingButton.interactable = isActive;
            View.StoryButton.interactable = !isActive;
        }
        
        private void SetPresentationState(bool isActive)
        {
            View.ConfirmButton.gameObject.SetActive(isActive);
            View.BackButton.gameObject.SetActive(isActive);
            View.GameModPresentation.gameObject.SetActive(isActive);
        }

        private void SaveGameModeSelection(EGameMod gameMod)
        {
            if (_dataService == null)
                return;

            _dataService.SaveGameMode(gameMod);
        }

        private void RestoreSavedGameMode()
        {
            if (_dataService == null || _gameModeSelectionParameters == null)
                return;

            var saved = _dataService.LoadGameMode(EGameMod.Training);
            if (saved == EGameMod.None)
                saved = EGameMod.Training;

            _gameModeSelectionParameters.SetSelectedGameMode(saved);
        }

        private void OnConfirmButtonClick()
        {
            _audioService.PlayUiAudio(EAudioType.Ui, EAudioSubType.MenuButtonConfirm);
            
            switch (_gameModeSelectionParameters.GameMod)
            {
                case EGameMod.Training:
                    _localWindowsService.CloseToWindow<MainMenuWindow>();
                    _gameSessionService?.BeginGame();
                    break;
                case EGameMod.Story:
                    _localWindowsService.AnimateWindow<MainMenuWindow>(Vector2.right * LEFT_SLIDE_1200);
                    _localWindowsService.AnimateWindow<MapWindow>(Vector2.right * LEFT_SLIDE_1200);
                    _localWindowsService.AnimateWindow<GameModWindow>(Vector2.right * LEFT_SLIDE_600);

                    SetPresentationState(false);

                    _delayTween?.Kill();
                    _delayTween = DOVirtual
                        .DelayedCall(_delaySlideAnimationOnView, () => _localWindowsService.OpenWindow<OpponentWindow>())
                        .SetUpdate(true).SetLink(View.gameObject);
                    break;
                case EGameMod.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnBackButtonClick()
        {
            _audioService.PlayUiAudio(EAudioType.Ui, EAudioSubType.MenuButtonBack);
            
            _localWindowsService.AnimateWindow<MainMenuWindow>(Vector2.zero);
            _localWindowsService.AnimateWindow<MapWindow>(Vector2.zero);
            
            _localWindowsService.CloseWindow();
        }
    }
}