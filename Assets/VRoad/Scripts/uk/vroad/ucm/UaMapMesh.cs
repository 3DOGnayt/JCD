using System;
using System.Collections;
using System.Collections.Generic;
using uk.vroad.api;
using uk.vroad.api.enums;
using uk.vroad.api.etc;
using MH = uk.vroad.api.enums.MaterialHint;
using uk.vroad.api.events;
using uk.vroad.api.geom;
using uk.vroad.api.map;
using uk.vroad.api.str;
using uk.vroad.apk;
using UnityEngine;
using UnityEngine.Rendering;

namespace uk.vroad.ucm
{


    public abstract class UaMapMesh : MonoBehaviour, LUAppState, LSimTimeSec
    {

        protected abstract bool UseMultipleSubMeshes();

        protected abstract App App();

        public bool MeshesReady() { return meshesCreated; }

        public GameObject layerLanes;
        public GameObject layerJunctions;
        public GameObject layerTurnArrows;
        public GameObject layerSignalHeads;
        public GameObject layerSignalArrows;
        public GameObject layerSignalWalks;

        public GameObject layerFootpaths;
        public GameObject layerCrossings;
        public GameObject layerZebras;

        public GameObject layerBusStops;
        public GameObject layerParking;
        public GameObject layerParkingDropOff;

        public GameObject layerSolidBuildings;
        public GameObject layerGlassBuildings;
        public GameObject layerWater;
        public GameObject layerTerrain;

        public GameObject layerBridges;
        public GameObject layerTunnels;
        public GameObject layerShoulderKerbing;
        public GameObject layerEmbankments;

        public Texture greenMan;
        public Texture redMan;

        public Texture terrainTexture;

        public const int LAYER_MINI_MAP = 8;
        public const int LAYER_SIM_MAP = 9;

        private bool meshesCreated;


        private readonly KHash<IStreme, int> stremeGreenMeshIndex = new KHash<IStreme, int>();
        private readonly KHash<ICrossing, int> crossingMeshIndex = new KHash<ICrossing, int>();
        private readonly KHash<IStop, int> stopMeshIndex = new KHash<IStop, int>();

        protected readonly KList<GameObject> layerGOs = new KList<GameObject>();

        private ITurn[] turnsJustChangedArray = IZero.ZERO_ITURN;
        private ICrossing[] crossingsJustChangedArray = IZero.ZERO_ICROSSING; //ZERO_RED_GREEN_CROSSING;


        protected virtual void Awake()
        {
            KEnv.Awake();

            layerGOs.Add(layerLanes);
            layerGOs.Add(layerJunctions);
            layerGOs.Add(layerTurnArrows);
            layerGOs.Add(layerSignalHeads);
            layerGOs.Add(layerSignalArrows);
            layerGOs.Add(layerSignalWalks);

            layerGOs.Add(layerFootpaths);
            layerGOs.Add(layerCrossings);
            layerGOs.Add(layerZebras);

            layerGOs.Add(layerBusStops);
            layerGOs.Add(layerParking);
            layerGOs.Add(layerParkingDropOff);

            layerGOs.Add(layerSolidBuildings);
            layerGOs.Add(layerGlassBuildings);
            layerGOs.Add(layerWater);
            layerGOs.Add(layerTerrain);

            layerGOs.Add(layerBridges); // The pillars and under surface of the bridges
            layerGOs.Add(layerTunnels);
            layerGOs.Add(layerShoulderKerbing);
            layerGOs.Add(layerEmbankments);

            App().AddEventConsumer(this); // was in Start
        }

        void Start()
        {
            ResetMapMesh();
            UaStateHandler.MostRecentInstance.AddListener(this);
        }

        protected virtual void Update()
        {
            if (UaPlaySim.AsFastAsPossible()) UpdateDisplay();
        }

        protected virtual void FixedUpdate()
        {
            if (!UaPlaySim.AsFastAsPossible()) UpdateDisplay();
        }

        protected virtual void SetTerrainMaterial()
        {
            MeshRenderer mr = layerTerrain.GetComponent<MeshRenderer>();
            mr.sharedMaterial.mainTexture = terrainTexture; // shared with embankments
        }

        public bool DeregisterFireMapChange() { return true; }


        protected virtual void ResetMapMesh()
        {
            meshesCreated = false;

            foreach (GameObject layerGO in layerGOs) { layerGO.GetComponent<MeshFilter>().mesh = new Mesh(); }
        }

