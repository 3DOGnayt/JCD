using uk.vroad.api;
using uk.vroad.api.xmpl;
using uk.vroad.ucm;
using UnityEngine;

namespace uk.vroad.xmpl
{
    public class UPlaySimExample : UaPlaySim
    {
        /* We want traffic to run if
         * - there is a PlaySim GameObject attached to the scene
         * AND
         * - the V-Road window is on the [Run Traffic] tab.
         * 
         * So the other tabs set runTraffic to false, which forces paused to be set in Update.
         *
         * Of course the user could manually set the toggle to true, then pause and resume using the menu.
         */
        public bool runTraffic;
        
        public static UPlaySimExample MostRecentInstance { get; private set;  }

        private ExampleApp app;
        
        protected override App App() { return app; }
        protected override bool IsFFwd()  { return false; }

        protected override void Awake()
        {
            app = ExampleApp.AwakeInstance();
            base.Awake();

            MostRecentInstance = this;
        }

        protected override void Update()
        {
            if (!runTraffic) paused = true;
            
            if (runTraffic && app.Asm().CurrentState() == AppState.ReadyToSimulate)
            {
                // Start Simulation automatically
                app.Asm().MakeTransition(ExampleStateTransition.runSimulation);
            }
            
            base.Update(); 
        }

        public void RunTraffic(bool v) { runTraffic = v; paused = !v; }
        
        public override void AppStateChanged(AppStateTransition ast)
        {
            base.AppStateChanged(ast);

            if (runTraffic)
            {
                if (ast == ExampleStateTransition.pauseWhileSimulating)
                {
                    paused = true;
                    Time.timeScale = 0; // Stop animation
                }
                else if (ast == ExampleStateTransition.resumeToSimulating)
                {
                    paused = false;
                    Time.timeScale = unity_xRT;
                }
            }
        }

    }
}