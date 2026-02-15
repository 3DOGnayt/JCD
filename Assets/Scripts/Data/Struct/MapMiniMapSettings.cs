using System;
using UnityEngine;

namespace Data.Struct
{
    [Serializable]
    public struct MapMiniMapSettings
    {
        public Texture2D MiniMapTexture;
        public Vector2 MapWorldSize;
        public Vector2 MapWorldCenter;
        public float ViewRadiusMeters;
        public bool RotateWithPlayer;
        public bool ClampToMapBounds;
    }
}