using System;
using UnityEngine;

namespace Data.Struct
{
    [Serializable]
    public struct OpponentCatalogEntry
    {
        public string DisplayName;
        public Sprite Preview;
        public float Difficulty;
    }
}