using uk.vroad.api;
using uk.vroad.api.input;
using uk.vroad.rvr;
using UnityEngine;
using UnityEngine.UI;

namespace uk.vroad.urvr
{
    public class UAutoResume : MonoBehaviour
    {
        public Slider autoResumeSlider;

        private bool visible;
        private float autoResumeAt;
        private float autoResumeDuration;

        private Game game;
        void Awake()
        {
            game = Game.AwakeInstance();
        }
        public void AutoResumeIn(int seconds)
        {
            autoResumeSlider.transform.parent.gameObject.SetActive(true);
            visible = true;
            autoResumeDuration = seconds;
            autoResumeAt = Time.unscaledTime + autoResumeDuration;

            autoResumeSlider.value = 0;
         }

      
        void Update()
        {
            if (visible)
            {
                AppState state = game.Gsm().CurrentState();
                
                if (state == AppState.MenuAtMapChoice || state == GameState.MenuComplete || 
                    state == GameState.PausedWhileNavigating || state == GameState.PausedAtTripChoice)
                {
                    float now = Time.unscaledTime;
                    float remaining = autoResumeAt - now;

                    if (remaining < 0)
                    {
                        autoResumeSlider.transform.parent.gameObject.SetActive(false);
                        visible = false;

                        game.Ew().FireAppDigitalEvent(AppDigitalFn.MenuResume, true);
                    }
                    else 
                    {
                        float val = (autoResumeDuration - remaining) / autoResumeDuration;
                        autoResumeSlider.value = val;
                    }
                }
                else
                {
                    autoResumeSlider.transform.parent.gameObject.SetActive(false); // 20211023
                    visible = false;
                }
            }
        }
    }
}