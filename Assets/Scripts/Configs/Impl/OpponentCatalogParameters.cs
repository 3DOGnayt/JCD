using System.Collections.Generic;
using Data.Struct;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/" + nameof(OpponentCatalogParameters), fileName = nameof(OpponentCatalogParameters), order = 4)]
    public class OpponentCatalogParameters : ScriptableObject
    {
        [SerializeField] private List<OpponentCatalogEntry> _opponents = new();

        public IReadOnlyList<OpponentCatalogEntry> Opponents => _opponents;
    }
}