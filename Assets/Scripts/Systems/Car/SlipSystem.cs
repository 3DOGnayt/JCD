using System.Collections.Generic;
using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems.Car
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
                .Build();

            _carAspectFactory = World.GetAspectFactory<CarSetupAspect>();
            _wheelInfoStash = World.GetStash<WheelInfoComponent>();
            _backSidewaysStiffnessStash = World.GetStash<BackStiffnessSidewaysComponent>();
            _frontSidewaysStiffnessStash = World.GetStash<FrontStiffnessSidewaysComponent>();
            _skidmarksStash = World.GetStash<SkidmarksComponent>();
            _rbStash = World.GetStash<RigidbodyComponent>();
            _transformStash = World.GetStash<TransformComponent>();

            _frontDriftValues = new Dictionary<Entity, float>();
        }

        // TODO: Refactoring
        public void OnUpdate(float deltaTime)
        {
            var carParameters = _carSelectionParameters != null ? _carSelectionParameters.SelectedCarParameters : null;
            if (carParameters == null)
                return;

            var slipParameters = carParameters.SlipParameters;
            if (slipParameters == null)
                return;

            var backMultiplierTarget = slipParameters.HandbrakeSidewaysBackMultiplier;
            var backEnterSpeed = slipParameters.BackStiffnessEnterSpeed;
            var backReturnSpeed = slipParameters.BackStiffnessReturnSpeed;

            var frontMultiplierTarget = slipParameters.HandbrakeSidewaysForwardMultiplier;
            var frontEnterSpeed = slipParameters.ForwardStiffnessEnterSpeed;
            var frontReturnSpeed = slipParameters.ForwardStiffnessReturnSpeed;

            foreach (var car in _cars)
            {
                var aspect = _carAspectFactory.Get(car);

                ref var backDrift = ref aspect.DriftMultiplier.Value;
                ref var handbrakePressed = ref aspect.HandbrakeInput.Value;
                ref var speedValue = ref aspect.Speed.Value;
                ref var backSpeedValue = ref aspect.BackSpeed.Value;
                ref var brakeInput = ref aspect.BrakeInput.Value;
                ref var skidFlag = ref _skidmarksStash.Get(car).Value;
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

                var helpersSetup = carParameters.MovementParameters.HelpersSetup;
                var canDriftNow = handbrakePressed && scalarSpeedKmh > helpersSetup.MinDriftSpeedKmh;

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

                if (applyBack)
                {
                    var backMultiplier = Mathf.Lerp(1f, backMultiplierTarget, backDrift);
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

                if (applyFront)
                {
                    var frontMultiplier = Mathf.Lerp(1f, frontMultiplierTarget, frontDrift);
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

                skidFlag = skidFromBrake || skidFromSlipAngle || skidFromFriction;
            }
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