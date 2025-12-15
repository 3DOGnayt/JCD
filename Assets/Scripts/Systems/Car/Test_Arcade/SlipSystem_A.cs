using System.Collections.Generic;
using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems.Car.Test_Arcade
{
    public class SlipSystem_A : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarParameters _carParameters;

        private Filter _cars;
        private AspectFactory<CarSetupAspect> _carAspectFactory;

        private Stash<WheelInfoComponent> _wheelInfoStash;
        private Stash<BackStiffnessSidewaysComponent> _backSidewaysStiffnessStash;
        private Stash<FrontStiffnessSidewaysComponent> _frontSidewaysStiffnessStash;

        private Dictionary<Entity, float> _frontDriftValues;

        private const float MinDriftSpeedKmh = 20f;

        public void OnAwake()
        {
            _cars = World.Filter
                .Extend<CarSetupAspect>()
                .With<WheelInfoComponent>()
                .With<BackStiffnessSidewaysComponent>()
                .With<FrontStiffnessSidewaysComponent>()
                .Build();

            _carAspectFactory = World.GetAspectFactory<CarSetupAspect>();
            _wheelInfoStash = World.GetStash<WheelInfoComponent>();
            _backSidewaysStiffnessStash = World.GetStash<BackStiffnessSidewaysComponent>();
            _frontSidewaysStiffnessStash = World.GetStash<FrontStiffnessSidewaysComponent>();

            _frontDriftValues = new Dictionary<Entity, float>();
        }

        public void OnUpdate(float deltaTime)
        {
            var slipParameters = _carParameters.SlipParameters;
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

                if (!_frontDriftValues.TryGetValue(car, out var frontDrift))
                    frontDrift = 0f;

                var wheelInfoComponent = _wheelInfoStash.Get(car);
                var backBaseSidewaysStiffness = _backSidewaysStiffnessStash.Get(car).Value;
                var frontBaseSidewaysStiffness = _frontSidewaysStiffnessStash.Get(car).Value;

                var forwardSpeedKmh = Mathf.Max(0f, speedValue);
                var backwardSpeedKmh = Mathf.Max(0f, Mathf.Abs(backSpeedValue));
                var scalarSpeedKmh = Mathf.Max(forwardSpeedKmh, backwardSpeedKmh);

                var canDriftNow = handbrakePressed && scalarSpeedKmh > MinDriftSpeedKmh;

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