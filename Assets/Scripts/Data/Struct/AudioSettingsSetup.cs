using System;
using Data.Enums;

namespace Data.Struct
{
    [Serializable]
    public struct AudioSettingsSetup
    {
        public EAudioType AudioType;
        public AudioSettings AudioSettings;
    }
}