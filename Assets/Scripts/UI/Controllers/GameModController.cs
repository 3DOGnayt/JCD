using System;
using Configs.Impl;
using Data.Enums;
using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using UI.Views;
using UI.Window;
using UniRx;
using Zenject;

namespace UI.Controllers
{
    public class GameModController : AUiController<GameModView>
    {
        private readonly ILocalWindowsService _localWindowsService;
        
        [Inject] private GameSelectionParameters _gameSelectionParameters;

        public GameModController(ILocalWindowsService localWindowsService)
        {
            _localWindowsService = localWindowsService;
        }

        public override void Initialize()
        {
            if (View.TrainingButton.interactable && View.StoryButton.interactable) 
                OnTrainingButtonClick();

            View.TrainingButton.OnClickAsObservable().Subscribe(_ => OnTrainingButtonClick()).AddTo(View);
            View.StoryButton.OnClickAsObservable().Subscribe(_ => OnStoryButtonClick()).AddTo(View);
            View.ConfirmButton.OnClickAsObservable().Subscribe(_ => OnConfirmButtonClick()).AddTo(View);
            View.BackButton.OnClickAsObservable().Subscribe(_ => OnBackButtonClick()).AddTo(View);
        }

        private void OnTrainingButtonClick()
        {
            SetInteractableButtons(false);
            _gameSelectionParameters.SetSelectedGameMode(EGameMod.Training);
        }

        private void OnStoryButtonClick()
        {
            SetInteractableButtons(true);
            _gameSelectionParameters.SetSelectedGameMode(EGameMod.Story);
        }

        private void SetInteractableButtons(bool isActive)
        {
            View.TrainingButton.interactable = isActive;
            View.StoryButton.interactable = !isActive;
        }

        private void OnConfirmButtonClick()
        {
            switch (_gameSelectionParameters.GameMod)
            {
                case EGameMod.Training:
                    _localWindowsService.OpenWindow<LoadingWindow>();
                    break;
                case EGameMod.Story:
                    _localWindowsService.OpenWindow<OpponentWindow>();
                    break;
                case EGameMod.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnBackButtonClick() => _localWindowsService.CloseWindow();
    }
}