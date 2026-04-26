using System;
using uk.vroad.api;
using uk.vroad.api.enums;
using uk.vroad.api.events;
using uk.vroad.api.geom;
using uk.vroad.api.input;
using uk.vroad.api.map;
using uk.vroad.api.route;
using uk.vroad.apk;
using uk.vroad.rvr;
using uk.vroad.ucm;
using UnityEngine;
using UnityEngine.Serialization;

namespace uk.vroad.urvr
{
    public class UMiniMapStopIcons : MonoBehaviour, LAppState, LPlayerRoute, LAppView
    {
        public GameObject taxiZonePrefab; // This is in the MiniMap Layer
        public GameObject busStopPrefab;
        public Sprite dotSprite;
        public GameObject taxiPolePrefab; // This is in the SimMap layer
        public GameObject busPolePrefab;
        
        private float DOT_SIZE = 6; 
        private float NORMAL_SIZE = 4; // This overrides the value in the prefab
        private const float LARGER_SIZE = 30; // This is used to highlight stops on a selected route
        private const float AUTOROUTE_ICONS_MINI_MAP = 30f;
        private const float AUTOROUTE_ICONS_SIM_MAP = 12f;
        private const float POLE_LENGTH = 200f;  // standard cylinder is 2 high, scaled by 100
        
        private KHash<IStop, GameObject> stopToIconGO = new KHash<IStop, GameObject>();
        private KHash<IZone, GameObject> drvZoneToIconGO = new KHash<IZone, GameObject>();
        private KHash<IStop, GameObject> stopToPoleGO = new KHash<IStop, GameObject>();
        private KHash<IZone, GameObject> drvZoneToPoleGO = new KHash<IZone, GameObject>();

        private PlayerRouteBuilder prb;
        private GameInputHandler gih;
        private bool mapJustLoadedInAnotherThread ;
        private IBranch autoOption;
        private bool optionsAreStops;
        private bool routeChanged;
        private bool viewChanged;
        private bool anyIconsVisible;
        
        private float rotDisplay;
            
        private readonly Color colorStops = new Color(0, 1f, 0.275f);
        private readonly Color colorTaxiZones = new Color(0, 0.834f, 0.952f);
        private Sprite taxiIconSprite;
        private Sprite busIconSprite;
        private Material taxiPoleMaterial;
        private Material busPoleMaterial;
        private Game game;
        
        void Awake()
        {
            game = Game.AwakeInstance();
            game.AddEventConsumer(this);

            prb = game.Prb();
            gih = game.Gih();

            taxiIconSprite = taxiZonePrefab.GetComponent<SpriteRenderer>().sprite;
            busIconSprite = busStopPrefab.GetComponent<SpriteRenderer>().sprite;

            taxiPoleMaterial = taxiPolePrefab.GetComponent<MeshRenderer>().sharedMaterial;
            busPoleMaterial = busPolePrefab.GetComponent<MeshRenderer>().sharedMaterial;
        }
        public bool DeregisterFireMapChange()
        {
            foreach (GameObject stopGO in stopToIconGO.Values)
            {
                Destroy(stopGO);
            }
            stopToIconGO.Clear();

            foreach (GameObject taxiZoneGO in drvZoneToIconGO.Values)
            {
                Destroy(taxiZoneGO);
            }
            drvZoneToIconGO.Clear();

            foreach (GameObject stopGO in stopToPoleGO.Values)
            {
                Destroy(stopGO);
            }
            stopToPoleGO.Clear();

            foreach (GameObject taxiZoneGO in drvZoneToPoleGO.Values)
            {
                Destroy(taxiZoneGO);
            }
            drvZoneToPoleGO.Clear();
            
            return true; 
        }


        public void AppStateChanged(AppStateTransition ast)
        {
            if (ast.after == AppState.ReadyToSimulate)
            {
                mapJustLoadedInAnotherThread = true;
            }
        }

