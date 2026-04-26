using uk.vroad.api;
using uk.vroad.api.str;
using uk.vroad.apk;
using uk.vroad.rvr;
using UnityEngine;
using UnityEngine.UI;

namespace uk.vroad.urvr
{
    public class UQuarterEnergy : MonoBehaviour
    {
        public Image[] spareBatteryImage;
        public Image[] vialImage;
        public Image batteryFillImage;
        public Slider batterySlider;
        public Text batteryText;
        public Text vialText;
        public Text storyText;
    
        private const int N_SPARE_BATT = 9;
        private const int FIXED_UPDATES_PER_TICK = 10;
        private readonly Color VIAL_GHOST = new Color(0.5f, 0.5f, 0.5f, 0.3f);
    
        private Game game;
        private bool uiNeedsRefresh;
        private int fixedUpdateCount;
        private static UQuarterEnergy instance;
    
        void Awake()
        {
            game = Game.AwakeInstance();
            instance = this;

            batterySlider.value = 0;
            batteryText.text = SC.MI;
            batteryFillImage.color = Color.white;
            int nVial = vialImage.Length;
        
            for (int i = 0; i < N_SPARE_BATT; i++) spareBatteryImage[i].enabled = false;
            for (int i = 0; i < nVial; i++) vialImage[i].color = VIAL_GHOST;

            uiNeedsRefresh = true;
        }

        void Start()
        {
            DisplayVials();
        
       
        }
    
        void FixedUpdate()
        {
            if (fixedUpdateCount++ >= FIXED_UPDATES_PER_TICK)
            {
                fixedUpdateCount = 0;
                if (game.Pe().EnergyChangeGradual()) uiNeedsRefresh = true;
            }
        }

        /// <summary>
        /// After a new level is attained, the increase in energy is added gradually to the total,
        /// So we need to refresh the display in Update/FixedUpdate, not just a one-off call in Start
        /// </summary>
        void Update()
        {
            if (uiNeedsRefresh)
            {
                uiNeedsRefresh = false;

                DisplayBatteryEnergy();
            }
        }

        public static void UpdateDisplayOnCloudScoreImport()
        {
            if (instance != null)
            {
                instance.DisplayVials();
                instance.DisplayBatteryEnergy();
            }
        }
    
        private void DisplayVials()
        {
            int vialCount = game.Pe().VialsCount();
            bool completed = game.Pe().VialsComplete();

            int nVial = vialImage.Length;
            if (completed) vialCount = nVial;  // Could theoretically be more than 36
        
            for (int vi = 0; vi < vialCount; vi++) vialImage[vi].color = Color.white;
       
            for (int vi = vialCount; vi < nVial; vi++) vialImage[vi].color = VIAL_GHOST;
        
            vialText.text = SC.N + vialCount + SC.S+CC.SLASH+SC.S + nVial;

            if (completed)
            {
                GameStateMachine gsm = game.Gsm();
                if (gsm.CurrentState() == AppState.WaitingForMapChoice)
                {
                    storyText.text = Babyl.Translation(BabylKey.CompleteHelpMsg);
                    storyText.alignment = TextAnchor.MiddleCenter;
                
                    gsm.MakeTransition(GameStateTransition.openCompleteMenu);
                }
          
            }
        }
    
        private void DisplayBatteryEnergy()
        {
            int range = game.Pe().Range();
            int spareBatteries = game.Pe().SpareBatteries();
            Color rangeColor = ENERGY_COLOURS[range];

            for (int i = 0; i < N_SPARE_BATT; i++)
            {
                spareBatteryImage[i].enabled = (spareBatteries > i);

                if (spareBatteryImage[i].enabled)
                {
                    spareBatteryImage[i].color = rangeColor;
                }
            }

            batteryFillImage.color = rangeColor;
            batterySlider.value = (float)game.Pe().EnergyRemainingOnSlider(); 
            batteryText.text = KFormat.Sprintf(SC.SF0F, game.Pe().EnergyRemainingKJ());
        }
    
        private static readonly Color[] ENERGY_COLOURS = { Color.green, Color.cyan, Color.blue, Color.yellow, Color.magenta, };


    }
}
