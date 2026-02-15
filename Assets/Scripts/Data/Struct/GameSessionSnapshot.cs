using Configs.Impl;
using Data.Enums;
using UnityEngine;

namespace Data.Struct
{
    public struct GameSessionSnapshot
    {
        public CarPresetParameters SelectedCar;
        public CarParameters SelectedCarParameters;
        public int SelectedCarIndex;

        public GameObject SelectedMapPrefab;
        public int SelectedMapIndex;
        public int SelectedMapSelectionCount;
        public int SelectedMapLapCount;
        public EMap SelectedMap;

        public EGameMod GameMod;

        public string SelectedOpponentName;
        public float SelectedOpponentDifficulty;
        public int SelectedOpponentIndex;
    }
}