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

namespace uk.vroad.urvr
{
     public class UProgressBarWithImage : MonoBehaviour, LAppState
     {
        public Image imageCityPhoto;
        public Sprite fallbackPhoto;
        public Text imageCityCredit;
        public UAutoResume autoResume;
        
        private Game game;
        private GameObject helpPanel;
        private Slider slider;
        private bool asleep;
        private bool hideLater;
        private int currentValue;

        private App App() { return game; }
        public bool DeregisterFireMapChange() { return true;}

        void Awake()  // The Progress Bar must be enabled in the inspector for Awake to be called
        {
            game = Game.AwakeInstance();
            App().AddEventConsumer(this);
            slider = GetComponent<Slider>();
           
            helpPanel = imageCityPhoto.gameObject.transform.parent.gameObject;
            helpPanel.SetActive(true);  // for the background mask
            UaMenu.HelpPanelForceVisible(true);

            imageCityPhoto.gameObject.SetActive(true);
            imageCityPhoto.sprite = fallbackPhoto;
            imageCityCredit.text = SC.N;

            AudioSource audioSrc = GetComponent<AudioSource>();
            audioSrc.volume = UAudioController.MasterVolume();
        
            int quarter = game.Pe().CurrentQuarter();
            KImage kImage = game.Lm().Snapshot(quarter);

            if (kImage != null)
            {
                Texture2D texture = new Texture2D(2, 2);
                byte[] imageBytes = kImage.AsByteArray();
                texture.LoadImage(imageBytes);

                var tw = texture.width;
                var th = texture.height;
                imageCityPhoto.sprite = Sprite.Create(texture, new Rect(0, 0, tw, th), new Vector2(0, 0));

                imageCityCredit.text = game.Lm().Credit(quarter);;
            }
        }
        void Update()
        {
            if (asleep) return;

            if (hideLater) HideAndSleep();
        }
        void FixedUpdate()
        {
            if (!asleep)
            {
                int progress = 100 * Reporter.ProgressTotal();
                if (progress > currentValue)
                {
                    int diff = progress - currentValue;
                    int inc = diff > 2500? 40: diff> 1000? 20: 10;
                    
                    currentValue += inc;
                    slider.value = currentValue;

                    if (currentValue > 9900)
                    {
                        hideLater = true;
                        progress = 100 * Reporter.ProgressTotal();
                    }
                        
                }
            }
        }

        void HideAndSleep()
        {
            slider.value = 0;
            gameObject.SetActive(false); 
            asleep = true;
            
            UaMenu.HelpPanelForceVisible(false);

            helpPanel.SetActive(false);
            imageCityPhoto.gameObject.SetActive(false);

            bool showTripHelpOnFirstOpen = KPrefs.GetInt(SF.PREFS_HIDE_INIT_HELP, 0) <= 1;
            if (showTripHelpOnFirstOpen) // && Gamepad.current != null)
            {
                KPrefs.SetInt(SF.PREFS_HIDE_INIT_HELP, 2);

                game.Ew().FireAppDigitalEvent(AppDigitalFn.Pause, true);
                
                autoResume.AutoResumeIn(10);
            }
        }
        
        public void AppStateChanged(AppStateTransition ast)
        {
            if (ast == AppStateTransition.abortedBuildEarly ||
                ast == AppStateTransition.abortedBuildLate ||
                ast == AppStateTransition.mapLoadFailed ||
                ast.after == AppState.ReadyToSimulate)
            {
                hideLater = true;
            }
        }

    }
}
