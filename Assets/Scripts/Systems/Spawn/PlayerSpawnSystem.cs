using Components;
using Configs.Impl;
using Data.Enums;
using Scellecs.Morpeh;
using Services;
using System;
using Helpers.CarView;
using UniRx;
using UnityEngine;
using Zenject;

namespace Systems.Spawn
{
    public sealed class PlayerSpawnSystem : ISystem 
    {
        [Inject] public World World { get; set;}
        [Inject] private DiContainer _container;

        private ILoadingService _loadingService;
        private IGameSessionService _gameSessionService;
        private CarSelectionParameters _carSelectionParameters;

        private Transform _playerGroup;
        private IDisposable _spawnDisposable;
        private const float SpawnProgressThreshold = 0.2f;
        private bool _hasSpawnedThisLoad;
        
        [Inject]
        public void Construct(
            ILoadingService loadingService,
            IGameSessionService gameSessionService,
            CarSelectionParameters carSelectionParameters
        )
        {
            _loadingService = loadingService;
            _gameSessionService = gameSessionService;
            _carSelectionParameters = carSelectionParameters;
        }
        
        public void OnAwake()
        {
            SetSpawnRoot();
            
            if (_loadingService != null)
            {
                _spawnDisposable = _loadingService.LoadingProgress.Subscribe(OnLoadingProgress);
            }
        }

        private void OnLoadingProgress(float progress)
        {
            if (progress <= 0f)
                _hasSpawnedThisLoad = false;

            if (_gameSessionService != null && _gameSessionService.Target != EGameSessionTarget.Game)
                return;

            if (_hasSpawnedThisLoad || progress < SpawnProgressThreshold)
                return;

            _hasSpawnedThisLoad = true;
            OnStartRace();
        }

        private void SetSpawnRoot() => _playerGroup = new GameObject("Player").transform;

        private void OnStartRace()
        {
            SpawnPlayer();
        }

        private void SpawnPlayer()
        {
            var selectedPreset = _carSelectionParameters != null
                ? _carSelectionParameters.SelectedCar
                : null;

            var player = selectedPreset != null ? selectedPreset.Car : null;
            if (player == null)
                return;

            var instance = _container.InstantiatePrefabForComponent<ICarView>(player, Vector3.zero, Quaternion.identity, _playerGroup);
            
            var entity = World.CreateEntity();
            AddGameComponents(entity, instance);
            AddInternalComponents(entity, instance);
            
            _gameSessionService?.RegisterRuntimeEntity(entity);
            _loadingService.PublishPlayerSpawned(instance);
            
            _gameSessionService?.RegisterRuntimeRoot(instance.CarTransform.gameObject);
        }

        private void AddGameComponents(Entity entity, ICarView carView)
        {
            var carSetup = carView.CarPresetParameters.CarSetup;
            
            entity.SetComponent(new SpeedComponent { Value = 0 });
            entity.SetComponent(new BackSpeedComponent { Value = 0 });
            entity.SetComponent(new GearComponent { Value = 0 });
            entity.SetComponent(new EngineRpmComponent { Value = 0 });
            entity.SetComponent(new SteeringAngleComponent { Value = carSetup.SteeringAngleMax });
            entity.SetComponent(new SteeringSpeedComponent { Value = carSetup.SteeringSpeed });
            entity.SetComponent(new BrakeInputComponent { Value = false });
            entity.SetComponent(new HandbrakeInputComponent { Value = false });
            entity.SetComponent(new DriftMultiplierComponent { Value = carSetup.DriftMultiplier });

            var maxSpeedComponent = new SpeedMaxComponent { Value = carSetup.SpeedMax };
            var maxRpmComponent = new EngineRpmMaxComponent { Value = carSetup.EngineRpmMax };

            entity.SetComponent(maxSpeedComponent);
            entity.SetComponent(new BackSpeedMaxComponent { Value = carSetup.BackSpeedMax });
            entity.SetComponent(new GearCountComponent { Value = carSetup.GearCount });
            entity.SetComponent(maxRpmComponent);
            
            entity.SetComponent(new CarViewComponent{ Value = carView });
            entity.SetComponent(new SkidmarksComponent{ Value = false });
            entity.SetComponent(new SkidSmokeHandleComponent{ Value = -1 });
            entity.SetComponent(new HeadlightsComponent { Value = EHeadlightsMode.Off });
            
        }

