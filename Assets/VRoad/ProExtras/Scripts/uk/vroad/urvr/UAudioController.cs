using System;
using uk.vroad.api;
using uk.vroad.api.etc;
using uk.vroad.api.input;
using uk.vroad.api.str;
using uk.vroad.apk;
using uk.vroad.rvr;
using uk.vroad.ucm;
using UnityEngine;
using UnityEngine.UI;

namespace uk.vroad.urvr
{
    // Other audio sources in:
    //           ChooseQuarter/EarthAudio (music for spinning earth)
    //           ProgressBar (music while loading map)
    public class UAudioController : MonoBehaviour, LUAppState, LAppInput
    {
        private const float DELTA_VOLUME = 0.10f;
        private static bool volUpPressed;
        private static bool volDnPressed;
        private static float beforeMute;
        private static float masterVolume = -1f; // force read from KPrefs on first use
        private static float beatMultiplier = 1.0f;

        public static float MasterVolume()
        {
            if (masterVolume < 0)
            {
                int savedVolume = KPrefs.GetInt(SF.PREFS_VOLUME, 50);
                masterVolume = 0.01f * savedVolume;
            }
            return masterVolume;
        }
        public static void SetMasterVolume(float v)
        {
            if (v < 0) v = 0;
            else if (v > 1f) v = 1f;

            if (Math.Abs(v - masterVolume) > 0.01f)
            {
                masterVolume = v;

                KPrefs.SetInt(SF.PREFS_VOLUME, (int) Math.Round(v * 100f));
                KPrefs.Save();
            }
        }
        public static bool ChangeMasterVolume(AppDigitalFn fn, bool isPressed)
        {
            if (fn == GameDigitalFn.VolumeUp) volUpPressed = isPressed;
            if (fn == GameDigitalFn.VolumeDown) volDnPressed = isPressed;
            
            float v = MasterVolume();
            bool isMuted = v == 0 && beforeMute != 0;
            bool bothPressed = volUpPressed && volDnPressed;

            if (bothPressed || (isPressed && fn == GameDigitalFn.VolumeMute))
            {
                if (isMuted) SetMasterVolume(beforeMute);

                else if (v != 0) { beforeMute = v; SetMasterVolume(0); }

                return true;
            }
            
            if (isPressed && fn == GameDigitalFn.VolumeUp)
            {
                SetMasterVolume(v + DELTA_VOLUME);
                return true;
            }
            
            if (isPressed && fn == GameDigitalFn.VolumeDown) 
            {
                if (isPressed) SetMasterVolume(v - DELTA_VOLUME);
                return true;
            }

            return false;
        }
       
        public static float BeatMultiplier()
        {
            return beatMultiplier;
        }

        
        public Slider volumeSlider;
        public AudioClip[] tracks;
        public AudioClip gameOverClip;
        public Text audioTrackName;
        
        private Game game;
        private AudioSource src;
        
        private void Awake()
        {
            game = Game.AwakeInstance();
            game.AddEventConsumer(this);
            src = GetComponent<AudioSource>();
        }
        
        
        void Start()
        {
            UaStateHandler.MostRecentInstance.AddListener(this);

            SetMainVolume(true);
            
#if UNITY_EDITOR
            string trk = src.clip.name;
            string[] trkParts = trk.Split('@');
            audioTrackName.text = trkParts[0];
#endif
        }

        public bool DeregisterFireMapChange() { return true; }
      
        // UPlayerEnergy has a reference to this object, and controls 
        public void UAppStateChanged(AppStateTransition ast)
        {
            if (ast.after == GameState.PausedWhileNavigating || ast.after == GameState.PausedAtTripChoice)
            {
                src.Pause();
                return;
            }
            
            // Resume the music if play has already started. 
            if (ast == GameStateTransition.resumeToNavigating)
            {
                src.Play();
                return;
            }

            // Play the music during trip choice if there is no countdown tick-tock clock
            if (game.Ptc().CountdownInitial() == 0)
            {
                if (ast == GameStateTransition.startSimulation)
                {
                    src.Play();
                    return;
                }
            }
            
            // Change tunes when we have a new destination in the same city
            if (ast == GameStateTransition.arrivalAtTargetContinueThisMap )
            {
                int nTracks = tracks.Length;

                int rti = Rng.NextInt(Rng.Vein.BONUS, nTracks);
                AudioClip clip = tracks[rti];
                src.clip = clip;

                // Animation Idle_Generic has two bobs of the head in 4.2 seconds
                // => 28.5 BPM if running at real time
                // normally 4x real time in game, hence 4 x 28.5 = 114 BPM

                const double bpmStd = 114;
                double bpm = bpmStd;

                string trk = clip.name;
                string[] trkParts = trk.Split('@');

                if (trkParts.Length >= 2)
                {
                    try
                    {
                        bpm = Int32.Parse(trkParts[1]);
                    }
                    catch (Exception)
                    {
                        bpm = bpmStd;
                    }

                    trk = trkParts[0];
                }

                beatMultiplier = (float) (bpm / bpmStd);

#if UNITY_EDITOR
                audioTrackName.text = trk;
#endif
            }

           
            if (ast.after == GameState.GameOver)
            {
                src.Stop();
                
                src.PlayOneShot(gameOverClip, 1.0f);

            }
        }

        // public bool AppInputDigitalEvent(AppDigitalFn fn, bool isOn) 
        // public bool AppInputAnalogEvent(AppAnalogFn afn, double value)

        public bool  AppInputDigitalEvent(AppDigitalFn fn, bool isPressed)
        {
            bool changed = ChangeMasterVolume(fn, isPressed);
            
            if (changed) SetMainVolume(true);

            return changed;
        }

        public bool AppInputAnalogEvent(AppAnalogFn afn, double value)
        {
            return false;
        }

        
        public void OnSliderValueChanged(float value)
        {
            SetMasterVolume(value);
            SetMainVolume(false);
        }
        private void SetMainVolume(bool updateSlider)
        {
            float v = MasterVolume();
            src.volume = v;
            if (updateSlider) volumeSlider.value = v;
        }

    }
}
