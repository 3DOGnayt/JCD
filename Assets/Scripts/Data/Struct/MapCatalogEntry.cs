using System;
using Data.Enums;
using UnityEngine;

namespace Configs.Impl
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