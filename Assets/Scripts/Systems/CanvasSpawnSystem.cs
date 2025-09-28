using Scellecs.Morpeh;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Systems
{
    public sealed class CanvasSpawnSystem : ISystem
    {
        [Inject] public World World { get; set; }
        [Inject] public Canvas _canvasPrefab;
        [Inject] public EventSystem _eventSystemPrefab;
        [Inject] private DiContainer _container;

        private Transform _canvasGroup;

        public void OnAwake()
        {
            SetSpawnRoot();
            SpawnUI();
        }

        private void SetSpawnRoot() => _canvasGroup = new GameObject("UI").transform;

        private void SpawnUI()
        {
            
            if (_canvasPrefab == null)
                return;
            if (_eventSystemPrefab == null)
                return;

            var gameCanvasTransform = _canvasPrefab.transform;
            _container.InstantiatePrefabForComponent<Canvas>(
                _canvasPrefab.gameObject,
                gameCanvasTransform.position,
                gameCanvasTransform.rotation,
                _canvasGroup);

            var eventSystemTransform = _eventSystemPrefab.transform;
            _container.InstantiatePrefabForComponent<EventSystem>(
                _eventSystemPrefab.gameObject,
                eventSystemTransform.position,
                eventSystemTransform.rotation,
                _canvasGroup);
        }

        public void OnUpdate(float deltaTime) { }

        public void Dispose() { }
    }
}