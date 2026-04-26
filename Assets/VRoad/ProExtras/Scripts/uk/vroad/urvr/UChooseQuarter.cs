using System;
using System.Text;
using uk.vroad.api;
using uk.vroad.api.etc;
using uk.vroad.api.str;
using uk.vroad.apk;
using uk.vroad.rvr;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if VROAD_RVR_RELEASE
using uk.vroad.osm;
using ZenFulcrum.EmbeddedBrowser;
#endif

namespace uk.vroad.urvr
{
    public class UChooseQuarter : MonoBehaviour, LChangeLevel
    {
        public Text ietfCodeText;

        // UI text objects containing strings that require translation
        public Text[] babylTexts;
        // All the other UI text objects in the scene - thi sis used to flag any UI Text that are unaccounted
        public Text[] noLangTexts;
        public AudioSource audioSource;
        public GameObject loadingOverlay; // this is used when browser is closed, while data is being fetched
        public Button levelDownButton;
        public Button levelUpButton;
        public Text currentLevelText;
        public Image levelChangerBackground;
        public GameObject levelComplete;
        public ULoadQuarter[] loadQuarters;
        
#if VROAD_RVR_RELEASE
        public Text customMapFailedMessage;
        public Text customMapHiddenReason;
        public GameObject browserParent;
        public Text customMapLabel;
        public Button customNameEditButton;
        public InputField suburbTextField;
        public InputField cityTextField;
        public GameObject customMapPanel;
        public Image customMapHiddenVial;

        public Browser browser;
        protected string customMapName;
        protected GameObject customBuildFailedGO;
        protected static string customBuildError;

#endif
        
       
        protected int countdownEnableKeys;
        
        
        private static bool playLevelUpSoundOnStart;
        
        private Game game;
        
        private App App() { return game; }
        
        public static UChooseQuarter Instance() { return instance; }
        public static UChooseQuarter InstanceQ() { return instance; }
        private static UChooseQuarter instance;
        
        public static void PlayLevelUpSoundOnStart()
        {
            playLevelUpSoundOnStart = true;
        }

        void Awake()
        {
            instance = this;
            game = Game.AwakeInstance();

            ietfCodeText.text = KEnv.LocalLanguageAndRegion();
            UGameHelp.ReadTextFromBabyl(name, babylTexts, noLangTexts);

            // If we are here, then we don't want auto-load. The auto-load function is there
            // to deal with the case where the GamePlay scene is started manually from the editor
            UPlayQuarter.CancelAutoLoad();
            
            game.Lm().Register(this);

#if VROAD_RVR_RELEASE
            bool showCustomMapPanel = game.Lm().CustomQuarterActive();
            customMapPanel.SetActive(showCustomMapPanel);

            customBuildFailedGO = customMapFailedMessage.transform.parent.gameObject;
            customBuildFailedGO.SetActive(false);
#endif
            game.Pe().ReadPrefsInSuitableThread();
        }

        void Update()
        {
#if VROAD_RVR_RELEASE
            if (customBuildError != null && ! customBuildFailedGO.activeSelf)
            {
                string failMsg = Babyl.Translation(BabylKey.CustomBuildFailedText) + SC.NL;
                failMsg += customBuildError + SC.NL;
                failMsg += Babyl.Translation(BabylKey.CustomBuildFailedTryAgain);

                customMapFailedMessage.text = KFormat.Sprintf(failMsg);
                customBuildFailedGO.SetActive(true);
            }
            if (game.Pe().SaveScoreEtcIfRequired()) SteamRoboVanRush.SaveScoreAndAchievementsToSteam();

#else
            game.Pe().SaveScoreEtcIfRequired();
#endif
        }

        void Start()
        {
            if (playLevelUpSoundOnStart)
            {
                if (playLevelUpSoundOnStart)
                {
                    audioSource.volume = UAudioController.MasterVolume(); //20211023
                    audioSource.Play();
                }

                playLevelUpSoundOnStart = false;
            }

            FireLevelChange(0);
        }
        
        void FixedUpdate()
        {
            if (countdownEnableKeys > 0 && --countdownEnableKeys == 0) UKeysRvr.IgnoreKeyInput(false);
            
#if VROAD_RVR_RELEASE           
            if (browserParent.activeSelf)  ((PointerUIBase)browser.UIHandler).ForceKeyboardHasFocus(true);
#endif
        }



