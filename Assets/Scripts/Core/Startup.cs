using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Core
{
    public class Startup : MonoBehaviour
    {
        [Inject] public World _world;
        [Inject] private ISystem[] _systems;

        private void Start()
        {
            var systemsGroup = _world.CreateSystemsGroup();

            for (var index = 0; index < _systems.Length; index++)
            {
                var system = _systems[index];
                systemsGroup.AddSystem(system);
            }

            _world.AddSystemsGroup(order: 0, systemsGroup);
        }

        public void Update()
        {
            _world.Update(Time.deltaTime);
        }
    }
}