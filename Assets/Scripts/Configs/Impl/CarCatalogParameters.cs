using System.Collections.Generic;
using Data.Struct;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/" + nameof(CarCatalogParameters), fileName = nameof(CarCatalogParameters), order = 2)]
    public class CarCatalogParameters : ScriptableObject
    {
        [SerializeField] private List<CarCatalogEntry> _cars = new();

        public IReadOnlyList<CarCatalogEntry> Cars => _cars;
    }
}