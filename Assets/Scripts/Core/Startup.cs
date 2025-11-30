using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Core
{
    public class Startup : MonoBehaviour
    {
        [Inject] public World _world;
        [Inject] private ISystem[] _systems;
        [Inject] private IFixedSystem[] _fixedSystems;

        private void Start()
        {
            var systemsGroup = _world.CreateSystemsGroup();
            var fixedSystemsGroup = _world.CreateSystemsGroup();

            foreach (var system in _systems) 
                systemsGroup.AddSystem(system);

            foreach (var fixedSystem in _fixedSystems) 
                fixedSystemsGroup.AddSystem(fixedSystem);

            _world.AddSystemsGroup(order: 0, systemsGroup);
            _world.AddSystemsGroup(order: 1, fixedSystemsGroup);
        }

        public void Update()
        {
            _world?.Update(Time.deltaTime);
        }

        public void FixedUpdate()
        {
            _world?.FixedUpdate(Time.fixedDeltaTime);
        }
    }
}