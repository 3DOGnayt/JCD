using Components;
using Configs.Impl;
using Data.Enums;
using Data.Struct;
using Helpers.Car;
using Scellecs.Morpeh;
using Services;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.Splines;
using Zenject;

namespace Systems.Spawn
{
    public sealed class OpponentSpawnSystem : ISystem
    {
        [Inject] public World World { get; set; }
        [Inject] private DiContainer _container;

        private IEventService _eventService;
        private IGameSessionService _gameSessionService;
        private CarSelectionParameters _carSelectionParameters;
        private GameSelectionParameters _gameSelectionParameters;
        private GameModeSelectionParameters _gameModeSelectionParameters;
        private OpponentCatalogParameters _opponentCatalogParameters;
        private CarCatalogParameters _carCatalogParameters;

        private Transform _opponentGroup;
        private IDisposable _spawnDisposable;
        private const float SpawnProgressThreshold = 0.2f;
        private const float OpponentSideOffset = 5f;
        private bool _hasSpawnedThisLoad;

        private SplineContainer _splineContainer;

        [Inject]
        public void Construct(
            IEventService eventService,
            IGameSessionService gameSessionService,
            CarSelectionParameters carSelectionParameters,
            GameSelectionParameters gameSelectionParameters,
            GameModeSelectionParameters gameModeSelectionParameters,
            OpponentCatalogParameters opponentCatalogParameters,
            CarCatalogParameters carCatalogParameters)
        {
            _eventService = eventService;
            _gameSessionService = gameSessionService;
            _carSelectionParameters = carSelectionParameters;
            _gameSelectionParameters = gameSelectionParameters;
            _gameModeSelectionParameters = gameModeSelectionParameters;
            _opponentCatalogParameters = opponentCatalogParameters;
            _carCatalogParameters = carCatalogParameters;
        }

        public void OnAwake()
        {
            EnsureSpawnRoot();

            if (_eventService != null) 
                _spawnDisposable = _eventService.LoadingProgress.Subscribe(OnLoadingProgress);
        }

        private void OnLoadingProgress(float progress)
        {
            if (progress <= 0f)
                _hasSpawnedThisLoad = false;

            if (_gameSessionService != null && _gameSessionService.Target != EGameSessionTarget.Game)
                return;

            if (_gameModeSelectionParameters != null && _gameModeSelectionParameters.GameMod != EGameMod.Story)
                return;

            if (_hasSpawnedThisLoad || progress < SpawnProgressThreshold)
                return;

            _hasSpawnedThisLoad = true;
            OnStartRace();
        }

        private void EnsureSpawnRoot()
        {
            if (_opponentGroup != null)
                return;

            _opponentGroup = new GameObject("Opponents").transform;
            _gameSessionService?.RegisterRuntimeRoot(_opponentGroup.gameObject);
        }

        private void OnStartRace()
        {
            EnsureSpawnRoot();
            SpawnOpponent();
        }

        private void SpawnOpponent()
        {
            if (!TryGetOpponentCarEntry(out var opponentEntry, out var opponentCarEntry))
                return;

            var opponentPrefab = opponentCarEntry.Preset != null ? opponentCarEntry.Preset.Car : null;
            if (opponentPrefab == null)
                return;

            var spawnRotation = Quaternion.identity;
            var spawnPosition = Vector3.right * OpponentSideOffset;

            var instance = _container.InstantiatePrefabForComponent<ICarView>(
                opponentPrefab, spawnPosition, spawnRotation, _opponentGroup);

            var entity = World.CreateEntity();
            AddGameComponents(entity, instance, opponentCarEntry);
            AddInternalComponents(entity, instance);
            AddOpponentComponents(entity, opponentEntry);

            _gameSessionService?.RegisterRuntimeEntity(entity);
            _gameSessionService?.RegisterRuntimeRoot(instance.CarTransform.gameObject);
        }

        private void AddOpponentComponents(Entity entity, OpponentCatalogEntry opponentEntry)
        {
            var behavior = opponentEntry.Behavior;
            var targetSpeed = behavior.TargetSpeedKmh > 0f ? behavior.TargetSpeedKmh : 35f;
            var lookAhead = behavior.LookAheadMeters > 0f ? behavior.LookAheadMeters : 8f;
            var maxSteerAngle = behavior.MaxSteerAngleDeg > 0f ? behavior.MaxSteerAngleDeg : 45f;

            entity.SetComponent(new OpponentSplineFollowComponent
            {
                ProgressT = 0f,
                LookAheadMeters = lookAhead,
                TargetSpeedKmh = targetSpeed,
                MaxSteerAngleDeg = maxSteerAngle
            });
        }

