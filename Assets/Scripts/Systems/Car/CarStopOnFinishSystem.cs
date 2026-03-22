using System;
using Components;
using Scellecs.Morpeh;
using Services;
using UniRx;
using UnityEngine;
using Zenject;

namespace Systems.Car
{
    public sealed class CarStopOnFinishSystem : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private IRaceTimerService _raceTimerService;
        [Inject] private IEventService _eventService;

        private const float StopDurationSeconds = 1f;

        private Filter _cars;
        private Stash<RigidbodyComponent> _rigidbodyStash;

        private bool _isStopping;
        private float _stopStartTime;
        private Rigidbody _targetRigidbody;
        private Vector3 _startLinearVelocity;
        private Vector3 _startAngularVelocity;

        private IDisposable _raceFinishedSubscription;
        private IDisposable _startRaceSubscription;

        public void OnAwake()
        {
            _cars = World.Filter
                .With<RigidbodyComponent>()
                .With<PlayerTagComponent>()
                .Build();

            _rigidbodyStash = World.GetStash<RigidbodyComponent>();

            _raceFinishedSubscription = _raceTimerService.RaceFinishedStream.Subscribe(_ => BeginStop());
            _startRaceSubscription = _eventService.StartRaceStream.Subscribe(_ => ResetStopState());
        }

        public void OnUpdate(float deltaTime)
        {
            if (!_isStopping)
                return;

            var elapsed = Time.time - _stopStartTime;
            var time = Mathf.Clamp01(elapsed / StopDurationSeconds);

            if (_targetRigidbody == null)
                return;

            _targetRigidbody.velocity = Vector3.Lerp(_startLinearVelocity, Vector3.zero, time);
            _targetRigidbody.angularVelocity = Vector3.Lerp(_startAngularVelocity, Vector3.zero, time);

            if (time >= 1f)
            {
                _targetRigidbody.velocity = Vector3.zero;
                _targetRigidbody.angularVelocity = Vector3.zero;
                _targetRigidbody.Sleep();
                _isStopping = false;
            }
        }

        private void BeginStop()
        {
            _isStopping = true;
            _stopStartTime = Time.time;
            _targetRigidbody = null;

            foreach (var car in _cars)
            {
                var rigidbody = _rigidbodyStash.Get(car).Value;
                if (rigidbody == null)
                    continue;

                _targetRigidbody = rigidbody;
                _startLinearVelocity = rigidbody.velocity;
                _startAngularVelocity = rigidbody.angularVelocity;
                break;
            }
        }

        private void ResetStopState()
        {
            _isStopping = false;
            _targetRigidbody = null;
            _startLinearVelocity = Vector3.zero;
            _startAngularVelocity = Vector3.zero;
        }

        public void Dispose()
        {
            _raceFinishedSubscription?.Dispose();
            _startRaceSubscription?.Dispose();
        }
    }
}