        IEnumerator CreateLayerMeshes(bool useMultipleSubMeshes)
        {
            // See lookup of layer gameObjects in Awake()
            if (UDbg.TIMING) UDbg.timerLoaderMesh = Reporter.Timer();

            if (useMultipleSubMeshes) // Multiple sub-meshes may be better on some devices
            {
                CreateNamedMeshObjects(5, layerLanes, LaneNamedSubMeshArray());
                yield return null;
                CreateNamedMeshObjects(10, layerJunctions, JunctionNamedSubMeshArray());
                yield return null;
                CreateNamedMeshObjects(15, layerTurnArrows, TurnArrowNamedSubMeshArray());
                yield return null;
                NamedSubMesh[][] shsma = SignalHeadNamedSubMeshArray();
                CreateNamedMeshObjects(20, layerSignalHeads, shsma[0]);
                yield return null;
                CreateNamedMeshObjects(25, layerSignalArrows, shsma[1]);
                yield return null;
                CreateNamedMeshObjects(30, layerSignalWalks, WalkSignalNamedSubMeshArray());
                yield return null;
                CreateNamedMeshObjects(35, layerBusStops, StopNamedSubMeshArray());
                yield return null;
                CreateNamedMeshObjects(38, layerParking, ParkingNamedSubMeshArray(false));
                yield return null;
                CreateNamedMeshObjects(40, layerParkingDropOff, ParkingNamedSubMeshArray(true));
                yield return null;

                CreateNamedMeshObjects(45, layerFootpaths, FootpathNamedSubMeshArray());
                yield return null;
                CreateNamedMeshObjects(50, layerCrossings, CrossingNamedSubMeshArray());
                yield return null;
                CreateNamedMeshObjects(55, layerZebras, ZebraNamedSubMeshArray());
                yield return null;

                CreateNamedMeshObjects(60, layerSolidBuildings, BuildingNamedMeshObjects(false));
                yield return null;
                CreateNamedMeshObjects(65, layerSolidBuildings, BuildingNamedMeshObjects(true));
                yield return null;

                CreateNamedMeshObjects(70, layerWater, WaterNamedSubMeshArray());
                yield return null;
                CreateLayerMesh(75, layerTerrain, TerrainSubMesh());
                yield return null;

                CreateNamedMeshObjects(77, layerBridges, BridgesNamedSubMeshArray());
                yield return null;
                CreateNamedMeshObjects(80, layerTunnels, ShouldersNamedSubMeshArray(MH.Tunnel, MH.Tunnel));
                yield return null;
                CreateNamedMeshObjects(85, layerShoulderKerbing, ShouldersNamedSubMeshArray(MH.Kerbing, MH.Bridge));
                yield return null;
                CreateNamedMeshObjects(90, layerEmbankments, ShouldersNamedSubMeshArray(MH.Embankment, MH.Embankment));
                yield return null;

                AddExtraLayerMultiSubMesh(100);
            }

            else
            {
                CreateLayerMesh(4, layerLanes, LaneSubMesh());
                yield return null;
                CreateLayerMesh(8, layerJunctions, JunctionSubMesh());
                yield return null;
                CreateLayerMesh(12, layerTurnArrows, TurnArrowSubMesh());
                yield return null;
                CreateLayerMesh(16, layerSignalHeads, SignalBoxSubMesh());
                yield return null;
                CreateLayerMesh(20, layerSignalArrows, SignalArrowSubMeshArray());
                yield return null; // need array here, materials change with signal state
                CreateLayerMesh(24, layerSignalWalks, WalkSignalSubMeshArray());
                yield return null; // need array here, materials change with signal state

                if (StopColorCanChange()) CreateLayerMesh(28, layerBusStops, StopSubMeshArray());
                else CreateLayerMesh(28, layerBusStops, StopSubMesh());
                yield return null;

                CreateLayerMesh(31, layerParking, ParkingSubMesh(false));
                yield return null;
                CreateLayerMesh(32, layerParkingDropOff, ParkingSubMesh(true));
                yield return null;

                CreateLayerMesh(36, layerFootpaths, FootpathSubMesh());
                yield return null;
                CreateLayerMesh(40, layerCrossings, CrossingSubMesh());
                yield return null;
                CreateLayerMesh(44, layerZebras, ZebraSubMesh());
                yield return null;

                CreateLayerMesh(48, layerSolidBuildings, BuildingSubMesh(false));
                yield return null;
                CreateLayerMesh(52, layerGlassBuildings, BuildingSubMesh(true));
                yield return null;
                CreateLayerMesh(60, layerWater, WaterSubMesh());
                yield return null;
                CreateLayerMesh(80, layerTerrain, TerrainSubMesh());
                yield return null;

                CreateLayerMesh(84, layerBridges, BridgesSubMesh());
                yield return null;
                CreateLayerMesh(88, layerTunnels, ShouldersSubMesh(MH.Tunnel, MH.Tunnel));
                yield return null;
                CreateLayerMesh(92, layerShoulderKerbing, ShouldersSubMesh(MH.Kerbing, MH.Bridge));
                yield return null;
                CreateLayerMesh(96, layerEmbankments, ShouldersSubMesh(MH.Embankment, MH.Embankment));
                yield return null;

                AddExtraLayerSingleSubMesh(100); // yield return null;
            }

            MeshesFinished();

            if (UDbg.TIMING) Reporter.Timer(UDbg.timerLoaderMesh, SU.U_TIMER01);
            if (UDbg.TIMING) Reporter.Timer(UDbg.timerLoaderTop, SU.U_TIMER02);
        }

        private void MeshesFinished()
        {
            Reporter.Progress(100);
            meshesCreated = true;
            OnMeshCreationFinish();
        }

        protected virtual void OnMeshCreationStart() { }

        protected virtual void OnMeshCreationFinish() { }

        public void MeshesCreate()
        {
            OnMeshCreationStart();

            SetTerrainMaterial();

            StartCoroutine(CreateLayerMeshes(UseMultipleSubMeshes()));
        }

        public virtual void UAppStateChanged(AppStateTransition ast) { }


        public void TimeSec()
        {
            double now = App().Sim().TimeNow();

            KList<ITurn> turnsJustChangedList = new KList<ITurn>();
            KList<ICrossing> crossingsJustChangedList = new KList<ICrossing>();

            foreach (ITurn turn in App().Map().Turns())
            {
                double changeTime = turn.LastChangeTime();
                if (changeTime == 0) continue;

                if (now - changeTime < 1.1) { turnsJustChangedList.Add(turn); }
            }

            turnsJustChangedArray = turnsJustChangedList.ToArray();

            foreach (ICrossing x in App().Map().Crossings())
            {
                if (x.IsRedGreen())
                {
                    double changeTime = x.LastChangeTime();
                    if (changeTime == 0) continue;

                    if (now - changeTime < 1.1) { crossingsJustChangedList.Add(x); }

                }
            }

            crossingsJustChangedArray = crossingsJustChangedList.ToArray();
        }

        private static readonly Color COLOR_OFF = new Color(0.25f, 0.25f, 0.25f, 1.0f);

        private static Color LampColour(ITurn turn)
        {
            Signal signal = turn.CurrentSignal();

            if (signal == Signal.Red) return Color.red;
            if (signal == Signal.Yellow) return Color.yellow;
            if (signal == Signal.YellowFlashing) return Color.yellow;
            if (signal == Signal.Green) return Color.green;
            if (signal == Signal.GreenYield) return Color.green;
            if (signal == Signal.GreenStop) return Color.green;

            return COLOR_OFF;
        }

