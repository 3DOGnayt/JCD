using Data.HelperClass;

namespace Configs
{
    public interface ICarMovementParameters
    {
        CarMovementHorizontalSetup Horizontal { get; }
        CarMovementVerticalSetup Vertical { get; }
        CarMovementArcadeAssistSetup ArcadeAssist { get; }
        SystemHelpersSetup HelpersSetup { get; }
        CarMovementAirControlSetup AirControl { get; }
        CarMovementVelocityAlignSetup VelocityAlign { get; }
    }
}