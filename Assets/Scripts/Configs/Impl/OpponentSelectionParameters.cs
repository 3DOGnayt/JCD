using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/SelectionParameters/" + nameof(OpponentSelectionParameters), fileName = nameof(OpponentSelectionParameters), order = 1)]
    public class OpponentSelectionParameters : ScriptableObject
    {
        [SerializeField] private string _selectedOpponentName;
        [SerializeField] private float _selectedOpponentDifficulty;
        [SerializeField] private int _selectedOpponentIndex;

        public string SelectedOpponentName => _selectedOpponentName;
        public float SelectedOpponentDifficulty => _selectedOpponentDifficulty;
        public int SelectedOpponentIndex => _selectedOpponentIndex;

        public void SetSelectedOpponent(string opponentName, float difficulty, int index)
        {
            _selectedOpponentName = opponentName;
            _selectedOpponentDifficulty = difficulty;
            _selectedOpponentIndex = index;
        }
    }
}