        protected virtual void UpdateDisplay()
        {
            if (!meshesCreated) return;

            ITurn[] ta = turnsJustChangedArray;
            turnsJustChangedArray = IZero.ZERO_ITURN;

            if (ta.Length > 0)
            {
                MeshRenderer mr = layerSignalArrows.GetComponent<MeshRenderer>();
                int nm = mr.materials.Length;

                foreach (ITurn turn in ta)
                {
                    Color lampColour = LampColour(turn);

                    foreach (IStreme s in turn.GetStremes())
                    {
                        int gmi = stremeGreenMeshIndex.Get(s);

                        if (gmi < 0 || gmi > nm - 3) continue;

                        for (int ci = 0; ci < 3; ci++)
                        {
                            Material mat = mr.materials[gmi + ci];
                            mat.color = lampColour;
                        }
                    }
                }
            }

            ICrossing[] rgxa = crossingsJustChangedArray;
            crossingsJustChangedArray = IZero.ZERO_ICROSSING;

            bool multipleMeshes = this.UseMultipleSubMeshes();

            if (rgxa.Length > 0)
            {
                MeshRenderer mr = layerSignalWalks.GetComponent<MeshRenderer>();
                Material[] mra = mr.materials;

                foreach (ICrossing rgx in rgxa)
                {
                    bool isgreen = rgx.IsWalkOn();
                    Texture sigTexture = isgreen ? greenMan : redMan;

                    int xmi = multipleMeshes ? crossingMeshIndex.Get(rgx) : 0;

                    if (xmi < 0 || xmi >= mra.Length - 1) { continue; }

                    Material matAB = mra[xmi];
                    Material matBA = mra[xmi + 1];

                    matAB.mainTexture = sigTexture;
                    matBA.mainTexture = sigTexture;
                }
            }
        }

        protected static Color COLOR_DROP_OFF = new Color(0.25f, 0.25f, 0.25f, 1.0f);


        protected virtual void AddExtraLayerMultiSubMesh(int progress) { }
        protected virtual void AddExtraLayerSingleSubMesh(int progress) { }


        protected void CreateLayerMesh(int progress, GameObject layerGO, SubMesh layerSubMesh)
        {
            CreateMesh(layerGO.GetComponent<MeshFilter>(), layerGO.GetComponent<MeshRenderer>(), layerSubMesh);

            Reporter.Progress(progress);
        }


        private static bool allowBigMeshes = true;
        private static bool quickSingleMesh = true;

        protected void CreateLayerMesh(int progress, GameObject layerGO, SubMesh[] layerSubMeshArray)
        {
            CreateMesh(layerGO.GetComponent<MeshFilter>(), layerGO.GetComponent<MeshRenderer>(), layerSubMeshArray);
            Reporter.Progress(progress);
        }

        public static void CreateMesh(MeshFilter mf, MeshRenderer mr, SubMesh[] sma)
        {
            Mesh mesh = mf.mesh;
            mesh.Clear();

            int nsm = sma.Length;

            if (quickSingleMesh && nsm == 1)
            {
                SubMesh subMesh = sma[0];

                SetBigMeshFlag(mesh, subMesh.Vertices.Length, subMesh.Triangles.Length);

                mesh.subMeshCount = 1;
                mesh.vertices = subMesh.Vertices;
                mesh.normals = subMesh.Normals;
                mesh.uv = subMesh.TexUV;
                mesh.SetTriangles(subMesh.Triangles, 0, true, 0);
            }
            else
            {
                int nv = 0;
                int nt = 0;

                // count total vertices in all sub-meshes
                foreach (SubMesh subMesh in sma)
                {
                    if (subMesh == null) continue;

                    nv += subMesh.Vertices.Length;
                    nt += subMesh.Triangles.Length;
                }

                SetBigMeshFlag(mesh, nv, nt);
                Vector3[] vertices = new Vector3[nv];
                Vector3[] normals = new Vector3[nv];
                Vector2[] texUV = new Vector2[nv];

                int baseVertex = 0;
                foreach (SubMesh subMesh in sma)
                {
                    if (subMesh == null) continue;

                    int nvSubMesh = subMesh.Vertices.Length;

                    Array.Copy(subMesh.Vertices, 0, vertices, baseVertex, nvSubMesh);
                    Array.Copy(subMesh.Normals, 0, normals, baseVertex, nvSubMesh);
                    Array.Copy(subMesh.TexUV, 0, texUV, baseVertex, nvSubMesh);

                    baseVertex += nvSubMesh;
                }

                mesh.subMeshCount = nsm;
                mesh.vertices = vertices;
                mesh.normals = normals;
                mesh.uv = texUV;

                baseVertex = 0;
                int subMeshIndex = 0;
                int[] zeroTri = new int[0];

                foreach (SubMesh subMesh in sma)
                {
                    int nvSubMesh = subMesh?.Vertices.Length ?? 0;
                    int[] tvia = subMesh == null ? zeroTri : subMesh.Triangles;
                    mesh.SetTriangles(tvia, subMeshIndex++, true, baseVertex);

                    baseVertex += nvSubMesh;
                }
            }

            mf.mesh = mesh;

            if (mr != null)
            {
                // There needs to be an array of materials, the same size as the number of sub meshes
                // One material per submesh.
                // Renderer.material creates a cloned instance, renderer.sharedMaterial uses a reference
                Material[] ma = new Material[nsm];
                Material[] mra = mr.sharedMaterials;
                for (int i = 0; i < nsm; i++)
                {
                    SubMesh subMesh = sma[i];
                    int mati = subMesh?.MaterialIndex ?? 0;
                    if (mati < 0 || mati >= mra.Length) mati = 0;
                    ma[i] = mr.sharedMaterials[mati];
                }

                mr.sharedMaterials = ma;
            }
        }

        // Create a GameObject for each named submesh
        private void CreateNamedMeshObjects(int progress, GameObject layerGO, NamedSubMesh[] nmoa)
        {
            MeshRenderer mr = layerGO.GetComponent<MeshRenderer>();
            Material mat = mr.sharedMaterial; // mr.material;

            foreach (NamedSubMesh nmo in nmoa)
            {
                GameObject go = new GameObject(nmo.GameObjectName, typeof(MeshFilter), typeof(MeshRenderer));

                MeshFilter mf = go.GetComponent<MeshFilter>();
                Mesh mesh = mf.mesh;

                SetBigMeshFlag(mesh, nmo.Vertices.Length, nmo.Triangles.Length);

                mesh.subMeshCount = 1;
                mesh.vertices = nmo.Vertices;
                mesh.normals = nmo.Normals;
                mesh.uv = nmo.TexUV;
                mesh.SetTriangles(nmo.Triangles, 0, true, 0);

                mr = go.GetComponent<MeshRenderer>();
                mr.sharedMaterials = new[] {mat,};

                go.transform.parent = layerGO.transform;
            }

            Reporter.Progress(progress);
        }

        public static void CreateMesh(MeshFilter mf, MeshRenderer mr, SubMesh subMesh)
        {
            Mesh mesh = mf.mesh;
            mesh.Clear();

            SetBigMeshFlag(mesh, subMesh.Vertices.Length, subMesh.Triangles.Length);

            mesh.subMeshCount = 1;
            mesh.vertices = subMesh.Vertices;
            mesh.normals = subMesh.Normals;
            mesh.uv = subMesh.TexUV;

            mesh.SetTriangles(subMesh.Triangles, 0, true, 0);
            mf.mesh = mesh; // 20220404
        }

