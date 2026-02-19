using System;
using System.Collections.Generic;
using Data.Enums;

namespace Data.Struct
{
    [Serializable]
    public struct AudioSettingsSetup
    {
        public EAudioType AudioType;
        public List<AudioSettingsEntry> AudioSettingsEntry;
    }
}