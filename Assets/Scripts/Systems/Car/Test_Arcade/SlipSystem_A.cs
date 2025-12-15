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

        private const float MinDriftSpeedKmh = 20f;

        public void OnAwake()
        {
            _cars = World.Filter
                .Extend<CarSetupAspect>()
                .With<WheelInfoComponent>()
                .With<BackStiffnessSidewaysComponent>()
                .Build();

            _carAspectFactory = World.GetAspectFactory<CarSetupAspect>();
            _wheelInfoStash = World.GetStash<WheelInfoComponent>();
            _backSidewaysStiffnessStash = World.GetStash<BackStiffnessSidewaysComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            var slipParameters = _carParameters.SlipParameters;
            if (slipParameters == null)
                return;

            var targetHandbrakeMultiplier = slipParameters.HandbrakeSidewaysMultiplier;
            var stiffnessEnterSpeed = slipParameters.StiffnessEnterSpeed;
            var stiffnessReturnSpeed = slipParameters.StiffnessReturnSpeed;

            foreach (var car in _cars)
            {
                var aspect = _carAspectFactory.Get(car);
                ref var driftValue = ref aspect.DriftMultiplier.Value;
                ref var handbrakePressed = ref aspect.HandbrakeInput.Value;
                ref var speedValue = ref aspect.Speed.Value;
                ref var backSpeedValue = ref aspect.BackSpeed.Value;

                var wheelInfoComponent = _wheelInfoStash.Get(car);
                var backBaseSidewaysStiffness = _backSidewaysStiffnessStash.Get(car).Value;

                var forwardSpeedKmh = Mathf.Max(0f, speedValue);
                var backwardSpeedKmh = Mathf.Max(0f, Mathf.Abs(backSpeedValue));
                var scalarSpeedKmh = Mathf.Max(forwardSpeedKmh, backwardSpeedKmh);

                if (driftValue < 0f)
                    driftValue = 0f;
                
                if (driftValue > 1f)
                    driftValue = 1f;

                var canDriftNow = handbrakePressed && scalarSpeedKmh > MinDriftSpeedKmh;

                if (canDriftNow)
                {
                    driftValue = Mathf.MoveTowards(driftValue, 1f, stiffnessEnterSpeed * deltaTime);

                    var stiffnessMultiplier = Mathf.Lerp(1f, targetHandbrakeMultiplier, driftValue);

                    ApplyBackWheelsSlip(wheelInfoComponent, backBaseSidewaysStiffness, stiffnessMultiplier);
                }
                else
                {
                    if (!handbrakePressed && driftValue > 0f)
                    {
                        driftValue = Mathf.MoveTowards(driftValue, 0f, stiffnessReturnSpeed * deltaTime);

                        var stiffnessMultiplier = Mathf.Lerp(1f, targetHandbrakeMultiplier, driftValue);

                        ApplyBackWheelsSlip(wheelInfoComponent, backBaseSidewaysStiffness, stiffnessMultiplier);

                        if (driftValue <= 0f)
                            driftValue = 0f;
                    }
                }
            }
        }

        private void ApplyBackWheelsSlip(
            WheelInfoComponent wheelInfoComponent,
            float backBaseSidewaysStiffness,
            float stiffnessMultiplier)
        {
            foreach (var info in wheelInfoComponent.WheelInfo)
            {
                if (info == null)
                    continue;

                if (info.Steering)
                    continue;

                if (info.LeftWheel != null)
                {
                    var sidewaysFriction = info.LeftWheel.sidewaysFriction;
                    sidewaysFriction.stiffness = backBaseSidewaysStiffness * stiffnessMultiplier;
                    info.LeftWheel.sidewaysFriction = sidewaysFriction;
                }

                if (info.RightWheel != null)
                {
                    var sidewaysFriction = info.RightWheel.sidewaysFriction;
                    sidewaysFriction.stiffness = backBaseSidewaysStiffness * stiffnessMultiplier;
                    info.RightWheel.sidewaysFriction = sidewaysFriction;
                }
            }
        }

        public void Dispose() { }
    }
}