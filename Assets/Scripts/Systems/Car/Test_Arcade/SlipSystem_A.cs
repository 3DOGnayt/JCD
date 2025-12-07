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

        public void OnAwake()
        {
            _cars = World.Filter
                .Extend<CarSetupAspect>() // DriftMultiplier + HandbrakeInput
                .With<WheelInfoComponent>()
                .With<BackStiffnessSidewaysComponent>()
                .Build();

            _carAspectFactory          = World.GetAspectFactory<CarSetupAspect>();
            _wheelInfoStash            = World.GetStash<WheelInfoComponent>();
            _backSidewaysStiffnessStash = World.GetStash<BackStiffnessSidewaysComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            var slipParams = _carParameters.SlipParameters;
            if (slipParams == null)
                return;

            float targetMultHandbrake = slipParams.HandbrakeSidewaysMultiplier;
            float lerpSpeed           = slipParams.StiffnessLerpSpeed;

            foreach (var car in _cars)
            {
                var aspect = _carAspectFactory.Get(car);
                ref var driftMult    = ref aspect.DriftMultiplier;
                ref var handbrakeInp = ref aspect.HandbrakeInput;

                var wheelInfoComp = _wheelInfoStash.Get(car);
                var backBaseStiff  = _backSidewaysStiffnessStash.Get(car).Value;

                bool handbrake = handbrakeInp.Value;

                // 1) считаем целевой множитель стиффнеса
                float target = handbrake ? targetMultHandbrake : 1f;

                // 2) плавно двигаемся к нему
                driftMult.Value = Mathf.MoveTowards(
                    driftMult.Value,
                    target,
                    lerpSpeed * deltaTime);

                float currentMult = driftMult.Value;

                // 3) применяем к задним колёсам (не рулевым)
                foreach (var info in wheelInfoComp.WheelInfo)
                {
                    if (info == null)
                        continue;

                    bool isRear = !info.Steering;

                    if (!isRear)
                        continue;

                    if (info.LeftWheel != null)
                    {
                        var sf = info.LeftWheel.sidewaysFriction;
                        sf.stiffness = backBaseStiff * currentMult;
                        info.LeftWheel.sidewaysFriction = sf;
                    }

                    if (info.RightWheel != null)
                    {
                        var sf = info.RightWheel.sidewaysFriction;
                        sf.stiffness = backBaseStiff * currentMult;
                        info.RightWheel.sidewaysFriction = sf;
                    }
                }
            }
        }

        public void Dispose() { }
    }
}