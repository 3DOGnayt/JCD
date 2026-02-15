using System;
using Configs.Impl;
using Data.Enums;
using KoboldUi.Element.Controller;
using UI.Views;

namespace UI.Controllers
{
    public class GameStoryController : AUiController<GameStoryView>
    {
        private readonly GameSelectionParameters _gameSelectionParameters;
        private IDisposable _totalTimeDisposable;

        public GameStoryController(GameSelectionParameters gameSelectionParameters)
        {
            _gameSelectionParameters = gameSelectionParameters;
        }

        public override void Initialize() { }

        protected override void OnOpen()
        {
            if (_gameSelectionParameters.GameMod == EGameMod.Training) 
                View.gameObject.SetActive(false);
        }
    }
}