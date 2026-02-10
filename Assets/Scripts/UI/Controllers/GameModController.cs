using System;
using Configs.Impl;
using DG.Tweening;
using Data.Enums;
using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
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
        private readonly ILocalWindowsService _localWindowsService;
        private Tween _presentationTween;
        
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

        protected override void OnOpen()
        {
            _presentationTween?.Kill();

            _presentationTween = DOVirtual.DelayedCall(View.PresentationDelay, () => SetPresentationState(true))
                .SetUpdate(true).SetLink(View.gameObject);
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
        
        private void SetPresentationState(bool isActive)
        {
            View.ConfirmButton.gameObject.SetActive(isActive);
            View.BackButton.gameObject.SetActive(isActive);
            View.GameModPresentation.gameObject.SetActive(isActive);
        }

        private void OnConfirmButtonClick()
        {
            switch (_gameSelectionParameters.GameMod)
            {
                case EGameMod.Training:
                    _localWindowsService.CloseToWindow<MainMenuWindow>();
                    _localWindowsService.OpenWindow<LoadingWindow>();
                    break;
                case EGameMod.Story:
                    _localWindowsService.AnimateWindow<MainMenuWindow>(Vector2.right * -1200);
                    _localWindowsService.AnimateWindow<MapWindow>(Vector2.right * -1200);
                    _localWindowsService.AnimateWindow<GameModWindow>(Vector2.right * -600);

                    SetPresentationState(false);

                    _localWindowsService.OpenWindow<OpponentWindow>();
                    break;
                case EGameMod.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnBackButtonClick()
        {
            _localWindowsService.AnimateWindow<MainMenuWindow>(Vector2.zero);
            _localWindowsService.AnimateWindow<MapWindow>(Vector2.zero);
            
            _localWindowsService.CloseWindow();
        }
    }
}