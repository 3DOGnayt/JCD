using Cameras;
using Cinemachine;
using Helpers;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class CameraInstaller : MonoInstaller
    {
        [SerializeField] private FollowingCamera _followingCamera;
        [SerializeField] private CinemachineFreeLook _freeLookCamera;
        [SerializeField] private MinimapCameraHolder _minimapCameraPrefab;
        
        public override void InstallBindings()
        {
            Container.Bind<FollowingCamera>().FromInstance(_followingCamera).AsSingle();
            Container.Bind<CinemachineFreeLook>().FromInstance(_freeLookCamera).AsSingle();
            Container.Bind<MinimapCameraHolder>().FromInstance(_minimapCameraPrefab).AsSingle();
        }
    }
}