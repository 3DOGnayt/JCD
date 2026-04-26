using System;
using uk.vroad.api;
using uk.vroad.api.etc;
using uk.vroad.api.str;
using uk.vroad.apk;
using uk.vroad.rvr;
using uk.vroad.ucm;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace uk.vroad.urvr
{
    public class UPlayQuarter : UaPlaySim
    {
        public Text timingText;


        // UI text objects containing strings that require translation
        public Text[] babylTexts;
        // All the other UI text objects in the scene - thi sis used to flag any UI Text that are unaccounted
        public Text[] noLangTexts;
        
        public Text debugRTFFText; // This should be inactive by default
        public Text debugMetricsText;// This should be inactive by default
        
        private Game game;
        
        protected override App App() { return game; }
        protected override void Awake()
        {
            game = Game.AwakeInstance();
            base.Awake();
           
#if UNITY_EDITOR
           debugRTFFText.gameObject.SetActive(true);
           debugMetricsText.gameObject.SetActive(true);
#endif
            
            UGameHelp.ReadTextFromBabyl(name, babylTexts, noLangTexts);
        }

        protected override void Update()
        {
            base.Update();
#if UNITY_EDITOR
            ShowDebugDisplays();
#endif
        }
        protected override void FixedUpdate()
        {
            timingText.text = new TimeHMS(sim?.TimeNow() ?? 0).ToString(false);

            base.FixedUpdate();
        }

        // For the case where the GamePlay scene is started manually from the editor (perhaps for testing)
        // this causes the default map to be loaded immediately
        private static bool autoLoadMapOnStart = true;

        public static void CancelAutoLoad()
        {
            autoLoadMapOnStart = false;
        }
        
        void LateUpdate()
        {
            if (autoLoadMapOnStart)
            {
                CancelAutoLoad();
                game.Lm().OpenSelectedQuarter();
            }
        }
        protected override void AsFastAsPossible(bool v)
        {
            base.AsFastAsPossible(v);
            game.Gew().FireSpeedChanged();
        }

        protected override bool IsFFwd()
        {
            return game.Gsc().IsFFwd();
        }
        
        public override void AppStateChanged(AppStateTransition ast)
        {
            base.AppStateChanged(ast);
            
            if (ast == GameStateTransition.gameOverWhileNavigating)
            {
                EnforceSingleStepMode();
            }
            
            else if (ast == GameStateTransition.pauseWhileNavigating ||
                     ast == GameStateTransition.pauseWhileWaitingForTripChoice)
            {
                paused = true;
                Time.timeScale = 0; // Stop animation
            }
            else if (ast == GameStateTransition.resumeToNavigating ||
                     ast == GameStateTransition.resumeToWaitingForTripChoice)
            {
                paused = false;
                Time.timeScale = unity_xRT;
            }
        }
      

#if UNITY_EDITOR
        private int fpsCalculated; // 202010504 currently a dbg display only, but could be used to control other factors
        private float smoothedDeltaTimeBetweenFrames = 0.02f;

        private void ShowDebugDisplays()
        {
            string ffOrRT = AsFastAsPossible() ? SU.TIME_FF : SU.TIME_RT;
            debugRTFFText.text = KFormat.Sprintf(SC.TIME_DISPLAY, sim_xRT, ffOrRT, unity_xRT);

            smoothedDeltaTimeBetweenFrames += (Time.unscaledDeltaTime - smoothedDeltaTimeBetweenFrames) * 0.1f;
            fpsCalculated = (int) Math.Round(1.0f / smoothedDeltaTimeBetweenFrames); // based on rolling average
            debugMetricsText.text = "-";
            /*
            debugMetricsText.text = UDbg.MetricsStr(fpsCalculated, lagMeasure)
                                    + ((UCamControllerMainRvr)UCamControllerMainRvr.Instance).metrics()
                                        //+ CamControllerMini.Instance.metrics()
                                        + ((UBotHandler)UBotHandler.Instance).metrics()
                                        +SC.N;
                                        //*/

        }
#endif
       
    }
}