        // By default, a mesh allows only 64K vertices (or triangles?) in all its sub-meshes combined.
        // larger meshes need to set this flag BEFORE setting triangle data
        // These larger meshes might not be supported on some hardware
        private static void SetBigMeshFlag(Mesh mesh, int nv, int nt)
        {
            if (allowBigMeshes && (nv > 65535 || nt > 65535)) mesh.indexFormat = IndexFormat.UInt32;
        }

        NamedSubMesh[] LaneNamedSubMeshArray()
        {
            ILane[] lanes = App().Map().Lanes();
            int n = lanes.Length;

            NamedSubMesh[] sma = new NamedSubMesh[n];

            for (int li = 0; li < n; li++)
            {
                ILane lane = lanes[li];
                sma[li] = LocusFullNamedMesh(lane, lane, 0, 1, 0, MH.Lane);
            }

            return sma;
        }

        SubMesh LaneSubMesh()
        {
            ILane[] lanes = App().Map().Lanes();
            int n = lanes.Length;
            TriMesh[] tma = new TriMesh[n];

            for (int li = 0; li < n; li++) { tma[li] = LocusFullWidthTriMesh(lanes[li], MH.Lane); }

            return TriangleSubMesh(TriMesh.Combine(tma));
        }


        // The top surface of junctions, rendered as asphalt
        NamedSubMesh[] JunctionNamedSubMeshArray()
        {
            IJunction[] jna = App().Map().Junctions();
            NamedSubMesh[] nmoa = new NamedSubMesh[jna.Length];
            for (int i = 0; i < jna.Length; i++)
            {
                nmoa[i] = TriangleNamedSubMesh(jna[i], jna[i].UpperSurfaceTriMesh());
            }

            return nmoa;
        }

        SubMesh JunctionSubMesh()
        {
            List<TriMesh> tml = new List<TriMesh>();

            foreach (IJunction jn in App().Map().Junctions()) { tml.Add(jn.UpperSurfaceTriMesh()); }

            return TriangleSubMesh(TriMesh.Combine(tml.ToArray()));
        }

        NamedSubMesh[] TurnArrowNamedSubMeshArray()
        {
            List<NamedSubMesh> sml = new List<NamedSubMesh>();

            foreach (ILane ln in App().Map().Lanes())
            {
                TriMesh triMeshShaft = ln.TurnArrowShaftTriMesh();
                // If it has no turn arrows, triMeshShaft will be TriMesh.EMPTY_TRIMESH

                if (triMeshShaft.TriangleArray().Length > 0)
                {
                    IStreme[] soa = ln.GetStremeO();
                    TriMesh[] tma = new TriMesh[1 + soa.Length];
                    tma[0] = triMeshShaft;

                    for (int i = 0; i < soa.Length; i++) tma[1 + i] = soa[i].TurnArrowTriMesh();

                    sml.Add(TriangleNamedSubMesh(ln, TriMesh.Combine(tma)));
                }
            }

            return sml.ToArray();
        }

        SubMesh TurnArrowSubMesh()
        {
            List<TriMesh> tml = new List<TriMesh>();

            foreach (IStreme s in App().Map().Stremes()) tml.Add(s.TurnArrowTriMesh());
            foreach (ILane ln in App().Map().Lanes()) tml.Add(ln.TurnArrowShaftTriMesh());

            return TriangleSubMesh(TriMesh.Combine(tml.ToArray()));
        }

        NamedSubMesh[][] SignalHeadNamedSubMeshArray()
        {
            List<NamedSubMesh> smlb = new List<NamedSubMesh>();
            List<NamedSubMesh> smla = new List<NamedSubMesh>();

            IStreme[] stremes = App().Map().Stremes();

            foreach (IStreme s in stremes)
            {
                if (!s.GetJunction().IsSignalized()) continue;

                TriMesh[] shtma = s.SignalHeadTriMesh();
                if (shtma != null && shtma.Length == 4)
                {
                    smlb.Add(TriangleNamedSubMesh(s, shtma[0]));

                    int gmi = smla.Count;
                    stremeGreenMeshIndex.Put(s, gmi);

                    smla.Add(TriangleNamedSubMesh(s, shtma[1]));
                    smla.Add(TriangleNamedSubMesh(s, shtma[2]));
                    smla.Add(TriangleNamedSubMesh(s, shtma[3]));
                }
            }

            NamedSubMesh[][] sma = new NamedSubMesh[2][];
            sma[0] = smlb.ToArray();
            sma[1] = smla.ToArray();

            return sma;
        }

        // Signal arrows change colour, so they must have variable materials: we need one sub mesh per signal arrow
        SubMesh[] SignalArrowSubMeshArray()
        {
            List<SubMesh> smla = new List<SubMesh>();

            IStreme[] stremes = App().Map().Stremes();

            foreach (IStreme s in stremes)
            {
                if (!s.GetJunction().IsSignalized()) continue;

                TriMesh[] shtma = s.SignalHeadTriMesh();
                if (shtma != null && shtma.Length == 4)
                {
                    int gmi = smla.Count;
                    stremeGreenMeshIndex.Put(s, gmi);

                    smla.Add(TriangleSubMesh(shtma[1]));
                    smla.Add(TriangleSubMesh(shtma[2]));
                    smla.Add(TriangleSubMesh(shtma[3]));
                }
            }

            return smla.ToArray();
        }

        // The signal head boxes do not change state/colour, so can be safely combined int a single object
        SubMesh SignalBoxSubMesh()
        {
            List<TriMesh> tml = new List<TriMesh>();
            IStreme[] stremes = App().Map().Stremes();

            foreach (IStreme s in stremes)
            {
                if (!s.GetJunction().IsSignalized()) continue;

                TriMesh[] shtma = s.SignalHeadTriMesh();
                if (shtma != null && shtma.Length == 4) { tml.Add(shtma[0]); }
            }

            return TriangleSubMesh(TriMesh.Combine(tml.ToArray()));
        }

