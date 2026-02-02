using System;
using System.Collections.Generic;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/" + nameof(CarCatalog), fileName = nameof(CarCatalog), order = 2)]
    public class CarCatalog : ScriptableObject
    {
        [SerializeField] private List<CarCatalogEntry> _cars = new List<CarCatalogEntry>();

        public IReadOnlyList<CarCatalogEntry> Cars => _cars;
    }

    [Serializable]
    public struct CarCatalogEntry
    {
        public string DisplayName;
        public CarPresetParameters Preset;
        public CarParameters Parameters;
        public Sprite Preview;
    }
}