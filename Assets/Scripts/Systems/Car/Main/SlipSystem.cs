using System.Collections.Generic;
using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems.Car.Main
{
    public class SlipSystem : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarSelectionParameters _carSelectionParameters;

        private Filter _cars;
        private AspectFactory<CarSetupAspect> _carAspectFactory;

        private Stash<WheelInfoComponent> _wheelInfoStash;
        private Stash<BackStiffnessSidewaysComponent> _backSidewaysStiffnessStash;
        private Stash<FrontStiffnessSidewaysComponent> _frontSidewaysStiffnessStash;
        private Stash<SkidmarksComponent> _skidmarksStash;
        private Stash<RigidbodyComponent> _rbStash;
        private Stash<TransformComponent> _transformStash;
        private Stash<VerticalInputComponent> _verticalInputStash;
        private Stash<HorizontalInputComponent> _horizontalInputStash;
        private Stash<DownshiftDriftComponent> _downshiftDriftStash;

        private Dictionary<Entity, float> _frontDriftValues;

        public void OnAwake()
        {
            _cars = World.Filter
                .Extend<CarSetupAspect>()
                .With<WheelInfoComponent>()
                .With<BackStiffnessSidewaysComponent>()
                .With<FrontStiffnessSidewaysComponent>()
                .With<SkidmarksComponent>()
                .With<RigidbodyComponent>()
                .With<TransformComponent>()
                .With<VerticalInputComponent>()
                .With<HorizontalInputComponent>()
                .With<DownshiftDriftComponent>()
                .Build();

            _carAspectFactory = World.GetAspectFactory<CarSetupAspect>();
            _wheelInfoStash = World.GetStash<WheelInfoComponent>();
            _backSidewaysStiffnessStash = World.GetStash<BackStiffnessSidewaysComponent>();
            _frontSidewaysStiffnessStash = World.GetStash<FrontStiffnessSidewaysComponent>();
            _skidmarksStash = World.GetStash<SkidmarksComponent>();
            _rbStash = World.GetStash<RigidbodyComponent>();
            _transformStash = World.GetStash<TransformComponent>();
            _verticalInputStash = World.GetStash<VerticalInputComponent>();
            _horizontalInputStash = World.GetStash<HorizontalInputComponent>();
            _downshiftDriftStash = World.GetStash<DownshiftDriftComponent>();

            _frontDriftValues = new Dictionary<Entity, float>();
        }

        // TODO: Refactoring
        public void OnUpdate(float deltaTime)
        {
            var slipParameters = _carSelectionParameters != null ? _carSelectionParameters.SlipParameters : null;
            var movementParameters = _carSelectionParameters != null ? _carSelectionParameters.MovementParameters : null;
            
            if (slipParameters == null || movementParameters == null)
                return;
            
            if (slipParameters == null)
                return;

            var backMultiplierTarget = slipParameters.HandbrakeSidewaysBackMultiplier;
            var backEnterSpeed = slipParameters.BackStiffnessEnterSpeed;
            var backReturnSpeed = slipParameters.BackStiffnessReturnSpeed;

            var frontMultiplierTarget = slipParameters.HandbrakeSidewaysForwardMultiplier;
            var frontEnterSpeed = slipParameters.ForwardStiffnessEnterSpeed;
            var frontReturnSpeed = slipParameters.ForwardStiffnessReturnSpeed;
            var touge = movementParameters.Touge;
            var useTouge = touge != null && touge.UseTougeHybridControl;

            foreach (var car in _cars)
            {
                var aspect = _carAspectFactory.Get(car);

                ref var backDrift = ref aspect.DriftMultiplier.Value;
                ref var handbrakePressed = ref aspect.HandbrakeInput.Value;
                ref var speedValue = ref aspect.Speed.Value;
                ref var backSpeedValue = ref aspect.BackSpeed.Value;
                ref var brakeInput = ref aspect.BrakeInput.Value;
                ref var skidFlag = ref _skidmarksStash.Get(car).Value;
                ref var downshiftDrift = ref _downshiftDriftStash.Get(car);
                var currentGear = aspect.Gear.Value;

                if (!_frontDriftValues.TryGetValue(car, out var frontDrift))
                    frontDrift = 0f;

                var wheelInfoComponent = _wheelInfoStash.Get(car);
                var backBaseSidewaysStiffness = _backSidewaysStiffnessStash.Get(car).Value;
                var frontBaseSidewaysStiffness = _frontSidewaysStiffnessStash.Get(car).Value;

                var rigidbodyComponent = _rbStash.Get(car);
                var transformComponent = _transformStash.Get(car);
                var rigidbody = rigidbodyComponent.Value;
                var transform = transformComponent.Value;

                if (rigidbody == null || transform == null)
                    continue;

                var vel = rigidbody.velocity;
                var flatVel = new Vector3(vel.x, 0f, vel.z);
                var speedTotalKmh = flatVel.magnitude * 3.6f;

                var forwardSpeedKmh = Mathf.Max(0f, speedValue);
                var backwardSpeedKmh = Mathf.Max(0f, Mathf.Abs(backSpeedValue));
                var scalarSpeedKmh = Mathf.Max(forwardSpeedKmh, backwardSpeedKmh);

                var helpersSetup = movementParameters.HelpersSetup;
                var canDriftNow = handbrakePressed && scalarSpeedKmh > helpersSetup.MinDriftSpeedKmh;
                var verticalInput = _verticalInputStash.Get(car).Value;
                var horizontalInput = _horizontalInputStash.Get(car).Value;
                var applyDownshift = UpdateDownshiftDrift(ref downshiftDrift, verticalInput, horizontalInput,
                    useTouge ? touge : null, deltaTime);

                bool applyBack;
                backDrift = UpdateDriftValueForAxle(
                    backDrift,
                    handbrakePressed,
                    canDriftNow,
                    backEnterSpeed,
                    backReturnSpeed,
                    deltaTime,
                    out applyBack);

                backDrift = Mathf.Clamp01(backDrift);
                applyBack |= applyDownshift;

                if (applyBack)
                {
                    var effectiveBackDrift = Mathf.Max(backDrift, downshiftDrift.Value);
                    var effectiveBackTarget = downshiftDrift.Value > backDrift
                        ? downshiftDrift.RearGripMultiplier
                        : backMultiplierTarget;
                    var backMultiplier = Mathf.Lerp(1f, effectiveBackTarget, effectiveBackDrift);
                    ApplyAxleSlip(wheelInfoComponent, backBaseSidewaysStiffness, backMultiplier,
                        applyToSteeringWheels: false);
                }

                bool applyFront;
                frontDrift = UpdateDriftValueForAxle(
                    frontDrift,
                    handbrakePressed,
                    canDriftNow,
                    frontEnterSpeed,
                    frontReturnSpeed,
                    deltaTime,
                    out applyFront);

                frontDrift = Mathf.Clamp01(frontDrift);
                _frontDriftValues[car] = frontDrift;
                applyFront |= applyDownshift;

                if (applyFront)
                {
                    var effectiveFrontDrift = Mathf.Max(frontDrift, downshiftDrift.Value);
                    var effectiveFrontTarget = downshiftDrift.Value > frontDrift
                        ? downshiftDrift.FrontGripMultiplier
                        : frontMultiplierTarget;
                    var frontMultiplier = Mathf.Lerp(1f, effectiveFrontTarget, effectiveFrontDrift);
                    ApplyAxleSlip(wheelInfoComponent, frontBaseSidewaysStiffness, frontMultiplier,
                        applyToSteeringWheels: true);
                }


                var flatFwd = new Vector3(transform.forward.x, 0f, transform.forward.z);

                var hasVelocity = flatVel.sqrMagnitude > 0.01f;
                var hasForward = flatFwd.sqrMagnitude > 0.01f;
                var hasDriveDir = hasForward && currentGear != 0;

                var slipAngle = 0f;

                if (hasVelocity && hasDriveDir)
                {
                    var velDir = flatVel.normalized;
                    var driveDir = flatFwd.normalized;

                    if (currentGear < 0)
                        driveDir = -driveDir;

                    slipAngle = Vector3.Angle(driveDir, velDir);
                }

                var skidFromBrake = speedTotalKmh > helpersSetup.MinBrakeSkidSpeedKmh && (brakeInput || handbrakePressed);

                var skidFromSlipAngle = hasVelocity && hasDriveDir && speedTotalKmh > helpersSetup.MinDriftSpeedKmh
                                        && slipAngle > helpersSetup.SlipAngleThresholdDeg;

                var checkDrift = backDrift > helpersSetup.DriftVisualThresh || frontDrift > helpersSetup.DriftVisualThresh;
                var skidFromFriction = speedTotalKmh > helpersSetup.MinDriftSpeedKmh * 0.5f && checkDrift;
                var skidFromDownshift = downshiftDrift.Value > helpersSetup.DriftVisualThresh;

                skidFlag = skidFromBrake || skidFromSlipAngle || skidFromFriction || skidFromDownshift;
            }
        }

        private bool UpdateDownshiftDrift(
            ref DownshiftDriftComponent downshiftDrift,
            float verticalInput,
            float horizontalInput,
            Data.HelperClass.CarMovementTougeSetup touge,
            float deltaTime
        )
        {
            if (touge == null)
            {
                if (downshiftDrift.Value <= 0f)
                    return false;

                downshiftDrift.Value = 0f;
                downshiftDrift.Timer = 0f;
                downshiftDrift.RearGripMultiplier = 1f;
                downshiftDrift.FrontGripMultiplier = 1f;
                return true;
            }

            if (downshiftDrift.Timer > 0f)
            {
                downshiftDrift.Timer -= deltaTime;
                downshiftDrift.Value = 1f;
                return true;
            }

            var holdTarget = verticalInput > 0.01f && Mathf.Abs(horizontalInput) >= touge.MinDownshiftDriftSteer
                ? touge.DriftHoldFromThrottle
                : 0f;

            var previousValue = downshiftDrift.Value;
            downshiftDrift.Value = Mathf.MoveTowards(downshiftDrift.Value, holdTarget,
                touge.DriftReturnSpeed * deltaTime);

            if (downshiftDrift.Value <= 0.001f)
            {
                downshiftDrift.Value = 0f;
                downshiftDrift.RearGripMultiplier = 1f;
                downshiftDrift.FrontGripMultiplier = 1f;
            }

            return !Mathf.Approximately(previousValue, downshiftDrift.Value);
        }

        private float UpdateDriftValueForAxle(
            float currentDrift,
            bool handbrakePressed,
            bool canDriftNow,
            float enterSpeed,
            float returnSpeed,
            float deltaTime,
            out bool shouldApplySlip)
        {
            shouldApplySlip = false;
            var drift = Mathf.Clamp01(currentDrift);

            if (canDriftNow)
            {
                drift = Mathf.MoveTowards(drift, 1f, enterSpeed * deltaTime);
                shouldApplySlip = true;
            }
            else
            {
                if (!handbrakePressed && drift > 0f)
                {
                    drift = Mathf.MoveTowards(drift, 0f, returnSpeed * deltaTime);
                    shouldApplySlip = true;

                    if (drift <= 0f)
                        drift = 0f;
                }
            }

            return drift;
        }

        private void ApplyAxleSlip(
            WheelInfoComponent wheelInfoComponent,
            float baseSidewaysStiffness,
            float stiffnessMultiplier,
            bool applyToSteeringWheels)
        {
            foreach (var info in wheelInfoComponent.WheelInfo)
            {
                if (info == null)
                    continue;

                if (info.Steering != applyToSteeringWheels)
                    continue;

                if (info.LeftWheel != null)
                {
                    var sf = info.LeftWheel.sidewaysFriction;
                    sf.stiffness = baseSidewaysStiffness * stiffnessMultiplier;
                    info.LeftWheel.sidewaysFriction = sf;
                }

                if (info.RightWheel != null)
                {
                    var sf = info.RightWheel.sidewaysFriction;
                    sf.stiffness = baseSidewaysStiffness * stiffnessMultiplier;
                    info.RightWheel.sidewaysFriction = sf;
                }
            }
        }

        public void Dispose() { }
    }
}
