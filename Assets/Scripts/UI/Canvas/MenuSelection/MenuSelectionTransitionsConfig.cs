using UnityEngine;

namespace UI.Canvas.MenuSelection
{
    [CreateAssetMenu(menuName = "UI/Menu Selection/Transitions Config", fileName = "MenuSelectionTransitionsConfig")]
    public class MenuSelectionTransitionsConfig : ScriptableObject
    {
        [SerializeField] private float _panelSlideDuration = 0.25f;
        [SerializeField] private AnimationCurve _panelSlideEase;
        [SerializeField] private float _menuLeftShift = 450f;
        [SerializeField] private float _mapSlideOffset = 600f;
        [SerializeField] private float _carSlideOffset = 1100f;
        [SerializeField] private float _raceSlideOffset = 600f;
        [SerializeField] private float _opponentSlideOffset = 600f;
        [SerializeField] private float _opponentPanelDelay = 0.25f;
        [SerializeField] private float _racePanelDelay = 0.25f;

        public float PanelSlideDuration => _panelSlideDuration;
        public AnimationCurve PanelSlideEase => _panelSlideEase;
        public float MenuLeftShift => _menuLeftShift;
        public float MapSlideOffset => _mapSlideOffset;
        public float CarSlideOffset => _carSlideOffset;
        public float RaceSlideOffset => _raceSlideOffset;
        public float OpponentSlideOffset => _opponentSlideOffset;
        public float OpponentPanelDelay => _opponentPanelDelay;
        public float RacePanelDelay => _racePanelDelay;
    }
}
