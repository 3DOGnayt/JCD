using System;
using Configs.Impl;
using Data.Enums;
using KoboldUi.Element.Controller;
using Services;
using UI.Views;

namespace UI.Controllers
{
    public class GameStoryController : AUiController<GameStoryView>
    {
        private readonly IRaceTimerService _raceTimerService;
        private readonly GameSelectionParameters _gameSelectionParameters;
        private IDisposable _totalTimeDisposable;

        public GameStoryController(
            IRaceTimerService raceTimerService,
            GameSelectionParameters gameSelectionParameters)
        {
            _raceTimerService = raceTimerService;
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