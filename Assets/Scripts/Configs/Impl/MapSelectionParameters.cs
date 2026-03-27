using Data.Enums;
using UnityEngine;
using UnityEngine.Splines;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/SelectionParameters/" + nameof(MapSelectionParameters), fileName = nameof(MapSelectionParameters), order = 1)]
    public class MapSelectionParameters : ScriptableObject
    {
        [SerializeField] private GameObject _selectedMapPrefab;
        [SerializeField] private int _selectedMapIndex;
        [SerializeField] private int _selectedMapSelectionCount;
        [SerializeField] private int _selectedMapLapCount;
        [SerializeField] private EMap _selectedMap;

        private SplineContainer _runtimeSpline;

        public GameObject SelectedMapPrefab => _selectedMapPrefab;
        public int SelectedMapIndex => _selectedMapIndex;
        public int SelectedMapSelectionCount => _selectedMapSelectionCount;
        public int SelectedMapLapCount => _selectedMapLapCount;
        public EMap SelectedMap => _selectedMap;
        public SplineContainer RuntimeSpline => _runtimeSpline;

        public void SetSelectedMap(GameObject mapPrefab, int index, EMap map, int selectionCount, int lapCount)
        {
            _selectedMapPrefab = mapPrefab;
            _selectedMapIndex = index;
            _selectedMapSelectionCount = selectionCount;
            _selectedMapLapCount = lapCount;
            _selectedMap = map;
            _runtimeSpline = null;
        }

        public void SetRuntimeSpline(SplineContainer spline)
        {
            _runtimeSpline = spline;
        }
    }
}