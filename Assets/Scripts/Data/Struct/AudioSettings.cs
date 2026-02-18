using System;
using Data.Enums;
using UnityEngine;

namespace Data.Struct
{
    [Serializable]
    public struct AudioSettings
    {
        public EAudioSubType AudioSubType;
        public AudioClip AudioClip;
        public float Volume;
    }
}