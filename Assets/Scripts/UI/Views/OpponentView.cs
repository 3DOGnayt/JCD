using System.Collections.Generic;
using KoboldUi.Element.View;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Views
{
    public class OpponentView : AUiAnimatedView
    {
        public List<Button> OpponentButtons;
        public Image OpponentPresentation;
        public Image OpponentDifficulty;
        [Space]
        public Button ConfirmButton;
        public Button BackButton;
    }
}