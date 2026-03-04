using Scellecs.Morpeh;
using Services;
using UnityEngine;
using Zenject;

namespace Core
{
    public class Startup : MonoBehaviour
    {
        [Inject] public World _world;
        [Inject] private ISystemService _systemService;

        private void Start()
        {
            _systemService?.RegisterInitialSystems();
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