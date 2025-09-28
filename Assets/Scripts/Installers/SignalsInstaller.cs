using Signals;
using Zenject;

namespace Installers
{
    public class SignalsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            DeclareSignals();
        }

        private void DeclareSignals()
        {
            Container.DeclareSignal<PlayerSpawnedSignal>();
        }
    }
}