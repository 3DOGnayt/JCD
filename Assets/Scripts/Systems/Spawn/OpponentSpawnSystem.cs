using Components;
using Configs.Impl;
using Data.Enums;
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

        private Transform _opponentGroup;
        private IDisposable _spawnDisposable;
        private const float SpawnProgressThreshold = 0.2f;
        private bool _hasSpawnedThisLoad;

        private SplineContainer _splineContainer;

        [Inject]
        public void Construct(
            IEventService eventService,
            IGameSessionService gameSessionService,
            CarSelectionParameters carSelectionParameters,
            GameSelectionParameters gameSelectionParameters,
            GameModeSelectionParameters gameModeSelectionParameters)
        {
            _eventService = eventService;
            _gameSessionService = gameSessionService;
            _carSelectionParameters = carSelectionParameters;
            _gameSelectionParameters = gameSelectionParameters;
            _gameModeSelectionParameters = gameModeSelectionParameters;
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
            var selectedPreset = _carSelectionParameters != null ? _carSelectionParameters.SelectedCar : null;
            var opponentPrefab = selectedPreset != null ? selectedPreset.Car : null;
            if (opponentPrefab == null)
                return;

            var spawnPosition = Vector3.zero;
            var spawnRotation = Quaternion.identity;

            if (TryResolveSpline())
            {
                var pos = _splineContainer.EvaluatePosition(0f);
                var tangent = _splineContainer.EvaluateTangent(0f);

                spawnPosition = (Vector3)pos;

                var forward = (Vector3)tangent;
                if (forward.sqrMagnitude > 0.001f)
                    spawnRotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
            }

            var instance = _container.InstantiatePrefabForComponent<ICarView>(
                opponentPrefab, spawnPosition, spawnRotation, _opponentGroup);

            var entity = World.CreateEntity();
            AddGameComponents(entity, instance);
            AddInternalComponents(entity, instance);
            AddOpponentComponents(entity);

            _gameSessionService?.RegisterRuntimeEntity(entity);
            _gameSessionService?.RegisterRuntimeRoot(instance.CarTransform.gameObject);
        }

        private void AddOpponentComponents(Entity entity)
        {
            entity.SetComponent(new OpponentSplineFollowComponent
            {
                ProgressT = 0f,
                LookAheadMeters = 8f,
                TargetSpeedKmh = 35f,
                MaxSteerAngleDeg = 45f
            });
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

        private bool TryResolveSpline()
        {
            if (_splineContainer != null)
                return true;

            var runtimeSpline = _gameSelectionParameters != null ? _gameSelectionParameters.RuntimeSpline : null;
            if (runtimeSpline == null)
                return false;

            _splineContainer = runtimeSpline;
            return true;
        }

        public void OnUpdate(float deltaTime) { }

        public void Dispose()
        {
            _spawnDisposable?.Dispose();
        }
    }
}