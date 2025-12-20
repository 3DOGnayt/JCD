using System.Collections.Generic;
using Components;
using Configs.Impl;
using Data;
using Scellecs.Morpeh;
using Services.Impl;
using UnityEngine;
using Zenject;

namespace Systems.Car.Test_Arcade
{
    public sealed class SkidSmokeSystem_A : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private ISkidSmokeService _smokeService;
        [Inject] private SkidSmokeParameters _smokeParams;

        private Filter _cars;
        private Stash<WheelInfoComponent> _wheelInfoStash;
        private Stash<SkidmarksComponent> _skidmarksStash;
        private Stash<SkidSmokeHandleComponent> _handleStash;

        private readonly List<Vector3> _wheelPositions = new List<Vector3>(8);

        public void OnAwake()
        {
            _cars = World.Filter
                .With<WheelInfoComponent>()
                .With<SkidmarksComponent>()
                .With<SkidSmokeHandleComponent>()
                .Build();

            _wheelInfoStash = World.GetStash<WheelInfoComponent>();
            _skidmarksStash = World.GetStash<SkidmarksComponent>();
            _handleStash = World.GetStash<SkidSmokeHandleComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var car in _cars)
            {
                ref var handleComp = ref _handleStash.Get(car);
                ref var skidFlag = ref _skidmarksStash.Get(car).Value;

                var hasWheelPositions = TryCollectWheelPositions(car, _wheelPositions);

                if (!skidFlag || !hasWheelPositions)
                {
                    StopSkidIfNeeded(ref handleComp);
                    continue;
                }

                if (handleComp.Value < 0)
                    StartSkid(ref handleComp, _wheelPositions);
                else
                    UpdateSkid(handleComp.Value, _wheelPositions);
            }
        }

        private bool TryCollectWheelPositions(Entity car, List<Vector3> buffer)
        {
            buffer.Clear();

            var wheelInfo = _wheelInfoStash.Get(car);
            var wheels = wheelInfo.WheelInfo;

            if (wheels == null || wheels.Count == 0)
                return false;

            var useFront = _smokeParams.EnableFrontSmoke;
            var useRear = _smokeParams.EnableBackSmoke;

            if (!useFront && !useRear)
                return false;

            foreach (var info in wheels)
            {
                if (info == null)
                    continue;

                var isFront = info.Steering;

                if (isFront && !useFront)
                    continue;

                if (!isFront && !useRear)
                    continue;

                if (info.LeftWheel != null)
                {
                    info.LeftWheel.GetWorldPose(out var pos, out _);
                    buffer.Add(pos);
                }

                if (info.RightWheel != null)
                {
                    info.RightWheel.GetWorldPose(out var pos, out _);
                    buffer.Add(pos);
                }
            }

            return buffer.Count > 0;
        }

        private void StopSkidIfNeeded(ref SkidSmokeHandleComponent handleComp)
        {
            if (handleComp.Value < 0)
                return;

            _smokeService.EndSkid(handleComp.Value);
            handleComp.Value = -1;
        }

        private void StartSkid(ref SkidSmokeHandleComponent handleComp, List<Vector3> wheelPositions)
        {
            var handle = _smokeService.BeginSkid(wheelPositions);
            handleComp.Value = handle;
        }

        private void UpdateSkid(int handle, List<Vector3> wheelPositions)
        {
            if (handle < 0)
                return;

            _smokeService.UpdateSkid(handle, wheelPositions);
        }

        public void Dispose() { }
    }
}