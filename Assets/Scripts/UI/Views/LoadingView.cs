using KoboldUi.Element.View;
using TMPro;
using UnityEngine;

namespace UI.Views
{
    public class LoadingView : AUiAnimatedView
    {
        public TMP_Text LoadingText;
        public float FakeLoadingDurationSeconds = 3.5f;
        public AnimationCurve LoadingCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    }
}