        void Update()
        {
            if (mapJustLoadedInAnotherThread)
            {
                mapJustLoadedInAnotherThread = false;

                CreateStopIcons();
            }
            UCamControllerMainRvr camCon = ((UCamControllerMainRvr) UCamControllerMainRvr.MostRecentInstance);
            float rotNow = (float) camCon.RotationApplied().Degrees();
            
            if (routeChanged)
            {
                routeChanged = false;
                ChangeStopIcons(false);
            }
            else if (viewChanged || (anyIconsVisible && Math.Abs(rotNow - rotDisplay) > 1f))
            {
                viewChanged = false;
                ChangeStopIcons(true);
            }
            
            rotDisplay = rotNow;
        }
        
        public void FireRouteChanged()
        {
            routeChanged = true;
        }
        public void FireViewChanged()
        {
            viewChanged = true;
        }

        private void ChangeStopIcons(bool mapChange)
        {
            bool forTaxi = prb.ShowTaxiRoute();
            KHashSet<IStop> visibleBusStops = null;
            KHashSet<IZone> visibleTaxiBays = null;
            IBranch autoOptionNew = prb.AutoFirst(forTaxi);
            Player player = Player.ActivePlayer();

            if (player == null || forTaxi)
            {
                // If choosing trip (no player), or showing taxi route,
                // create an empty srHash so that all stops will be hidden
                visibleBusStops = new KHashSet<IStop>();
                visibleTaxiBays = new KHashSet<IZone>();
            }
            else
            {
                // First look at (immediate) options from player choice branch, and show only a subset of stops/parking
                // for some special cases
                IBranch[] opts = prb.AvailableOptions(forTaxi);
                int nao = opts.Length;
                int loi = prb.LitOptionIndex(forTaxi);

                for (int opti = 0; opti < nao; opti++)
                {
                    int di = opti - loi;
                    Material mat = UPlayerRoute.OptionMaterialStatic(di);

                    IBranch option = opts[opti];
                    if (option == null) continue;
                    
                    
                    {
                        // (1) If the option point is at a boarding stop, then there will be one option 
                        // for each route from that stop. Show only the stops after the board stop on these routes
                        if (option is IBranchBusOnboard bbo)
                        {
                            if (visibleBusStops == null) visibleBusStops = new KHashSet<IStop>();

                            // Find all the stops after the boarding stop
                            IStop[] routeStops = bbo.GetHalt().GetBusroute().GetStops();
                            IStop boardStop = bbo.GetStop();
                            bool foundBoardStop = false;
                            foreach (IStop routeStop in routeStops)
                            {
                                if (foundBoardStop)
                                {
                                    if (!visibleBusStops.Contains(routeStop))
                                    {
                                        visibleBusStops.Add(routeStop);

                                        GameObject stopIconGO = stopToIconGO.Get(routeStop);
                                        SpriteRenderer sr = stopIconGO?.GetComponent<SpriteRenderer>();
                                        sr.color = mat.color;

                                        GameObject stopPoleGO = stopToPoleGO.Get(routeStop);
                                        MeshRenderer mr = stopPoleGO?.GetComponent<MeshRenderer>();
                                        mr.material = mat;
                                    }
                                }
                                else if (routeStop == boardStop) foundBoardStop = true;
                            }

                        }
                        
                        // (2) If the option point is aboard a bus route, there will be one option for each possible 
                        // arrival (alighting) stop. Show only these stops
                        else if (option is IBranchBusArrival bba)
                        {
                            if (visibleBusStops == null) visibleBusStops = new KHashSet<IStop>();

                            IStop stop = bba.GetStop();
                            if (!visibleBusStops.Contains(stop))
                            {
                                visibleBusStops.Add(stop);

                                GameObject stopIconGO = stopToIconGO.Get(stop);
                                SpriteRenderer sr = stopIconGO?.GetComponent<SpriteRenderer>();
                                sr.color = mat.color;

                                GameObject stopPoleGO = stopToPoleGO.Get(stop);
                                MeshRenderer mr = stopPoleGO?.GetComponent<MeshRenderer>();
                                mr.material = mat;
                            }
                        }

                        // (3) If the player is aboard a taxi, there will be one options for each possible
                        // parking destination, Show these here in the appropriate colour
                        else if (option is IBranchTaxiDestinationLane bvdl)
                        {
                            if (visibleTaxiBays == null) visibleTaxiBays = new KHashSet<IZone>();

                            ITaxiZone zone = bvdl.GetTaxiZone();
                            if (!visibleTaxiBays.Contains(zone))
                            {
                                visibleTaxiBays.Add(zone);

                                GameObject taxiZoneIconGo = drvZoneToIconGO.Get(zone);
                                SpriteRenderer sr = taxiZoneIconGo.GetComponent<SpriteRenderer>();
                                sr.color = mat.color;

                                GameObject taxiPoleGO = drvZoneToPoleGO.Get(zone);
                                MeshRenderer mr = taxiPoleGO?.GetComponent<MeshRenderer>();
                                mr.material = mat;
                            }

                        }
                        
                        else if (option is IBranchTaxiDestinationEdge bvde)
                        {
                            if (visibleTaxiBays == null) visibleTaxiBays = new KHashSet<IZone>();

                            IZone zone = bvde.GetDrvZone();
                            if (!visibleTaxiBays.Contains(zone))
                            {
                                visibleTaxiBays.Add(zone);

                                GameObject taxiZoneIconGo = drvZoneToIconGO.Get(zone);
                                SpriteRenderer sr = taxiZoneIconGo.GetComponent<SpriteRenderer>();
                                sr.color = mat.color;

                                GameObject taxiPoleGO = drvZoneToPoleGO.Get(zone);
                                MeshRenderer mr = taxiPoleGO?.GetComponent<MeshRenderer>();
                                mr.material = mat;
                            }
                        }
                    }
                }

                if (player.Aboard() || player.Ped().HasChosenLeg())
                {
                    // If player is aboard or waiting to board, but has already chosen where to get off,
                    // create an empty hashset so that all stops will be hidden
                    if (visibleBusStops == null) visibleBusStops = new KHashSet<IStop>();
                    if (visibleTaxiBays == null) visibleTaxiBays = new KHashSet<IZone>();
                }

            }

            bool optionsAreStopsNew = visibleBusStops != null || visibleTaxiBays != null;

            if (optionsAreStopsNew != optionsAreStops || (optionsAreStops && autoOptionNew != autoOption))
            {
                optionsAreStops = optionsAreStopsNew;
                autoOption = autoOptionNew;

                EnableStops(visibleBusStops, visibleTaxiBays);
            }
            else if (mapChange)
            {
                EnableStops(visibleBusStops, visibleTaxiBays);
            }
            
            // If there are no stops as route options, look through the auto route and highlight any branches
            // That get on or off a taxi or bus
            if (!optionsAreStops && anyIconsVisible)
            {
                float autoSize = gih.ShowMiniMap()? AUTOROUTE_ICONS_MINI_MAP : AUTOROUTE_ICONS_SIM_MAP;
                Material autoMat = UPlayerRoute.OptionMaterialStatic(0);
                Color autoColour = autoMat.color;
                IBranch[] aba = prb.AutoRouteBranches(forTaxi);
                foreach (IBranch ab in aba)
                {

                    if (ab is IBranchBusOnboard bbo)
                    {
                        SetSpriteSizeColour(stopToIconGO.Get(bbo.GetStop()), autoSize, autoColour);
                        SetPoleMaterial(stopToPoleGO.Get(bbo.GetStop()), autoMat);
                    }

                    else if (ab is IBranchBusArrival bba)
                    {
                        SetSpriteSizeColour(stopToIconGO.Get(bba.GetStop()), autoSize, autoColour);
                        SetPoleMaterial(stopToPoleGO.Get(bba.GetStop()), autoMat);
                    }

                    else if (ab is IBranchTaxiOriginLane bvol)
                    {
                        SetSpriteSizeColour(drvZoneToIconGO.Get(bvol.GetZone()), autoSize, autoColour);
                        SetPoleMaterial(drvZoneToPoleGO.Get(bvol.GetZone()), autoMat);
                    }

                    else if (ab is IBranchTaxiDestinationLane bvdl)

                    {
                        SetSpriteSizeColour(drvZoneToIconGO.Get(bvdl.GetZone()), autoSize, autoColour);
                        SetPoleMaterial(drvZoneToPoleGO.Get(bvdl.GetZone()), autoMat);
                    }

                }
            }
        }

