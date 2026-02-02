using System;
using System.Collections.Generic;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/" + nameof(MapCatalog), fileName = nameof(MapCatalog), order = 3)]
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