using System;
using Configs.Impl;
using Data.Enums;
using KoboldUi.Element.Controller;
using UI.Views;

namespace UI.Controllers
{
    public class GameStoryController : AUiController<GameStoryView>
    {
        private readonly GameModeSelectionParameters _gameModeSelectionParameters;
        private IDisposable _totalTimeDisposable;

        public GameStoryController(GameModeSelectionParameters gameModeSelectionParameters)
        {
            _gameModeSelectionParameters = gameModeSelectionParameters;
        }

        public override void Initialize() { }

        protected override void OnOpen()
        {
            if (_gameModeSelectionParameters.GameMod == EGameMod.Training) 
                View.gameObject.SetActive(false);
        }
    }
}