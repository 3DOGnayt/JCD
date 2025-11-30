using System.Collections.Generic;
using Data;

namespace Services
{
    public interface IInputService
    {
        void ApplyHorizontalMove(float currentSteeringAngle, float steeringSpeed, List<WheelInfo> wheelInfos);
        void ApplyVerticalMove(float currentMotorTorque, float verticalDirection, List<WheelInfo> wheelInfos);
    }
}