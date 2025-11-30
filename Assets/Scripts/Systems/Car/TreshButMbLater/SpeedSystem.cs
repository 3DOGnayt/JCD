// using Components;
// using Scellecs.Morpeh;
// using UnityEngine;
//
// namespace Systems.Car
// {
//     public class SpeedSystem : IFixedSystem
//     {
//         public World World { get; set; }
//
//         private Filter _carFilter;
//         private Stash<RigidbodyComponent> _rbStash;
//         private Stash<SpeedComponent> _speedStash;
//
//         // приватное поле для предыдущей позиции (одна машина)
//         private Vector3 _prevPosition;
//         private bool _hasPrev;
//
//         // параметры сглаживания и защиты от спайков
//         private const float TauSeconds = 0.25f;   // время реакции EMA
//         private const float MaxReasonableKmh = 600f; // анти-спайк
//
//         public void OnAwake()
//         {
//             _carFilter  = World.Filter
//                 .With<RigidbodyComponent>()   // нужен только transform.position
//                 .With<SpeedComponent>()
//                 .Build();
//
//             _rbStash    = World.GetStash<RigidbodyComponent>();
//             _speedStash = World.GetStash<SpeedComponent>();
//
//             _hasPrev = false;
//         }
//
//         public void OnUpdate(float dt)
//         {
//             float alpha = 1f - Mathf.Exp(-dt / Mathf.Max(0.0001f, TauSeconds));
//
//             foreach (var e in _carFilter)
//             {
//                 ref var rb  = ref _rbStash.Get(e);
//                 ref var spd = ref _speedStash.Get(e);
//
//                 Vector3 p = rb.Value.transform.position;
//
//                 if (!_hasPrev)
//                 {
//                     _prevPosition = p;
//                     spd.Value = 0f;
//                     _hasPrev = true;
//                     continue;
//                 }
//
//                 float rawKmh = ((p - _prevPosition).magnitude / Mathf.Max(dt, 1e-6f)) * 3.6f;
//                 _prevPosition = p;
//
//                 if (rawKmh > MaxReasonableKmh) rawKmh = MaxReasonableKmh;
//
//                 spd.Value += (rawKmh - spd.Value) * alpha;
//             }
//         }
//
//         public void Dispose() { }
//     }
// }