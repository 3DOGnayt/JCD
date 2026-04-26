using System;
using uk.vroad.api.events;
using uk.vroad.api.map;
using uk.vroad.api.sim;
using uk.vroad.api.str;
using uk.vroad.apk;
using uk.vroad.rvr;
using uk.vroad.ucm;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace uk.vroad.urvr
{
    public class UPlayerEnergy : MonoBehaviour, LPlayer, LSimTimeStep
    {
        public AudioClip rewardClip;
        public AudioClip bonusClip;
        public AudioClip runRedClip;
        public AudioClip fineClip;
        public AudioClip refreshClip;
        public AudioClip vialClip;
        public AudioClip fallbackClip;
        
        public AudioSource audioSourceHUD;
        
        public Text batteryText;
        public Text bonusText;
        public Text virusText;
        public Text powerText;
        public Text targetTextName;
        public Text targetTextReward;
        public Text targetTextDistance;
        public GameObject targetFlagIcon;
        public GameObject targetVialIcon;
        public Slider batterySlider;
        public Image batteryFillImage;
        public Image[] spareBatteryImage;
        public Slider virusSlider;
        public Image virusFillImage;
        
        public Image speedoImage;
        public GameObject speedoFF;
        public Text speedoMinusText;
        public GameObject speedoPlus;
        public Image abandonButtonImage;
        public Image runRedButtonImage;
        public Sprite abandonBusSprite;
        public Sprite abandonTaxiSprite;
        public Text abandonCostText;
        public Sprite runRedPedSprite;
        public Sprite runRedTaxiSprite;

        public Sprite[] speedSprites;
        public Sprite spriteTaxi;
        public Sprite spriteBus;
        public Sprite spriteRedMan;
        public Sprite spriteRedSignal;
        public Sprite spriteCongestion;
        public Sprite spriteCrossing;

        public Image taxiRouteButtonImage;
        public Sprite spritePedTaxi_Ped;
        public Sprite spritePedTaxi_Taxi;
        
        private EnergyEvent pendingEnergyEvent = EnergyEvent.None;
        private AlertEvent pendingAlert = AlertEvent.None;
        private RectTransform speedoRect;
        private GameObject speedoMinus;
        private GameObject taxiRouteButton;
        
        private double pendingEnergyDelta;
        private int abandonChargeOnButton = -1;
        private bool refreshEnergyDisplay; // in main Unity Thread
        private bool refreshSpeedDisplay; // in main Unity Thread
        private bool wasAboard;
        private bool wasBlocked;
        private bool wasTaxiBlocked;
        private bool wasViralRed;
        private bool wasTaxiRoute;

        private Game game;
        
        void Awake()
        {
            game = Game.AwakeInstance();
            
            string init = SC.MI;
            batterySlider.value = 0;
            batteryText.text = init;
            bonusText.text = init;
            virusText.text = init;
            targetTextName.text = init;
            targetTextReward.text = init;
            targetTextDistance.text = init;
            batteryFillImage.color = Color.white;
            powerText.text = init;

            speedoRect = speedoImage.gameObject.GetComponent<RectTransform>();
            speedoMinus = speedoMinusText.transform.parent.gameObject;

            taxiRouteButton = taxiRouteButtonImage.transform.parent.gameObject;
            
            int nb = spareBatteryImage.Length;
            for (int i = 0; i < nb; i++) spareBatteryImage[i].enabled = false;

            game.Pe().ReadPrefsInSuitableThread();

            game.AddEventConsumer(this);

            refreshEnergyDisplay = true;
        }

        public bool DeregisterFireMapChange() { return true; }
        
        public void EnergyChanged(double energyRemaining)
        {
            refreshEnergyDisplay = true;
        }
        
        void Update()
        {
            if (refreshEnergyDisplay)
            {
                refreshEnergyDisplay = false;

                DisplayBatteryEnergy();
                DisplayVirus();
                DisplayTargetDistance();
            }

            if (refreshSpeedDisplay)
            {
                refreshSpeedDisplay = false;
                DisplaySpeed();
            }
            
            if (pendingEnergyEvent != EnergyEvent.None)
            {
                Player player = Player.ActivePlayer();
                Vector3 soundLocation = player?.Centre().ToVector3() ?? game.Map().GetCentrePoint().ToVector3();
                float vol = UAudioController.MasterVolume();
                int kj = (int) Math.Round(pendingEnergyDelta);
                pendingEnergyDelta = 0;

                UGameStateHandler ugsh = (UGameStateHandler) UaStateHandler.MostRecentInstance;
                ugsh.CentralMessageAndIcon(pendingEnergyEvent, kj);
                
                switch (pendingEnergyEvent)
                {
                    case EnergyEvent.Reward:
                        audioSourceHUD.Pause();
                        audioSourceHUD.PlayOneShot(rewardClip);
                        break;
                    case EnergyEvent.Bonus:
                        AudioSource.PlayClipAtPoint(bonusClip, soundLocation, vol);
                        break;
                    
                    case EnergyEvent.FineJayWalking:
                    case EnergyEvent.FineRedLight:
                        AudioSource.PlayClipAtPoint(fineClip, soundLocation, vol);
                        break;
                    
                    case EnergyEvent.RefreshTrips:
                        AudioSource.PlayClipAtPoint(refreshClip, soundLocation, vol);
                        break;
                    
                    case EnergyEvent.AbandonBus:
                    case EnergyEvent.AbandonTaxi:
                        AudioSource.PlayClipAtPoint(fineClip, soundLocation, vol);
                        break;

                    default:
                        AudioSource.PlayClipAtPoint(fallbackClip, soundLocation, vol);
                        break;
                }


                pendingEnergyEvent = EnergyEvent.None;
            }

            if (pendingAlert != AlertEvent.None)
            {
                Player player = Player.ActivePlayer();
                Vector3 soundLocation = player?.Centre().ToVector3() ?? game.Map().GetCentrePoint().ToVector3();
                float vol = UAudioController.MasterVolume();

                UGameStateHandler ugsh = (UGameStateHandler) UaStateHandler.MostRecentInstance;
                ugsh.CentralMessageAndIcon(pendingAlert);

                switch (pendingAlert)
                {
                    case AlertEvent.Launch:
                        if (! audioSourceHUD.isPlaying) audioSourceHUD.Play();
                        break;
                    
                    case AlertEvent.RequestJayWalk:
                    case AlertEvent.RequestRedLightRun:
                        PlayEventClip(runRedClip, soundLocation, vol);
                        break;
                    
                    case AlertEvent.CollectVial:
                        PlayEventClip(vialClip, soundLocation, vol);
                        break;

                    default:
                        PlayEventClip(fallbackClip, soundLocation, vol);
                        break;
                }
                

                pendingAlert = AlertEvent.None;
            }

            if (game.Pe().SaveScoreEtcIfRequired())
            {
#if VROAD_RVR_RELEASE  
                SteamRoboVanRush.SaveScoreAndAchievementsToSteam();
#endif
            }
        }

        private void PlayEventClip(AudioClip clip, Vector3 location, float vol)
        {
            // AudioSource.PlayClipAtPoint(clip, location, vol);
            
            audioSourceHUD.PlayOneShot(clip, vol);
        }
        
        private void DisplayTargetDistance()
        {
            Player player = Player.ActivePlayer();
            IPedZone dest = (IPedZone) player?.Ped().GetDestination();
            if (dest == null) return;
            string edgeStr = dest.IsOnEdge() ? SC.S+SC.RT_ARROW + SC.PP : SC.N;
            double toTarget = player.Centre().DistanceXY(dest.Location());
            
            targetTextName.text = dest.Description() + edgeStr;
            targetTextReward.text = KFormat.Sprintf(SC.SF0F+SC.S+SI.KJ, game.Pe().RewardOnArrival());
            targetTextDistance.text = KFormat.Sprintf(SC.SF0F+SC.S+SI.M, toTarget);

            bool destIsVial = dest == game.Vg().VialZone();
            targetFlagIcon.SetActive(!destIsVial);
            targetVialIcon.SetActive( destIsVial);
        }
        private void DisplayVirus()
        {
            int max = GameFeatures.VIRAL_CONTACT_OVERLOAD;
            int vc = game.Pe().ViralContacts();
            if (vc > max) vc = max;
            float vcp = (float) vc / (float) max;

            string fmt = SC.SF0F;
            // Never show 100%: show 99.xx% until game over
            if (vcp > 0.99f) fmt = SC.SF2F;
            
            virusText.text = KFormat.Sprintf(fmt, 100F * vcp);
            virusSlider.value = vcp;
            
            if (!wasViralRed && vcp > 0.9f)
            {
                virusText.color = Color.red;
                wasViralRed = true;
                virusFillImage.color = Color.red;
            }
        }
        private void DisplayBatteryEnergy()
        {
            int NSP = 9;
            PlayerEnergy pe = game.Pe();
            
            int range = pe.Range();
            int spareBatteries = pe.SpareBatteries();
            Color rangeColor = ENERGY_COLOURS[range];

            for (int i = 0; i < NSP; i++)
            {
                spareBatteryImage[i].enabled = (spareBatteries > i);
                if (spareBatteryImage[i].enabled)
                {
                    spareBatteryImage[i].color = rangeColor;
                }
            }

            batteryFillImage.color = rangeColor;
            batterySlider.value = (float)pe.EnergyRemainingOnSlider(); 
            batteryText.text = KFormat.Sprintf(SC.SF0F+SC.S+SI.KJ, pe.EnergyRemainingKJ());
            bonusText.text = KFormat.Sprintf(SC.SF0F+SC.S+SI.KJ, pe.Bonus());
            
            powerText.text = KFormat.Sprintf(SC.SF0F+SC.S+SI.W, pe.PowerCurrently());
        }

        private static readonly Color[] ENERGY_COLOURS = { Color.green, Color.cyan, Color.blue, Color.yellow, Color.magenta, };

        public void EnergyEventOccurred(EnergyEvent pe, double delta)
        {
            pendingEnergyEvent = pe;
            pendingEnergyDelta += delta;
            
            // KDbg.ReportU(KDbg.UPlayerEnergy03, pe);
        }

        public void SpeedChanged()
        {
            refreshSpeedDisplay = true;
        }

        public void Alert(AlertEvent ae)
        {
            pendingAlert = ae;
        }
        
        public void TimeStep()
        {
            Player player = Player.ActivePlayer();
            if (player == null) return;

            if (player.Blocked() != wasBlocked)
            {
                wasBlocked = !wasBlocked;
                refreshSpeedDisplay = true;
            }

            bool aboard = player.Aboard();
            if (aboard != wasAboard)
            {
                wasAboard = !wasAboard;
                refreshSpeedDisplay = true;
            }
            
            ITaxi taxi = player.Ped().GetTaxi();
            if (taxi != null && taxi.IsBlocked() != wasTaxiBlocked)
            {
                wasTaxiBlocked = !wasTaxiBlocked;
                refreshSpeedDisplay = true;
            }

            int charge = (int) (aboard? (taxi != null? game.Pe().FineAbandonTaxi(player): game.Pe().FineAbandonBus()): 0);
            if (charge != abandonChargeOnButton)
            {
                refreshSpeedDisplay = true;
            }

            bool showTaxiRoute = game.Prb().ShowTaxiRoute();
            if (showTaxiRoute != wasTaxiRoute)
            {
                wasTaxiRoute = showTaxiRoute;
                refreshSpeedDisplay = true;
            }
        }

        private void DisplaySpeed() // IN main Unity Thread
        {

            Player player = Player.ActivePlayer();
            if (player == null) return;

            IBus bus = player.GetBus();
            ITaxi taxi = player.GetTaxi();
            bool aboard = player.Aboard();
            bool showFF = UaPlaySim.AsFastAsPossible();
            bool atRed = false;
            bool speedButtonsAllowOnlyFFwd = false;
            bool showSpeedo = false;
            
            if (bus != null)
            {
                SetSpeedoIcon(spriteBus, Color.green, showFF);
                speedButtonsAllowOnlyFFwd = true;
            }
            else if (taxi != null)
            {
                if (taxi.IsBlocked())
                {
                    speedButtonsAllowOnlyFFwd = true;
                    
                    if (taxi.IsBlockedByRedSignal())
                    {
                        SetSpeedoIcon(spriteRedSignal, showFF);
                        atRed = true;
                    }
                    else if (taxi.IsBlockedByParking())
                    {
                        SetSpeedoIcon(spriteTaxi, Color.green, showFF);
                    }
                    else if (taxi.IsBlockedByCrossing())
                    {
                        SetSpeedoIcon(spriteCrossing, showFF);
                    }
                    else SetSpeedoIcon(spriteCongestion, showFF);
                }
                else showSpeedo = true;// Taxi Speed
            }
            else if (player.Blocked())
            {
                speedButtonsAllowOnlyFFwd = true;

                IPed ped = player.Ped();
                
                if (ped.IsWaitingAtBusStop()) SetSpeedoIcon(spriteBus, Color.red, showFF);
                else if (ped.IsWaitingAtTaxiBay()) SetSpeedoIcon(spriteTaxi, Color.red, showFF);
                else if (ped.IsWaitingAtCrossing()) { SetSpeedoIcon(spriteRedMan, showFF); atRed = true; }
                else showSpeedo = true;
            }
            else showSpeedo = true; // player walking

            GameSimControl gsc = game.Gsc();
            
            if (showSpeedo)
            {
                SetSpeedoIcon(speedSprites[(int) gsc.SpeedIndicator()]); 
            }

            if (speedButtonsAllowOnlyFFwd)
            {
                speedoMinus.SetActive(gsc.IsFFwd());
                speedoPlus.SetActive(!gsc.IsFFwd());
            }
            else
            {
                speedoMinus.SetActive(!aboard || !gsc.IsStop());
                speedoMinusText.text = !aboard && gsc.IsStop() ? SC.OFF_ARROW : SC.N+CC.MINUS;

                speedoPlus.SetActive(!gsc.IsFFwd());
            }

            PlayerEnergy pe = game.Pe();
            int charge = (int) (aboard? (taxi != null? pe.FineAbandonTaxi(player): pe.FineAbandonBus()): 0);
            bool canAbandon = aboard && charge < pe.EnergyRemainingKJ();
                
            // We could also disable the abandon button if speed was too fast, but this can lead
            // to button flashing on and off, with no explanation - better to leave it visible and
            // show a message why it is not possible (as we already do)

            if (canAbandon)
            {
                abandonButtonImage.sprite = bus != null ? abandonBusSprite : abandonTaxiSprite;
                abandonCostText.text = charge < 1? SC.N: SC.MI + charge;
                abandonChargeOnButton = charge;
            }

            if (atRed) runRedButtonImage.sprite = taxi != null ? runRedTaxiSprite : runRedPedSprite;
            
            abandonButtonImage.gameObject.transform.parent.gameObject.SetActive(canAbandon);
            abandonCostText.gameObject.SetActive(canAbandon);
            runRedButtonImage.gameObject.transform.parent.gameObject.SetActive(atRed);
            
            if (taxi != null) taxiRouteButtonImage.sprite = wasTaxiRoute ? spritePedTaxi_Taxi : spritePedTaxi_Ped;
            taxiRouteButton.SetActive(taxi != null);
        }

        private void SetSpeedoIcon(Sprite sp)
        {
            SetSpeedoIcon(sp, Color.white);
        }
        private void SetSpeedoIcon(Sprite sp, bool showFF)
        {
            SetSpeedoIcon(sp, Color.white, showFF);
        }

        private void SetSpeedoIcon(Sprite sp, Color color)
        {
            SetSpeedoIcon(sp, color, false);
        }
        private void SetSpeedoIcon(Sprite sp, Color color, bool showFF)
        {
            float aspect = sp.rect.width / sp.rect.height;
            float height = 160;

            speedoRect.sizeDelta = new Vector2(aspect * height, height);
            speedoImage.sprite = sp;
            speedoImage.color = color;

            speedoFF.SetActive(showFF);
        }
    }
}
