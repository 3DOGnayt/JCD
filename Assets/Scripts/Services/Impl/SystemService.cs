using System;
using System.Collections.Generic;
using Scellecs.Morpeh;
using Systems.Car;
using Systems.MiniMapCamera;
using Systems.Spawn;
using UniRx;

namespace Services.Impl
{
    public class SystemService : ISystemService, IDisposable
    {
        private static readonly HashSet<Type> DelayedUpdateSystemTypes = new()
        {
            typeof(MinimapSpawnSystem),
            typeof(MinimapFollowSystem)
        };
        
        private static readonly HashSet<Type> DelayedFixedSystemTypes = new()
        {
            typeof(SkidAudioSystem),
            typeof(EngineAudioSystem),
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
        private IDisposable _isLoadingCompletedSubscription;

        private bool _initialRegistered;
        private bool _delayedRegistered;
        private bool _delayedUpdateRegistered;

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

            if (eventService == null)
                return;
            
            _playerSpawnedSubscription = eventService.PlayerSpawnedStream.Subscribe(_ => RegisterDelayedFixedSystems());
            _isLoadingCompletedSubscription = eventService.IsLoadingCompleted
                .Where(isCompleted => isCompleted)
                .Subscribe(_ => RegisterDelayedUpdateSystems());
        }

        public void RegisterInitialSystems()
        {
            if (_initialRegistered)
                return;

            _systemsGroup = _world.CreateSystemsGroup();

            foreach (var system in _systems)
            {
                if (!IsDelayedUpdateSystem(system)) 
                    _systemsGroup.AddSystem(system);
            }

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

        private void RegisterDelayedUpdateSystems()
        {
            if (_delayedUpdateRegistered)
                return;

            var delayedUpdateSystemsGroup = _world.CreateSystemsGroup();
            foreach (var updateSystem in _systems)
            {
                if (IsDelayedUpdateSystem(updateSystem))
                    delayedUpdateSystemsGroup.AddSystem(updateSystem);
            }

            _world.AddSystemsGroup(order: 2, delayedUpdateSystemsGroup);
            _delayedUpdateRegistered = true;
        }

        private void RegisterDelayedFixedSystems()
        {
            if (_delayedRegistered)
                return;

            var delayedFixedSystemsGroup = _world.CreateSystemsGroup();
            foreach (var fixedSystem in _fixedSystems)
            {
                if (IsDelayedFixedSystem(fixedSystem))
                    delayedFixedSystemsGroup.AddSystem(fixedSystem);
            }

            _world.AddSystemsGroup(order: 3, delayedFixedSystemsGroup);
            _delayedRegistered = true;
        }

        private static bool IsDelayedFixedSystem(IFixedSystem system)
        {
            return system != null && DelayedFixedSystemTypes.Contains(system.GetType());
        }
        
        private static bool IsDelayedUpdateSystem(ISystem system)
        {
            return system != null && DelayedUpdateSystemTypes.Contains(system.GetType());
        }

        public void Dispose()
        {
            _playerSpawnedSubscription?.Dispose();
            _isLoadingCompletedSubscription?.Dispose();
        }
    }
}