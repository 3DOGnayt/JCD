using Components;
using Configs.Impl;
using Core;
using Data;
using Data.Enums;
using Data.Helpers;
using Scellecs.Morpeh;
using UnityEngine;
using Views;
using Views.Impl;
using Zenject;

namespace Systems.Car.Test_Arcade
{
    public sealed class CarLightsSystem_A : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarLightsParameters _lightsParams;

        private Filter _cars;
        private Stash<CarViewComponent> _carViewStash;
        private Stash<HeadlightsComponent> _headlightsStash;
        private Stash<BrakeInputComponent> _brakeStash;
        private Stash<HandbrakeInputComponent> _handbrakeStash;
        
        private bool _headlightsKeyPrev; // TODO: this before update input system or new system 

        public void OnAwake()
        {
            _cars = World.Filter
                .With<CarViewComponent>()
                .With<HeadlightsComponent>()
                .With<BrakeInputComponent>()
                .With<HandbrakeInputComponent>()
                .Build();

            _carViewStash = World.GetStash<CarViewComponent>();
            _headlightsStash = World.GetStash<HeadlightsComponent>();
            _brakeStash = World.GetStash<BrakeInputComponent>();
            _handbrakeStash = World.GetStash<HandbrakeInputComponent>();
            
            if (_lightsParams == null)
                Debug.LogError($"{nameof(CarLightsSystem_A)}: CarLightsParameters is null. Lights will be disabled.");
        }

        public void OnUpdate(float deltaTime)
        {
            var keyNow = Input.GetKey(KeyCode.L);
            var togglePressed = keyNow && !_headlightsKeyPrev;

            _headlightsKeyPrev = keyNow;

            foreach (var car in _cars)
            {
                ref var viewComp = ref _carViewStash.Get(car);
                var carView = viewComp.Value as IEffectsView;
                if (carView == null)
                    continue;

                var lights = carView.CarEffectsSetup.carLightsSetup;
                if (lights == null)
                    continue;

                ref var headlights = ref _headlightsStash.Get(car);
                if (togglePressed)
                    headlights.Value = NextMode(headlights.Value);

                var isBraking = _brakeStash.Get(car).Value || _handbrakeStash.Get(car).Value;

                ApplyFrontLights(lights, headlights.Value);
                ApplyRearLights(lights, isBraking);
            }
        }

        private static EHeadlightsMode NextMode(EHeadlightsMode current)
        {
            return current switch
            {
                EHeadlightsMode.Off => EHeadlightsMode.Low,
                EHeadlightsMode.Low => EHeadlightsMode.High,
                EHeadlightsMode.High => EHeadlightsMode.Off,
                _ => EHeadlightsMode.Off
            };
        }

        private void ApplyFrontLights(CarLightsSetup lightsSetup, EHeadlightsMode mode)
        {
            var forwardLeft = lightsSetup.ForwardLeft;
            var forwardRight = lightsSetup.ForwardRight;

            if (forwardLeft == null && forwardRight == null)
                return;

            var enable = mode != EHeadlightsMode.Off;

            var targetRange = mode == EHeadlightsMode.High
                ? _lightsParams.HighRange
                : _lightsParams.LowRange;

            var targetIntensity = mode == EHeadlightsMode.High
                ? _lightsParams.HighIntensity
                : _lightsParams.LowIntensity;

            if (forwardLeft != null)
            {
                forwardLeft.enabled = enable;
                if (enable)
                {
                    forwardLeft.range = targetRange;
                    forwardLeft.intensity = targetIntensity;
                }
            }

            if (forwardRight != null)
            {
                forwardRight.enabled = enable;
                if (enable)
                {
                    forwardRight.range = targetRange;
                    forwardRight.intensity = targetIntensity;
                }
            }
        }

        private void ApplyRearLights(CarLightsSetup lightsSetup, bool isBraking)
        {
            var backLeft = lightsSetup.BackLeft;
            var backRight = lightsSetup.BackRight;

            if (backLeft == null && backRight == null)
                return;

            if (backLeft != null)
                backLeft.enabled = isBraking;

            if (backRight != null)
                backRight.enabled = isBraking;
        }

        public void Dispose() { }
    }
}