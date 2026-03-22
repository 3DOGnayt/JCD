using System;
using Configs.Impl;
using UnityEngine;

namespace Data.Struct
{
    [Serializable]
    public struct CarCatalogEntry
    {
        public string DisplayName;
        public CarPresetParameters Preset;
        public CarMovementParameters MovementParameters;
        public CarSpeedsPresetParameters SpeedsPresetParameters;
        public CarSlipParameters SlipParameters;
        public CarEngineAudioParameters EngineAudioParameters;
        public Sprite Preview;
    }
}