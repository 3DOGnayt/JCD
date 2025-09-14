using System.Collections.Generic;
using Data;

namespace Services
{
    public interface IInputService
    {
        void ApplySpeed_PreFin(float maxMotorTorque, float maxSteeringAngle, List<WheelInfo> wheelInfos);
    }
}