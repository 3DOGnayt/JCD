using System.Collections.Generic;
using Data;

namespace Services
{
    public interface IInputService
    {
        void ApplySpeed_Test(CarSetup carSetup, List<WheelInfo> wheelInfos);
    }
}