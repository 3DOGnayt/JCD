using Data.Enums;
using Helpers.Race;
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
        [SerializeField] private float _selectedOpponentSpawnSideOffset;
        [SerializeField] private Vector3 _selectedUnitSpawnEulerAngles;
        [SerializeField] private EMap _selectedMap;

        private SplineContainer _runtimeSpline;
        private SplineContainer _runtimeSplineInner;
        private SplineContainer _runtimeSplineOuter;
        private MapSplineProvider.OpponentSplineEntry[] _runtimeOpponentSplines;

        public GameObject SelectedMapPrefab => _selectedMapPrefab;
        public int SelectedMapIndex => _selectedMapIndex;
        public int SelectedMapSelectionCount => _selectedMapSelectionCount;
        public int SelectedMapLapCount => _selectedMapLapCount;
        public float SelectedOpponentSpawnSideOffset => _selectedOpponentSpawnSideOffset;
        public Vector3 SelectedUnitSpawnEulerAngles => _selectedUnitSpawnEulerAngles;
        public EMap SelectedMap => _selectedMap;
        public SplineContainer RuntimeSpline => _runtimeSpline;
        public SplineContainer RuntimeSplineInner => _runtimeSplineInner;
        public SplineContainer RuntimeSplineOuter => _runtimeSplineOuter;
        public MapSplineProvider.OpponentSplineEntry[] RuntimeOpponentSplines => _runtimeOpponentSplines;

        public void SetSelectedMap(
            GameObject mapPrefab,
            int index,
            EMap map,
            int selectionCount,
            int lapCount,
            float opponentSpawnSideOffset = 0f,
            Vector3 unitSpawnEulerAngles = default)
        {
            _selectedMapPrefab = mapPrefab;
            _selectedMapIndex = index;
            _selectedMapSelectionCount = selectionCount;
            _selectedMapLapCount = lapCount;
            _selectedOpponentSpawnSideOffset = opponentSpawnSideOffset;
            _selectedUnitSpawnEulerAngles = unitSpawnEulerAngles;
            _selectedMap = map;
            _runtimeSpline = null;
            _runtimeSplineInner = null;
            _runtimeSplineOuter = null;
            _runtimeOpponentSplines = null;
        }

        public void SetRuntimeSpline(SplineContainer spline)
        {
            _runtimeSpline = spline;
        }

        public void SetRuntimeSplines(SplineContainer innerSpline, SplineContainer outerSpline)
        {
            _runtimeSplineInner = innerSpline;
            _runtimeSplineOuter = outerSpline;
        }

        public void SetRuntimeOpponentSplines(MapSplineProvider.OpponentSplineEntry[] opponentSplines)
        {
            _runtimeOpponentSplines = opponentSplines;
        }
    }
}