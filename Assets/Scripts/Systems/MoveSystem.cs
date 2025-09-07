using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems
{
    public sealed class MoveSystem : ISystem 
    {
        [Inject] public World World { get; set;}

        private Filter _filter;
        private Stash<SpeedComponent> _speedStash;
    
        public void OnAwake()
        {
            _filter = World.Filter.With<SpeedComponent>().Build();
            _speedStash = World.GetStash<SpeedComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                ref var speed = ref _speedStash.Get(entity);
                Debug.Log($"Car {entity.Id} speed: {speed.Value}");
            }
        }

        public void Dispose() { }
    }
}