using System;
using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using Services;
using UniRx;
using Zenject;

namespace Systems.Car.Opponents.Rail
{
    public sealed partial class OpponentRailSystem : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private GameSelectionParameters _gameSelectionParameters;
        [Inject] private IUnitRaceTimerService _unitRaceTimerService;
        [Inject] private IEventService _eventService;

        private Filter _opponents;
        private Stash<TransformComponent> _transformStash;
        private Stash<RigidbodyComponent> _rigidbodyStash;
        private Stash<OpponentSplineFollowComponent> _followStash;
        private Stash<HorizontalInputComponent> _horizontalStash;
        private Stash<VerticalInputComponent> _verticalStash;
        private Stash<HandbrakeInputComponent> _handbrakeStash;
        private bool _raceStarted;
        private IDisposable _startRaceSubscription;
        private IDisposable _countdownFinishedSubscription;
        private IDisposable _gameStartedSubscription;

        public void OnAwake()
        {
            _opponents = World.Filter
                .With<OpponentTagComponent>()
                .With<TransformComponent>()
                .With<RigidbodyComponent>()
                .With<OpponentSplineFollowComponent>()
                .With<HorizontalInputComponent>()
                .With<VerticalInputComponent>()
                .With<HandbrakeInputComponent>()
                .With<SpeedComponent>()
                .Build();

            _transformStash = World.GetStash<TransformComponent>();
            _rigidbodyStash = World.GetStash<RigidbodyComponent>();
            _followStash = World.GetStash<OpponentSplineFollowComponent>();
            _horizontalStash = World.GetStash<HorizontalInputComponent>();
            _verticalStash = World.GetStash<VerticalInputComponent>();
            _handbrakeStash = World.GetStash<HandbrakeInputComponent>();

            _raceStarted = false;
            if (_eventService != null)
            {
                _gameStartedSubscription = _eventService.IsGameStarted.Subscribe(OnGameStartedChanged);
                _startRaceSubscription = _eventService.StartRaceStream.Subscribe(_ => _raceStarted = false);
                _countdownFinishedSubscription = _eventService.CountdownFinishedStream.Subscribe(_ => _raceStarted = true);
            }
        }

        public void Dispose()
        {
            _startRaceSubscription?.Dispose();
            _countdownFinishedSubscription?.Dispose();
            _gameStartedSubscription?.Dispose();
        }

        private void OnGameStartedChanged(bool isStarted)
        {
            if (!isStarted)
                _raceStarted = false;
        }
    }
}