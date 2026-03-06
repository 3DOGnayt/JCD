using Data.Enums;
using Data.HelperClass;

namespace Services.Impl
{
    public partial class DataService
    {
        private const string MasterVolumeKey = "Settings.Audio.Master";
        private const string SfxVolumeKey = "Settings.Audio.Sfx";
        private const string MusicVolumeKey = "Settings.Audio.Music";
        private const string UiVolumeKey = "Settings.Audio.Ui";

        public AudioVolumeSetup LoadAudioVolumes(AudioVolumeSetup fallback)
        {
            var defaultMaster = fallback != null ? fallback.Master : 1f;
            var defaultSfx = fallback != null ? fallback.Sfx : 1f;
            var defaultMusic = fallback != null ? fallback.Music : 1f;
            var defaultUi = fallback != null ? fallback.Ui : 1f;

            return new AudioVolumeSetup
            {
                Master = LoadFloat(MasterVolumeKey, defaultMaster),
                Sfx = LoadFloat(SfxVolumeKey, defaultSfx),
                Music = LoadFloat(MusicVolumeKey, defaultMusic),
                Ui = LoadFloat(UiVolumeKey, defaultUi)
            };
        }

        public float LoadAudioVolume(EAudioType type, float fallback)
        {
            var key = GetVolumeKey(type);
            if (string.IsNullOrEmpty(key))
                return fallback;

            return LoadFloat(key, fallback);
        }

        public void SaveAudioVolume(EAudioType type, float value)
        {
            var key = GetVolumeKey(type);
            if (string.IsNullOrEmpty(key))
                return;

            SaveFloat(key, value);
        }

        private static string GetVolumeKey(EAudioType type)
        {
            switch (type)
            {
                case EAudioType.Master:
                    return MasterVolumeKey;
                case EAudioType.Sfx:
                    return SfxVolumeKey;
                case EAudioType.Music:
                    return MusicVolumeKey;
                case EAudioType.Ui:
                    return UiVolumeKey;
                default:
                    return string.Empty;
            }
        }
    }
}