        // Returns a sub-mesh for each Red-Green crossing, each has its own material, which can be switched
        // to red or green as required
        NamedSubMesh[] WalkSignalNamedSubMeshArray()
        {
            List<NamedSubMesh> sml = new List<NamedSubMesh>();

            ICrossing[] crossings = App().Map().Crossings();

            foreach (ICrossing x in crossings)
            {
                if (x.IsRedGreen())
                {
                    TriMesh[] tma = x.SignalTriMesh();

                    if (tma != null && tma.Length == 2) // One mesh for each end of the crossing (direction of flow)
                    {
                        // crossingMeshIndex.Put(x, sml.Count);

                        sml.Add(TriangleNamedSubMesh(x, tma[0]));
                        sml.Add(TriangleNamedSubMesh(x, tma[1]));
                    }
                }
            }

            return sml.ToArray();
        }

        SubMesh[] WalkSignalSubMeshArray()
        {
            List<SubMesh> sml = new List<SubMesh>();

            ICrossing[] crossings = App().Map().Crossings();

            foreach (ICrossing x in crossings)
            {
                if (x.IsRedGreen())
                {
                    TriMesh[] tma = x.SignalTriMesh();

                    if (tma != null && tma.Length == 2) // One mesh for each end of the crossing (direction of flow)
                    {
                        crossingMeshIndex.Put(x, sml.Count);

                        sml.Add(TriangleSubMesh(x, tma[0]));
                        sml.Add(TriangleSubMesh(x, tma[1]));
                    }
                }
            }

            return sml.ToArray();
        }

        SubMesh[] StopSubMeshArray()
        {
            float dz = 0.05f;
            List<SubMesh> sml = new List<SubMesh>();

            IStop[] stops = App().Map().Stops();
            foreach (IStop s in stops)
            {
                if (s.IsAtKerb() && s.GetHalts().Length > 0)
                {
                    stopMeshIndex.Put(s, sml.Count);
                    AddTriangleSubMesh(sml, s.BusBayTriMesh(), dz);
                }
            }

            return sml.ToArray();
        }

        NamedSubMesh[] StopNamedSubMeshArray()
        {
            float dz = 0.05f;
            List<NamedSubMesh> sml = new List<NamedSubMesh>();

            IStop[] stops = App().Map().Stops();
            foreach (IStop stop in stops)
            {
                if (stop.IsAtKerb() && stop.GetHalts().Length > 0)
                {
                    stopMeshIndex.Put(stop, sml.Count);
                    sml.Add(TriangleNamedSubMesh(stop, stop.BusBayTriMesh(), dz));
                }
            }

            return sml.ToArray();
        }

        protected virtual Color StopColor(IStop stop)
        {
            return COLOR_DROP_OFF;
        }

        protected virtual bool StopColorCanChange()
        {
            return false;
        }

        SubMesh StopSubMesh()
        {
            float dz = 0.05f;
            List<TriMesh> tml = new List<TriMesh>();

            IStop[] stops = App().Map().Stops();
            foreach (IStop s in stops)
            {
                if (s.IsAtKerb() && s.GetHalts().Length > 0) { tml.Add(s.BusBayTriMesh()); }
            }

            return TriangleSubMesh(TriMesh.Combine(tml.ToArray()), dz);
        }

        protected void UpdateBusStopColours()
        {
            MeshRenderer mr = layerBusStops.GetComponent<MeshRenderer>();
            Material[] mra = mr.materials;

            foreach (IStop stop in App().Map().Stops())
            {
                if (stop.IsAtKerb())
                {
                    int xmi = stopMeshIndex.Get(stop);

                    Material mat = mra[xmi];
                    Color colorOld = mat.color;
                    Color colorNew = StopColor(stop);
                    if (!colorNew.Equals(colorOld)) { mat.color = colorNew; }
                }
            }
        }

        NamedSubMesh[] ParkingNamedSubMeshArray(bool dropOff)
        {
            float dz = 0.05f;
            List<NamedSubMesh> sml = new List<NamedSubMesh>();

            IDrvZone[] drvZones = App().Map().DrvZones();
            foreach (IDrvZone kz in drvZones)
            {
                if (kz is ITaxiZone vz && vz.IsDropOffOnly() == dropOff)
                {
                    TriMesh tm = vz.ParkingTriMesh();
                    sml.Add(TriangleNamedSubMesh(vz, tm, dz));
                }
            }

            return sml.ToArray();
        }

        SubMesh ParkingSubMesh(bool dropOff)
        {
            float dz = 0.05f;
            List<TriMesh> tml = new List<TriMesh>();

            IDrvZone[] drvZones = App().Map().DrvZones();
            foreach (IDrvZone kz in drvZones)
            {
                if (kz is ITaxiZone vz && vz.IsDropOffOnly() == dropOff) { tml.Add(vz.ParkingTriMesh()); }
            }

            return TriangleSubMesh(TriMesh.Combine(tml.ToArray()), dz);
        }

        private static bool buildingAdjusterCanBeAsynchronous = false;

        private static bool BuildingAdjusterInProgress()
        {
            if (buildingAdjusterCanBeAsynchronous) return false; // BuildingAdjuster.MostRecentInProgress();

            return false;
        }

        NamedSubMesh[] BuildingNamedMeshObjects(bool isGlass)
        {
            List<NamedSubMesh> nmol = new List<NamedSubMesh>();

            IOutline[] outlines = App().Map().Outlines();

            bool allGlass = BuildingAdjusterInProgress();

            foreach (IOutline ol in outlines)
            {
                if (ol.IsBuilding() && isGlass == (allGlass || ol.IsBuildingStraddlingRoad()))
                {
                    TriMesh walls = ol.WallsTriMesh();
                    TriMesh roof = ol.UpperSurfaceTriMesh();

                    nmol.Add(TriangleNamedSubMesh(ol, TriMesh.Combine(new[] {walls, roof,})));
                }
            }


            return nmol.ToArray();
        }

        SubMesh BuildingSubMesh(bool isGlass)
        {
            List<TriMesh> tml = new List<TriMesh>();
            IOutline[] outlines = App().Map().Outlines();

            bool allGlass = BuildingAdjusterInProgress();

            foreach (IOutline ol in outlines)
            {
                if (ol.IsBuilding() && isGlass == (allGlass || ol.IsBuildingStraddlingRoad()))
                {
                    tml.Add(ol.WallsTriMesh());

                    tml.Add(ol.UpperSurfaceTriMesh());
                }
            }

            return TriangleSubMesh(TriMesh.Combine(tml.ToArray()));
        }

