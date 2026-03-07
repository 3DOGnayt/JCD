using System;
using System.Collections.Generic;
using Scellecs.Morpeh;
using Systems.Car;
using UniRx;

namespace Services.Impl
{
    public class SystemService : ISystemService, IDisposable
    {
        private static readonly HashSet<Type> DelayedFixedSystemTypes = new()
        {
            typeof(GearShiftSystem),
            typeof(SlipSystem),
            typeof(RPMSystem),
            typeof(PhysicsSpeedSystem)
        };

        private readonly World _world;
        private readonly ISystem[] _systems;
        private readonly IFixedSystem[] _fixedSystems;

        private SystemsGroup _systemsGroup;
        private SystemsGroup _fixedSystemsGroup;
        private IDisposable _playerSpawnedSubscription;

        private bool _initialRegistered;
        private bool _delayedRegistered;

        public SystemService(
            World world,
            ISystem[] systems,
            IFixedSystem[] fixedSystems,
            IEventService eventService
        )
        {
            _world = world;
            _systems = systems;
            _fixedSystems = fixedSystems;

            if (eventService != null)
                _playerSpawnedSubscription = eventService.PlayerSpawnedStream.Subscribe(_ => RegisterDelayedSystems());
        }

        public void RegisterInitialSystems()
        {
            if (_initialRegistered)
                return;

            _systemsGroup = _world.CreateSystemsGroup();

            foreach (var system in _systems)
                _systemsGroup.AddSystem(system);

            _fixedSystemsGroup = _world.CreateSystemsGroup();
            foreach (var fixedSystem in _fixedSystems)
            {
                if (!IsDelayedFixedSystem(fixedSystem))
                    _fixedSystemsGroup.AddSystem(fixedSystem);
            }

            _world.AddSystemsGroup(order: 0, _systemsGroup);
            _world.AddSystemsGroup(order: 1, _fixedSystemsGroup);
            _initialRegistered = true;
        }

        public void RegisterDelayedSystems()
        {
            if (_delayedRegistered)
                return;

            var delayedFixedSystemsGroup = _world.CreateSystemsGroup();
            foreach (var fixedSystem in _fixedSystems)
            {
                if (IsDelayedFixedSystem(fixedSystem))
                    delayedFixedSystemsGroup.AddSystem(fixedSystem);
            }

            _world.AddSystemsGroup(order: 2, delayedFixedSystemsGroup);
            _delayedRegistered = true;
        }

        private static bool IsDelayedFixedSystem(IFixedSystem system)
        {
            return system != null && DelayedFixedSystemTypes.Contains(system.GetType());
        }

        public void Dispose()
        {
            _playerSpawnedSubscription?.Dispose();
        }
    }
}