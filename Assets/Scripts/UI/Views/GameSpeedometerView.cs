using KoboldUi.Element.View;
using TMPro;
using UnityEngine.UI;

namespace UI.Views
{
    public class GameSpeedometerView : AUiAnimatedView
    {
        public TMP_Text SpeedText;
        public TMP_Text GearText;
        public Image RpmFill;
        public float RpmMaxFill = 0.8f;
    }
}