        NamedSubMesh[] WaterNamedSubMeshArray()
        {
            List<NamedSubMesh> sml = new List<NamedSubMesh>();

            IOutline[] outlines = App().Map().Outlines();

            foreach (IOutline ol in outlines)
            {
                if (ol.IsWater()) { sml.Add(TriangleNamedSubMesh(ol, ol.UpperSurfaceTriMesh())); }
            }

            return sml.ToArray();
        }

        SubMesh WaterSubMesh()
        {
            List<TriMesh> tml = new List<TriMesh>();
            IOutline[] outlines = App().Map().Outlines();

            foreach (IOutline ol in outlines)
            {
                if (ol.IsWater()) { tml.Add(ol.UpperSurfaceTriMesh()); }
            }

            return TriangleSubMesh(TriMesh.Combine(tml.ToArray()));
        }

        SubMesh TerrainSubMesh()
        {
            return TriangleSubMesh(App().Map().Terrain().TriMesh(), 0);
        }

        // The bridge piers and under-surface of bridges, rendered in concrete
        NamedSubMesh[] BridgesNamedSubMeshArray()
        {
            List<NamedSubMesh> sml = new List<NamedSubMesh>();

            foreach (IRoad road in App().Map().Roads())
            {
                TriMesh triMesh = road.ShoulderTriMesh();
                MaterialHint mh = triMesh.GetMaterialHint();

                if (mh == MH.Bridge)
                {
                    List<TriMesh> tml = new List<TriMesh>();
                    tml.Add(road.BridgePiersTriMesh());

                    foreach (ILane lane in road.GetLanes()) { tml.Add(lane.UnderSurfaceTriMesh(road.Thickness())); }

                    IFootpath fpk = road.GetSideWalkK();
                    if (fpk != null) tml.Add(fpk.UnderSurfaceTriMesh(road.Thickness()));

                    IFootpath fpm = road.GetSideWalkM();
                    if (fpm != null) tml.Add(fpm.UnderSurfaceTriMesh(road.Thickness()));

                    sml.Add(TriangleNamedSubMesh(road, TriMesh.Combine(tml.ToArray())));
                }
            }

            foreach (IJunction jn in App().Map().Junctions())
            {
                if (jn.IsBridge()) { sml.Add(TriangleNamedSubMesh(jn, jn.UnderSurfaceTriMesh())); }
            }

            foreach (ICorner cr in App().Map().Corners())
            {
                TriMesh underSurface = cr.UnderSurfaceTriMesh();
                if (underSurface != null) sml.Add(TriangleNamedSubMesh(cr, underSurface));
            }

            return sml.ToArray();
        }

        // The bridge piers and under-surface of bridges, rendered in concrete
        SubMesh BridgesSubMesh()
        {
            List<TriMesh> tml = new List<TriMesh>();

            foreach (IRoad road in App().Map().Roads())
            {
                TriMesh triMesh = road.ShoulderTriMesh();
                MaterialHint mh = triMesh.GetMaterialHint();

                if (mh == MH.Bridge)
                {
                    tml.Add(road.BridgePiersTriMesh());

                    foreach (ILane lane in road.GetLanes()) { tml.Add(lane.UnderSurfaceTriMesh(road.Thickness())); }

                    IFootpath fpk = road.GetSideWalkK();
                    if (fpk != null) tml.Add(fpk.UnderSurfaceTriMesh(road.Thickness()));

                    IFootpath fpm = road.GetSideWalkM();
                    if (fpm != null) tml.Add(fpm.UnderSurfaceTriMesh(road.Thickness()));
                }
            }

            foreach (IJunction jn in App().Map().Junctions())
            {
                if (jn.IsBridge()) tml.Add(jn.UnderSurfaceTriMesh());
            }

            foreach (ICorner cr in App().Map().Corners())
            {
                TriMesh underSurface = cr.UnderSurfaceTriMesh();
                if (underSurface != null) tml.Add(underSurface);
            }

            return TriangleSubMesh(TriMesh.Combine(tml.ToArray()));
        }

        //
        NamedSubMesh[] ShouldersNamedSubMeshArray(MaterialHint mh1, MaterialHint mh2)
        {
            List<NamedSubMesh> sml = new List<NamedSubMesh>();

            foreach (IRoad road in App().Map().Roads())
            {
                TriMesh shoulderTriMesh = road.ShoulderTriMesh();
                MaterialHint mh = shoulderTriMesh.GetMaterialHint();

                if (mh == mh1 || mh == mh2) { sml.Add(TriangleNamedSubMesh(road, shoulderTriMesh)); }
            }

            foreach (IJunction jn in App().Map().Junctions())
            {
                TriMesh shoulderTriMesh = jn.ShoulderTriMesh();
                MaterialHint mh = shoulderTriMesh.GetMaterialHint();

                if (mh == mh1 || mh == mh2) { sml.Add(TriangleNamedSubMesh(jn, shoulderTriMesh)); }
            }

            foreach (ICorner cr in App().Map().Corners())
            {
                TriMesh shoulderTriMesh = cr.ShoulderTriMesh();
                MaterialHint mh = shoulderTriMesh.GetMaterialHint();

                if (mh == mh1 || mh == mh2) { sml.Add(TriangleNamedSubMesh(cr, shoulderTriMesh)); }
            }

            return sml.ToArray();
        }

        SubMesh ShouldersSubMesh(MaterialHint mh1, MaterialHint mh2)
        {
            List<TriMesh> tml = new List<TriMesh>();

            foreach (IRoad road in App().Map().Roads())
            {
                TriMesh shoulderTriMesh = road.ShoulderTriMesh();
                MaterialHint mh = shoulderTriMesh.GetMaterialHint();

                if (mh == mh1 || mh == mh2) { tml.Add(shoulderTriMesh); }
            }


            foreach (IJunction jn in App().Map().Junctions())
            {
                TriMesh shoulderTriMesh = jn.ShoulderTriMesh();
                MaterialHint mh = shoulderTriMesh.GetMaterialHint();

                if (mh == mh1 || mh == mh2) { tml.Add(shoulderTriMesh); }
            }

            foreach (ICorner cr in App().Map().Corners())
            {
                TriMesh shoulderTriMesh = cr.ShoulderTriMesh();
                MaterialHint mh = shoulderTriMesh.GetMaterialHint();

                if (mh == mh1 || mh == mh2) { tml.Add(shoulderTriMesh); }
            }

            return TriangleSubMesh(TriMesh.Combine(tml.ToArray()));
        }

