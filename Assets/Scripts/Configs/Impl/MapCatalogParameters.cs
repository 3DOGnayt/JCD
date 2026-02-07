using System;
using System.Collections.Generic;
using Data.Enums;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/" + nameof(MapCatalogParameters), fileName = nameof(MapCatalogParameters), order = 3)]
    public class MapCatalogParameters : ScriptableObject
    {
        [SerializeField] private List<MapCatalogEntry> _maps = new List<MapCatalogEntry>();

        public IReadOnlyList<MapCatalogEntry> Maps => _maps;
    }

    [Serializable]
    public struct MapCatalogEntry
    {
        public EMap EMap;
        public GameObject Prefab;
        public Sprite Preview;
        public int SelectionCount;
        public int LapCount;
    }
}