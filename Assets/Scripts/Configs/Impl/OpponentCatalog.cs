using System;
using System.Collections.Generic;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/" + nameof(OpponentCatalog), fileName = nameof(OpponentCatalog), order = 4)]
    public class OpponentCatalog : ScriptableObject
    {
        [SerializeField] private List<OpponentCatalogEntry> _opponents = new List<OpponentCatalogEntry>();

        public IReadOnlyList<OpponentCatalogEntry> Opponents => _opponents;
    }

    [Serializable]
    public struct OpponentCatalogEntry
    {
        public string DisplayName;
        public Sprite Preview;
        public float Difficulty;
    }
}