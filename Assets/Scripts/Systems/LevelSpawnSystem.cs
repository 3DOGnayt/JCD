using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems
{
    public sealed class LevelSpawnSystem : ISystem 
    {
        [Inject] public World World { get; set;}
        
        private Transform _levelGroup;
        
        public void OnAwake()
        {
            SetSpawnRoot();
            SpawnLevel();
        }

        private void SetSpawnRoot() => _levelGroup = new GameObject("Level").transform;

        private void SpawnLevel()
        {
            
        }

        public void OnUpdate(float deltaTime) { }
        public void Dispose() { }
    }
}