using System.Collections.Generic;
using KoboldUi.Element.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Views
{
    public class MapView : AUiAnimatedView
    {
        public List<Button> MapButtons;
        public List<TMP_Text> MapButtonsText;
        public Image MapPresentation;
        public TMP_Dropdown MusicList;
        [Space]
        public Button ConfirmButton;
        public Button BackButton;
        [Space]
        public float PresentationDelay;
    }
}