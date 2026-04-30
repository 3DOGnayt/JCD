using System;
using Components;
using Scellecs.Morpeh;
using Services;
using UniRx;
using Zenject;

namespace Systems.Race
{
    public sealed class CarStopOnFinishSystem : StopOnFinishBaseSystem
    {
        [Inject] private IUnitRaceTimerService _unitRaceTimerService;
        [Inject] private IEventService _eventService;

        private Filter _playerCars;
        private Filter _opponentCars;
        private Stash<RigidbodyComponent> _rigidbodyStash;
        private Stash<PlayerTagComponent> _playerTagStash;
        private Stash<OpponentTagComponent> _opponentTagStash;

        private IDisposable _startRaceSubscription;
        private IDisposable _opponentFinishedSubscription;

        private StopState _playerStopState;
        private StopState _opponentStopState;

        public override void OnAwake()
        {
            _playerCars = World.Filter
                .With<RigidbodyComponent>()
                .With<PlayerTagComponent>()
                .Build();

            _opponentCars = World.Filter
                .With<RigidbodyComponent>()
                .With<OpponentTagComponent>()
                .Build();

            _rigidbodyStash = World.GetStash<RigidbodyComponent>();
            _playerTagStash = World.GetStash<PlayerTagComponent>();
            _opponentTagStash = World.GetStash<OpponentTagComponent>();

            _opponentFinishedSubscription = _unitRaceTimerService.RaceFinishedEntityStream.Subscribe(OnUnitFinished);
            _startRaceSubscription = _eventService.StartRaceStream.Subscribe(_ => ResetStopStates());
        }

        public override void OnUpdate(float deltaTime)
        {
            UpdateStop(ref _playerStopState);
            UpdateStop(ref _opponentStopState);
        }

        private void BeginPlayerStop()
        {
            _playerStopState.TargetRigidbody = null;

            foreach (var car in _playerCars)
            {
                var rigidbody = _rigidbodyStash.Get(car).Value;
                if (rigidbody == null)
                    continue;

                BeginStop(ref _playerStopState, rigidbody);
                break;
            }
        }

        private void BeginOpponentStop()
        {
            _opponentStopState.TargetRigidbody = null;

            foreach (var car in _opponentCars)
            {
                var rigidbody = _rigidbodyStash.Get(car).Value;
                if (rigidbody == null)
                    continue;

                BeginStop(ref _opponentStopState, rigidbody);
                break;
            }
        }

        private void OnUnitFinished(Entity entity)
        {
            if (_playerTagStash.Has(entity))
            {
                BeginPlayerStop();
                return;
            }

            if (_opponentTagStash.Has(entity))
                BeginOpponentStop();
        }

        private void ResetStopStates()
        {
            ResetStopState(ref _playerStopState);
            ResetStopState(ref _opponentStopState);
        }

        public override void Dispose()
        {
            _opponentFinishedSubscription?.Dispose();
            _startRaceSubscription?.Dispose();
        }
    }
}
