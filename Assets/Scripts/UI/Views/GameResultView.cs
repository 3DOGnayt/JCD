using System.Collections.Generic;
using DG.Tweening;
using KoboldUi.Element.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Views
{
    public class GameResultView : AUiAnimatedView
    {
        public Image Win;
        public Image Lose;
        [Space] 
        public GameObject ResultButtons;
        public Button Retry;
        public Button MainMenu;
        [Space]
        public GameObject TimePanel;
        public TMP_Text TotalTime;
        public List<TMP_Text> SelectionTimeList;
        [Space]
        public float WinMoveUpDistance = 120f;
        public float WinMoveUpDuration = 0.35f;
        public float DelayBeforeMoveResult = 1f;
        public float DelayBeforeShowResults = 0.4f;
        [Space]
        public Ease MoveEase;
    }
}