        private void SetSpriteSizeColour(GameObject go, float size, Color c)
        {
            go.GetComponent<SpriteRenderer>().color = c;
            go.transform.localScale = new Vector3(size, size, 1);
        }
        private void SetPoleMaterial(GameObject go, Material mat)
        {
            go.GetComponent<MeshRenderer>().material = mat;
        }
        private void CreateStopIcons()
        {
            bool onR = game.Map().DriveOnRight();
            
            foreach (IStop stop in game.Map().Stops())
            {
                if (stop.GetHalts().Length == 0) continue; // No dwells ==> no bus routes here

                Xyzbg c1 = stop.Position(); // lane centre line position
                double w = 2.0;
                if (stop.IsAtKerb())
                {
                    w = stop.GetLane().Width() * 0.5;
                    IFootpath fp = stop.GetFootpath();
                    if (fp != null) w += fp.Width();
                }
                Xyz fp1 = c1.Plus(c1.Bearing().Plus(onR ? Angle.A90 : Angle.ANEG90).UnitVectorXY().MultipliedBy(w));
                Vector3 pos = fp1.ToVector3(POLE_LENGTH);
                Quaternion rot = Quaternion.Euler(90f, 0, 0);
                GameObject stopIconGO = Instantiate(busStopPrefab, pos, rot, transform);
                stopToIconGO.Put(stop, stopIconGO);
                stopIconGO.SetActive(false);
                
                pos = fp1.ToVector3(POLE_LENGTH * 0.5f);
                GameObject busPoleGo = Instantiate(busPolePrefab, pos, Quaternion.identity, transform);
                stopToPoleGO.Put(stop, busPoleGo);
                busPoleGo.SetActive(false);
            }

            // One of these is created for each kar zone, as any kar zone can be a destination for
            // a taxi, not just TaxiZone (on-map parking zones)
            // However, an edge (Kar) zone is shown only if it is a destination option when the player
            // is in a taxi
            foreach (IDrvZone zone in game.Map().DrvZones())
            {
                Xyz loc = zone.Location();
                Vector3 pos = loc.ToVector3(POLE_LENGTH);
                Quaternion rot = Quaternion.Euler(90f, 0, 0);
                GameObject taxiZoneIconGo = Instantiate(taxiZonePrefab, pos, rot, transform);
                drvZoneToIconGO.Put(zone, taxiZoneIconGo);
                taxiZoneIconGo.SetActive(false);

                pos = loc.ToVector3(POLE_LENGTH * 0.5f);
                GameObject taxiPoleGo = Instantiate(taxiPolePrefab, pos, Quaternion.identity, transform);
                drvZoneToPoleGO.Put(zone, taxiPoleGo);
                taxiPoleGo.SetActive(false);
            }

            EnableStops(null, null);
        }

