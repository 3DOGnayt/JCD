using System.Collections.Generic;
using KoboldUi.Element.View;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Views
{
    public class MapView : AUiAnimatedView
    {
        public List<Button> MapButtons;
        public Image MapPresentation;
        [Space]
        public Button ConfirmButton;
        public Button BackButton;
    }
}