        private void FireLevelChange(int change)
        {
            PlayerEnergy pe = game.Pe();

            if (change == 1) pe.CurrentLevelUp();
            else if (change == -1) pe.CurrentLevelDown();

            // This is called initially with change == 0

            LevelManager lm = game.Lm();

            currentLevelText.text = SC.N + pe.CurrentLevel() + SC.S + CC.SLASH + SC.S + LevelManager.N_LEVELS;

            levelDownButton.gameObject.SetActive(pe.CurrentLevelDownIsEnabled());
            levelUpButton.gameObject.SetActive(pe.CurrentLevelUpIsEnabled());

            levelComplete.SetActive(lm.IsLevelComplete());

#if VROAD_RVR_RELEASE

            bool customActive = lm.CustomQuarterActive();
            customMapPanel.SetActive(customActive);

            int needVials = lm.CustomMapNeedVials();
            customMapHiddenVial.gameObject.SetActive(needVials > 0);

            string msg = lm.CustomMapHiddenReason();
            customMapHiddenReason.text = msg;
            customMapHiddenReason.transform.parent.gameObject.SetActive(msg != null);

#endif

            if (change != 0)
            {
                int tryQuarter = 1;
                while (pe.HaveCollectedVial(pe.CurrentLevel(), lm.SelectedQuarter()) && tryQuarter <= 5)
                {
                    lm.SetSelectedQuarter(tryQuarter);
                    tryQuarter++;
                }

                foreach (ULoadQuarter ldq in loadQuarters) { ldq.LoadSnapshot(); }
            }
        }

        private static readonly Color NOT_SELECTED = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        private static readonly Color SELECTED = new Color(0.97f, 0.78f, 0f, 0.5f);
        public void LevelChangerSelected(bool selected)
        {
            levelChangerBackground.color = selected ? SELECTED : NOT_SELECTED;
        }

        // This is called by LevelManager catching GamePlay events
        public void LevelChangeRequested(int inc)
        {
            PlayerEnergy pe = game.Pe();
		
            if (inc == 1 && pe.CurrentLevelUpIsEnabled()) FireLevelChange(1);
            else if (inc == -1 && pe.CurrentLevelDownIsEnabled()) FireLevelChange(-1);
        }

        public void ActiveQuarterChanged()
        {
            foreach (ULoadQuarter ldq in loadQuarters)
            {
                ldq.SetColourAndSize();
            }
        }

        public void ActiveQuarterChangeDisabled(int opt)
        {
            if (opt == 0)
            {
                foreach (ULoadQuarter lq in loadQuarters) lq.FlagCannotLoad();
            }
            else if (opt > 0 && opt <= loadQuarters.Length) loadQuarters[opt - 1].FlagCannotLoad();
        }
        
        #region RVR
#if VROAD_RVR_RELEASE 

        // For the browser dimension height of 1064:
        // An initial location of 15,15 with zoom 3.25 gets NZ into the bottom right corner but just outside the red box
        // also visible but outside is Iceland, Finland, Alaska, Northern Russia and the southern tip of Chile
		
        private static double centreLat = 15;
        private static double centreLng = 15;
        private static double centreZoom = 3.25;

        public void ShowBrowser()
        {
            customBuildError = null;
            customBuildFailedGO.SetActive(false);
            
            levelDownButton.gameObject.SetActive(false); // cannot change levels while browser active
            levelUpButton.gameObject.SetActive(false);

            game.Gsm().MakeTransition(GameStateTransition.openBrowser);
            
        centreLat = 51.5;
        centreLng = 0;
        centreZoom = 16;
        
            string url = KFormat.Sprintf(SO.BROWSER_URL, centreLat, centreLng, centreZoom);
            browser.Url = url;
            
            RectTransform rect = browser.transform.parent.gameObject.GetComponent<RectTransform>();
            float w = Math.Min(Screen.width - 40, 1880);
            float h = Math.Min(Screen.height - 40, 1040);
            rect.sizeDelta = new Vector2(w, h);
            
            browserParent.SetActive(true);
        }
        
         // Whoops, didn't mean to open browser. This is called by X button at browser top-right
        public void BrowserClose()
        {
            game.Gsm().MakeTransition(GameStateTransition.cancelBrowser);

            browserParent.SetActive(false);
        }

        
        public void CustomMapName(string mapName)
        {
            customMapName = mapName;
                
            string[] parts = FilenameWrapper.PLAIN.CountryCitySuburb(customMapName);
            
            //string country = parts[0];
            string city = parts[1];
            string suburb = parts[2];
            //string hectares = parts[3];
            
            suburbTextField.SetTextWithoutNotify(suburb);
            cityTextField.SetTextWithoutNotify(city);

        }

         private static bool SuitableCharForFile(char c)
        {
            if (c == CC.SPACE || c == CC.MINUS) return true;
            if (c == CC.RNBL || c == CC.RNBR) return true;
            if (char.IsLetterOrDigit(c)) return true;

            return false;
        }

