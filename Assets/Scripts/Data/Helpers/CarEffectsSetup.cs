using System;
using UnityEngine;

namespace Data.Helpers
{
    [Serializable]
    public class CarEffectsSetup
    {
        public TrailRenderer[] TracesWheels; // TODO: plan B for skidmarks
        [Space]
        public CarLightsSetup carLightsSetup;
    }
}