        // This can be called with a null Hash, in which case all stops are shown, and reset to their normal colour
        // otherwise, only the stops in the hash will be shown, and no change will be made to their colour
        // (which will be set before or after this)
        //
        // For karZones, if it is a null hash then only the parking zones
        private void EnableStops(KHashSet<IStop> visibleBusStops, KHashSet<IZone> visibleTaxiBays)
        {
            bool mapXRay = gih.GetMapState() == MapState.XRay;
            bool normalDisplay = visibleBusStops == null && visibleTaxiBays == null;
            bool dotDisplay = normalDisplay && gih.ShowMiniMap();
            bool showBusIcons = visibleBusStops != null || (mapXRay && visibleTaxiBays == null); // 20211021
            bool showTaxiIcons = visibleTaxiBays != null || (mapXRay && visibleBusStops == null); 
            bool onSimMap = ! gih.ShowMiniMap();
            bool showPoles = gih.MapOn() && !gih.ShowMiniMap();

            Vector3 optionScale = new Vector3(LARGER_SIZE, LARGER_SIZE, 1);
            Vector3 normalScale = new Vector3(NORMAL_SIZE, NORMAL_SIZE, 1);
            Vector3 dotScale = new Vector3(DOT_SIZE, DOT_SIZE, 1);
            Vector3 appliedScale =  dotDisplay? dotScale: normalDisplay? normalScale : optionScale;
           
            if (!showBusIcons && !showTaxiIcons && !anyIconsVisible) return;
            
            bool anyVisible = false;
            
            foreach (IStop stop in stopToIconGO.Keys)
            {
                // If the (chosen route) visible set is null, then only kerb stops are displayed
                bool visible = showBusIcons && (visibleBusStops?.Contains(stop) ?? stop.IsAtKerb());

                if (normalDisplay) // that is, we are showing stops for the purpose of boarding
                {
                    // Don't show a stop if boarding here does not allow a trip to the PedZone destination
                    bool leadsToDest = prb.RouteExistsForPlayer(stop);
                    if (!leadsToDest) visible = false;
                }

                GameObject stopIconGO = stopToIconGO.Get(stop);
                GameObject stopPoleGo = stopToPoleGO.Get(stop);
                
                stopIconGO.SetActive(visible);
                stopPoleGo.SetActive(visible && showPoles);
                
                if (visible)
                {
                    SpriteRenderer sr = stopIconGO.GetComponent<SpriteRenderer>();

                    if (normalDisplay)  sr.color = colorStops;  // else color has been set outside of here

                    sr.sprite = dotDisplay ? dotSprite : busIconSprite;
                   
                    stopIconGO.transform.localScale = appliedScale;
                    stopIconGO.transform.rotation = Quaternion.Euler(90, onSimMap?rotDisplay:0, 0);
                    stopIconGO.layer = onSimMap? UMapMeshRvr.LAYER_SIM_MAP : UMapMeshRvr.LAYER_MINI_MAP;

                    if (normalDisplay)
                    {
                        MeshRenderer mr = stopPoleGo.GetComponent<MeshRenderer>();
                        mr.material = busPoleMaterial;
                    }
                       

                    anyVisible = true;
                }
            }

            foreach (IZone zone in drvZoneToIconGO.Keys)
            {
                bool visible = showTaxiIcons && (visibleTaxiBays == null || visibleTaxiBays.Contains(zone));

                if (normalDisplay)
                {
                    if (zone is ITaxiZone taxiZone)  { if (taxiZone.IsDropOffOnly()) visible = false; }
                    else /*edge traffic zone */ visible = false;
                }

                GameObject taxiZoneIconGo = drvZoneToIconGO.Get(zone);
                GameObject taxiPoleGo     = drvZoneToPoleGO.Get(zone);
                
                taxiZoneIconGo.SetActive(visible);
                taxiPoleGo.SetActive(visible && showPoles);

                if (visible)
                {
                    SpriteRenderer sr = taxiZoneIconGo.GetComponent<SpriteRenderer>();

                    if (normalDisplay) sr.color = colorTaxiZones; // else color has been set outside of here

                    sr.sprite = dotDisplay ? dotSprite : taxiIconSprite;
                    taxiZoneIconGo.transform.localScale = appliedScale;
                    taxiZoneIconGo.transform.rotation = Quaternion.Euler(90, onSimMap?rotDisplay:0, 0);
                    taxiZoneIconGo.layer = onSimMap ? UMapMeshRvr.LAYER_SIM_MAP : UMapMeshRvr.LAYER_MINI_MAP;

                    if (normalDisplay)
                    {
                        MeshRenderer mr = taxiPoleGo.GetComponent<MeshRenderer>();
                        mr.material = taxiPoleMaterial;
                    }
                    anyVisible = true;
                }
            }

            anyIconsVisible = anyVisible;
        }

    }
}