        private static string SuitableForFile(string s)
        {
            char[] sca = s.ToCharArray();
            StringBuilder sb = new StringBuilder();
            char prev = CC.SPACE;
            
            foreach (char sc in sca)
            {
                char tc = SuitableCharForFile(sc)? sc: CC.MINUS;

                if (prev != CC.MINUS || tc != CC.MINUS) { sb.Append(tc); prev = tc; }
            }

            return sb.ToString();
        }
        public void DoneEditing()
        {
            string[] parts = FilenameWrapper.PLAIN.CountryCitySuburb(customMapName);
            string country = parts[0];
            string city1 = parts[1];
            string suburb1 = parts[2];
            string hectares = parts[3];
            
            string city2 = SuitableForFile(cityTextField.text);
            string suburb2 = SuitableForFile(suburbTextField.text);

            cityTextField.SetTextWithoutNotify(city2);
            suburbTextField.SetTextWithoutNotify(suburb2);
            
            string stem1 = FilenameWrapper.FileStem(country, city1, suburb1, hectares);
            string stem2 = FilenameWrapper.FileStem(country, city2, suburb2, hectares);

            KDir dir = new KDir(KEnv.VroadWriteDir());
            KFile vroad1 = new KFile(dir, stem1 + SC.SUFFIX_DOT_VROAD);
            KFile vroad2 = new KFile(dir, stem2 + SC.SUFFIX_DOT_VROAD);

            string snapPrefix = LevelManager.CustomPrefix(game.Lm().CurrentLevel());
            KFile snap1 = new KFile(dir, snapPrefix + stem1 + SC.SUFFIX_DOT_PNG);
            KFile snap2 = new KFile(dir, snapPrefix + stem2 + SC.SUFFIX_DOT_PNG);

            KDir dirLegacy = new KDir(KEnv.VroadLegacyWriteDir());
            if (!vroad1.Exists()) vroad1 = new KFile(dirLegacy, stem1 + SC.SUFFIX_DOT_VROAD_LEGACY);
            if (!snap1.Exists()) snap1 = new KFile(dirLegacy, snapPrefix + stem1 + SC.SUFFIX_DOT_PNG);

            if (snap1.Exists() && vroad1.Exists())
            {
                snap1.MoveTo(snap2);
                vroad1.MoveTo(vroad2);

                customMapName = stem2;
                
                customMapLabel.text = FilenameWrapper.PLAIN.Display(stem2, false, false);
                
                game.Lm().RenameCustomMap(customMapName);
            }
            
            customMapLabel.gameObject.SetActive(true);
            customNameEditButton.gameObject.SetActive(true);
            
            suburbTextField.gameObject.SetActive(false);
            cityTextField.gameObject.SetActive(false);

            countdownEnableKeys = 50;
        }

       
        public void StartEditing()
        {
            if (!game.Lm().CustomQuarterSelected())
            {
                game.Lm().SetSelectedQuarter(LevelManager.CUSTOM_QUARTER);
                ActiveQuarterChanged();
            }

            UKeysRvr.IgnoreKeyInput(true); // otherwise WASD will select maps, space will load current map!
            
            customMapLabel.gameObject.SetActive(false);
            customNameEditButton.gameObject.SetActive(false);
            
            suburbTextField.gameObject.SetActive(true);
            cityTextField.gameObject.SetActive(true);
       }

        public static void UpdateLevelSelectorOnCloudScoreImport()
        {
            if (Instance() != null) InstanceQ().FireLevelChange(0);
        }


        public static void SetCustomBuildError(string s)
        {
            customBuildError = s;  // will be shown later
        }

        
        // This is called by Browser Callback
        public void BuildNewModel(string latLong)
        {
            browserParent.SetActive(false);
            loadingOverlay.SetActive(true);
            
            bool ok = BuildMapFromDownload(latLong);
            
            if (!ok) loadingOverlay.SetActive(false);
           
        }

        protected bool BuildMapFromDownload(string latLong)
        {
            string[] parts = KTools.SplitQuick(latLong, CC.COMMA);

            if (parts.Length < 2) // latLong spec is invalid
            { 
                // No change of game state required, still waiting for map choice
                customMapFailedMessage.text = Babyl.Translation(BabylKey.CustomBuildFailedText);
                customBuildFailedGO.SetActive(true);

                return false;
            }

            // We could create this here to help with setting a zoom level
            // but another will be created in MapBuilder constructor
            // if you comment this in, make sure the using statements are inside #if
            // IBoundsBox bbox = OsmBoundsBox.CreateFromLatLong(latLong);
            
            if (parts.Length >= 4)
            {
                double latMin = KTools.ParseDoubleX0(parts[0]);
                double lonMin = KTools.ParseDoubleX0(parts[1]);
                double latMax = KTools.ParseDoubleX0(parts[2]);
                double lonMax = KTools.ParseDoubleX0(parts[3]);

                // cache the centre of the bounds box to be used when next launching the browser
                centreLat = 0.5 * (latMin + latMax);
                centreLng = 0.5 * (lonMin + lonMax);
                centreZoom = 16; // To-DO calculate zoom level from bbox size
            }

            // *Before* starting the build, change the scene synchronously (in this thread), register all the listeners
            App().DeleteEventConsumers();  
            SceneManager.LoadScene(PlaySimSceneName());

            Reporter.ProgressPartsUI(4);
            
            // The MapBuilder constructor will MTop.Build and started the build process in another thread.
            MapBuilder.BuildMapFromDownload(game, latLong);

            return true;
        }
        
        private string PlaySimSceneName()  { return SG.NAVIGATION_SCENE; }

#else
        public void ShowBrowser() { }
        public void BrowserClose() { }
        public void CustomMapName(string mapName) { }

        public static void SetCustomBuildError(string s) { }
#endif
        #endregion
    }
}
