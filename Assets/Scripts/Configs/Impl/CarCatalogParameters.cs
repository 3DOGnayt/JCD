using System;
using System.Collections.Generic;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/" + nameof(CarCatalogParameters), fileName = nameof(CarCatalogParameters), order = 2)]
    public class CarCatalogParameters : ScriptableObject
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