        private void AddInternalComponents(Entity entity, ICarView carView)
        {
            AddCommonComponents(entity, carView);
            AddMainCarComponents(entity, carView);
            AddWheelInfoComponents(entity, carView);
            AddFrontWheelComponents(entity, carView);
            AddBackWheelComponents(entity, carView);
        }

        private void AddCommonComponents(Entity entity, ICarView carView)
        {
            //TODO: example
            var playerTag = World.GetStash<PlayerTagComponent>();
            playerTag.Set(entity, new PlayerTagComponent());
            //entity.SetComponent(new PlayerTagComponent());
            
            entity.SetComponent(new TransformComponent { Value = carView.CarTransform });
            entity.SetComponent(new PositionComponent { Value = carView.CarTransform.position });
            entity.SetComponent(new RotationComponent { Value = carView.CarTransform.rotation });
            entity.SetComponent(new ScaleComponent { Value = 1 });
            entity.SetComponent(new RigidbodyComponent { Value = carView.CarRigidbody });
            
            entity.SetComponent(new VerticalInputComponent {Value = 0 });
            entity.SetComponent(new HorizontalInputComponent {Value = 0 });
        }

        private void AddMainCarComponents(Entity entity, ICarView carView)
        {
            var carParameters = carView.CarPresetParameters.CarMassSetup;

            entity.SetComponent(new CarMassComponent { Value = carParameters.Mass });
            entity.SetComponent(new AutomaticCenterOfMassComponent { Value = carParameters.AutomaticCenterOfMass });
            entity.SetComponent(new CenterOfMassComponent { Value = carParameters.CenterOfMass });
        }

        private void AddWheelInfoComponents(Entity entity, ICarView instance)
        {
            var wheelInfos = instance.CarWheelInfos;
            
            entity.SetComponent(new WheelInfoComponent { WheelInfo = wheelInfos });
        }

        private void AddFrontWheelComponents(Entity entity, ICarView carView)
        {
            var frontMainWheelParameters = carView.CarPresetParameters.FrontWheelSetup.mainWheelSetup;
            var frontSuspensionSpring = carView.CarPresetParameters.FrontWheelSetup.SuspensionSpring;
            var frontForwardFriction = carView.CarPresetParameters.FrontWheelSetup.ForwardFriction;
            var frontSidewaysFriction = carView.CarPresetParameters.FrontWheelSetup.SidewaysFriction;

            entity.SetComponent(new FrontWheelMassComponent { Value = frontMainWheelParameters.Mass });
            entity.SetComponent(new FrontWheelRadiusComponent { Value = frontMainWheelParameters.Radius });
            entity.SetComponent(new FrontDampingRateComponent { Value = frontMainWheelParameters.DampingRate });
            entity.SetComponent(new FrontSuspensionDistanceComponent { Value = frontMainWheelParameters.SuspensionDistance });
            entity.SetComponent(new FrontForceAppPointDistanceComponent { Value = frontMainWheelParameters.ForceAppPointDistance });
            entity.SetComponent(new FrontWheelCenterComponent { Value = frontMainWheelParameters.Center });

            entity.SetComponent(new FrontSpringComponent { Value = frontSuspensionSpring.Spring });
            entity.SetComponent(new FrontDamperComponent { Value = frontSuspensionSpring.Damper });
            entity.SetComponent(new FrontTargetPositionComponent { Value = frontSuspensionSpring.TargetPosition });

            entity.SetComponent(new FrontExtremumSlipForwardComponent { Value = frontForwardFriction.ExtremumSlip });
            entity.SetComponent(new FrontExtremumValueForwardComponent { Value = frontForwardFriction.ExtremumValue });
            entity.SetComponent(new FrontAsymptoteSlipForwardComponent { Value = frontForwardFriction.AsymptoteSlip });
            entity.SetComponent(new FrontAsymptoteValueForwardComponent { Value = frontForwardFriction.AsymptoteValue });
            entity.SetComponent(new FrontStiffnessForwardComponent { Value = frontForwardFriction.Stiffness });

            entity.SetComponent(new FrontExtremumSlipSidewaysComponent { Value = frontSidewaysFriction.ExtremumSlip });
            entity.SetComponent(new FrontExtremumValueSidewaysComponent { Value = frontSidewaysFriction.ExtremumValue });
            entity.SetComponent(new FrontAsymptoteSlipSidewaysComponent { Value = frontSidewaysFriction.AsymptoteSlip });
            entity.SetComponent(new FrontAsymptoteValueSidewaysComponent { Value = frontSidewaysFriction.AsymptoteValue });
            entity.SetComponent(new FrontStiffnessSidewaysComponent { Value = frontSidewaysFriction.Stiffness });
        }

