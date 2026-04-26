using System.Collections.Generic;
using uk.vroad.api.events;
using uk.vroad.api.geom;
using uk.vroad.api.input;
using uk.vroad.api.route;
using uk.vroad.api.sim;
using uk.vroad.rvr;
using uk.vroad.ucm;
using UnityEngine;

namespace uk.vroad.urvr
{
    public abstract class UPlayerRouteBase : MonoBehaviour, LPlayerRoute, LAppView, LSimTimeSec
    {
        protected abstract bool IsMini();
        protected abstract float RouteWidthK();
        protected abstract float RouteWidthM();

        protected abstract float RouteElevation();
        protected abstract float OptionElevation();
        protected abstract float RouteWidthOptK(int opti, int nopt);
        protected abstract float RouteWidthOptM(int opti, int nopt);
       
        private bool rebuildRoute;
        protected Game game;
        protected PlayerRouteBuilder prb;

        protected virtual void Awake()
        {
            game = Game.AwakeInstance();
            prb = game.Prb();

        }
        protected virtual void Start()
        {
            game.AddEventConsumer(this);
        }

        public void FireRouteChanged()
        {
            rebuildRoute = true;
        }

        public void FireViewChanged()
        {
            rebuildRoute = true;
        }

        public bool DeregisterFireMapChange() { return true; }

        public virtual void TimeSec()  {}

        protected const int matManual = 0;
        protected const int matAuto = 1;
        private const int matMorePrev = 2;
        private const int matMoreNext = 3;
        private const int matMorePrevPrev = 4;
        private const int matMoreNextNext = 5;
        private const int matTaxi = 6;
        private const int matBus = 7;

        protected abstract void UpdateOnRouteChanged();
        
        protected virtual bool IsActive(Player player)
        {
            if (player == null || player.Arrived() || player.GetTrip() == null) return false;

            return true;
        }

