using uk.vroad.api;
using uk.vroad.api.xmpl;
using uk.vroad.ucm;

namespace uk.vroad.xmpl
{
    public class UBotHandlerExample : UaBotHandler
    {
        private ExampleApp app;
        
        protected override void Awake()
        {
            app = ExampleApp.AwakeInstance();
            base.Awake();
        }
        protected override App App() { return app; }
        protected override AppStateTransition StartSimTransition() { return ExampleStateTransition.runSimulation; }

    }
}