using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems.Car.Test_Arcade
{
    public sealed class PhysicsSpeedSystem_A : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarMovementParameters _params;

        private Filter _cars;
        private AspectFactory<CarSetupAspect> _carAspectFactory;
        private Stash<RigidbodyComponent> _rigidbodyStash;
        private Stash<TransformComponent> _transformStash;
        private Stash<VerticalInputComponent> _vertStash;

        private float _assist;

        public void OnAwake()
        {
            _cars = World.Filter
                .Extend<CarSetupAspect>()          // Speed + BackSpeed и прочее
                .With<RigidbodyComponent>()
                .With<TransformComponent>()
                .With<VerticalInputComponent>()    // чтобы знать, жмём ли газ
                .Build();

            _carAspectFactory = World.GetAspectFactory<CarSetupAspect>();
            _rigidbodyStash   = World.GetStash<RigidbodyComponent>();
            _transformStash   = World.GetStash<TransformComponent>();
            _vertStash        = World.GetStash<VerticalInputComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var car in _cars)
            {
                var aspect = _carAspectFactory.Get(car);
                ref var speed     = ref aspect.Speed;
                ref var backSpeed = ref aspect.BackSpeed;

                var rbComp  = _rigidbodyStash.Get(car);
                var trComp  = _transformStash.Get(car);
                var vert       = _vertStash.Get(car);

                if (rbComp.Value == null || trComp.Value == null)
                    continue;

                var rb      = rbComp.Value;
                var forward = trComp.Value.forward;

                // текущий вектор скорости
                var velocity = rb.velocity;

                // 🔧 Аркадный ассист — отдельно вынесенный метод
                velocity = ApplyArcadeAssist(velocity, vert.Value, deltaTime);

                // записали обратно в Rigidbody
                rb.velocity = velocity;

                // === считаем скорости УЖЕ после ассиста ===

                var forwardSpeedMps = Vector3.Dot(velocity, forward);
                var forwardKmh      = Mathf.Abs(forwardSpeedMps) * 3.6f;

                if (forwardSpeedMps >= 0f)
                {
                    speed.Value     = forwardKmh;
                    backSpeed.Value = 0f;
                }
                else
                {
                    speed.Value     = 0f;
                    backSpeed.Value = forwardKmh; // или Mathf.Abs(forwardKmh), если хочешь только модуль
                }

                // если хочешь, можешь отдельно хранить общий модуль:
                // var speedKmhTotal = velocity.magnitude * 3.6f;
            }
        }

        /// <summary>
        /// Немного "поддерживает" скорость, когда жмём газ вперёд,
        /// чтобы поворот не съедал её слишком резко.
        /// Работает только при достаточной скорости.
        /// </summary>
        private Vector3 ApplyArcadeAssist(
            Vector3 velocity,
            float verticalInput,
            float deltaTime)
        {
            // ассист выключен — просто обновляем lastSpeed
            if (!_params.UseArcadeAssist)
            {
                _assist = velocity.magnitude;
                return velocity;
            }

            float speedMps = velocity.magnitude;

            // скорость слишком маленькая или газа нет — ассист не нужен
            float minSpeedMps = _params.ArcadeAssistMinSpeedKmh / 3.6f;

            if (speedMps < minSpeedMps || verticalInput <= 0.01f)
            {
                _assist = speedMps;
                return velocity;
            }

            // инициализация lastSpeed при первом ходе
            if (_assist <= 0.01f)
                _assist = speedMps;

            // если начали терять скорость — чуть подтягиваем назад к прошлой
            if (speedMps < _assist)
            {
                // превращаем ArcadeAssistLerpSpeed в коэффициент для Lerp за секунду
                float t = 1f - Mathf.Exp(-_params.ArcadeAssistLerpSpeed * deltaTime);
                float targetSpeed = Mathf.Lerp(speedMps, _assist, t);

                if (velocity.sqrMagnitude > 0.0001f)
                    velocity = velocity.normalized * targetSpeed;

                speedMps = targetSpeed;
            }
            else
            {
                // если набрали больше — сдвигаем "эталон" вверх
                _assist = speedMps;
            }

            return velocity;
        }

        public void Dispose() { }
    }
}