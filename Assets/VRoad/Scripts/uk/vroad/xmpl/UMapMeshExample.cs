using uk.vroad.api;
using uk.vroad.api.map;
using uk.vroad.api.xmpl;

using uk.vroad.ucm;
using UnityEditor;
using UnityEngine;

namespace uk.vroad.xmpl
{
    public class UMapMeshExample : UaMapMesh
    {
        public Material unityTerrainMaterial;

        // These are set in the options Tab in the V-Road editor window, and copied to here.
        // Without VRoadWindow, they could be controlled from the owning game object by making them public
        private bool createMultipleMeshes;
        private bool createUnityTerrain;

        public static UMapMeshExample MostRecentInstance { get; private set;  }

        protected override bool UseMultipleSubMeshes() { return createMultipleMeshes; }

        public bool CreateMultipleMeshes { set => createMultipleMeshes = value; }
        public bool CreateUnityTerrain { set => createUnityTerrain = value; }

        protected override App App() { return app; }

        private ExampleApp app;

        protected override void Awake()
        {
            MostRecentInstance = this;
            
            app = ExampleApp.AwakeInstance();
            base.Awake();
        }

        protected override void AddExtraLayerMultiSubMesh(int progress)
        {
            AddExtraLayer(progress);
        }

        protected override void AddExtraLayerSingleSubMesh(int progress)
        {
            AddExtraLayer(progress);
        }

        private GameObject unityTerrainGO;
        
        public void SaveTerrain(string meshPath)
        {
            Terrain terrain = unityTerrainGO.GetComponent<Terrain>();
            string terrainFileName = meshPath + SA.TERRAIN + SA.SUFFIX_ASSET;
            AssetDatabase.CreateAsset(terrain.terrainData, terrainFileName);
        }
        
        private void AddExtraLayer(int progress)
        {
            if (createUnityTerrain)
            {
                GameObject parentGO = gameObject;
                // MapMesh is single child of this script's GO, put Unity terrain there, to be saved with prefab
                if (gameObject.transform.childCount == 1) parentGO = gameObject.transform.GetChild(0).gameObject;
                
                unityTerrainGO = new GameObject("UnityTerrain", typeof(Terrain), typeof(TerrainCollider));
                Terrain unityTerrain = unityTerrainGO.GetComponent<Terrain>();
                TerrainCollider tc = unityTerrainGO.GetComponent<TerrainCollider>();
                unityTerrainGO.transform.parent = parentGO.transform;
                unityTerrain.materialTemplate = unityTerrainMaterial;
                unityTerrainGO.transform.localPosition = new Vector3(-100, 0, -100);
                
                float bb = (float) app.Map().GetBorder();
                float mw = (float) app.Map().GetWidth();
                float mh = (float) app.Map().GetHeight();

                ITerrain terrainGrid = app.Map().Terrain();

                // This is by default 33 (32+1), and apparently should be a power of 2 + 1. 
                int nr = 513;
                float stepX = mw / (float) nr;
                float stepY = mh / (float) nr;

                // heightmap values should be in the range 0..1
                float[,] heightMap = new float[nr, nr];

                // First iteration finds min/max height and height range
                float hMin = float.MaxValue;
                float hMax = -float.MaxValue;
                for (int xi = 0; xi < nr; xi++)
                {
                    for (int yi = 0; yi < nr; yi++)
                    {
                        float h = (float) terrainGrid.GetHeightTri(-bb + stepX * xi, -bb + stepY * yi);
                        // heightMap[nr-1-xi, nr-1-yi] = h;
                        heightMap[yi, xi] = h;

                        if (h < hMin) hMin = h;
                        if (h > hMax) hMax = h;
                    }
                }

                // Second iteration normalizes height to range
                float hRange = hMax - hMin;
                for (int xi = 0; xi < nr; xi++)
                {
                    for (int yi = 0; yi < nr; yi++)
                    {
                        float h = heightMap[xi, yi];
                        heightMap[xi, yi] = (h - hMin) / hRange;
                    }
                }

                TerrainData terrainData = new TerrainData();
                terrainData.heightmapResolution = nr;
                terrainData.size = new Vector3(mw, hRange, mh);
                terrainData.SetHeights(0, 0, heightMap);

                unityTerrain.terrainData = terrainData;
                tc.terrainData = terrainData;

                Terrain.SetConnectivityDirty();
            }
        }


    }
}
