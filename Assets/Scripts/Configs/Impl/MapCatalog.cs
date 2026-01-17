using System;
using System.Collections.Generic;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Map/MapCatalog", fileName = "MapCatalog")]
    public class MapCatalog : ScriptableObject
    {
        [SerializeField] private List<MapCatalogEntry> _maps = new List<MapCatalogEntry>();

        public IReadOnlyList<MapCatalogEntry> Maps => _maps;
    }

    [Serializable]
    public struct MapCatalogEntry
    {
        public string DisplayName;
        public GameObject Prefab;
        public Sprite Preview;
    }
}
