using Data.Enums;
using Data.HelperClass;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/SelectionParameters/" + nameof(AudioSelectionParameters), fileName = nameof(AudioSelectionParameters), order = 1)]
    public class AudioSelectionParameters : ScriptableObject
    {
        [SerializeField] private EAudioSubType _selectedMusicSubType;
        [SerializeField] private int _selectedMusicIndex;
        [SerializeField] private AudioVolumeSetup volumeSetup = new();

        public EAudioSubType SelectedMusicSubType => _selectedMusicSubType;
        public int SelectedMusicIndex => _selectedMusicIndex;
        public AudioVolumeSetup VolumeSetup => volumeSetup;

        public void SetSelectedMusic(EAudioSubType subType, int index)
        {
            _selectedMusicSubType = subType;
            _selectedMusicIndex = index;
        }

        public void SetMasterVolume(float value) => volumeSetup.Master = value;
        public void SetMusicVolume(float value) => volumeSetup.Music = value;
        public void SetSfxVolume(float value) => volumeSetup.Sfx = value;
        public void SetUiVolume(float value) => volumeSetup.Ui = value;
    }
}