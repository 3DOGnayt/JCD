using Data.HelperClass;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/" + nameof(CarMovementParameters), fileName = nameof(CarMovementParameters))]
    public class CarMovementParameters : ScriptableObject, ICarMovementParameters
    {
        [Header("Horizontal Parameters")]
        [SerializeField] private CarMovementHorizontalSetup _horizontal;
        [Header("Vertical Parameters")]
        [SerializeField] private CarMovementVerticalSetup _vertical;
        [Header("Arcade Assist")]
        [SerializeField] private CarMovementArcadeAssistSetup _arcadeAssist;
        [Header("Touge Hybrid Control")]
        [SerializeField] private CarMovementTougeSetup _touge = new CarMovementTougeSetup();
        [Header("System Helpers")]
        [SerializeField] private SystemHelpersSetup _helpersSetup;
        [Header("Air Control")]
        [SerializeField] private CarMovementAirControlSetup _airControl;
        [Header("Velocity Align")]
        [SerializeField] private CarMovementVelocityAlignSetup _velocityAlign;

        public CarMovementHorizontalSetup Horizontal => _horizontal;
        public CarMovementVerticalSetup Vertical => _vertical;
        public CarMovementArcadeAssistSetup ArcadeAssist => _arcadeAssist;
        public CarMovementTougeSetup Touge => _touge;
        public SystemHelpersSetup HelpersSetup => _helpersSetup;
        public CarMovementAirControlSetup AirControl => _airControl;
        public CarMovementVelocityAlignSetup VelocityAlign => _velocityAlign;
    }
}
