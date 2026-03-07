using UnityEngine;
using Zenject;

namespace Cameras
{
    public sealed class MinimapSpawner : MonoBehaviour
    {
        [SerializeField] private MinimapCameraController _minimapCameraPrefab;

        private DiContainer _container;

        [Inject]
        private void Construct(DiContainer container)
        {
            _container = container;
        }

        public MinimapCameraController SpawnAttached(Transform parent)
        {
            if (_minimapCameraPrefab == null || parent == null)
                return null;

            var instance = _container.InstantiatePrefabForComponent<MinimapCameraController>(_minimapCameraPrefab);
            instance.transform.SetParent(parent, false);
            return instance;
        }
    }
}