using System.Collections.Generic;
using Data.Helpers;

namespace Configs
{
    public interface ISpeedsPreset
    {
        List<CarSpeedSettings> CarSpeedSettings { get; }
    }
}