using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems
{
    public sealed class StartSpawnSystem : ISystem 
    {
        [Inject] public World World { get; set;}
        [Inject] private CarPreset _carPreset;
        
        private Transform _root;
        private Transform _playerGroup;
        private Transform _levelGroup;
        
        public void OnAwake()
        {
            SetSpawnRoot();

            SpawnLevel();
            SpawnPlayer();
        }

        private void SetSpawnRoot()
        {
            
        }

        private void SpawnLevel()
        {
            
        }

        private void SpawnPlayer()
        {
            var player = _carPreset.Car;
            
            if (player == null)
                return;

            var instance = Object.Instantiate(player, Vector3.zero, Quaternion.identity);
            var entity = World.CreateEntity();

            AddCommonComponents(entity, instance.transform, player);
            entity.SetComponent(new PlayerTagComponent());
        }

        private void AddCommonComponents(Entity entity, Transform instanceTransform, GameObject player)
        {
            entity.SetComponent(new SpeedComponent { Value = 100});
        }

        public void OnUpdate(float deltaTime) { }
        public void Dispose() { }
    }
}