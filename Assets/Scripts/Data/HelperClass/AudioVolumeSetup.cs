using System;

namespace Data.HelperClass
{
    [Serializable]
    public class AudioVolumeSetup
    {
        public float Master = 1f;
        public float Music = 1f;
        public float Sfx = 1f;
        public float Ui = 1f;
    }
}