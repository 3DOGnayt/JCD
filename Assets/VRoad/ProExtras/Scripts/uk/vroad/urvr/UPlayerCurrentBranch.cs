using System.Collections.Generic;
using uk.vroad.api.enums;
using uk.vroad.api.etc;
using uk.vroad.api.geom;
using uk.vroad.api.map;
using uk.vroad.api.route;
using uk.vroad.api.sim;

using uk.vroad.rvr;
using uk.vroad.ucm;
using UnityEngine;

namespace uk.vroad.urvr
{
    public class UPlayerCurrentBranch : MonoBehaviour
    {
        protected Game game;
        private PlayerRouteBuilder prb;
        protected virtual float RouteWidthK() { return UPlayerRoute.MAIN_ROUTE_KW; }
        protected virtual float RouteWidthM() { return UPlayerRoute.MAIN_ROUTE_MW; }
        protected virtual float RouteElevation() { return UPlayerRoute.MAIN_ROUTE_ELEV; }

        protected virtual bool DrawPyramids() { return true; }
        protected virtual bool IsMini() { return false; }

        protected virtual void Awake()
        {
            game = Game.AwakeInstance();
            prb = game.Prb();
        }

        protected virtual bool IsActive(Player player)
        {
            if (player == null || player.Ped().HasArrived() || player.Ped().GetTrip() == null) return false;

            return true;
        }
        