        protected virtual void Update()
        {
            Player player = Player.ActivePlayer();
            if (!IsActive(player))
            {
                MeshFilter mf = gameObject.GetComponent<MeshFilter>();
                mf.mesh.Clear();
                return;
            }

            if (rebuildRoute)
            {
                rebuildRoute = false;
                
                bool forTaxi = prb.ShowTaxiRoute();

                IVkl vkl = player.GetVkl();
                IMind vm = vkl?.GetMind();

                float kw = RouteWidthK();
                float mw = RouteWidthM();
                float dz = RouteElevation();
                float oz = OptionElevation();

                List<TriMesh> tmlMan = new List<TriMesh>();
                List<TriMesh> tmlAuto = new List<TriMesh>();

                IBranch playerBranch = prb.CachedPlayerBranch(forTaxi);
                // The current walkway or course (road or streme when aboard) is drawn dynamically each Update 
                // in UPlayerCurrentBranch so that the near end of the route is underneath the player's feet

                IBranch[] mba = prb.ManRouteBranches(forTaxi);
                IBranch[] aba = prb.AutoRouteBranches(forTaxi);

                int nm = mba.Length;
                int na = aba.Length;

                IBranch auto0 = na == 0 ? null : aba[0];
                IBranch prev = playerBranch;
                //Branch nxtb = nm > 0 ? mba[0] : auto0;

                bool mini = IsMini();

                // If a branch has been added to man route by the user, then colour the whole man route blue
                
                bool colourAsManual = prb.ManRouteExtendedByUserRequest();
                
                // Create mesh for Manual route, for selected branches following current branch
                for (int mi = 0; mi < nm; mi++)
                {
                    IBranch mrb = mba[mi];
                    
                    // If next is auto0, this colours blue the out-connection to auto
                    // but auto has already coloured its own in-connection
                    IBranch next = mi < nm - 1 ? mba[mi + 1] : auto0;
                    bool drawToNext = mi < nm - 1;
                    
                    List<TriMesh> tml = colourAsManual ? tmlMan : tmlAuto;
                   
                    MeshTools.AddTriMeshesForConnectionAndBranch(tml, tml,tml,
                        prev, mrb, next, drawToNext, kw, mw, vm, mini, player.Ped());
                    prev = mrb;
                }

                IBranch choiceBranch = prev; // last man branch, or playerBranch if no man route

                List<TriMesh> tmlPrev = new List<TriMesh>();
                List<TriMesh> tmlNext = new List<TriMesh>();
                List<TriMesh> tmlPrevPrev = new List<TriMesh>();
                List<TriMesh> tmlNextNext = new List<TriMesh>();
                List<TriMesh> tmlTaxi = new List<TriMesh>(); 
                List<TriMesh> tmlBus = new List<TriMesh>();
                IBranch[] avOpts = prb.AvailableOptions(forTaxi);
                int nAvOpt = avOpts.Length;

                int aoi = prb.OptionIndex(forTaxi, auto0);
                if (aoi < 0)
                {
                    if (auto0 == null) aoi = 0;
                    else
                    {
                        for (int avi = 0; avi < nAvOpt; avi++)
                        {
                            if (avOpts[avi].IsConnectedTo(auto0))
                            {
                                aoi = avi;
                                break;
                            }
                        }

                        if (aoi < 0) aoi = 0;
                    }
                }

                float optKw = RouteWidthOptK(aoi, nAvOpt);
                float optMw = RouteWidthOptM(aoi, nAvOpt);

                for (int ai = 0; ai < na; ai++) //Create mesh for auto route
                {
                    IBranch arb = aba[ai];
                    IBranch next = ai < na - 1 ? aba[ai + 1] : null;
                    bool drawToNext = ai < na - 1;
                    
                    // Add all to tmlAuto, so that if taxi-on or bus-on is in auto then it is shown in gold
                    MeshTools.AddTriMeshesForConnectionAndBranch(tmlAuto, tmlAuto, tmlAuto, 
                        prev, arb, next, drawToNext, optKw, optMw, vm, mini, player.Ped());
                    prev = arb;
                }

                /* THIS IS NOW DRAWN IN UPlayerCurrentBranch
                
                // When player is aboard, and we are showing ped route, we still want to show the taxi/bus route
                // to the off-stop
                if (player.Aboard())
                {
                    // If there is a man route, then the taxi or bus route to the off-bay or off-stop
                    // should be shown in man route colour, otherwise auto-colour
                    List<TriMesh> tmlAboard = nm > 0 ? tmlMan : tmlAuto;
                    
                    if (!forTaxi && vkl is Taxi)
                    {
                        Branch[] vaba = prb.AutoRouteBranches(true);
                        Branch vprev = null;
                        int nva = vaba.Length;
                        for (int vai = 0; vai < nva; vai++)
                        {
                            Branch varb = vaba[vai];
                            Branch vnext = vai < nva - 1 ? vaba[vai + 1] : null;
                            MeshTools.AddTriMeshesForConnectionAndBranch(tmlAboard, tmlAboard, tmlAboard, 
                                vprev, varb, vnext, optKw, optMw, vm, mini);
                            vprev = varb;
                        }
                    }
                    else if (vkl is Bus bus && nxtb is BranchBusArrival bba)
                    {
                        MeshTools.AddTriMeshesForBusroute(bus.Busroute(), null, bus.NextRoad(), bba,
                            tmlAboard, optKw, optMw, vm, mini);
                        
                    }
                }
                //*/
                
                // For each available option, find its index relative to auto and build a mesh into the appropriate
                // list which will be rendered in the corresponding option material
                for (int opti = 0; opti < nAvOpt; opti++)
                {
                    List<TriMesh> triMeshList;
                    if (opti == aoi - 2) triMeshList = tmlPrevPrev;
                    else if (opti == aoi - 1) triMeshList = tmlPrev;
                    else if (opti == aoi + 1) triMeshList = tmlNext;
                    else if (opti == aoi + 2) triMeshList = tmlNextNext;
                    else continue;

                    IBranch avOpt = avOpts[opti];

                    optKw = RouteWidthOptK(opti, nAvOpt);
                    optMw = RouteWidthOptM(opti, nAvOpt);

                    MeshTools.AddTriMeshesForConnectionAndBranch(triMeshList, tmlTaxi, tmlBus, 
                        choiceBranch, avOpt, null, false, optKw, optMw, vm, mini, player.Ped());
                }

                
                SubMesh smManual = UMapMeshRvr.TriangleSubMesh(tmlMan, dz, matManual);
                SubMesh smAuto = UMapMeshRvr.TriangleSubMesh(tmlAuto, dz, matAuto);
                SubMesh smPrev = UMapMeshRvr.TriangleSubMesh(tmlPrev, dz-oz, matMorePrev);
                SubMesh smNext = UMapMeshRvr.TriangleSubMesh(tmlNext, dz+oz, matMoreNext);
                SubMesh smPrevPrev = UMapMeshRvr.TriangleSubMesh(tmlPrevPrev, dz-oz-oz, matMorePrevPrev);
                SubMesh smNextNext = UMapMeshRvr.TriangleSubMesh(tmlNextNext, dz+oz+oz, matMoreNextNext);
                SubMesh smTaxi = UMapMeshRvr.TriangleSubMesh(tmlTaxi, dz, matTaxi);
                SubMesh smBus = UMapMeshRvr.TriangleSubMesh(tmlBus, dz, matBus);

                SubMesh[] sma = {smManual, smAuto, smPrev, smNext, smPrevPrev, smNextNext, smTaxi, smBus,};
                MeshFilter mf = gameObject.GetComponent<MeshFilter>();
                MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
                UMapMeshRvr.CreateMesh(mf, mr, sma);

                UpdateOnRouteChanged();

            }

        }
    }
}
