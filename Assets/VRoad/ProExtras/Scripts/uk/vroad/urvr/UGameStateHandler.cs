using System;
using System.Collections;
using uk.vroad.api;
using uk.vroad.api.etc;
using uk.vroad.api.events;
using uk.vroad.api.input;
using uk.vroad.api.str;
using uk.vroad.apk;

using uk.vroad.rvr;
using uk.vroad.ucm;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace uk.vroad.urvr
{
    public class UGameStateHandler : UaStateHandler, IScreenGrabber, LAppFeedback, LAppView
    {
        public Text countdownTimerText;
        public GameObject timeUI;

        public GameObject viewUI;
        public GameObject playUI;
        public GameObject statusUI;
        public GameObject virusUI;
        public GameObject playTargetUI;
        public GameObject tripUI;
        public GameObject speedoUI;
       
        public GameObject volumeSlider;
        public GameObject helpPanel;
        public UTripChooser uTripChooser;
        public Text centralMsgText;
        public Image centralIconImage;

        public Sprite bonusBatterySprite;
        public Sprite rewardSprite;
        public Sprite requestJaySprite;
        public Sprite requestRedSprite;
        public Sprite redLightCameraSprite;
        public Sprite refreshSprite;
        public Sprite vialSprite;
        public Sprite energyOutSprite;
        public Sprite collisionPedSprite;
        public Sprite collisionTaxiSprite;
        public Sprite viralOverloadSprite;
        public Sprite abandonTaxiSprite;
        public Sprite abandonBusSprite;
        
        /////////////////////////////////////
        private Game game;
        
        private GameObject countdownParent;
        private RectTransform countdownRect;

        private KFile screenGrabRequested;
        private string screenGrabPath;

        protected float countdownToLoadScene;
        protected IEnumerator loadSceneRoutine;

        protected GameObject centralMsg;
        private GameObject centralIcon;
        private RectTransform centralMsgRect;
        private RectTransform centralIconRect;
        private float messageTimer;
        private float iconTimer;
        private bool viewChanged;
        
        protected override App App()  { return game; }

        protected override void Awake()
        {
            game = Game.AwakeInstance();
            base.Awake();
            
            countdownParent = countdownTimerText.transform.parent.gameObject;
            countdownRect = countdownParent.GetComponent<RectTransform>();
            countdownParent.SetActive(false);


            centralMsg = centralMsgText.transform.parent.gameObject;
            centralMsgRect = centralMsg.GetComponent<RectTransform>();
            centralMsg.SetActive(false);
            
            centralIcon = centralIconImage.gameObject;
            centralIconRect = centralIcon.GetComponent<RectTransform>();
            centralIcon.SetActive(false);

            EnableUI(false, false);
        }

        
        void FixedUpdate()
        {
            float tick = Time.fixedDeltaTime;
            
            if (centralIcon.activeSelf)
            {
                if (iconTimer > 1.5f * tick) iconTimer -= tick;
                else 
                {
                    centralIconImage.sprite = null;
                    centralIcon.SetActive(false);
                    iconTimer = 0;
                }
            }
            if (centralMsg.activeSelf)
            {
                if (messageTimer > 1.5f * tick) messageTimer -= tick;
                else 
                {
                    centralMsgText.text = SC.S;
                    centralMsg.SetActive(false);
                    messageTimer = 0;
                }
            }

            if (countdownToLoadScene > 0)
            {
                countdownToLoadScene -= tick;
                if (countdownToLoadScene < 0.001f && loadSceneRoutine != null)
                {
                    StartCoroutine(loadSceneRoutine);
                }

            }
        }
        
        protected override void Update()
        {
            base.Update();
            
            if (game.Gsm().CurrentState() == AppState.ReadyToSimulate)
            {
                // Start Simulation automatically
                game.Gsm().MakeTransition(GameStateTransition.startSimulation);
            }


            if (simReady)
            {
                double timeRemaining = App().Sim().TimeUntilForcedEscape();
                bool isVisible = timeRemaining < 3600;

                if (isVisible != countdownParent.activeSelf)
                {
                    countdownParent.SetActive(isVisible);
                    timeUI.SetActive(!isVisible);
                }
                
                if (isVisible)
                {
                    double tFinal = 300;
                    countdownTimerText.text = (new TimeHMS(timeRemaining)).ToString(false);
                    countdownTimerText.color = timeRemaining < tFinal ? Color.red : Color.white;
                    if (timeRemaining < tFinal)
                    {
                        float q = (float) ((tFinal - timeRemaining) / tFinal); // 0 .. 1
                        float y = 40f + (660f * q);

                        countdownRect.anchoredPosition = new Vector2(0, y);

                        float size = 1f + (q * 3f);
                        countdownRect.sizeDelta = new Vector2(200 * size, 60 * size);

                        if (timeRemaining < 0.2 * tFinal) countdownTimerText.fontSize = 150;
                        else if (timeRemaining < 0.4 * tFinal) countdownTimerText.fontSize = 125;
                        else if (timeRemaining < 0.6 * tFinal) countdownTimerText.fontSize = 100;
                        else if (timeRemaining < 0.8 * tFinal) countdownTimerText.fontSize = 80;
                        ///else same size, but red. No room yet to make font bigger
                    }
                }
            }


           

            
            if (viewChanged)
            {
                viewChanged = false;

                OnViewChange();
            }

            

            if (screenGrabRequested != null)
            {
                screenGrabPath = screenGrabRequested.FullPath();
                screenGrabRequested = null;

                PreScreenGrab();
            }

        }

        public void FireViewChanged()
        {
            viewChanged = true;
        }

        protected virtual void OnViewChange()
        {
            // Switch speedo off while in dolly state for better video capture
            speedoUI.SetActive(game.Gih().GetCameraState() != CameraState.Dolly);
        }
        
        private void EnableUI(bool forTripChoice, bool forPlay)
        {
            viewUI.SetActive(forPlay);
            statusUI.SetActive(forTripChoice || forPlay);
            virusUI.SetActive(forPlay);
            tripUI.SetActive(forTripChoice);
            playUI.SetActive(forPlay);
            playTargetUI.SetActive(forPlay);  //The chequered flag display, positioned as part of status, but enabled as part of playUI
            speedoUI.SetActive(forPlay);
            volumeSlider.SetActive(forTripChoice || forPlay);
        }
        protected override bool HasMapAlreadyLoaded(AppState currentState)
        {
            if (currentState == GameState.WaitingForTripChoice) return true;
            if (currentState == GameState.PausedAtTripChoice) return true;
            if (currentState == GameState.TripChosen) return true;
            if (currentState == GameState.Navigating) return true;
            if (currentState == GameState.PausedWhileNavigating) return true;

            return base.HasMapAlreadyLoaded(currentState);
        }

        public override void UAppStateChanged(AppStateTransition ast)
        {
            base.UAppStateChanged(ast);
            
            if (ast == GameStateTransition.startSimulation)
            {
                EnableUI(true, false);
            }
            
            else if (ast == GameStateTransition.escapeFromQuarterWait)
            {
                EnableUI(false, false);

                game.DeleteEventConsumers();

                PlayerEnergy pe = game.Pe();
                if (pe.HaveCollectedVial())
                {
                    string msg = SC.N + pe.VialsCount() + SC.S + CC.SLASH + SC.S + pe.VialsCountComplete();
                    CentralMessage(msg, 30);
                    CentralIcon(vialSprite, 30, 150, 150);

                    countdownToLoadScene = 30;
                    loadSceneRoutine = LoadSceneDefault();
                }
                else
                {
                    StartCoroutine(LoadSceneDefault());
                }
                
            }

            else if (ast == GameStateTransition.escapeFromQuarterPlay )
            {
                EnableUI(false, false);
                
                game.DeleteEventConsumers();
                
                StartCoroutine(LoadSceneDefault());
            }

            else if (ast == GameStateTransition.arrivalAtTargetContinueThisMap)
            {
                EnableUI(true, false);
            }
           
            else if (ast == GameStateTransition.arrivalAtTargetLevelUp)
            {
                EnableUI(false, false);

                // immediately followed by startNewLevel below
            }
            else if (ast == GameStateTransition.startNewLevel)
            {
                game.DeleteEventConsumers();
                
                UChooseQuarter.PlayLevelUpSoundOnStart();

                CentralMessage(Babyl.Translation(BabylKey.NewLevel), 30);
                // ... ? CentralIcon(icon, 30, width, 150);

                countdownToLoadScene = 30;
                loadSceneRoutine = LoadSceneDefault();
            }

            else if (ast == GameStateTransition.startNavigating)
            {
                EnableUI(false, true);
            }

            else if (ast == GameStateTransition.gameOverWhileNavigating)
            {
                EnableUI(false, false);
                statusUI.SetActive(true); // see energy / virus while game over icon showing
                
                int reason = game.Gsc().GameOverReason();
                
                Sprite icon = energyOutSprite; //  GameStateHandler.GAME_OVER_ENERGY_EXHAUSTED
                int width = 150;
                
                if (reason == GameSimControl.GAME_OVER_VIRUS) icon = viralOverloadSprite;
                else if (reason == GameSimControl.GAME_OVER_COLLISION_PED) { icon = collisionPedSprite; width = 200; }
                else if (reason == GameSimControl.GAME_OVER_COLLISION_TAXI) { icon = collisionTaxiSprite; width = 200; }

                CentralMessage(Babyl.Translation(BabylKey.GameOver), 30);
                CentralIcon(icon, 30, width, 150);
            }

            else if (ast == GameStateTransition.removeGameOverMsg)
            {
                centralMsg.SetActive(false);
                statusUI.SetActive(false);
               
                game.DeleteEventConsumers();
                
                game.Pe().TimeRewind(); // Reset energy to initial
                
                StartCoroutine(LoadSceneDefault());
            }
            
            
            else if (ast == AppStateTransition.mapLoadFailed)
            {
                // EnableUI(false, false);
                
                if (UDbg.CLEAN_EXIT) UDbg.CatchThis(SU.EXIT_03);
                
                App().DeleteEventConsumers();
               
                StartCoroutine(LoadSceneDefault());
            }
            
            else if (ast == AppStateTransition.abortedBuildEarly || ast == AppStateTransition.abortedBuildLate)
            {
                // This happens when a custom map build fails
                //
                // The ControlsCanvas is lowered by UProgressBar
#if VROAD_RVR_RELEASE
                UChooseQuarter.SetCustomBuildError(uk.vroad.osm.OpenStreetMap.CustomBuildError()); 
#else
                UChooseQuarter.SetCustomBuildError(ExternalMapBuilder.BuildError()); 
#endif
                EnableUI(false, false);
                
            }
        }

        private void OnChooseQuarterSceneLoaded(AsyncOperation obj)
        {
        }


        public void CentralMessageAndIcon(AlertEvent alertEvent)
        {
            switch (alertEvent)
            {
                case AlertEvent.Launch:
                    // no text or sprite, this event is used to start music playing
                    break;
                
                case AlertEvent.RequestJayWalk:
                    CentralIcon(requestJaySprite, 5, 160, 120);
                    break;
                
                case AlertEvent.RequestRedLightRun:
                    CentralIcon(requestRedSprite, 5, 160, 120);
                    break;

                
                case AlertEvent.CollectVial:
                    CentralIcon(vialSprite, 20, 240, 240);
                    break;

                case AlertEvent.EnergyTooLow:
                    CentralMessage(Babyl.Translation(BabylKey.EnergyTooLow), 20);
                    break;
                
                case AlertEvent.SpeedTooHigh:
                    CentralMessage(Babyl.Translation(BabylKey.SpeedTooHigh), 20);
                    break;
                
                case AlertEvent.NotAboard:
                    CentralMessage(Babyl.Translation(BabylKey.NotAboard), 20);
                    break;

                case AlertEvent.NoFootpath:
                    CentralMessage(Babyl.Translation(BabylKey.NoFootpath), 20);
                    break;

                default:
                    UDbg.CatchThis(new ArgumentException(alertEvent.ToString()));
                    break;
            }
            
        }
        public void CentralMessageAndIcon(EnergyEvent energyEvent, int kj)
        {
            switch (energyEvent)
            {
                
                case EnergyEvent.Reward:
                    CentralMessage(kj, 30);
                    CentralIcon(rewardSprite, 20, 90, 90);
                    break;
                
                case EnergyEvent.Bonus:
                    CentralMessage(kj, 20); 
                    CentralIcon(bonusBatterySprite, UBonusCells.RewardColor(kj), 20, 90, 90);
                    break;

                case EnergyEvent.FineJayWalking:
                case EnergyEvent.FineRedLight:
                    CentralMessage(kj, 20);
                    CentralIcon(redLightCameraSprite, 20, 120, 120);
                    break;
               
                case EnergyEvent.RefreshTrips:
                    CentralMessage(kj, 10);
                    CentralIcon(refreshSprite, 10, 90, 90);
                    break;
                
                case EnergyEvent.AbandonTaxi:
                    CentralMessage(kj, 10);
                    CentralIcon(abandonTaxiSprite, 10, 90, 90);
                    break;
                case EnergyEvent.AbandonBus:
                    CentralMessage(kj, 10);
                    CentralIcon(abandonBusSprite, 10, 90, 90);
                    break;

                default:
                    UDbg.CatchThis(new ArgumentException(energyEvent.ToString()));
                    break;
            }

        }
        
        protected void CentralMessage(int kj, int msgTime)
        {
            string msg = kj >= 0 ? CC.PLUS + SC.N + kj : SC.N + kj;
            CentralMessage(msg, msgTime);
        }
        protected void CentralMessage(string msg, int msgTime)
        {
            string[] lines = KTools.SplitQuick(msg, CC.NEWLN);
            float w = longestLineWidth(lines) * 36f; // font size is 48
            float h = lines.Length * 50f;
            centralMsgText.text = msg;
            messageTimer = msgTime;
            centralMsgRect.sizeDelta = new Vector2(w, h);
            centralMsg.SetActive(true);
        }

        protected void CentralIcon(Sprite icon, int iconTime, int w, int h)
        {
            CentralIcon(icon, Color.white, iconTime, w, h);
        }

        protected void CentralIcon(Sprite icon, Color color, int iconTime, int w, int h)
        {
            centralIconRect.sizeDelta = new Vector2(w, h);
            centralIconImage.sprite = icon;
            centralIconImage.color = color;
            iconTimer = iconTime;
            centralIcon.SetActive(true);
        }

        private int longestLineWidth(string[] lines)
        {
           
            int longest = 1;
            foreach (string line in lines)
            {
                if (line.Length > longest) longest = line.Length;
            }

            return longest;
        }

        protected override void OnMapLoadFailure()
        {
            StartCoroutine(LoadSceneDefault());
        }
        
        private IEnumerator LoadSceneDefault()
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SG.INITIAL_SCENE);
            asyncLoad.completed += OnChooseQuarterSceneLoaded;

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone) yield return null;
        }

        
        
        private IEnumerator TakeScreenShotCo(string pathToScreenGrabFile)
        {
            yield return new WaitForEndOfFrame();
            
            ScreenCapture.CaptureScreenshot(pathToScreenGrabFile);

            PostScreenGrab();
           
        }

        
        // The screen grab is used to create a snapshot for a custom map
        public void ScreenGrab(KFile file)
        {
            screenGrabRequested = file;
            // this can be called in any thread, Update() will set screenGrabPath and trigger action in LateUpdate
        }
      
       
        private void PreScreenGrab()
        {
            EnableUI(false, false);
                
            if (helpPanel.activeSelf) helpPanel.SetActive(false);
                
            uTripChooser.RibbonsEnabled(false);
        }

        private void PostScreenGrab()
        {
            if (game.Gsm().CurrentState() == GameState.Navigating) EnableUI(false, true);
            if (game.Gsm().CurrentState() == GameState.WaitingForTripChoice) EnableUI(true, false);
            uTripChooser.RibbonsEnabled(true);
        }

        void LateUpdate()
        {
            if (screenGrabPath != null)
            {
                StartCoroutine(TakeScreenShotCo(screenGrabPath));
                screenGrabPath = null;
            }
        }

        
        
        public void DigitalEventNotConsumed(AppButton button, AppDigitalFn dfn)
        {
            UGamePadRvr.Rumble(50, 0, 100);
        }

    }
}
