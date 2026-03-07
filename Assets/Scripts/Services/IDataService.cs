using Data.Enums;
using Data.HelperClass;
using Data.Struct;
using System.Collections.Generic;

namespace Services
{
    public interface IDataService
    {
        AudioVolumeSetup LoadAudioVolumes(AudioVolumeSetup fallback);
        float LoadAudioVolume(EAudioType type, float fallback);
        void SaveAudioVolume(EAudioType type, float value);
        GameSelectionSaveData LoadGameSelection();
        void SaveGameSelection(GameSelectionSaveData data);
        List<TrainingTimeScoreSetup> LoadTrainingTimeScores();
        void SaveTrainingTimeScores(IReadOnlyList<TrainingTimeScoreSetup> entries);
    }
}
