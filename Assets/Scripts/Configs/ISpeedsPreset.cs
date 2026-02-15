using System.Collections.Generic;
using Data.HelperClass;

namespace Configs
{
    public interface ISpeedsPreset
    {
        List<CarSpeedSetup> CarSpeedSettings { get; }
    }
}