using Cinemachine;
using UI.Helpers;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class CameraInstaller : MonoInstaller
    {
        [SerializeField] private FollowingCamera _followingCamera;
        [SerializeField] private CinemachineFreeLook _freeLookCamera;
        
        public override void InstallBindings()
        {
            Container.Bind<FollowingCamera>().FromInstance(_followingCamera).AsSingle();
            Container.Bind<CinemachineFreeLook>().FromInstance(_freeLookCamera).AsSingle();
        }
    }
}