        private void AddTriangleSubMesh(List<SubMesh> sml, TriMesh triMesh)
        {
            sml.Add(TriangleSubMesh(triMesh));
        }

        protected void AddTriangleSubMesh(List<SubMesh> sml, TriMesh triMesh, float dz)
        {
            sml.Add(TriangleSubMesh(triMesh, dz));
        }

        public static SubMesh LocusSubMesh(ILocus locus, float start, float end, float kw, float mw, float dh,
            MaterialHint mh)
        {
            return TriangleSubMesh(locus.UpperSurfaceTriMesh(start, end, kw, mw, mh), dh);
        }

        public static TriMesh LocusTriMesh(ILocus locus, float start, float end, float kw, float mw, MaterialHint mh)
        {
            return locus.UpperSurfaceTriMesh(start, end, kw, mw, mh);
        }

        private static NamedSubMesh LocusFullNamedMesh(object mapObject, ILocus locus, float kw, float mw, float dh,
            MaterialHint mh)
        {
            return (NamedSubMesh) TriangleSubMesh(mapObject, locus.UpperSurfaceTriMesh(0, 1, kw, mw, mh), dh);
        }

        private static SubMesh LocusFullSubMesh(ILocus locus, float kw, float mw, float dh, MaterialHint mh)
        {
            return TriangleSubMesh(locus.UpperSurfaceTriMesh(0, 1, kw, mw, mh), dh);
        }

        public static TriMesh LocusFullTriMesh(ILocus locus, float kw, float mw, MaterialHint mh)
        {
            return locus.UpperSurfaceTriMesh(0, 1, kw, mw, mh);
        }

        private static TriMesh LocusFullWidthTriMesh(ILocus locus, MaterialHint mh)
        {
            return locus.UpperSurfaceTriMesh(0, 1, 0, 1, mh);
        }

        public static TriMesh LocusStripedTriMesh(ILocus locus, int nStripes, MaterialHint mh)
        {
            if (nStripes < 2) nStripes = 2;
            TriMesh[] tma = new TriMesh[nStripes];
            double sw = 1.0 / (nStripes + (nStripes - 1)); // There are (nStripes - 1) gaps between stripes
            double gw = sw;
            for (int si = 0; si < nStripes; si++)
            {
                double kw = (sw + gw) * si;
                double mw = kw + sw;

                tma[si] = locus.UpperSurfaceTriMesh(0, 1, kw, mw, mh);
            }

            return TriMesh.Combine(tma);
        }

        private static void CheckValid(Vector3 v)
        {
            if (float.IsInfinity(v.x) || float.IsNaN(v.x)) { v.x = 0; }

            if (float.IsInfinity(v.y) || float.IsNaN(v.y)) { v.y = 0; }

            if (float.IsInfinity(v.z) || float.IsNaN(v.z)) { v.z = 0; }
        }

        private static void CheckValid(Vector2 v)
        {
            if (float.IsInfinity(v.x) || float.IsNaN(v.x)) { v.x = 0; }

            if (float.IsInfinity(v.y) || float.IsNaN(v.y)) { v.y = 0; }
        }

        public static SubMesh TriangleSubMesh(List<TriMesh> tml, float dh, int mati)
        {
            return TriangleSubMesh(TriMesh.Combine(tml.ToArray()), dh, mati);
        }

        public static SubMesh TriangleSubMesh(TriMesh triMesh, float dh = 0, int mati = 0)
        {
            return TriangleSubMesh(null, triMesh, dh, mati);
        }

        public static NamedSubMesh TriangleNamedSubMesh(object mapObject, TriMesh triMesh, float dh = 0, int mati = 0)
        {
            return (NamedSubMesh) TriangleSubMesh(mapObject, triMesh, dh, mati);
        }

        public static SubMesh TriangleSubMesh(object mapObject, TriMesh triMesh, float dh = 0, int mati = 0)
        {
            if (triMesh == null) triMesh = TriMesh.EMPTY_TRIMESH;

            int[] tvia = triMesh.TriangleCornerIndices();
            Xyz[] tva = triMesh.Vertices();
            Xyz[] vna = triMesh.Normals();
            Xy[] texUV = triMesh.TextureUV();

            int nv = tva.Length;

            Vector3[] va = new Vector3[nv];
            Vector2[] uva = new Vector2[nv];
            Vector3[] na = new Vector3[nv];

            for (int vi = 0; vi < nv; vi++)
            {
                va[vi] = tva[vi].ToVector3(dh);
                uva[vi] = texUV[vi].ToVector2();
                na[vi] = vna[vi].ToVector3();

                CheckValid(va[vi]);
                CheckValid(uva[vi]);
                CheckValid(na[vi]);
            }

            if (mapObject == null) return new SubMesh(va, tvia, na, uva, mati);

            return new NamedSubMesh(mapObject, va, tvia, na, uva, mati);
        }

        // All footpaths and corners
        NamedSubMesh[] FootpathNamedSubMeshArray()
        {
            IFootpath[] footpaths = App().Map().Footpaths();

            List<NamedSubMesh> smList = new List<NamedSubMesh>();

            foreach (IFootpath fp in footpaths) { smList.Add(LocusFullNamedMesh(fp, fp, 0, 1, 0, MH.Walkway)); }

            foreach (ICorner cr in App().Map().Corners())
            {
                smList.Add(TriangleNamedSubMesh(cr, cr.UpperSurfaceTriMesh()));
            }

            return smList.ToArray();
        }

        SubMesh FootpathSubMesh()
        {
            List<TriMesh> tml = new List<TriMesh>();

            foreach (IFootpath fp in App().Map().Footpaths()) { tml.Add(LocusFullWidthTriMesh(fp, MH.Walkway)); }

            foreach (ICorner cr in App().Map().Corners())
            {
                if (cr == null) continue;
                tml.Add(cr.UpperSurfaceTriMesh());
            }

            return TriangleSubMesh(TriMesh.Combine(tml.ToArray()));
        }

        // All crossings except Zebras
        NamedSubMesh[] CrossingNamedSubMeshArray()
        {
            List<NamedSubMesh> smList = new List<NamedSubMesh>();

            foreach (ICrossing x in App().Map().Crossings())
            {
                if (!x.IsZebra()) smList.Add(LocusFullNamedMesh(x, x, 0, 1, 0, MH.Crossing));
            }

            return smList.ToArray();
        }

