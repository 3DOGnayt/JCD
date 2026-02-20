using Data.Enums;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/SelectionParameters/" + nameof(AudioSelectionParameters), fileName = nameof(AudioSelectionParameters), order = 1)]
    public class AudioSelectionParameters : ScriptableObject
    {
        [SerializeField] private EAudioSubType _selectedMusicSubType;
        [SerializeField] private int _selectedMusicIndex;

        public EAudioSubType SelectedMusicSubType => _selectedMusicSubType;
        public int SelectedMusicIndex => _selectedMusicIndex;

        public void SetSelectedMusic(EAudioSubType subType, int index)
        {
            _selectedMusicSubType = subType;
            _selectedMusicIndex = index;
        }
    }
}