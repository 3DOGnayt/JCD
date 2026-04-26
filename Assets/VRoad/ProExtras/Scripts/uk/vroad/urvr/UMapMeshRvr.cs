using System.Collections.Generic;

using uk.vroad.api;
using uk.vroad.api.etc;
using uk.vroad.api.geom;
using uk.vroad.api.map;
using uk.vroad.api.str;
using uk.vroad.rvr;
using uk.vroad.ucm;
using UnityEngine;
using UnityEngine.UI;

namespace uk.vroad.urvr
{
    public class UMapMeshRvr : UaMapMesh, LAppView
    {
        public GameObject layerCentreLines;
        public GameObject layerVial;
        public Texture[] terrainTextures;
        
        // References to these materials are required to make "Solid" buildings transparent in XRay mode
        public Material solidBuildingMat;
        public Material glassBuildingMat;

        public Text quarterText;
        private int quarterNameCountdown;
        private RectTransform quarterNameRect;
        
        
        private const int QNAME_STATIC = 300;
        private const int QNAME_MOVING = QNAME_STATIC;
        private const double QNAME_SMALL = 0.5;
        private const float QNAME_HA = -25f;
        private const float QNAME_HB = -100f;

        protected override App App() { return game; }
        protected override bool UseMultipleSubMeshes() { return false; } 

        private Game game;
        protected override void Awake()
        {
#if VROAD_RVR_RELEASE
            uk.vroad.osk.KTerrain.Awake();  // Required if using in-process map builder
#endif
            
            game = Game.AwakeInstance();
            base.Awake();
        
            layerGOs.Add(layerCentreLines);
            layerGOs.Add(layerVial);
            
            quarterNameRect = quarterText.transform.parent.GetComponent<RectTransform>();

            viewChanged = true;
        }

#if VROAD_RVR_RELEASE
        protected override void Update()
        {
            uk.vroad.osk.KTerrain.LoadTerrainIfWaiting();  // Required if using in-process map builder
            
            base.Update();
        }
#endif
        
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            
            if (!MeshesReady()) return;
            
            if (quarterNameCountdown > 0)
            {
                quarterNameCountdown--;
                
                if (quarterNameCountdown < QNAME_MOVING)
                {
                    
                    float q = quarterNameCountdown / (float) QNAME_MOVING;
                    float size =  (float) (QNAME_SMALL + ((1 - QNAME_SMALL) * q));
                    float y = QNAME_HA + (q * (QNAME_HB - QNAME_HA)); 

                    quarterNameRect.anchoredPosition = new Vector2(0, y);
                    quarterNameRect.localScale = new Vector3(size, size, 1);
                }

            }
        }
        public override void UAppStateChanged(AppStateTransition ast)
        {
            base.UAppStateChanged(ast);
            
            layerVial.SetActive(ast.before == AppState.ReadyToSimulate);
            
            if (ast == GameStateTransition.startNavigating)
            {
                quarterText.transform.parent.gameObject.SetActive(false);

                UpdateBusStopColours();
            }
            
            if (ast == GameStateTransition.removeGameOverMsg)
            {
                ResetMapMesh();
            }
        }

        protected override void OnMeshCreationStart()
        {
            IMap map = game.Map();
            quarterText.text = game.GetMapSuburb() + SC.CMS + game.GetMapCity();
            quarterNameCountdown = QNAME_STATIC + QNAME_MOVING;
            quarterNameRect.anchoredPosition = new Vector2(0, QNAME_HB);
            quarterText.transform.parent.gameObject.SetActive(true);

        }
        protected override void ResetMapMesh()
        {
            base.ResetMapMesh();
            
            quarterText.transform.parent.gameObject.SetActive(false);
            quarterText.text = SC.N;

        }
        public void ShowVialOnMap(bool visible)
        {
            layerVial.SetActive(visible);
        }

        protected override void AddExtraLayerMultiSubMesh(int progress)
        {
            CreateLayerMesh(progress-1, layerVial, VialSubMeshArray());
            CreateLayerMesh(progress, layerCentreLines, CentreLineSubMesh()); // should be array?

        }