        // All crossings except Zebras
        SubMesh CrossingSubMesh()
        {
            List<TriMesh> tml = new List<TriMesh>();

            foreach (ICrossing x in App().Map().Crossings())
            {
                if (!x.IsZebra()) tml.Add(LocusFullWidthTriMesh(x, MH.Crossing));
            }

            return TriangleSubMesh(TriMesh.Combine(tml.ToArray()));
        }

        // Zebra crossings only
        NamedSubMesh[] ZebraNamedSubMeshArray()
        {
            List<NamedSubMesh> smList = new List<NamedSubMesh>();
            foreach (ICrossing x in App().Map().Crossings())
            {
                if (x.IsZebra()) smList.Add(LocusFullNamedMesh(x, x, 0, 1, 0, MH.Zebra));
            }

            return smList.ToArray();
        }

        SubMesh ZebraSubMesh()
        {
            List<TriMesh> tml = new List<TriMesh>();

            foreach (ICrossing x in App().Map().Crossings())
            {
                if (x.IsZebra()) tml.Add(LocusFullWidthTriMesh(x, MH.Zebra));
            }

            return TriangleSubMesh(TriMesh.Combine(tml.ToArray()));
        }

        private static readonly float TAXI_HEIGHT = 2.0f;
        private static readonly float PARKING_ORIGIN_PYRAMID_HEIGHT = 4f;
        private static readonly float PARKING_ORIGIN_PYRAMID_RADIUS = 2f;
        private static readonly float PARKING_DESTINATION_PYRAMID_HEIGHT = 50f;
        private static readonly float PARKING_DESTINATION_PYRAMID_RADIUS = 10f;

        public static void ParkingOriginTriMeshes(List<TriMesh> triMeshList, ITaxiZone taxiZone)
        {
            int nMarkers = taxiZone.Bays();

            for (int i = 0; i < nMarkers; i++)
            {
                Xyz p = taxiZone.BayCentre(i).PlusZ(TAXI_HEIGHT);
                triMeshList.Add(MeshTools.PyramidTriMesh(p, PARKING_ORIGIN_PYRAMID_HEIGHT,
                    PARKING_ORIGIN_PYRAMID_RADIUS));
            }
        }

        public static TriMesh ParkingDestinationTriMesh(IDrvZone drvZone)
        {
            Xyz p = drvZone.Location().PlusZ(TAXI_HEIGHT);

            return MeshTools.PyramidTriMesh(p, PARKING_DESTINATION_PYRAMID_HEIGHT, PARKING_DESTINATION_PYRAMID_RADIUS);
        }

        private static readonly float BOARD_STAND_PYRAMID_HEIGHT = 4f;
        private static readonly float BOARD_STAND_PYRAMID_RADIUS = 2f;
        private static readonly float BAIL_STAND_PYRAMID_HEIGHT = 50f;
        private static readonly float BAIL_STAND_PYRAMID_RADIUS = 10f;
        private static readonly float BUS_HEIGHT = 2.2f;

        public static TriMesh StopTriMesh(IStop stop, bool boarding)
        {
            Xyz p = stop.Position().PlusZ(BUS_HEIGHT);

            if (boarding) return MeshTools.PyramidTriMesh(p, BOARD_STAND_PYRAMID_HEIGHT, BOARD_STAND_PYRAMID_RADIUS);
            return MeshTools.PyramidTriMesh(p, BAIL_STAND_PYRAMID_HEIGHT, BAIL_STAND_PYRAMID_RADIUS);
        }

#if UNITY_EDITOR

        private void SaveMesh(string meshPath, GameObject meshGO, string meshName)
        {
            string meshAssetPath = meshPath + meshName + SA.SUFFIX_ASSET; // a relative name
            
            MeshFilter mf = meshGO.GetComponent<MeshFilter>();
            Mesh mesh = mf.sharedMesh;
            
            // We don't use mesh colliders in the simulation, but they might be used in a prefab
            // Make sure the mesh collider has a reference to the mesh here
            MeshCollider mc = meshGO.GetComponent<MeshCollider>();
            if (mc != null) mc.sharedMesh = mesh;

            UnityEditor.MeshUtility.Optimize(mesh);

            UnityEditor.AssetDatabase.CreateAsset(mesh, meshAssetPath);
        }
        public string SaveMapMesh(bool separateMeshes)
        {
            string name = App().Map().GetSuburb();
            string meshStem = MeshTools.VRoadRoot() + SC.FS + SA.MESH_GEN_DIR + SC.FS + name;
            string meshPath = meshStem + SC.FS;
            int suffix = 1;
            KDir meshDir = new KDir(meshPath);

            while (meshDir.Exists())
            {
                suffix++;
                meshPath = meshStem + suffix + SC.FS;
                meshDir = new KDir(meshPath);
            }
            meshDir.Create();
            
            UnityEditor.AssetDatabase.StartAssetEditing();
            
            foreach (GameObject layerGO in layerGOs)
            {
                int nc = layerGO.transform.childCount;

                if (separateMeshes && nc > 0)
                {
                    for (int ci = 0; ci < nc; ci++)
                    {
                        GameObject childGO = layerGO.transform.GetChild(ci).gameObject;
                        string meshName = KFormat.Sprintf(SA.PREFAB_MESH_NAME_FMT, layerGO.name, ci);
                        SaveMesh(meshPath, childGO, meshName);
                    }
                }
                else
                {
                   SaveMesh(meshPath, layerGO, layerGO.name);
                }
            }

            UnityEditor.AssetDatabase.StopAssetEditing();
            
            return meshPath;
        }

        public string SaveMapAsPrefab()
        {
            GameObject mapMesh = layerLanes.transform.parent.gameObject;
            string name = App().Map().GetSuburb();

            string prefabStem = MeshTools.VRoadRoot() + SC.FS + SA.PREFAB_GEN_DIR + SC.FS + name;
            string prefabPath = prefabStem + SA.SUFFIX_PREFAB;
            KFile file = new KFile(prefabPath);
            int suffix = 1;
            
            while (file.Exists())
            {
                suffix++;
                prefabPath = prefabStem + suffix + SA.SUFFIX_PREFAB;
                file = new KFile(prefabPath);
            }
            
            UnityEditor.PrefabUtility.SaveAsPrefabAsset(mapMesh, prefabPath);
            
            return prefabPath;
        }
#endif
    }
}