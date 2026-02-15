using System;
using Data.Enums;
using UnityEngine;

namespace Data.Struct
{
    [Serializable]
    public struct MapCatalogEntry
    {
        public EMap EMap;
        public GameObject Prefab;
        public Sprite Preview;
        public int SelectionCount;
        public int LapCount;
        public MapMiniMapSettings MiniMap;
    }
}