        protected override void AddExtraLayerSingleSubMesh(int progress)
        {
            CreateLayerMesh(progress-1, layerVial, VialSubMesh());
            CreateLayerMesh(progress, layerCentreLines, CentreLineSubMesh()); 

        }

        SubMesh[] VialSubMeshArray()
        {
            float dz = 0.05f;
            List<SubMesh> sml = new List<SubMesh>();

            IPedZone pz = game.Vg().VialZone();
            if (pz != null)
            {
                TriMesh tm = pz.TriMesh();
                AddTriangleSubMesh(sml, tm, dz);
            }

            return sml.ToArray();
        }

        SubMesh VialSubMesh()
        {
            float dz = 0.05f;
            List<TriMesh> tml = new List<TriMesh>();

            IPedZone pz = game.Vg().VialZone();
            if (pz != null)
            {
                tml.Add((pz.TriMesh()));
            }
            return TriangleSubMesh(TriMesh.Combine(tml.ToArray()), dz);
        }
        SubMesh CentreLineSubMesh()
        {
            List<TriMesh> tml = new List<TriMesh>();

            // 400 hectares at aspect of 16:9 is 2666 x 1500
            // 300 hectares at aspect of 16:9 is 2308 x 1300
            //
            // ew is width from area + 200 (2 x border of 100)

            double ew = game.Map().GetWidth();
            double scaledWidth = ew * 0.002;  // 2866 * 0.002 = 5.732

            double miniMapRoadWidth = scaledWidth;
            double miniMapPedwayWidth = 0.6 * scaledWidth;
            
            foreach (IRoad rd in game.Map().Roads())
            {
                if (!rd.IsCentreLineReversed())
                {
                    tml.Add(rd.GetCentreLine().CentreLineTriMesh(miniMapRoadWidth));
                }
            }

            foreach (IFootpath fp in game.Map().Footpaths())
            {
                if (fp.IsPedway())
                {
                    tml.Add(fp.GetCentreLine().CentreLineTriMesh(miniMapPedwayWidth));
                }
            }

            return TriangleSubMesh(TriMesh.Combine(tml.ToArray()));
        }

        protected override bool StopColorCanChange()
        {
            return true;
        }
        protected override Color StopColor(IStop stop)
        {
            bool isWhite = game.Prb().RouteExistsForPlayer(stop);
            return isWhite ? Color.white : COLOR_DROP_OFF;
        }
      
        private bool viewChanged; // set to change object visibility in main Unity Thread

        public void FireViewChanged()
        {
            viewChanged = true;
        }

        private void FireMapOrCameraChanged()
        {
            viewChanged = false;
            
            layerCentreLines.SetActive(game.Gih().ShowMiniMap());

            bool isGlass = game.Gih().ShowTransparentBuildings();
            MeshRenderer mr = layerSolidBuildings.GetComponent<MeshRenderer>();
            mr.sharedMaterial = isGlass ? glassBuildingMat: solidBuildingMat;
        }

        protected override void UpdateDisplay()
        {
            if (viewChanged) FireMapOrCameraChanged();

            base.UpdateDisplay();
        }
       
        protected override void SetTerrainMaterial()
        {
            Texture appliedTexture = terrainTextures[0];

            // Set terrain material using MapEdit Generic_Edit Terrain=ConcreteKhaki
            ICouple terrainCouple = App().Map().Couple(SF.TERRAIN);
            string terrainName = terrainCouple?.Value();
            
            if (string.IsNullOrEmpty(terrainName))
            {
                int ntex = terrainTextures.Length;
                int terrainTXI = Rng.NextInt(Rng.Vein.COLOURS, ntex);
                appliedTexture = terrainTextures[terrainTXI];
            }
            else
            {
                foreach (Texture tex in terrainTextures)
                {
                    if (terrainName.Equals(tex.name)) // tex.name is tex-map file name, without .jpg
                    {
                        appliedTexture = tex; 
                        break;
                    }
                }
            }
            
            MeshRenderer mr = layerTerrain.GetComponent<MeshRenderer>();
            mr.sharedMaterial.mainTexture = appliedTexture; // shared with embankments
        }

    }
}
