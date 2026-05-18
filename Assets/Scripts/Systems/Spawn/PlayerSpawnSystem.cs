using Components;
using Configs.Impl;
using Data.Enums;
using Scellecs.Morpeh;
using Services;
using System;
using Helpers.Car;
using UniRx;
using UnityEngine;
using Zenject;

namespace Systems.Spawn
{
    public sealed class PlayerSpawnSystem : ISystem 
    {
        [Inject] public World World { get; set;}
        [Inject] private DiContainer _container;

        private IEventService _eventService;
        private IGameSessionService _gameSessionService;
        private CarSelectionParameters _carSelectionParameters;
        private GameSelectionParameters _gameSelectionParameters;
        private IUnitRaceTimerService _unitRaceTimerService;

        private Transform _playerGroup;
        private IDisposable _spawnDisposable;
        private const float SpawnProgressThreshold = 0.2f;
        private bool _hasSpawnedThisLoad;
        
        [Inject]
        public void Construct(
            IEventService eventService,
            IGameSessionService gameSessionService,
            CarSelectionParameters carSelectionParameters,
            GameSelectionParameters gameSelectionParameters,
            IUnitRaceTimerService unitRaceTimerService
        )
        {
            _eventService = eventService;
            _gameSessionService = gameSessionService;
            _carSelectionParameters = carSelectionParameters;
            _gameSelectionParameters = gameSelectionParameters;
            _unitRaceTimerService = unitRaceTimerService;
        }
        
        public void OnAwake()
        {
            SetSpawnRoot();
            
            if (_eventService != null) 
                _spawnDisposable = _eventService.LoadingProgress.Subscribe(OnLoadingProgress);
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
            var selectedPreset = _carSelectionParameters != null ? _carSelectionParameters.SelectedCar : null;
            var player = selectedPreset != null ? selectedPreset.Car : null;
            
            if (player == null)
                return;

            var spawnRotation = ResolvePlayerSpawnRotation();
            var instance = _container.InstantiatePrefabForComponent<ICarView>(player, Vector3.zero, spawnRotation, _playerGroup);
            
            var entity = World.CreateEntity();
            AddGameComponents(entity, instance);
            AddInternalComponents(entity, instance);
            
            _gameSessionService?.RegisterRuntimeEntity(entity);
            _eventService.PublishPlayerSpawned(instance);
            _unitRaceTimerService?.SetPlayerEntity(entity);
            
            _gameSessionService?.RegisterRuntimeRoot(instance.CarTransform.gameObject);
        }

        private Quaternion ResolvePlayerSpawnRotation()
        {
            return _gameSelectionParameters != null
                ? Quaternion.Euler(_gameSelectionParameters.SelectedUnitSpawnEulerAngles)
                : Quaternion.identity;
        }

        private void AddGameComponents(Entity entity, ICarView carView)
        {
            var movementParameters = _carSelectionParameters != null ? _carSelectionParameters.MovementParameters : null;
            var horizontal = movementParameters != null ? movementParameters.Horizontal : null;
            var vertical = movementParameters != null ? movementParameters.Vertical : null;
            
            var speedMax = _carSelectionParameters != null ? _carSelectionParameters.GetSpeedMaxKmh() : 0f;
            var backSpeedMax = _carSelectionParameters != null ? _carSelectionParameters.GetBackSpeedMaxKmh() : 0f;
            var gearCount = _carSelectionParameters != null ? _carSelectionParameters.GetGearCount() : 0;

            var steeringAngleMax = horizontal?.SteeringAngleMax ?? 0f;
            var steeringSpeed = horizontal?.SteeringSpeed ?? 0f;
            var engineRpmMax = vertical?.MaxRpm ?? 0f;
            
            entity.SetComponent(new SpeedComponent { Value = 0 });
            entity.SetComponent(new BackSpeedComponent { Value = 0 });
            entity.SetComponent(new GearComponent { Value = 0 });
            entity.SetComponent(new EngineRpmComponent { Value = 0 });
            entity.SetComponent(new SteeringAngleComponent { Value = steeringAngleMax });
            entity.SetComponent(new SteeringSpeedComponent { Value = steeringSpeed });
            entity.SetComponent(new BrakeInputComponent { Value = false });
            entity.SetComponent(new HandbrakeInputComponent { Value = false });
            entity.SetComponent(new ShiftUpInputComponent { Value = false });
            entity.SetComponent(new ShiftDownInputComponent { Value = false });
            entity.SetComponent(new ManualGearOverrideComponent { Timer = 0f });
            entity.SetComponent(new DriftMultiplierComponent { Value = 0f });
            entity.SetComponent(new DownshiftDriftComponent
            {
                Value = 0f,
                Timer = 0f,
                RearGripMultiplier = 1f,
                FrontGripMultiplier = 1f
            });
            entity.SetComponent(new ArcadeAssistSpeedComponent { Value = 0f });
            entity.SetComponent(new SplineProgressComponent());
            entity.SetComponent(new SplineDeltaComponent { Value = float.NaN });

            var maxSpeedComponent = new SpeedMaxComponent { Value = speedMax };
            var maxRpmComponent = new EngineRpmMaxComponent { Value = engineRpmMax };

            entity.SetComponent(maxSpeedComponent);
            entity.SetComponent(new BackSpeedMaxComponent { Value = backSpeedMax });
            entity.SetComponent(new GearCountComponent { Value = gearCount });
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
            AddSkidAudioComponent(entity, carView);
            AddEngineAudioComponent(entity, carView);
        }

        private void AddSkidAudioComponent(Entity entity, ICarView carView)
        {
            if (carView == null)
                return;

            entity.SetComponent(new SkidAudioComponent { Source = null, CurrentVolume = 0f });
        }

        private void AddEngineAudioComponent(Entity entity, ICarView carView)
        {
            if (carView == null)
                return;

            entity.SetComponent(new EngineAudioComponent
            {
                LowSource = null,
                MedSource = null,
                HighSource = null,
                LowVolume = 0f,
                MedVolume = 0f,
                HighVolume = 0f
            });
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
            entity.SetComponent(new RaceLapStateComponent());
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