        protected virtual void Update()
        {
            MeshFilter mf = gameObject.GetComponent<MeshFilter>();
            MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();

            Player player = Player.ActivePlayer();

            if (! IsActive(player))
            {
                Mesh mesh = mf.mesh;
                mesh.Clear();
                return;
            }
            //bool aboard  = player.Aboard();
            //IVkl vkl = player.GetVkl();
            //if (aboard && vkl == null) return;
            
            float kw = RouteWidthK();
            float mw = RouteWidthM();
            float dz = RouteElevation();

            bool aboard = player.Aboard();
            ILocus locus = player.GetLocus();
            if (locus == null) return;
            double ld = player.LocusDistance();
            bool fwd = player.LocusForward();
            double pd = ld / locus.Length();

            float da = fwd ? (float) pd : 0f;
            float db = fwd ? 1f : 1f - (float) pd;

            bool forTaxi = prb.ShowTaxiRoute();
            IBranch[] mba = prb.ManRouteBranches(forTaxi);
            IBranch[] aba = prb.AutoRouteBranches(forTaxi);
            int nm = mba.Length;
            int na = aba.Length;
            IBranch next = nm > 0? mba[0]: na > 0? aba[0]: null;
            IBranch curr = prb.CachedPlayerBranch(forTaxi);
            MaterialHint mh = MaterialHint.PlayerRoute;
            
            List<TriMesh> triMeshList = new List<TriMesh>();
            List<TriMesh> triMeshListPrev = new List<TriMesh>();
            List<TriMesh> triMeshListNext = new List<TriMesh>();

            // For current branch, prev does not matter, because the locus will always be drawn from the player
            // current position, so even if it has just stepped off a bus, we don't need to know that here.
            if (aboard)
            {
                IVkl vkl = player.GetVkl();
                IMind vm = vkl.GetMind();
                IRoad taxiPrev = null;
                
                if (locus is ILane lane)
                {
                    if (vkl is IBus bus)
                    {
                        IBranchBusArrival bba = next as IBranchBusArrival; 

                        if (bba?.GetStop() is IStop stop && stop.GetLane() == lane)
                        {
                            // About to arrive, so need to draw only part of this lane
                            
                            db = (float) (stop.DistanceOnLane() / lane.Length());
                            MeshTools.DrawLane(triMeshList, lane, da, db, kw, mw, vm);
                        }
                        else
                        {
                            MeshTools.DrawLane(triMeshList, lane, da, db, kw, mw, vm);
                            IRoad nextRoad = bus.GetNextRoad();
                            IStreme so = lane.GetStremeOutTo(nextRoad); // for both max and mini, draw a single streme
                            if (so != null) triMeshList.Add(UMapMeshRvr.LocusTriMesh(so, 0, 1, kw, mw, mh));
                            MeshTools.AddTriMeshesForBusroute(bus.GetBusroute(), null, nextRoad, bba, triMeshList, kw, mw, vm, IsMini());
                        }
                    }
                    else if (vkl is ITaxi taxiA)
                    {
                        if (taxiA.GetBayLane() == lane && taxiA.IntendedBay() >= 0)
                        {
                            float bayDB = (float) (taxiA.IntendedZone().BayEndDistance(taxiA.IntendedBay()) / lane.Length());
                            MeshTools.DrawLane(triMeshList, lane, da, bayDB, kw, mw, vm);

                            if (lane.GetRoad().LaneCount() > 1 && taxiA.IsDeparting())
                            {
                                ILane runningLane = lane.GetRoad().GetLanes()[1];
                                MeshTools.DrawLane(triMeshList, runningLane, bayDB, 1, kw, mw, vm);
                            }
                        }
                        else
                        {
                            MeshTools.DrawLane(triMeshList, lane, da, db, kw, mw, vm);
                        }
                        

                        taxiPrev = taxiA.GetRoad();
                    }
                }
                else if (locus is IStreme)
                {
                    triMeshList.Add(UMapMeshRvr.LocusTriMesh(locus, da, db, kw, mw, mh));

                    ILocus nextLocus = vkl.GetNextLocus();
                    if (nextLocus != null)  triMeshList.Add(UMapMeshRvr.LocusFullTriMesh(nextLocus, kw, mw, mh));
                }
                       
                if (!forTaxi && vkl is ITaxi taxiB) // player is aboard a taxi, but currently showing ped route (to choose taxi dest)
                {
                    // we want to show the taxi route to the (current) taxi dest
                            
                    IRoad[] roads = prb.ManAndAutoRouteTaxiRoads();
                    MeshTools.AddTriMeshesForTaxiRoute(taxiPrev, roads, -1, taxiB.IntendedBay(),triMeshList, kw, mw, vm, IsMini());
                }
            }
            else
            {
                if (locus is IWalkway w)
                {
                    IBranch wb = fwd ? w.BranchCD() : w.BranchDC();
                    IWalkBranch wd = (IWalkBranch) wb;
                    
                    float daOpt = da;
                    float dbOpt = db;
                    bool drawWalkwayAsOption = false;
                    int pib = player.Ped().IntendedBay();
                    
                    if (next is IBranchTaxiOriginLane bvol && pib > 0) // walking to taxi bay, or waiting at it
                    {
                        // While waiting for a taxi, the current branch is walkway, and BranchTaxiOriginLane is the currently selected exit
                        
                        drawWalkwayAsOption = true;
                        ITaxiZone taxiZone = bvol.GetTaxiZone();
                        float bayQ = (float)(taxiZone.BayEndDistance(pib) / taxiZone.GetLane().Length());
                        
                        if (fwd) db = bayQ;
                        else da = bayQ;
                    }
                    else if (next is IBranchBusBoarding bbbNext)  // walking, not yet at bus stop
                    {
                        drawWalkwayAsOption = true;
                        IStop ks = bbbNext.GetStop(); 
                        float standQ = (float) (ks.DistanceOnLane() / ks.GetLane().Length());
                        if (fwd) db = standQ;
                        else da = standQ;
                    }
                    else if (curr is IBranchBusBoarding bbbCurr) // waiting at bus stop
                    {
                        drawWalkwayAsOption = false; // U-Turn needed, 
                       
                        if (DrawPyramids())  triMeshList.Add(UMapMeshRvr.StopTriMesh(bbbCurr.GetStop(), true));
                    }

                    if (curr == wd)
                        MeshTools.DrawWalkway(triMeshList, null, wd, next, false, da, db, kw, mw, player.Ped());

                    if (drawWalkwayAsOption)
                    {
                        // The selected next branch is for boarding taxi or bus, and the alternative is (to continue on)
                        // the current walkway. This class is updated every time step, so draw the alternative colour
                        // on the current walkway from the player's feet to the end (or start if direction DC)
                        int nxi = prb.OptionIndex(forTaxi, next);
                        int wdi = prb.OptionIndex(forTaxi, wd);
                        List<TriMesh> tmOptionList = wdi < nxi ? triMeshListPrev : triMeshListNext;
                        bool walkwayOnLeft = game.Map().DriveOnRight() ? wdi > nxi : wdi < nxi;
                        float kwOpt = walkwayOnLeft ? UPlayerRoute.MAIN_ROUTE_KWL : UPlayerRoute.MAIN_ROUTE_KWR;
                        float mwOpt = walkwayOnLeft ? UPlayerRoute.MAIN_ROUTE_MWL : UPlayerRoute.MAIN_ROUTE_MWR;
                        
                        MeshTools.DrawWalkway(tmOptionList, null, wd, null, false, daOpt, dbOpt, kwOpt, mwOpt, player.Ped());
                    }
                }
                else if (locus is ITie)
                {
                    triMeshList.Add(UMapMeshRvr.LocusTriMesh(locus, da, db, kw, mw, mh));

                    ILocus nextLocus = player.GetNextLocus();
                    
                    // If player is on a course, then it is committed to the next walkway, which will be the player Branch. 
                    // The route options will be drawn from the course after the end of playerBranch
                    if (nextLocus != null)  triMeshList.Add(UMapMeshRvr.LocusFullTriMesh(nextLocus, kw, mw, mh));
                }
            }

            // If we have 4 materials in the GameObject, then we need 4 meshes, even if two are empty
            // Otherwise Unity shows an error about a mismatch of materials to meshes, and complains about performance
            // Also, an unexpected material can be applied to the mesh  
            const int matManual = 0;
            const int matAuto = 1;
            const int matMorePrev = 2;
            const int matMoreNext = 3;

            TriMesh triMeshLit = TriMesh.Combine(triMeshList.ToArray());

            // If there are manual branches selected, and the manual route was extended by user action
            // (not because it is the only exit from current branch) then we want to colour the current branch in manual
            // colour. otherwise, colour as auto
            bool colourAsManual = nm > 0 && prb.ManRouteExtendedByUserRequest();
            SubMesh smManual = UMapMeshRvr.TriangleSubMesh(colourAsManual? triMeshLit : null, dz, matManual);
            SubMesh smAuto   = UMapMeshRvr.TriangleSubMesh(colourAsManual ? null: triMeshLit , dz, matAuto);
            SubMesh smPrev = UMapMeshRvr.TriangleSubMesh(triMeshListPrev, dz, matMorePrev);
            SubMesh smNext = UMapMeshRvr.TriangleSubMesh(triMeshListNext, dz, matMoreNext);

            SubMesh[] sma = { smManual, smAuto, smPrev, smNext, };
            UMapMeshRvr.CreateMesh(mf, mr, sma);
            
        }
    }
}
