using System.Collections.Generic;
using Data.Struct;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/" + nameof(MapCatalogParameters), fileName = nameof(MapCatalogParameters), order = 3)]
    public class MapCatalogParameters : ScriptableObject
    {
        [SerializeField] private List<MapCatalogEntry> _maps = new();

        public IReadOnlyList<MapCatalogEntry> Maps => _maps;
    }
}