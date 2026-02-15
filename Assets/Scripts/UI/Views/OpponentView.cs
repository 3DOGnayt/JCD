using System.Collections.Generic;
using KoboldUi.Element.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Views
{
    public class OpponentView : AUiAnimatedView
    {
        public List<Button> OpponentButtons;
        public List<TMP_Text> OpponentButtonsText;
        public Image OpponentPresentation;
        public Image OpponentDifficulty;
        [Space]
        public Button ConfirmButton;
        public Button BackButton;
    }
}