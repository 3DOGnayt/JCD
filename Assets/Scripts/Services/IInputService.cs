using System.Collections.Generic;
using Data.HelperClass;

namespace Services
{
    public interface IInputService
    {
        void ApplyHorizontalMove(float currentSteeringAngle, float steeringSpeed, List<WheelInfoSetup> wheelInfos);
        void ApplyVerticalMove(float currentMotorTorque, float verticalDirection, List<WheelInfoSetup> wheelInfos);
    }
}