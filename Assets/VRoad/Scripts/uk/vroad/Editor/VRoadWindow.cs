using System;
using System.Linq;
using uk.vroad.api;
using uk.vroad.api.etc;
using uk.vroad.api.geom;
using uk.vroad.api.map;
using uk.vroad.api.sim;
using uk.vroad.api.str;
using uk.vroad.api.xmpl;
using uk.vroad.apk;
using uk.vroad.pac;
using uk.vroad.ucm;
using uk.vroad.xmpl;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace uk.vroad.Editor
{
    #region STRING_CONSTANTS
    public static class EDSC // Editor string constants, for translation (see note on Babyl, at end)
    {
        public const string MENU_TITLE = "Tools/Global Roads \uff06 Traffic";
        public const string WINDOW_TITLE = "Global Roads \uff06 Traffic";
        public const string TAB_BUILD = "Build";
        public const string TAB_REBUILD = "Rebuild";
        public const string TAB_TRAFFIC =  "Traffic";
        public const string TAB_OPTIONS =  "Options";
        public const string TAB_PRO = "[ Pro ]";
        public const string TAB_LITE = "[ Lite ]";
        public const string ACTION_BUILD = ""; // "Build Map";
        public const string ACTION_REBUILD = "To Rebuild Map";
        public const string ACTION_TRAFFIC = ""; // "Run Traffic";
        public const string ACTION_PREFAB = "Save as Prefab";
        public const string TOOLTIP_BUILD = "Download OSM Data and Write a V-Road Map File";
        public const string TOOLTIP_REBUILD = "Read a cached OSM File and Write a V-Road Map File";
        public const string TOOLTIP_TRAFFIC = "Read a V-Road Map File and Run Traffic";
        public const string HINT_BUILD = "Map Centre and Area Not Specified";
        public const string HINT_REBUILD = "No OSM File Selected";
        public const string HINT_TRAFFIC = "No V-Road File Selected";
        public const string HINT_PREFAB = "Save as Prefab when Built";
        public const string ALREADY_PREFAB = "Already Saved Prefab";
        public const string HINT_FORMAT = "[ %s ]";
        
        public const string OSM_FILE_DESC = "Open Cached OSM JSON";
        public const string VROAD_FILE_DESC = "Open VRoad File";
        public const string MAP_CENTRE = "Map Centre (Lat, Long)";
        public const string MAP_AREA = "Map Area (Ha)";
        public const string MAX_AREA = "Maximum Map Area (Ha)";
        public const string MAP_SIZE_E = "Map Size East (m)";
        public const string MAP_SIZE_N = "Map Size North (m)";
        public const string HINT_OSM = "(select OSM File)";
        public const string HINT_VROAD = "(select V-Road File)";

        public const string MAP_SUBURB  = "Suburb";
        public const string MAP_CITY    = "City";
        public const string MAP_COUNTRY = "Country";

        public const string UI_POLL = "Locate map with browser"; 
        public const string UI_POLL_TT = "Poll the system clipboard for a (latitude, longitude) location. " +
                                         "If the V-Road map server is not available, you can use any web map that " +
                                         "copies to clipboard, such as maps.google.com / right-click.\n\n" +
                                         "Once you have (latitude, longitude), switch this toggle off to enter " +
                                         "required area and map size manually below."; 
        public const string UI_LAUNCH = "Launch Browser";
        public const string UI_MISSING = "Missing from Scene: %s";
        public const string UI_SAVE_MESHES = "Saved Meshes to: %s";
        public const string UI_SAVE_MESHES_TERRAIN = "Saved Meshes and Terrain to: %s";
        public const string UI_SAVE_PREFAB = "Saved Map to: %s";
        public const string UI_SAVE_PREFAB_FAIL = "Failed to save prefab: ";
        public const string UI_NO_TERRAIN_ = "Free Terrain Tiles Used Up. New Terrain will be Flat. \n";
        public const string UI_NO_TERRAIN_BUILD =  UI_NO_TERRAIN_ + "Enter Mapbox token in Options Tab.";
        public const string UI_NO_TERRAIN_OPTS = UI_NO_TERRAIN_ + "Enter Mapbox token above.";

        public const string USER_AGENT = "User-Agent for Download";
        public const string TAXIS_BUSES = "Taxis & Buses";
        public const string SAMPLE_SOURCE_CODE = "Sample Source Code";
        
        public const string TRAFFIC_VAR = "Traffic Variation";
        public const string RUNNING = "Running";
        public const string ON = "Available";
        public const string OFF = "Not Available";

        public const string CREATE_MULTIPLE_MESHES = "Create Multiple Meshes";
        public const string CREATE_MULTIPLE_MESHES_TT = 
            "Multiple meshes use more resources, but make it easier to edit that layer. " +
            "For example, you may want to delete some buildings and keep others.";
        
        public const string CREATE_UNITY_TERRAIN = "Create Unity Terrain";
        public const string CREATE_UNITY_TERRAIN_TT =
            "Create a Unity terrain object in addition to the V-Road terrain surface. " +
            "The Unity terrain object can be edited with Unity's terrain tools, " +
            "to raise or lower levels, add trees and create holes. The V-Road terrain " +
            "surface already has holes for tunnels and for water.";
        
        public const string DIVISION_ABUNDANCE = "Car,Coach,Truck %";
        public const string DIVISION_ABUNDANCE_TT = 
            "Set the abundance values of each class of vehicles. For example: " +
            "60,20,20 will create 60% cars, 20% coaches, 20% trucks";
        
        public const string MAPBOX_KEY = "Mapbox Token";
        public const string MAPBOX_KEY_TT = 
            "Paste your map box token here, to download terrain tiles. " +
            "Get your own token (for free) by registering on mapbox.com";
        
        public const string FREE_TILES = "Tiles Remaining";
        public const string FREE_TILES_TT = 
            "Number of free terrain tiles remaining before registration with Mapbox is required";
        
        public const string PLAY_SCENE = "▷  Play Scene '%s' %s";
        public const string OPEN_SCENE = "Open Scene '%s' to Activate";

        public const string OPEN_SCENE_ON_TAB_CHANGE = "Auto-Open Scene for Tab";
        public const string OPEN_SCENE_ON_TAB_CHANGE_TT =
            "Automatically close the current scene and open the scene required for the selected tab";
        
        
        
        // The constants in this class could be converted into an enum and a text file, e.g.
        // enum EditorBabylKey { WINDOW_TITLE, ... }   and text file line: WINDOW_TITLE = "Virtual Road"
        //
        // then use Babyl.translate(EditorBabylKey.WINDOW_TITLE)  instead of WINDOW_TITLE
        //
        // Keys need to be unique in all tex files, there is no 'namespace' equivalent
    }

    public static class EDSF // Editor string constants, NO translation
    {
        public const string SCENE_PATH_REL = "Scenes/";
        public const string SCENE_BUILD = "BuildMap";
        public const string SCENE_TRAFFIC = "RunTraffic";
        public const string SCENE_SUFFIX = ".unity";
        public const string SUFFIX_OSM = "json";
        public const string SUFFIX_VROAD = "vroad";
        public const string STYLE_TEXTFIELD = "TextField";
        
        public const string REF_BUILDMAP = "UBuildMap ";
        public const string REF_RUNTRAFFIC = "URunTraffic ";
        public const string REF_MAPMESH = "UaMapMesh ";
        public const string REF_APPSTATE = "UaAppStateHandler ";
        public const string REF_EXIT = "UExitHandler ";
        public const string REF_MOUSE = "UaMouse ";
        public const string REF_CAM = "UaCamControllerMain ";
        
        public const string SLIPPY_MAP_URL_PARAMS = "http://vroad.uk/sm/?lat=%.6f&lng=%.6f&zoom=%.2f";
        public const string PREFS_BROWSER_LAT = "BrowserLat";
        public const string PREFS_BROWSER_LNG = "BrowserLng";
        public const string PREFS_AUTO_OPEN_SCENE = "AutoOpenScene";
    }
    #endregion
   
    public class VRoadWindow : EditorWindow
    {
        [MenuItem(EDSC.MENU_TITLE)]
        static void Init() // To call this after script edit, see forceInitOnScriptReload below 
        {
            KEnv.Awake();
            //Type nextToType = Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll");
            //VRoadWindow window = GetWindow<VRoadWindow>(new Type[] { nextToType, });
            VRoadWindow window = GetWindow<VRoadWindow>();
            window.SetupReferencesInEditMode();
            window.ClearBuildMap();
            window.ClearRunTraffic();
            window.titleContent = new GUIContent(EDSC.WINDOW_TITLE);
            window.InitUA();
            window.Show();

            MeshTools.VRoadRoot(); // Check VRoad package location relative to Assets

            InitScenes();
        }

        private static void InitScenes()
        {
            EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;
            SceneManager.activeSceneChanged += OnSceneChanged;

            int sc = SceneManager.loadedSceneCount;
            if (sc > 0) currentScene = EditorSceneManager.GetSceneAt(0).name;
            
            // Debug.Log(sc == 0? "No Scenes Loaded on Init()":"Current Scene on Init(): "+currentScene);
        }
        
        // Set this to force re-init after editing any script in IDE. By default, this is false, because
        // when editing some other script, (not VRoadWindow), we don't really want to force a call to KEnv.Awake() etc 
        private static bool forceInitOnScriptReload = false; 

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            if (forceInitOnScriptReload) Init();
            else InitScenes();
        }


        private const bool CAN_REBUILD = true;

        private const int ITAB_BUILD = 0;
        private const int ITAB_REBUILD = CAN_REBUILD? 1: -1;
        private const int ITAB_TRAFFIC = CAN_REBUILD? 2: 1;
        private const int ITAB_OPTIONS = CAN_REBUILD? 3: 2;
        private const int ITAB_VARIANT = CAN_REBUILD? 4: 3;
            
        private static readonly string[] tabs4 = { EDSC.TAB_BUILD, EDSC.TAB_TRAFFIC, EDSC.TAB_OPTIONS, SC.N,  };
        private static readonly string[] tabs5 = { EDSC.TAB_BUILD, EDSC.TAB_REBUILD, EDSC.TAB_TRAFFIC, EDSC.TAB_OPTIONS, SC.N, };
        private static readonly string[] tabs = CAN_REBUILD ? tabs5 : tabs4;

        private static readonly string[] actions4 = { EDSC.ACTION_BUILD, EDSC.ACTION_TRAFFIC,SC.N,SC.N, };
        private static readonly string[] actions5 = { EDSC.ACTION_BUILD, EDSC.ACTION_REBUILD, EDSC.ACTION_TRAFFIC,SC.N,SC.N, };
        private static readonly string[] actions = CAN_REBUILD ? actions5 : actions4;

        private static readonly string[] tooltips4 = { EDSC.TOOLTIP_BUILD, EDSC.TOOLTIP_TRAFFIC,SC.N,SC.N, };
        private static readonly string[] tooltips5 = { EDSC.TOOLTIP_BUILD, EDSC.TOOLTIP_REBUILD, EDSC.TOOLTIP_TRAFFIC,SC.N,SC.N, };
        private static readonly string[] tooltips = CAN_REBUILD ? tooltips5 : tooltips4;

        private static readonly string[] hints4 = { EDSC.HINT_BUILD, EDSC.HINT_TRAFFIC,SC.N,SC.N, };
        private static readonly string[] hints5 = { EDSC.HINT_BUILD, EDSC.HINT_REBUILD, EDSC.HINT_TRAFFIC,SC.N,SC.N, };
        private static readonly string[] hints = CAN_REBUILD ? hints5 : hints4;

        private const int helpBoxHeight = 32;
        private const float marginX = 8;
        private const float lineY = 16;

        private static string currentScene = SC.N;
        private static bool openSceneOnTabChange;
        private static bool hasSceneChanged;

        private string missingReferences = SC.N;
        
        private UBuildMap buildMap;
        private URunTraffic runTraffic;
        private UMapMeshExample mapMesh;
        private UPlaySimExample playSim;
        private App app;
        
        private bool isPlaying;
        private int traffic;
        
        private IBoundsBox boundsBoxFromBrowser;
        private string clipboardCache = SC.N;
        private string mapLatLongStr;
        private string mapCentreLatLong;
        private string mapSizeEM;
        private string mapSizeNM;
        private string mapAreaHa;
        private bool initGUI;
        private bool pollClipboard;
        private bool validVRoadFileSelected;
        private bool validOSMFileSelected;
        private bool createMultipleMeshes;
        private bool createUnityTerrain;
        private bool savedPrefab;
        private int activeTab;
        private int playTab = -1;
        private string osmFilePath;
        private string osmFileName;
        private string vroadFilePath;
        private string vroadFileName;
        private string vroadDivisionWeights;
        private string savedFiles;
        private string mapBoxKey;
        
        private bool checkOpenScene;
        private bool playActionEnabled;
        private bool freeTilesUsedUp;

        void InitUA()
        {
            VRoad.InitUser(CloudProjectSettings.userName);
            savedFiles = null;
            clipboardCache = SC.N;
            initGUI = true;
            openSceneOnTabChange = PlayerPrefs.GetInt(EDSF.PREFS_AUTO_OPEN_SCENE) > 0;
        }

        void SetupReferencesInEditMode()
        {
            missingReferences = SC.N;

            if (activeTab == ITAB_TRAFFIC)
            {
                URunTraffic[] runTraffics = Resources.FindObjectsOfTypeAll<URunTraffic>();
                if (runTraffics.Length == 1) runTraffic = runTraffics[0];
                else missingReferences += EDSF.REF_RUNTRAFFIC;
            }
            else
            {
                UBuildMap[] buildMaps = Resources.FindObjectsOfTypeAll<UBuildMap>();
                if (buildMaps.Length == 1) { buildMap = buildMaps[0]; }
                else missingReferences += EDSF.REF_BUILDMAP;
            }
          

            UMapMeshExample[] mapMeshes = Resources.FindObjectsOfTypeAll<UMapMeshExample>();
            if (mapMeshes.Length == 1) { mapMesh = mapMeshes[0]; }
            else missingReferences += EDSF.REF_MAPMESH;

            UaStateHandler[] stateHandlers = Resources.FindObjectsOfTypeAll<UaStateHandler>();
            if (stateHandlers.Length != 1) missingReferences += EDSF.REF_APPSTATE;

            UExitHandler[] exitHandlers = Resources.FindObjectsOfTypeAll<UExitHandler>();
            if (exitHandlers.Length != 1) missingReferences += EDSF.REF_EXIT;

            UaMouse[] mouseHandlers = Resources.FindObjectsOfTypeAll<UaMouse>();
            if (mouseHandlers.Length != 1) missingReferences += EDSF.REF_MOUSE;

            UaCamControllerMain[] camControllers = Resources.FindObjectsOfTypeAll<UaCamControllerMain>();
            if (camControllers.Length != 1) missingReferences += EDSF.REF_CAM;

          
            UPlaySimExample[] playSims = Resources.FindObjectsOfTypeAll<UPlaySimExample>();
            if (playSims.Length == 1) playSim = playSims[0];
        }

        void SetupReferencesInPlayMode()
        {
            missingReferences = SC.N;

            if (activeTab == ITAB_TRAFFIC)
            {
                buildMap = null;
                runTraffic = URunTraffic.MostRecentInstance;
                if (runTraffic == null) missingReferences += EDSF.REF_RUNTRAFFIC;
            }
            else
            {
                runTraffic = null;
                buildMap = UBuildMap.MostRecentInstance;
                if (buildMap == null) missingReferences += EDSF.REF_BUILDMAP;
            }
           
            mapMesh = UMapMeshExample.MostRecentInstance;
            if (mapMesh == null) missingReferences += EDSF.REF_MAPMESH;
            else
            {
                mapMesh.CreateMultipleMeshes = this.createMultipleMeshes;
                mapMesh.CreateUnityTerrain = this.createUnityTerrain;
            }

            if (UaStateHandler.MostRecentInstance == null) missingReferences += EDSF.REF_APPSTATE;
            if (UaMouse.MostRecentInstance == null) missingReferences += EDSF.REF_MOUSE;
            if (UaCamControllerMain.MostRecentInstance == null) missingReferences += EDSF.REF_CAM;

            
            playSim = UPlaySimExample.MostRecentInstance;
            traffic = 0;

        }

        void ClearBuildMap()
        {
            if (buildMap != null) buildMap.SetupBuild(null, null);
        }
        void ClearRunTraffic()
        {
            if (runTraffic != null) runTraffic.SetupTraffic(null);
        }
        bool AllReferencesFound() { return missingReferences.Length == 0; }

        private string SceneForActiveTab()
        {
            return activeTab == ITAB_BUILD || activeTab == ITAB_REBUILD? EDSF.SCENE_BUILD :
                activeTab == ITAB_TRAFFIC ? EDSF.SCENE_TRAFFIC :  null;
        }

        // This will be fired when scene changes in editor
        static void OnSceneChanged(Scene oldScene, Scene newScene)
        {
            currentScene = newScene == null? SC.N: newScene.name;
            hasSceneChanged = true;
            //Debug.Log("Current Scene now: "+currentScene);
        }

       
        void OnGUI()
        {
            bool isPro = VRoad.GotPro(); // Cannot call this in static / constructor code above

            string tabVariant = isPro ? EDSC.TAB_PRO : EDSC.TAB_LITE;
            tabs[ITAB_VARIANT] = tabVariant;

            int prevTab = activeTab; // GUI.changed returns true when you click on the SAME tab again
            activeTab = GUILayout.Toolbar(activeTab, tabs);
            bool activeTabChanged = activeTab != prevTab; // not the same as GUI.changed
            bool nowPlaying = EditorApplication.isPlaying;
            
           

            if (openSceneOnTabChange)
            {
                string expected = SceneForActiveTab();

                if (checkOpenScene && !nowPlaying)
                {
                    checkOpenScene = false;

                    if (currentScene != null && expected != null && !currentScene.Equals(expected))
                    {
                        string sceneDir = MeshTools.VRoadRoot() + SC.FS + EDSF.SCENE_PATH_REL;
                        EditorSceneManager.OpenScene(sceneDir + expected + EDSF.SCENE_SUFFIX);

                        hasSceneChanged = true;
                        //currentScene = expected;
                    }
                }

                if (activeTabChanged)
                {
                    if (nowPlaying)
                    {
                        bool willChange = currentScene != null && expected != null && !currentScene.Equals(expected);

                        if (willChange) // Stop playing on change of tab, if scene will change
                        {
                            EditorApplication.isPlaying = false;
                            nowPlaying = false;
                        }
                    }

                    checkOpenScene = true; // next time round
                }
            }

            if (activeTabChanged) savedFiles = null;
            
            // When Editor starts to Play, references are lost
            bool playStateChanged = false;
            if (nowPlaying != isPlaying)
            {
                isPlaying = nowPlaying;
                playStateChanged = true;

                if (!isPlaying) VRoad.FreeTilesClearCache();
            }

            if (playStateChanged || hasSceneChanged || activeTabChanged)
            {
                if (isPlaying) SetupReferencesInPlayMode();

                else
                {
                    SetupReferencesInEditMode();

                    if (AllReferencesFound())
                    {
                        bool inBuildScene = EDSF.SCENE_BUILD.Equals(currentScene);
                        bool inTrafficScene = EDSF.SCENE_TRAFFIC.Equals(currentScene);

                        if (activeTab == ITAB_BUILD && inBuildScene) SetupBuildMap(SC.N, mapLatLongStr);
                        else if (activeTab == ITAB_REBUILD && inBuildScene) SetupBuildMap(osmFileName, SC.N);
                        else if (activeTab == ITAB_TRAFFIC && inTrafficScene) SetupRunTraffic(vroadFileName);
                        else
                        {
                            SetupBuildMap(SC.N, SC.N);
                            SetupRunTraffic(SC.N);
                        }
                    }

                    // For simpler presentation of options, switch off traffic on tabs other than Traffic Tab.
                    // (To run traffic on another tab ensure there is a PlaySim Object and comment out this line)
                    if (playSim != null) playSim.RunTraffic(activeTab == ITAB_TRAFFIC);
                }
            }

            float line = 2f;
            float nl = 6f;
            float columnWidth = Math.Min(300, position.width - (2 * marginX));
            double maxArea = isPro ? MapConstants.BBOX_MAX_AREA_PRO : MapConstants.BBOX_MAX_AREA_LITE;

            playActionEnabled = false;

            switch (activeTab)
            {
                case ITAB_BUILD:
                {
                    GUILayout.BeginArea(areaRect(line, nl, columnWidth), EditorStyles.helpBox);
                    {
                        GUILayout.BeginHorizontal();
                        pollClipboard = EditorGUILayout.Toggle(new GUIContent(EDSC.UI_POLL, EDSC.UI_POLL_TT), 
                            pollClipboard, GUILayout.MaxWidth(columnWidth - 120));
                        if (GUILayout.Button(EDSC.UI_LAUNCH)) OpenSlippyMapInBrowser();
                        GUILayout.EndHorizontal();

                        EditorGUILayout.Space(lineY);

                        bool applyChange = hasSceneChanged || initGUI;

                        if (pollClipboard) //when polling clipboard, text fields can not be edited
                        {
                            GUI.enabled = true;
                            NoEditTextField(EDSC.MAP_CENTRE, mapCentreLatLong, columnWidth);
                            NoEditTextField(EDSC.MAP_AREA, mapAreaHa, columnWidth);
                            NoEditTextField(EDSC.MAP_SIZE_E, mapSizeEM, columnWidth);
                        }
                        else
                        {
                            GUI.enabled = true; // are text fields editable
                            GUI.changed = false;
                            mapCentreLatLong = EditorGUILayout.DelayedTextField(EDSC.MAP_CENTRE, mapCentreLatLong);
                            bool centreChanged = GUI.changed;
                            GUI.changed = false;
                            mapAreaHa = EditorGUILayout.DelayedTextField(EDSC.MAP_AREA, mapAreaHa);
                            bool areaChanged = GUI.changed;
                            GUI.changed = false;
                            mapSizeEM = EditorGUILayout.DelayedTextField(EDSC.MAP_SIZE_E, mapSizeEM);
                            bool widthChanged = GUI.changed;
                            GUI.changed = false;

                            if (areaChanged)
                            {
                                double minSide = MapConstants.BBOX_MIN_SIDE;
                                double ha_m2 = MapConstants.BBOX_HA_M2;
                                double minArea = minSide * minSide / ha_m2;
                                double area = Math.Max(minArea, Math.Min(KTools.ParseDouble(mapAreaHa), maxArea));
                                double sizeWE = Math.Sqrt((area * ha_m2) * MapConstants.BBOX_ASPECT_STD);
                                mapAreaHa = KFormat.Sprintf(SC.SF0F, area); // might have been clamped
                                mapSizeEM = KFormat.Sprintf(SC.SF0F, sizeWE);
                            }

                            applyChange |= centreChanged || areaChanged || widthChanged;
                        }

                        if (applyChange)
                        {
                            mapLatLongStr = mapCentreLatLong + CC.COMMA + mapAreaHa + CC.COMMA + mapSizeEM;
                            ApplyValidBoundsBox(mapLatLongStr);
                        }

                        //GUI.enabled = false; // N-S not editable
                        //EditorGUILayout.TextField(EDSC.MAP_SIZE_N, mapSizeNM);
                        GUI.enabled = true;
                        NoEditTextField(EDSC.MAP_SIZE_N, mapSizeNM, columnWidth);
                    }
                    GUILayout.EndArea();

                    playActionEnabled = BoundsBoxIsValid();

                    line += nl + 3; nl = 3f;
                    GUILayout.BeginArea(areaRect(line, nl, columnWidth), GUIStyle.none);
                    {
                        ActionButtonOrReferenceWarning();
                        
                        // SaveAsPrefab(nowPlaying);
                    }
                    GUILayout.EndArea();
                    
                    if (freeTilesUsedUp)
                    {
                        GUI.enabled = true;
                        
                        line += nl + 3;
                        nl = 3f;
                        GUILayout.BeginArea(areaRect(line, nl, columnWidth), GUIStyle.none);
                        {
                            EditorGUILayout.HelpBox(EDSC.UI_NO_TERRAIN_BUILD, MessageType.Warning);
                        }
                        GUILayout.EndArea();
                    }
                    break;
                }

                case ITAB_REBUILD:
                {
                    if (hasSceneChanged) ValidateOsmFile(osmFilePath);
                    if (!isPlaying) savedPrefab = false;
                    
                    GUILayout.BeginArea(areaRect(line, nl, columnWidth), EditorStyles.helpBox);
                    {
                        GUI.enabled = true;
                        GUILayout.BeginHorizontal();
                        bool validFileOSM = string.IsNullOrWhiteSpace(osmFileName);
                        string displayedLabelOSM = validFileOSM ? EDSC.HINT_OSM : osmFileName;
                        NoEditTextField(displayedLabelOSM, columnWidth-60);
                        if (GUILayout.Button(SC.ELLIPSIS, GUILayout.MinHeight(16))) SelectCachedOSMFile();
                        GUILayout.EndHorizontal();
                        EditorGUILayout.Space(lineY);

                        DisplayFileParts(osmFileName, columnWidth);
                    }
                    GUILayout.EndArea();
                    playActionEnabled = validOSMFileSelected;

                    line += nl + 3; nl = 3f;
                    GUILayout.BeginArea(areaRect(line, nl, columnWidth), GUIStyle.none);
                    {
                        ActionButtonOrReferenceWarning();

                        SaveAsPrefab(nowPlaying);
                    }
                    GUILayout.EndArea();

                    break;
                }

                case ITAB_TRAFFIC:
                {
                    if (hasSceneChanged) ValidateVRoadFile(vroadFilePath);

                    GUILayout.BeginArea(areaRect(line, nl, columnWidth), EditorStyles.helpBox);
                    {
                        playActionEnabled = validVRoadFileSelected;

                        GUI.enabled = true;
                        GUILayout.BeginHorizontal();
                        bool validFile = string.IsNullOrWhiteSpace(vroadFileName);
                        string displayedLabel = validFile ? EDSC.HINT_VROAD : vroadFileName;
                        NoEditTextField(displayedLabel, columnWidth-60);
                        GUI.enabled = true;
                        if (GUILayout.Button(SC.ELLIPSIS, GUILayout.MinHeight(16))) SelectVRoadFile();
                        GUILayout.EndHorizontal();

                        EditorGUILayout.Space(lineY);

                        DisplayFileParts(vroadFileName, columnWidth);
                    }
                    GUILayout.EndArea();

                    line += nl + 3;
                    nl = 3f;
                    GUILayout.BeginArea(areaRect(line, nl, columnWidth), GUIStyle.none);
                    {
                        ActionButtonOrReferenceWarning();

                        if (isPlaying && app != null)
                        {
                            EditorGUILayout.Space(lineY);

                            GUI.enabled = true;
                            GUILayout.BeginHorizontal();
                            {
                                GUILayoutOption optBW = GUILayout.MaxWidth(24); // columnWidth / 4);
                                GUILayoutOption optH = GUILayout.Height(20); 

                                GUILayout.Label(EDSC.TRAFFIC_VAR, optH);
                                if (GUILayout.Button(SC.MI, optBW, optH)){ traffic--; VRoad.AdvanceReleaseTime(-60); }
                                GUILayout.Label(SC.N+traffic, EDSF.STYLE_TEXTFIELD, optBW, optH);
                                if (GUILayout.Button(SC.PL, optBW, optH)){ traffic++; VRoad.AdvanceReleaseTime(+60); }
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                    GUILayout.EndArea();

                    break;
                }
                case ITAB_OPTIONS:
                {
                    GUILayout.BeginArea(areaRect(line, nl, columnWidth), EditorStyles.helpBox);

                    var contentMM = new GUIContent(EDSC.CREATE_MULTIPLE_MESHES, EDSC.CREATE_MULTIPLE_MESHES_TT);
                    createMultipleMeshes = EditorGUILayout.Toggle(contentMM, createMultipleMeshes);
                    
                    var contentUT = new GUIContent(EDSC.CREATE_UNITY_TERRAIN, EDSC.CREATE_UNITY_TERRAIN_TT);
                    createUnityTerrain = EditorGUILayout.Toggle(contentUT, createUnityTerrain);

                    GUI.changed = false;
                    var contentDA = new GUIContent(EDSC.DIVISION_ABUNDANCE, EDSC.DIVISION_ABUNDANCE_TT);
                    vroadDivisionWeights = EditorGUILayout.TextField(contentDA, vroadDivisionWeights);
                    if (GUI.changed && buildMap != null) buildMap.divisionWeights = vroadDivisionWeights;
                    
                    GUI.changed = false;
                    var contentMB = new GUIContent(EDSC.MAPBOX_KEY, EDSC.MAPBOX_KEY_TT);
                    mapBoxKey = EditorGUILayout.DelayedTextField(contentMB, mapBoxKey);
                    if (GUI.changed) VRoad.MapBoxKeyStore(mapBoxKey);

                    if (string.IsNullOrWhiteSpace(mapBoxKey) || SF.CFGVAL_TERRAIN_TOKEN_PROMPT.Equals(mapBoxKey))
                    {
                        int freeTiles = VRoad.FreeTilesRemaining();
                        string msg = freeTiles < 0 ? "(unknown)" : SC.N + freeTiles;

                        NoEditTextField(EDSC.FREE_TILES, EDSC.FREE_TILES_TT, msg, columnWidth / 2, columnWidth);
                        freeTilesUsedUp = (freeTiles == 0);

                        // freeTilesUsedUp = freeTiles < 17;
                    }
                    else freeTilesUsedUp = false;

                    GUI.changed = false;
                    var contentOSTC = new GUIContent(EDSC.OPEN_SCENE_ON_TAB_CHANGE, EDSC.OPEN_SCENE_ON_TAB_CHANGE_TT);
                    openSceneOnTabChange = EditorGUILayout.Toggle(contentOSTC, openSceneOnTabChange);
                    if (GUI.changed)
                    {
                        PlayerPrefs.SetInt(EDSF.PREFS_AUTO_OPEN_SCENE, openSceneOnTabChange ? 1 : 0);
                        PlayerPrefs.Save();
                    }

                    GUILayout.EndArea();

                    if (freeTilesUsedUp)
                    {
                        line += nl + 3;
                        nl = 3f;
                        GUILayout.BeginArea(areaRect(line, nl, columnWidth), GUIStyle.none);
                        {
                            EditorGUILayout.HelpBox(EDSC.UI_NO_TERRAIN_OPTS, MessageType.Warning);
                        }
                        GUILayout.EndArea();
                    }

                    
                    break;
                }
                case ITAB_VARIANT:
                {
                    GUILayout.BeginArea(areaRect(line, nl, columnWidth), EditorStyles.helpBox);

                    GUI.enabled = true;
                    NoEditTextField(EDSC.MAX_AREA, KFormat.Sprintf(SC.SF0F, maxArea), columnWidth);
                    NoEditTextField(EDSC.TAXIS_BUSES, isPro ? EDSC.ON : EDSC.OFF, columnWidth);
                    NoEditTextField(EDSC.SAMPLE_SOURCE_CODE, isPro ? EDSC.ON : EDSC.OFF, columnWidth);

                    EditorGUILayout.Space(lineY);
                    NoEditTextField(EDSC.USER_AGENT, KHttp.UserAgent(), columnWidth);

                    GUILayout.EndArea();

                    break;
                }
            }

            initGUI = false;
            hasSceneChanged = false;
        }

        private void SaveAsPrefab(bool nowPlaying)
        {
            if (missingReferences.Length == 0)
            {
                EditorGUILayout.Space(lineY);

                bool canSave = nowPlaying && playTab == ITAB_REBUILD && !savedPrefab && 
                               AllReferencesFound() && mapMesh.MeshesReady();
                string saveMsg = canSave ? EDSC.ACTION_PREFAB : 
                    HintFormat(savedPrefab? EDSC.ALREADY_PREFAB: EDSC.HINT_PREFAB);
                
                GUI.enabled = canSave;
                if (GUILayout.Button(saveMsg, GUILayout.Height(helpBoxHeight)))
                {
                    try
                    {
                        string meshPath = mapMesh.SaveMapMesh(createMultipleMeshes);
                        HighlightFileOrDir(meshPath);

                        string saveMeshMsg = EDSC.UI_SAVE_MESHES;
                        if (createUnityTerrain)
                        {
                            mapMesh.SaveTerrain(meshPath);
                            saveMeshMsg = EDSC.UI_SAVE_MESHES_TERRAIN;
                        }
                        savedFiles = KFormat.Sprintf(saveMeshMsg, meshPath);
                        
                        string prefabPath = mapMesh.SaveMapAsPrefab();
                        savedFiles += SC.NL + KFormat.Sprintf(EDSC.UI_SAVE_PREFAB, prefabPath);
                        HighlightFileOrDir(prefabPath);

                        EditorGUILayout.HelpBox(savedFiles, MessageType.Info);

                        savedPrefab = true;
                    }
                    catch (Exception e)
                    {
                        EditorGUILayout.HelpBox(EDSC.UI_SAVE_PREFAB_FAIL + e.Message, MessageType.Warning);
                    }
                }
            }
        }
        private void ActionButtonOrReferenceWarning()
        {
            GUI.enabled = true;

            string expectedScene = SceneForActiveTab();
            bool correctScene = currentScene.Equals(expectedScene);

            if (!correctScene)
            {
                EditorGUILayout.HelpBox(KFormat.Sprintf(EDSC.OPEN_SCENE, expectedScene), MessageType.Info);
            }
            else if (missingReferences.Length > 0)
            {
                EditorGUILayout.HelpBox(KFormat.Sprintf(EDSC.UI_MISSING, missingReferences), MessageType.Warning);
            }
            else if (isPlaying)
            {
                if (app == null)
                {
                    string msg = activeTab == ITAB_TRAFFIC? runTraffic.ProgressActivity() : buildMap.ProgressActivity();
                    EditorGUILayout.HelpBox(msg, MessageType.Info);
                }
                else
                {
                    GUI.enabled = false;
                    GUILayout.Button(HintFormat(EDSC.RUNNING), GUILayout.Height(helpBoxHeight));
                }
            }
            else
            {
                GUI.enabled = playActionEnabled;

                string hint = hints[activeTab];

                string buttonAction = KFormat.Sprintf(EDSC.PLAY_SCENE, expectedScene, actions[activeTab]);
                
                string buttonTooltip = tooltips[activeTab];
                GUIContent buttonContent = playActionEnabled ? new GUIContent(buttonAction, buttonTooltip)
                        : new GUIContent(HintFormat(hint));

                if (GUILayout.Button(buttonContent, GUILayout.Height(helpBoxHeight)))
                {
                    playTab = activeTab;
                    PlayerPrefs.Save(); // Saved in the editor node in the registry
                    EditorApplication.isPlaying = true;
                }
            }

            GUI.enabled = true;
        }

       
        private void NoEditTextField(string label, string value, float columnWidth)
        {
            NoEditTextField(label, null, value, columnWidth / 2, columnWidth);
        }
       
        private static void NoEditTextField(string label, string tooltip, string value, float labelWidth, float columnWidth)
        {
            GUILayout.BeginHorizontal();
            {
                var content = label == null? null: tooltip == null? new GUIContent(label): new GUIContent(label, tooltip);
                if (content != null) EditorGUILayout.LabelField(content, GUILayout.MaxWidth(labelWidth-1));
                
                NoEditTextField(value, columnWidth - labelWidth);
            }
            GUILayout.EndHorizontal();
        }

        private static void NoEditTextField(string value, float width)
        {
            Color std = GUI.contentColor;
            GUI.contentColor = new Color(.3f,.7f,1f);
            GUILayout.Label(value, EDSF.STYLE_TEXTFIELD, GUILayout.MaxWidth(width));
            GUI.contentColor = std;
        }
        private string HintFormat(String hint) { return KFormat.Sprintf(EDSC.HINT_FORMAT, hint); }

        private void HighlightFileOrDir(string path)
        {
            if (path == null) return;
            int np = path.Length;
            while (np > 0 && path[np - 1] == CC.SLASH)
            {
                np--;
                path = path.Substring(0, np);
            }
            // Load object
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)); 
            // Select the object in the project folder
            //Selection.activeObject = obj;
            // Also flash the folder yellow to highlight it
            EditorGUIUtility.PingObject(obj);
        }
        private Rect areaRect(float line, float nl, float columnWidth)
        {
            return new Rect(marginX, lineY * line, columnWidth, lineY * (line + nl));
        }
      
        private bool BoundsBoxIsValid()
        {
            return boundsBoxFromBrowser != null;
        }
        
        void OpenSlippyMapInBrowser()
        {
            float lastLat = PlayerPrefs.GetFloat(EDSF.PREFS_BROWSER_LAT);
            float lastLng = PlayerPrefs.GetFloat(EDSF.PREFS_BROWSER_LNG);
            
            if (lastLat == 0) Application.OpenURL(VRoad.SlippyMapURL());
            else
            {
                // see https://wiki.openstreetmap.org/wiki/Zoom_levels
                // tilesize = 40075017 * Math.cos(latRad) / Math.pow(2, zoom);
                double targetArea = VRoad.GotPro() ? 400 : 100; // in hectares
                double G = 4 * 40075017;
                double W = Math.Sqrt(targetArea * 10000) * (4.0 / 3.0); // For aspect 16:9, w = X*4/3, H = X*3/4
                double latRad = Math.PI * lastLat / 180.0;
                double zoom = Math.Log(G * Math.Cos(latRad) / W) / Math.Log(2);
                
                Application.OpenURL(KFormat.Sprintf(EDSF.SLIPPY_MAP_URL_PARAMS, lastLat, lastLng, zoom));
            }

            pollClipboard = true;
        }

        private void DisplayFileParts(string fileName, float columnWidth)
        {
            string[] ccsa = fileName == null? null: FilenameWrapper.PLAIN.CountryCitySuburb(fileName);;
            if (ccsa == null || ccsa.Length != 4) ccsa = new string[] {SC.N, SC.N, SC.N, SC.N,};

            float labelWidth = 60;
            NoEditTextField(EDSC.MAP_SUBURB, null, ccsa[2], labelWidth, columnWidth);
            NoEditTextField(EDSC.MAP_CITY, null, ccsa[1], labelWidth, columnWidth);
            NoEditTextField(EDSC.MAP_COUNTRY, null, ccsa[0], labelWidth, columnWidth);
            NoEditTextField(EDSC.MAP_AREA, null, ccsa[3], labelWidth, columnWidth);
        }

        private void SelectCachedOSMFile()
        {
            validOSMFileSelected = false;
            string dir = KEnv.OsmDir();
            string path = EditorUtility.OpenFilePanel(EDSC.OSM_FILE_DESC, dir, EDSF.SUFFIX_OSM);
            if (string.IsNullOrEmpty(path)) return;

            ValidateOsmFile(path);
        }
        
        void ValidateOsmFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            
            KFile file = new KFile(path);
            if (file.Exists())
            {
                string[] ccsa = FilenameWrapper.PLAIN.CountryCitySuburb(file.FileName());
                if (ccsa.Length == 4)
                {
                    int ha = KTools.ParseInt(ccsa[3]);
                    validOSMFileSelected = ha > 0;
                }
            }

            if (validOSMFileSelected)
            {
                osmFilePath = path;
                osmFileName = file.FileName();
                SetupBuildMap(osmFileName, SC.N);
            }
            else
            {
                ClearBuildMap();
                osmFileName = SC.N;
                osmFilePath = SC.N;
            }
        }

        void SelectVRoadFile()
        {
            validVRoadFileSelected = false;
            string dir = KEnv.VroadWriteDir();
            string path = EditorUtility.OpenFilePanel(EDSC.VROAD_FILE_DESC, dir, EDSF.SUFFIX_VROAD);
            if (string.IsNullOrEmpty(path)) return;

            ValidateVRoadFile(path);
        }
        
        void ValidateVRoadFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            
            KFile file = new KFile(path);
            
            if (file.Exists())
            {
                string[] ccsa = FilenameWrapper.PLAIN.CountryCitySuburb(file.FileName());
                if (ccsa.Length == 4)
                {
                    int ha = KTools.ParseInt(ccsa[3]);
                    validVRoadFileSelected = ha > 0;
                }
            }

            if (validVRoadFileSelected)
            {
                vroadFileName = file.FileName();
                vroadFilePath = path;
                SetupRunTraffic(vroadFileName);
            }
            else
            {
                ClearRunTraffic();
                vroadFileName = SC.N;
                vroadFilePath = SC.N;
            }
        }

        
        private void SetupBuildMap(string path, string latLong)
        {
            if ((activeTab == ITAB_BUILD || activeTab == ITAB_REBUILD) && AllReferencesFound())
            {
                if (buildMap == null) missingReferences += EDSF.REF_BUILDMAP;

                else buildMap.SetupBuild(path, latLong);
            }
        }
        private void SetupRunTraffic(string path)
        {
            if (activeTab == ITAB_TRAFFIC && AllReferencesFound())
            {
                if (runTraffic == null) missingReferences += EDSF.REF_RUNTRAFFIC;

                else runTraffic.SetupTraffic(path);
            }
        }

        private bool ApplyValidBoundsBox(string latLong)
        {
            boundsBoxFromBrowser = VRoad.CreateFromLatLong(latLong);

            if (!BoundsBoxIsValid()) return false;


            Xy centreLL = boundsBoxFromBrowser.CentreLatLong();
            double sizeWE = boundsBoxFromBrowser.SizeWestToEast();
            double sizeSN = boundsBoxFromBrowser.SizeSouthToNorth();
            int area = boundsBoxFromBrowser.AreaHectares();

            mapCentreLatLong = KFormat.Sprintf(SC.SF5F + CC.COMMA + SC.SF5F, centreLL.Y(), centreLL.X());
            mapAreaHa = KFormat.Sprintf(SC.SFD, area);
            mapSizeEM = KFormat.Sprintf(SC.SF0F, sizeWE);
            mapSizeNM = KFormat.Sprintf(SC.SF0F, sizeSN);

            mapLatLongStr = mapCentreLatLong + CC.COMMA + mapAreaHa + CC.COMMA + mapSizeEM;
            SetupBuildMap(SC.N, mapLatLongStr);

            PlayerPrefs.SetFloat(EDSF.PREFS_BROWSER_LAT, (float) centreLL.Y());
            PlayerPrefs.SetFloat(EDSF.PREFS_BROWSER_LNG, (float) centreLL.X());

            return true;
        }

        // This is an EditorWindow: Awake is called when the window is opened for the first time
        void Awake()
        {
            mapBoxKey = VRoad.MapBoxKeyQuery();
        }
        
        void Update()
        {
            if (!isPlaying)
            {
                string clipboard = GUIUtility.systemCopyBuffer;

                if (pollClipboard && clipboard != null && !clipboard.Equals(clipboardCache) &&
                    UBuildMap.LooksLikeLatLong(clipboard))
                {
                    clipboardCache = clipboard;

                    string latlong = clipboard.Trim(); // do not trim cached copy

                    if (ApplyValidBoundsBox(latlong)) Repaint();
                }
            }

            // When build is complete (Meshes ready) set vroad file path so that it shows in browse box on traffic tab
            if (isPlaying && app == null && AllReferencesFound() && mapMesh.MeshesReady())
            {
                app = ExampleApp.AwakeInstance(); // this does not go in Awake, because this is an EditorWindow

                if (app != null)
                {
                    KFile vroadFile = app.GetDataFile();
                    if (vroadFile != null)
                    {
                        vroadFilePath = vroadFile.FullPath();
                        vroadFileName = vroadFile.FileName();
                        ValidateVRoadFile(vroadFilePath);
                        
                        osmFilePath = KEnv.OsmDir() + vroadFile.FilenameWithoutExtension() + SC.SUFFIX_DOT_JSON;
                        ValidateOsmFile(osmFilePath);

                        Repaint();
                    }
                }
            }

            if (!isPlaying && app != null)
            {
                app = null;
            }

        }
        
        void OnSelectionChange()
        {
            UaCamControllerMain cam = UaCamControllerMain.MostRecentInstance;
            UaBotHandler bh = UaBotHandler.Instance;
            if (cam == null || bh == null) return;
            
            GameObject[] goa = Selection.gameObjects;
            if (goa == null || goa.Length == 0)
            {
                // stop tracking any previous Bot
                cam.TrackThis(null);
            }
            else // there is at least one selection
            {
                GameObject primarySelection = goa[0];

                // Is this a vehicle / pedestrian?
                IBit bit = bh.LookupBit(primarySelection);
                if (bit != null) cam.TrackThis(primarySelection);
                
                // If the primary selection is something else (e.g. the camera)
                // then do not stop tracking the previous bot, so that it is possible
                // to adjust public test variables on the camera while tracking
            }
        }

        
       
    }
}