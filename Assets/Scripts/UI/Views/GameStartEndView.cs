using System.Collections.Generic;
using KoboldUi.Element.View;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Views
{
    public class GameStartEndView : AUiAnimatedView
    {
        public List<Image> СountdownList;
        public float CountdownStartDelaySeconds = 0.5f;
        [Space]
        public List<float> CountdownHoldSeconds = new();
        public float CountdownHoldSecondsDefault = 0.6f;
        public float CountdownFadeInSeconds = 0.2f;
        public float CountdownFadeOutSeconds = 0.2f;
        public float WinFadeInSeconds = 0.2f;
        public float ResultHoldSeconds = 1.2f;
        [Space]
        public Image Win;
        public Image Lose;
    }
}