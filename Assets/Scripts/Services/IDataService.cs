using Data.Enums;
using Data.HelperClass;

namespace Services
{
    public interface IDataService
    {
        AudioVolumeSetup LoadAudioVolumes(AudioVolumeSetup fallback);
        float LoadAudioVolume(EAudioType type, float fallback);
        void SaveAudioVolume(EAudioType type, float value);
    }
}
