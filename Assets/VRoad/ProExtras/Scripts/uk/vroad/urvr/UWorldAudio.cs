using uk.vroad.api.input;
using uk.vroad.rvr;
using UnityEngine;
using UnityEngine.UI;

// Depends on External Asset SpinFree spinning Earth
namespace uk.vroad.urvr
{
    public class UWorldAudio : MonoBehaviour, LAppInput
    {
        public Slider volumeSlider;
        private AudioSource earthAudioSource;

        void Awake()
        {
            Game game = Game.AwakeInstance();
            game.AddEventConsumer(this);
            earthAudioSource = gameObject.GetComponent<AudioSource>();
            SetEarthVolume(true);
        }
        public bool DeregisterFireMapChange()  { return true; }
        
        public bool AppInputAnalogEvent(AppAnalogFn afn, double value)  { return false; }
        
        public bool AppInputDigitalEvent(AppDigitalFn fn, bool isPressed)
        {
            bool changed = UAudioController.ChangeMasterVolume(fn, isPressed);

            if (changed) SetEarthVolume(true);

            return changed;
        }
        
        public void OnSliderValueChanged(float value)
        {
            UAudioController.SetMasterVolume(value);
            SetEarthVolume(false);
        }
        private void SetEarthVolume(bool updateSlider)
        {
            if (!gameObject.activeSelf) return;
            
            float v = UAudioController.MasterVolume();
            earthAudioSource.volume = v;
            if (updateSlider) volumeSlider.value = v;
        }
    }
}
