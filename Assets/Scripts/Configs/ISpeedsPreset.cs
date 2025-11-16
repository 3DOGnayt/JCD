using System.Collections.Generic;
using Configs.Helpers;

namespace Configs
{
    public interface ISpeedsPreset
    {
        List<CarSpeedSettings> CarSpeedSettings { get; }
    }
}