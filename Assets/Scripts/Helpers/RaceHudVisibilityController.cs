using Services;
using UniRx;
using UnityEngine;
using Zenject;

namespace Helpers
{
    public class RaceHudVisibilityController : MonoBehaviour
    {
        [SerializeField] private GameObject _buttonsContainer;
        [SerializeField] private GameObject _parametersContainer;
        
        [Inject]
        public void Construct(ILoadingService loadingService)
        {
            loadingService.IsLoadingCompleted.Subscribe(OnPlayerSpawned).AddTo(this);
        }

        private void OnPlayerSpawned(bool isActive)
        {
            SetHudVisible(isActive);
        }

        private void SetHudVisible(bool isVisible)
        {
            if (_buttonsContainer != null)
                _buttonsContainer.SetActive(isVisible);
            if (_parametersContainer != null)
                _parametersContainer.SetActive(isVisible);
        }
    }
}
