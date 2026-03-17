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
        public CarParameters Parameters;
        public CarEngineAudioParameters EngineAudioParameters;
        public Sprite Preview;
    }
}