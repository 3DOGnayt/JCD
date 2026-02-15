using KoboldUi.Element.View;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Views
{
    public class GameModView : AUiAnimatedView
    {
        public Button TrainingButton;
        public Button StoryButton;
        public Image GameModPresentation;
        [Space]
        public Button ConfirmButton;
        public Button BackButton;
        [Space]
        public float PresentationDelay;
    }
}