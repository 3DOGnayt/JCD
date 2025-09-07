using Tools;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class CameraInstaller : MonoInstaller
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private FolovingCamera _folovingCamera;
        
        public override void InstallBindings()
        {
            Container.Bind<Camera>().FromInstance(_camera).AsSingle();
            Container.Bind<FolovingCamera>().FromInstance(_folovingCamera).AsSingle();
        }
    }
}