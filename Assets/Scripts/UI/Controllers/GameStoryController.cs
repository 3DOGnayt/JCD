using System;
using Components;
using Configs.Impl;
using Data.Enums;
using KoboldUi.Element.Controller;
using UI.Views;
using UniRx;
using UnityEngine;
using Scellecs.Morpeh;

namespace UI.Controllers
{
    public class GameStoryController : AUiController<GameStoryView>
    {
        private readonly GameModeSelectionParameters _gameModeSelectionParameters;
        private readonly World _world;

        private Filter _players;
        private Stash<SplineDeltaComponent> _deltaStash;
        
        private float _lastDisplayedDelta;
        private bool _hasDisplayedDelta;

        public GameStoryController(
            GameModeSelectionParameters gameModeSelectionParameters,
            World world)
        {
            _gameModeSelectionParameters = gameModeSelectionParameters;
            _world = world;
        }

        public override void Initialize()
        {
            if (_world == null)
                return;

            _players = _world.Filter
                .With<PlayerTagComponent>()
                .With<SplineDeltaComponent>()
                .Build();

            _deltaStash = _world.GetStash<SplineDeltaComponent>();
            Observable.EveryUpdate().Subscribe(_ => UpdateAverageText()).AddTo(View);
        }

        protected override void OnOpen()
        {
            if (_gameModeSelectionParameters.GameMod == EGameMod.Training) 
                View.gameObject.SetActive(false);
        }

        private void UpdateAverageText()
        {
            if (_players == null || _players.IsEmpty())
            {
                SetDefaultText();
                return;
            }

            var player = _players.First();
            var delta = _deltaStash.Get(player).Value;

            if (float.IsNaN(delta) || float.IsInfinity(delta))
            {
                SetDefaultText();
                return;
            }

            if (_hasDisplayedDelta && Mathf.Abs(delta - _lastDisplayedDelta) < 0.1f)
                return;

            _hasDisplayedDelta = true;
            _lastDisplayedDelta = delta;

            var sign = delta >= 0f ? "+" : "-";
            var value = Mathf.Abs(delta);
            SetAverageText($"{sign}{value:0.0}m");
        }

        private void SetAverageText(string value)
        {
            if (View.AverageText == null)
                return;

            View.AverageText.text = value;
        }

        private void SetDefaultText()
        {
            if (!_hasDisplayedDelta)
                return;

            _hasDisplayedDelta = false;
            SetAverageText("--");
        }
    }
}