using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using UnityEngine;
using Views;
using Zenject;

namespace Systems.Car
{
    public sealed class CollisionSlideSystem : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarCollisionSlideParameters _slideParams;

        private Filter _cars;
        private Stash<CarViewComponent> _carViewStash;

        public void OnAwake()
        {
            _cars = World.Filter
                .With<CarViewComponent>()
                .Build();

            _carViewStash = World.GetStash<CarViewComponent>();

            if (_slideParams == null)
                Debug.LogError($"{nameof(CollisionSlideSystem)}: CarCollisionSlideParameters is null.");
        }

        public void OnUpdate(float deltaTime)
        {
            if (_slideParams == null)
                return;

            var minSpeedMps      = _slideParams.MinSpeedKmh / 3.6f;
            var minSlideAngleDeg = _slideParams.MinSlideAngleDeg;

            foreach (var car in _cars)
            {
                ref var viewComp = ref _carViewStash.Get(car);

                var effectsView  = viewComp.Value as IEffectsView;
                var carViewForRb = viewComp.Value; // ICarView

                if (effectsView == null || carViewForRb == null)
                    continue;

                var listener = effectsView.CollisionListener;
                if (listener == null || !listener.HasCollision)
                    continue;

                var rigidbody = carViewForRb.CarRigidbody;
                if (rigidbody == null)
                {
                    listener.Clear();
                    continue;
                }

                var velocity = rigidbody.velocity;
                var speed    = velocity.magnitude;

                // слишком медленно — не трогаем
                if (speed < minSpeedMps)
                {
                    listener.Clear();
                    continue;
                }

                var normal = listener.Normal;
                if (normal.sqrMagnitude < 0.0001f)
                {
                    listener.Clear();
                    continue;
                }

                normal.Normalize();

                // угол между скоростью и направлением В СТЕНУ (-normal)
                var angle = Vector3.Angle(-normal, velocity.normalized);

                // слишком фронтально — оставляем как есть, пусть "бьётся"
                if (angle < minSlideAngleDeg)
                {
                    listener.Clear();
                    continue;
                }

                // --- разложение скорости ---

                var dot = Vector3.Dot(velocity, normal);       // >0: уже от стены, <0: в стену
                var vN  = Vector3.Project(velocity, normal);   // нормальная составляющая
                var vT  = velocity - vN;                       // вдоль стены

                var tangentKeep = _slideParams.TangentKeep;
                var normalKeep  = _slideParams.NormalKeep;
                var bounce      = _slideParams.BounceFactor;

                Vector3 newVelocity;

                if (dot < 0f)
                {
                    // Летим В СТЕНУ -> отражаем нормальную компоненту наружу.
                    // vN сейчас направлен в ту же сторону, что normal,
                    // но dot < 0 => по сути "в стену", разворачиваем:
                    var vInto = vN;          // компонент в стену (по знаку)
                    var vOut  = -vInto;      // отражённый наружу

                    // По углу плавно усиливаем bounce:
                    var angleFactor = Mathf.InverseLerp(minSlideAngleDeg, 90f, angle);
                    var bounceFactor = Mathf.Lerp(0f, bounce, angleFactor);

                    newVelocity = vT * tangentKeep + vOut * bounceFactor;
                }
                else
                {
                    // Уже скользим вдоль / от стены — просто приглушаем нормальную часть
                    newVelocity = vT * tangentKeep + vN * normalKeep;
                }

                // опционально: не давать скорости падать ниже некого процента
                var newSpeed = newVelocity.magnitude;
                if (newSpeed > 0.01f)
                {
                    var minKeep = 0.5f; // можно вынести в параметры, если зайдёт
                    var minAllowed = speed * minKeep;

                    if (newSpeed < minAllowed)
                        newVelocity = newVelocity.normalized * minAllowed;
                }

                rigidbody.velocity = newVelocity;

                // можно слегка приглушить вращение, чтобы не крутило как спиннер:
                // rigidbody.angularVelocity *= 0.7f;

                listener.Clear();
            }
        }

        public void Dispose() { }
    }
}
