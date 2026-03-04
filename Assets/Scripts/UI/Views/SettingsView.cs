using KoboldUi.Element.View;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Views
{
    public class SettingsView : AUiAnimatedView
    {
        public Slider MasterVolume;
        public Slider SfxVolume;
        public Slider MusicVolume;
        public Slider UiVolume;
        [Space]
        public Button CloseButton;
    }
}