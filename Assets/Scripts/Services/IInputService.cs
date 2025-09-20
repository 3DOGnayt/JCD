using System.Collections.Generic;
using Data;

namespace Services
{
    public interface IInputService
    {
        void ApplyMove(float maxMotorTorque, float maxSteeringAngle, List<WheelInfo> wheelInfos);
    }
}