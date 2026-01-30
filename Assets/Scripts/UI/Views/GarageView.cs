using System.Collections.Generic;
using KoboldUi.Element.View;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Views
{
    public class GarageView : AUiAnimatedView
    {
        public List<Button> CarButtons;
        public Image CarPresentation;
        [Space]
        public Button ConfirmButton;
        public Button BackButton;
    }
}