        private void AddGameComponents(Entity entity, ICarView carView, CarCatalogEntry opponentCarEntry)
        {
            var movementParameters = opponentCarEntry.MovementParameters;
            var horizontal = movementParameters != null ? movementParameters.Horizontal : null;
            var vertical = movementParameters != null ? movementParameters.Vertical : null;

            var speedsPreset = opponentCarEntry.SpeedsPresetParameters;
            var speedMax = _carSelectionParameters != null ? _carSelectionParameters.GetSpeedMaxKmh(speedsPreset) : 0f;
            var backSpeedMax = _carSelectionParameters != null ? _carSelectionParameters.GetBackSpeedMaxKmh(speedsPreset) : 0f;
            var gearCount = _carSelectionParameters != null ? _carSelectionParameters.GetGearCount(speedsPreset) : 0;

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
            entity.SetComponent(new DriftMultiplierComponent { Value = 0f });

            var maxSpeedComponent = new SpeedMaxComponent { Value = speedMax };
            var maxRpmComponent = new EngineRpmMaxComponent { Value = engineRpmMax };

            entity.SetComponent(maxSpeedComponent);
            entity.SetComponent(new BackSpeedMaxComponent { Value = backSpeedMax });
            entity.SetComponent(new GearCountComponent { Value = gearCount });
            entity.SetComponent(maxRpmComponent);

            entity.SetComponent(new CarViewComponent { Value = carView });
            entity.SetComponent(new SkidmarksComponent { Value = false });
            entity.SetComponent(new SkidSmokeHandleComponent { Value = -1 });
            entity.SetComponent(new HeadlightsComponent { Value = EHeadlightsMode.Off });
        }

        private void AddInternalComponents(Entity entity, ICarView carView)
        {
            AddCommonComponents(entity, carView);
            AddMainCarComponents(entity, carView);
            AddWheelInfoComponents(entity, carView);
            AddFrontWheelComponents(entity, carView);
            AddBackWheelComponents(entity, carView);
            AddSkidAudioComponent(entity);
            AddEngineAudioComponent(entity);
        }

        private void AddSkidAudioComponent(Entity entity)
        {
            entity.SetComponent(new SkidAudioComponent { Source = null, CurrentVolume = 0f });
        }

        private void AddEngineAudioComponent(Entity entity)
        {
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
            var opponentTag = World.GetStash<OpponentTagComponent>();
            opponentTag.Set(entity, new OpponentTagComponent());

            entity.SetComponent(new TransformComponent { Value = carView.CarTransform });
            entity.SetComponent(new PositionComponent { Value = carView.CarTransform.position });
            entity.SetComponent(new RotationComponent { Value = carView.CarTransform.rotation });
            entity.SetComponent(new ScaleComponent { Value = 1 });
            entity.SetComponent(new RigidbodyComponent { Value = carView.CarRigidbody });

            entity.SetComponent(new VerticalInputComponent { Value = 0 });
            entity.SetComponent(new HorizontalInputComponent { Value = 0 });
            entity.SetComponent(new RaceLapStateComponent());
        }

        public void Dispose()
        {
            _spawnDisposable?.Dispose();
        }

        private bool TryGetOpponentCarEntry(out OpponentCatalogEntry opponentEntry, out CarCatalogEntry carEntry)
        {
            opponentEntry = default;
            carEntry = default;

            if (_opponentCatalogParameters == null || _carCatalogParameters == null)
                return false;

            var opponents = _opponentCatalogParameters.Opponents;
            if (opponents == null || opponents.Count == 0)
                return false;

            var opponentIndex = _gameSelectionParameters != null ? _gameSelectionParameters.SelectedOpponentIndex : -1;
            if (opponentIndex < 0 || opponentIndex >= opponents.Count)
                opponentIndex = 0;

            opponentEntry = opponents[opponentIndex];
            var cars = _carCatalogParameters.Cars;
            if (cars == null || cars.Count == 0)
                return false;

            var carIndex = opponentEntry.CarIndex;
            if (carIndex < 0 || carIndex >= cars.Count)
                carIndex = 0;

            carEntry = cars[carIndex];
            return carEntry.Preset != null && carEntry.Preset.Car != null;
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
    }
}