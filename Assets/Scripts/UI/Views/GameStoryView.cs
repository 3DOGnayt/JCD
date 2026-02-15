using KoboldUi.Element.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Views
{
    public class GameStoryView : AUiAnimatedView
    {
        public TMP_Text AverageText;
        [Space]
        public Image OpponentImage;
        [Space]
        public TMP_Text OpponentNameAndCar;
        public TMP_Text PlayerNameAndCar;
    }
}