        private void AddBackWheelComponents(Entity entity, ICarView carView)
        {
            var backMainWheelParameters = carView.CarPresetParameters.BackWheelSetup.mainWheelSetup;
            var backSuspensionSpring = carView.CarPresetParameters.BackWheelSetup.SuspensionSpring;
            var backForwardFriction = carView.CarPresetParameters.BackWheelSetup.ForwardFriction;
            var backSidewaysFriction = carView.CarPresetParameters.BackWheelSetup.SidewaysFriction;

            entity.SetComponent(new BackWheelMassComponent { Value = backMainWheelParameters.Mass });
            entity.SetComponent(new BackWheelRadiusComponent { Value = backMainWheelParameters.Radius });
            entity.SetComponent(new BackDampingRateComponent { Value = backMainWheelParameters.DampingRate });
            entity.SetComponent(new BackSuspensionDistanceComponent { Value = backMainWheelParameters.SuspensionDistance });
            entity.SetComponent(new BackForceAppPointDistanceComponent { Value = backMainWheelParameters.ForceAppPointDistance });
            entity.SetComponent(new BackWheelCenterComponent { Value = backMainWheelParameters.Center });

            entity.SetComponent(new BackSpringComponent { Value = backSuspensionSpring.Spring });
            entity.SetComponent(new BackDamperComponent { Value = backSuspensionSpring.Damper });
            entity.SetComponent(new BackTargetPositionComponent { Value = backSuspensionSpring.TargetPosition });

            entity.SetComponent(new BackExtremumSlipForwardComponent { Value = backForwardFriction.ExtremumSlip });
            entity.SetComponent(new BackExtremumValueForwardComponent { Value = backForwardFriction.ExtremumValue });
            entity.SetComponent(new BackAsymptoteSlipForwardComponent { Value = backForwardFriction.AsymptoteSlip });
            entity.SetComponent(new BackAsymptoteValueForwardComponent { Value = backForwardFriction.AsymptoteValue });
            entity.SetComponent(new BackStiffnessForwardComponent { Value = backForwardFriction.Stiffness });

            entity.SetComponent(new BackExtremumSlipSidewaysComponent { Value = backSidewaysFriction.ExtremumSlip });
            entity.SetComponent(new BackExtremumValueSidewaysComponent { Value = backSidewaysFriction.ExtremumValue });
            entity.SetComponent(new BackAsymptoteSlipSidewaysComponent { Value = backSidewaysFriction.AsymptoteSlip });
            entity.SetComponent(new BackAsymptoteValueSidewaysComponent { Value = backSidewaysFriction.AsymptoteValue });
            entity.SetComponent(new BackStiffnessSidewaysComponent { Value = backSidewaysFriction.Stiffness });
        }

        public void OnUpdate(float deltaTime) { }
        public void Dispose()
        {
            _spawnDisposable?.Dispose();
        }
    }
}
