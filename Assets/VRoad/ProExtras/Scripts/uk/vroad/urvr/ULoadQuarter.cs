using uk.vroad.api;
using uk.vroad.api.etc;
using uk.vroad.api.events;
using uk.vroad.api.input;
using uk.vroad.api.str;
using uk.vroad.apk;
using uk.vroad.rvr;
using uk.vroad.ucm;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace uk.vroad.urvr
{
    // There is one instance of this attached to each of the buttons
    //
    // The 'quarterIndex' value is combined with LevelManager.currentLevel to get a model
    // See LevelManager.modelName(quarterIndex)
    
    
    public class ULoadQuarter : MonoBehaviour, LAppState, LAppInput
    {
        private static readonly Color colorSelected = new Color(0.95f, 0.8f, 0.05f, 1.0f);
        private static readonly Color colorNotSelected = new Color(0.34f, 0.69f, 0.89f, 1.0f);
        private static readonly Color colorNotActive = new Color(0.4f, 0.4f, 0.04f, 0.5f);

        private static float SIZE_SELECTED = 1.2f;

        public int quarterIndex;
        public GameObject loadingOverlay;  // must be set for all 5
        public GameObject vialIcon;
        public Sprite fallbackImage;

        private Game game;
        protected LevelManager lm;
        private float vialSizeCount;
        private RectTransform vialRect;
        private Image panelImage;
        private bool panelImageSet;

        private bool loadMapLater;
       
        private App App() { return game; }
        public bool DeregisterFireMapChange() { return true; }

        protected virtual void Awake()
        {
            game = Game.AwakeInstance();
            App().AddEventConsumer(this);
            
            lm = game.Lm();
            
            vialRect = vialIcon.GetComponent<RectTransform>();
            
            GameObject panel = gameObject.transform.parent.gameObject;
            panelImage = panel.GetComponent<Image>();
            panelImageSet = true;
        }

        protected virtual void Start()
        {
            vialIcon.SetActive(false);
            
            LoadSnapshot();
        }

        protected virtual void FixedUpdate()
        {
            if (!vialIcon.activeSelf)
            {
                vialSizeCount = 0;
                return;
            }

            if (vialSizeCount > 199.99) return;
            if (vialSizeCount < 0) vialSizeCount = 0;

            vialSizeCount += 1f;
            if (vialSizeCount < 50) vialSizeCount = 50; // decrease this to increase the static pause
            if (vialSizeCount > 199.99) vialSizeCount = 200;

            float scale = vialSizeCount < 100 ? 100f : (200f - vialSizeCount);

            float scw = Screen.width - 20;
            
            float size = 1f + (scale * 0.012f * scw * 0.001f);
            vialRect.localScale = new Vector3(size, size, 1);

            float sw = 0.01f * ((0.14f * scw) - 25f);
            float sh = sw * (9f / 16f);
            
            float x = 25f + (sw * scale);
            float y = -40f - (sh * scale);
            vialRect.anchoredPosition = new Vector2(x, y);
            vialRect.transform.eulerAngles = new Vector3(0, 0, scale * 3.60f);
            
            // When you read the .eulerAngles property, Unity converts the Quaternion's internal representation of the
            // rotation to Euler angles. Because, there is more than one way to represent any given rotation using
            // Euler angles, the values you read back out may be quite different from the values you assigned.
            // This can cause confusion if you are trying to gradually increment the values to produce animation. 
            //
            // Thus it is better to set the euler angles from a stored or calculated vector on each frame,
            // rather than to try to read the current euler angles and write back a modified version
        }
        
        
        protected virtual void LateUpdate() 
        {
            if (quarterIndex != lm.SelectedQuarter()) return; // should not be here unless they match

          
            if (!LoadMapLater()) return;
            loadMapLater = false;

            SceneManager.LoadScene(PlaySimSceneName());
            App().DeleteEventConsumers();
            
#if VROAD_RVR_RELEASE
            Reporter.ProgressPartsUI(3);
#endif
            LoadMapNow();
        }

        protected void SetLoadMapLater() { loadMapLater = true; }
        protected bool LoadMapLater() { return loadMapLater; } 

        protected virtual bool QuarterMapExists()
        {
            return true;
        }
        protected virtual bool CanLoadQuarter()
        {
            // Sometimes, a quarter can be marked as the selected one, even  though it is not available
            return ! lm.HaveCollectedVial(lm.SelectedQuarter());
        }
        protected virtual void TryBuildMap() {}
        
        public virtual void OnButtonPressQuarter()
        {
            if (quarterIndex == lm.SelectedQuarter())
            {
                if (QuarterMapExists())
                {
                    if (CanLoadQuarter())
                    {
                        loadingOverlay.SetActive(true);
                        SetLoadMapLater();
                    }
                }
                else TryBuildMap();
            }
            else
            {
                lm.SetSelectedQuarter(quarterIndex);
                UChooseQuarter.InstanceQ().ActiveQuarterChanged();
            }
        }

        protected virtual void LoadMapNow()
        {
            lm.OpenSelectedQuarter();
        }

        protected virtual string PlaySimSceneName()  { return SG.NAVIGATION_SCENE; }

        public virtual void LoadSnapshot()
        {
            Image image = GetComponent<Image>();
            KImage kImage = lm.Snapshot(quarterIndex);
            Sprite cityPhotoSprite;

            if (kImage == null)
            {
                cityPhotoSprite = fallbackImage;
            }
            else
            {
                Texture2D texture = new Texture2D(2, 2);
                byte[] imageBytes = kImage.AsByteArray();
                texture.LoadImage(imageBytes);

                var tw = texture.width;
                var th = texture.height;
                cityPhotoSprite = Sprite.Create(texture, new Rect(0, 0, tw, th), new Vector2(0, 0));
            }

            image.type = Image.Type.Simple; // otherwise it defaults to sliced, with warning 'no border'
            image.sprite = cityPhotoSprite;

            string mapName = lm.MapName(quarterIndex);

            Text label = GetComponentInChildren<Text>();

            if (mapName == null) label.text = Babyl.Translation(BabylKey.CustomCity);
            else
            {
                string[] ccsa = FilenameWrapper.PLAIN.CountryCitySuburb(mapName);
                string cityName = ccsa[1];

                string key = KFormat.Sprintf(SG.QMAP, lm.CurrentLevel(), quarterIndex);
                if (Babyl.TranslationExists(key)) cityName = Babyl.Translation(key);
                
                label.text = cityName;

                if (quarterIndex == LevelManager.CUSTOM_QUARTER)
                {
                    UChooseQuarter.InstanceQ().CustomMapName(mapName);
                }
            }

            SetColourAndSize();

        }

        /* Unity recommends a scene to be loaded using a co-routine, but if we do this we need to wait until
       the co-routine completes before opening our data file. The technique of using the event 
           asyncLoad.completed += OnPlayQuarterSceneLoaded;
       appears to work, until we go back to this scene on the next level. We then get an error saying that
       'the object of type LoadQuarter has been destroyed but you are still truing to access it'
       As explained in the forum post, this is (possibly) because there is a reference left over from the 
       event delegate.
       
       Anyway, because our scene is so lightweight, it turns out that using the direct synchronous call to 
       SceneManager.LoadScene() works just fine, with no noticeable delay, so let's do this instead.
       
   
           public void PlayThisQuarter___ASync()
           {
               if (!quarterSceneActive) return;
               quarterSceneActive = false;
   
               StartCoroutine(LoadScenePlayQuarter());
           }
   
           IEnumerator LoadScenePlayQuarter()
           {
               AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SG.PlayQuarter);
               asyncLoad.completed += OnPlayQuarterSceneLoaded;
               
               // Wait until the asynchronous scene fully loads
               while (!asyncLoad.isDone) yield return null;
           }
   
           private void OnPlayQuarterSceneLoaded(AsyncOperation obj)
           {
               // See https://forum.unity.com/threads/the-object-of-type-x-has-been-destroyed-but-you-are-still-trying-to-access-it.454891/
   
               OpenQuarterFile();
           }
   
           //*/

        public void AppStateChanged(AppStateTransition ast)
        {
            if (ast.after == AppState.WaitingForMapChoice)
            {
                SetColourAndSize();
            }
        }

        public virtual void SetColourAndSize()
        {
            SetColourAndSize(quarterIndex == lm.SelectedQuarter());
        }
        private void SetColourAndSize(bool thisQuarterSelected)
        {
            if (!panelImageSet)  return; // this can be called before Awake / Start?
            
            Vector3 rtScale = new Vector3(1, 1, 1);
           

            if (lm.HaveCollectedVial(quarterIndex))
            {
                panelImage.color = colorNotActive;
                vialIcon.SetActive(true);
            }
            else if (thisQuarterSelected)
            {
                panelImage.color = colorSelected;
                panelImage.gameObject.transform.SetAsLastSibling();

                if (! lm.IsLevelChangerSelected())
                {
                    rtScale.x = SIZE_SELECTED;
                    rtScale.y = SIZE_SELECTED;
                }

                vialIcon.SetActive(false);
            }
            else
            {
                panelImage.color = colorNotSelected;
                vialIcon.SetActive(false);
            }

            RectTransform rt = panelImage.gameObject.GetComponent<RectTransform>();
            rt.localScale = rtScale;
            

        }
        public bool AppInputDigitalEvent(AppDigitalFn fn, bool isPress)
        {
            // Level Manager sets up selected quarter on press, and it may be called before or after this object
            
            if (fn == GameDigitalFn.CitySelect)
            {
                if (lm.ValidSelectedQuarter() && quarterIndex == lm.SelectedQuarter() && isPress)
                {
                    OnButtonPressQuarter();
                }

                return true;
            }
           
            return false;
        }

        public bool AppInputAnalogEvent(AppAnalogFn afn, double value)
        {
            return false;
        }

        public void FlagCannotLoad()
        {
            vialSizeCount = 0; // re-start animation
        }

    }
}
