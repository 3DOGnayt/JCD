using System.Collections.Generic;
using KoboldUi.Element.View;
using UnityEngine.UI;

namespace UI.Views
{
    public class GameStartEndView : AUiAnimatedView
    {
        public List<Image> СountdownList;
        public float CountdownStartDelaySeconds = 0.5f;
        public List<float> CountdownHoldSeconds = new();
        public float CountdownHoldSecondsDefault = 0.6f;
        public float CountdownFadeInSeconds = 0.2f;
        public float CountdownFadeOutSeconds = 0.2f;
        public float WinFadeInSeconds = 0.2f;
        public Image Win;
        public Image Lose;
    }
}