using uk.vroad.api;
using uk.vroad.api.etc;
using uk.vroad.api.events;
using uk.vroad.api.geom;
using uk.vroad.api.input;
using uk.vroad.api.map;
using uk.vroad.api.route;
using uk.vroad.api.sim;
using uk.vroad.api.str;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public class PlayerRouteBuilder : LBotArrive, LSimTimeStep, LSimRewind, LAppInput, LAppState, IEventDistributor
    {
        private const int OPTMASK_L = 4;
        private const int OPTMASK_F = 2;
        private const int OPTMASK_R = 1;
        private static readonly IBranch[] ZERO_BRANCH = new IBranch[] {  };
        private IBranch cachedPedBranch;
        private IBranch cachedTaxiBranch;
        private IZone cachedTaxiDestination;
        private bool cachedAboard;
        private KList<IBranch> manRouteV = new KList<IBranch>();
        private KList<IBranch> manRouteP = new KList<IBranch>();
        private KHashSet<IBranch> manRouteByUserRequest = new KHashSet<IBranch>();
        private KList<IBranch> autoRouteV = new KList<IBranch>();
        private KList<IBranch> autoRouteP = new KList<IBranch>();
        private IBranch[] allOptionsP = ZERO_BRANCH;
        private IBranch[] allOptionsV = ZERO_BRANCH;
        private IBranch[] availableOptionsP = ZERO_BRANCH;
        private IBranch[] availableOptionsV = ZERO_BRANCH;
        private AxilType optionButtonsAxilTypeP = AxilType.WalkJunction;
        private int optionButtonsEnabledMaskP = 0;
        private int optionButtonsEnabledMaskV = 0;
        private KHashSet<IStop> activeStopsForPlayer = new KHashSet<IStop>();
        private KHash<IBranch, IStop[]> activeStopsPerWalk = new KHash<IBranch, IStop[]>();
        private int busTakeStopIndex = -1;
        private bool showTaxiRoute;
        private bool bpTakeLit;
        private bool bpManPrune;
        private AppDigitalFn bpRequestLit = AppDigitalFn.NoAction;
        private bool fireRouteChangeAtTimeStep = false;
        private readonly KList<LPlayerRoute> listeners = new KList<LPlayerRoute>();
        private readonly Game game;

        public static uk.vroad.rvr.PlayerRouteBuilder Awake(Game game)
        {
            lock (typeof(PlayerRouteBuilder))
            {
                return new uk.vroad.rvr.PlayerRouteBuilder(game);
            }
        }

        private PlayerRouteBuilder(Game gm)
        {
            game = gm;
            gm.AddEventDistributor(this);
            gm.AddEventConsumer(this);
        }

        public virtual void AddEventConsumer(LEvent eventConsumer)
        {
            if (eventConsumer is LPlayerRoute) listeners.Add((LPlayerRoute)eventConsumer);
        }

        public virtual void RemoveEventConsumer(LEvent consumer)
        {
            lock (this)
            {
                if (consumer is LPlayerRoute) listeners.Remove((LPlayerRoute)consumer);
            }
        }

        public virtual bool DeregisterFireMapChange()
        {
            ClearRoute();
            return false;
        }

        public virtual void TimeRewind()
        {
            ClearRoute();
        }

        private void FireRouteChangedLater()
        {
            fireRouteChangeAtTimeStep = true;
        }

        private void FireRouteChanged()
        {
            foreach (LPlayerRoute listener in listeners)
            {
                listener.FireRouteChanged();
            }
        }

        private void ClearRoute()
        {
            autoRouteP.Clear();
            manRouteP.Clear();
            cachedPedBranch = null;
            autoRouteV.Clear();
            manRouteV.Clear();
            manRouteByUserRequest.Clear();
            cachedTaxiBranch = null;
            allOptionsP = ZERO_BRANCH;
            allOptionsV = ZERO_BRANCH;
            availableOptionsP = ZERO_BRANCH;
            availableOptionsV = ZERO_BRANCH;
            optionButtonsEnabledMaskP = 0;
            optionButtonsEnabledMaskV = 0;
            optionButtonsAxilTypeP = AxilType.WalkJunction;
            BusTakeStopIndexReset();
            ShowTaxiRoute(false);
            bpTakeLit = false;
            bpManPrune = false;
            bpRequestLit = AppDigitalFn.NoAction;
            FireRouteChangedLater();
        }

        public virtual IBranch AutoFirst(bool forTaxi)
        {
            KList<IBranch> autoRoute = forTaxi ? autoRouteV : autoRouteP;
            return autoRoute.Count == 0 ? null : autoRoute[0];
        }

        public virtual IBranch ManLast(bool forTaxi)
        {
            KList<IBranch> manRoute = forTaxi ? manRouteV : manRouteP;
            return manRoute.Count == 0 ? null : manRoute[manRoute.Count - 1];
        }

        public virtual int LitOptionIndex(bool forTaxi)
        {
            return OptionIndex(forTaxi, AutoFirst(forTaxi));
        }

        public virtual int OptionIndex(bool forTaxi, IBranch opt)
        {
            IBranch[] ba = AvailableOptions(forTaxi);
            for (int i = 0; i < ba.Length; i++)
            {
                if (ba[i] == opt) return i;
            }
            return -1;
        }

        public virtual IBranch[] AllOptions(bool forTaxi)
        {
            return forTaxi ? allOptionsV : allOptionsP;
        }

        public virtual IBranch[] AvailableOptions(bool forTaxi)
        {
            return forTaxi ? availableOptionsV : availableOptionsP;
        }

        private int OptionButtonsEnabledMask()
        {
            return ShowTaxiRoute() ? optionButtonsEnabledMaskV : optionButtonsEnabledMaskP;
        }

        private void OptionButtonEnableUpOnly(bool forTaxi)
        {
            if (forTaxi) optionButtonsEnabledMaskV = OPTMASK_F;
            else optionButtonsEnabledMaskP = OPTMASK_F;
        }

        public virtual AxilType OptionButtonsAxilType(bool forTaxi)
        {
            return forTaxi ? AxilType.RoadJunction : optionButtonsAxilTypeP;
        }

        public virtual void TimeStep()
        {
            FixedUpdateForPlayer();
            if (fireRouteChangeAtTimeStep) 
            {
                fireRouteChangeAtTimeStep = false;
                FireRouteChanged();
            }
        }

        public virtual void Depart()
        {
            FixedUpdateForPlayer();
        }

        public virtual void Arrive(IBot bot, IZone z)
        {
            if (bot is IPed && Player.IsPlayer((IPed)bot)) ClearRoute();
        }

        public virtual void OnUTurn1()
        {
            ClearRoute();
        }

        public virtual void OnUTurn2(IBranch current)
        {
            AutoRouteBuild(false, null, current);
        }

        public virtual void OnBailOut()
        {
            ClearRoute();
        }

        private void AutoRouteBuild(bool forTaxi, IBranch lastChosenCheck, IBranch selectedOption)
        {
            Player player = Player.ActivePlayer();
            if (player == null) return;
            IZone dest;
            IBranch recent;
            IRouter router;
            int mi;
            KList<IBranch> autoRoute;
            KList<IBranch> manRoute;
            ITaxi taxi = player.GetTaxi();
            if (forTaxi && taxi != null) 
            {
                router = game.Sim().RouterDrv();
                dest = taxi.GetDestination();
                recent = taxi.GetRecentBranch();
                mi = taxi.GetMind().RouterIndex();
                autoRoute = autoRouteV;
                manRoute = manRouteV;
                cachedTaxiDestination = dest;
            }
            else 
            {
                router = game.Sim().RouterPed();
                dest = player.GetDestination();
                recent = player.GetRecentBranch();
                mi = player.GetMind().RouterIndex();
                autoRoute = autoRouteP;
                manRoute = manRouteP;
            }
            autoRoute.Clear();
            if (recent == null || dest == recent.GetZone()) return;
            IBranch lastChosen = recent;
            foreach (IBranch mb in manRoute)
            {
                if (mb.GetZone() == dest) return;
                lastChosen = mb;
            }
            int dzi = dest.RouterIndex();
            IBranch current = forTaxi ? cachedTaxiBranch : cachedPedBranch;
            IBranch start = selectedOption;
            if (start == null) 
            {
                IBranch[] avba = AvailableOptions(forTaxi);
                start = LowestCostOption(router, dzi, mi, avba);
            }
            IBranch branch = start;
            IBranch prev = lastChosen;
            if (start is IWalkBranch && start == ManLast(forTaxi)) 
            {
                IBranch[] barredExits = BarredExitsOnWalkway((IWalkBranch)start);
                IBranch best = router.LowestCostExit(start, dzi, mi, barredExits);
                if (best is IWalkBranch) 
                {
                    autoRoute.Add(start);
                    branch = best;
                }
                else branch = best;
            }
            while (branch != null && !autoRoute.Contains(branch))
            {
                autoRoute.Add(branch);
                if (dest == branch.GetZone()) return;
                int notExit = NotExit(prev, branch, recent, manRoute, autoRoute);
                prev = branch;
                branch = LowestCostExitOrUTurn(player, router, prev, dzi, mi, notExit);
            }
            CheckAutorouteValidity(player, forTaxi);
            IBranch autoFirst = autoRoute.Count > 0 ? autoRoute[0] : null;
            autoRoute.Clear();
            if (autoFirst != null) autoRoute.Add(autoFirst);
        }

        private int NotExit(IBranch branch, IBranch exitToAvoid)
        {
            int nx = branch.Exits();
            for (int x = 1; x <= nx; x++)
            {
                if (branch.ExitBranch(x) == exitToAvoid) return x;
            }
            return 0;
        }

        private int NotExit(IBranch prev, IBranch branch, IBranch first, KList<IBranch> manRoute, KList<IBranch> autoRoute)
        {
            int nx = branch.Exits();
            if (branch is IBranchBusArrival) 
            {
                if (nx <= 1) return 0;
                for (int x = 1; x <= nx; x++)
                {
                    if (branch.ExitBranch(x) is IBranchBusBoarding) return x;
                }
            }
            if (prev is IBranchTaxiDestinationLane && branch is IWalkBranch) 
            {
                IWalkBranch wd = (IWalkBranch)branch;
                if (nx <= 1) return 0;
                for (int x = 1; x <= nx; x++)
                {
                    if (wd.ExitBranch(x) is IBranchTaxiOrigin) return x;
                }
            }
            if (branch is IWalkBranch && (prev is IBranchBusBoarding || prev is IBranchTaxiOriginLane)) 
            {
                for (int x = 1; x <= nx; x++)
                {
                    if (branch.ExitBranch(x) == prev) return x;
                }
            }
            for (int x_1 = 1; x_1 <= nx; x_1++)
            {
                IBranch exit = branch.ExitBranch(x_1);
                if (exit == first) return x_1;
                foreach (IBranch mb in manRoute)
                {
                    if (exit == mb) return x_1;
                }
                foreach (IBranch ab in autoRoute)
                {
                    if (exit == ab) return x_1;
                }
            }
            return 0;
        }

        public virtual bool AppInputDigitalEvent(AppDigitalFn fn, bool isPressed)
        {
            if (!isPressed) return false;
            AppState state = game.Gsm().CurrentState();
            bool acceptOptions = state == GameState.Navigating;
            if (acceptOptions) 
            {
                if (fn == GameDigitalFn.RouteSelect) return TakeLit();
                else if (fn == GameDigitalFn.RouteLeft) return RequestLit(fn);
                else if (fn == GameDigitalFn.RouteForward) return RequestLit(fn);
                else if (fn == GameDigitalFn.RouteRight) return RequestLit(fn);
                else if (fn == GameDigitalFn.RouteUndoUTurn) 
                {
                    if (RequestPrune()) return true;
                    return game.Gsc().RequestUTurn(false);
                }
            }
            return false;
        }

        public virtual bool AppInputAnalogEvent(AppAnalogFn axis, double value)
        {
            return false;
        }

        private bool TakeLit()
        {
            bpTakeLit = true;
            return true;
        }

        private bool RequestLit(AppDigitalFn fn)
        {
            bpRequestLit = fn;
            return true;
        }

        public virtual bool PruneAvailable()
        {
            bool forTaxi = ShowTaxiRoute();
            KList<IBranch> manRoute = forTaxi ? manRouteV : manRouteP;
            int nm = manRoute.Count;
            if (nm == 0) return false;
            IBranch prev = null;
            IBranch current = CachedPlayerBranch(forTaxi);
            if (ChoiceFromHere(forTaxi, prev, current)) return true;
            prev = current;
            foreach (IBranch manI in manRoute)
            {
                if (ChoiceFromHere(forTaxi, prev, manI)) return true;
                prev = manI;
            }
            return false;
        }

        private bool RequestPrune()
        {
            KList<IBranch> manRoute = ShowTaxiRoute() ? manRouteV : manRouteP;
            if (PruneAvailable()) 
            {
                bpManPrune = true;
                return true;
            }
            return false;
        }

        public virtual IBranch PreOptionBranch(bool forTaxi)
        {
            IBranch manLast = ManRouteLast(forTaxi);
            return manLast != null ? manLast : CachedPlayerBranch(forTaxi);
        }

        private KBool NewPlayerBranch(bool forTaxi, IBranch current)
        {
            if (forTaxi) cachedTaxiBranch = current;
            else cachedPedBranch = current;
            FireRouteChangedLater();
            KList<IBranch> manRoute = forTaxi ? manRouteV : manRouteP;
            int nm = manRoute.Count;
            if (nm == 1) 
            {
                IBranch man1 = manRoute[0];
                if (!current.IsConnectedTo(man1) && man1 != current) 
                {
                    if (UDbg.ROUTE_PLAYER) SE.Report(SE.ROUTER_02, current, man1);
                    manRoute.Clear();
                    nm = 0;
                }
            }
            if (nm >= 2) 
            {
                IBranch man2 = manRoute[1];
                if (man2 == current) 
                {
                    if (UDbg.ROUTE_PLAYER) SE.Report(SE.ROUTER_03, current, manRoute[0]);
                    ManRouteRemove(manRoute, 0);
                    nm--;
                }
            }
            if (nm > 0 && manRoute[0] == current) 
            {
                if (current.MidOptions() > 0 && current == AutoFirst(forTaxi)) {}
                else ManRouteRemove(manRoute, 0);
            }
            if (manRoute.Count > 0) return null;
            KList<IBranch> autoRoute = forTaxi ? autoRouteV : autoRouteP;
            int na = autoRoute.Count;
            if (na == 0) return KBool.TRUE;
            IBranch autoFirst = autoRoute[0];
            if (autoFirst == current) 
            {
                if (current.MidOptions() == 0) RemoveFirst(autoRoute);
                return KBool.FALSE;
            }
            if (current.IsConnectedTo(autoFirst)) return null;
            for (int ai = 1; ai < na; ai++)
            {
                IBranch autoI = autoRoute[ai];
                if (current == autoI) 
                {
                    if (current.MidOptions() > 0) ai--;
                    if (UDbg.ROUTE_PLAYER) 
                    {
                        if (ai >= 0) SE.Report(SE.ROUTER_04, ai, current);
                    }
                    for (int rai = 0; rai <= ai; rai++)
                    {
                        RemoveFirst(autoRoute);
                    }
                    return KBool.FALSE;
                }
            }
            return KBool.TRUE;
        }

        public virtual void FixedUpdateForPlayer()
        {
            lock (this)
            {
                Player player = Player.ActivePlayer();
                if (player == null || player.Finished()) return;
                KBool rebuildAuto = null;
                if (player.Aboard() != cachedAboard) 
                {
                    cachedAboard = player.Aboard();
                    rebuildAuto = KBool.TRUE;
                }
                ITaxi taxi = cachedAboard ? player.GetTaxi() : null;
                bool playerAboardTaxi = cachedAboard && taxi != null;
                if (game.Gsc().ToggleControlPedTaxiRequested()) 
                {
                    if (ShowTaxiRoute()) 
                    {
                        ShowTaxiRoute(false);
                        rebuildAuto = KBool.TRUE;
                    }
                    else if (!ShowTaxiRoute() && playerAboardTaxi) 
                    {
                        ShowTaxiRoute(true);
                        rebuildAuto = KBool.TRUE;
                    }
                }
                if (ShowTaxiRoute() && !playerAboardTaxi) ShowTaxiRoute(false);
                IBranch recentBranch = playerAboardTaxi ? taxi.GetRecentBranch() : player.GetRecentBranch();
                if (recentBranch == null) return;
                IBranch cachedBranch = CachedPlayerBranch(playerAboardTaxi);
                if (cachedBranch != recentBranch) 
                {
                    KBool rebuildAuto2 = NewPlayerBranch(playerAboardTaxi, recentBranch);
                    if (rebuildAuto == null || (rebuildAuto == KBool.FALSE && rebuildAuto2 == KBool.TRUE)) rebuildAuto = rebuildAuto2;
                }
                if (playerAboardTaxi && CachedPlayerBranch(false) == null) 
                {
                    NewPlayerBranch(false, player.GetRecentBranch());
                    rebuildAuto = KBool.TRUE;
                }
                if (rebuildAuto != null) 
                {
                    RouteOptionsFromLastSelected(player.Ped(), playerAboardTaxi);
                    if (rebuildAuto.BoolValue()) AutoRouteBuild(playerAboardTaxi, null, null);
                }
                bool reselectExit = false;
                bool forTaxi = ShowTaxiRoute();
                if (bpManPrune) 
                {
                    bpManPrune = false;
                    if (ManRouteDeleteLast(player, forTaxi)) 
                    {
                        CheckAutorouteValidity(player, forTaxi);
                        FireRouteChangedLater();
                        reselectExit = true;
                    }
                    else game.Gsc().RequestUTurn(false);
                }
                else if (bpTakeLit) 
                    {
                        bpTakeLit = false;
                        TakeLit(player, forTaxi);
                        FireRouteChangedLater();
                        reselectExit = true;
                    }
                    else if (bpRequestLit != AppDigitalFn.NoAction) 
                    {
                        AppDigitalFn fn = bpRequestLit;
                        bpRequestLit = AppDigitalFn.NoAction;
                        KBool result = ApplyRequestedOption(fn, forTaxi);
                        if (result == KBool.TRUE) reselectExit = true;
                        else if (result == KBool.FALSE) bpTakeLit = true;
                    }
                if (reselectExit) 
                {
                    if (!ShowTaxiRoute()) player.Ped().ForceChooseNextLeg();
                    else if (playerAboardTaxi) taxi.ReselectExit();
                }
                if (!cachedAboard && recentBranch.MidOptions() > 0) 
                {
                    IBranch[] allOpts = AllOptions(forTaxi);
                    IBranch[] newRouteOptions = PrunePassedParkingOrStops((IWalkBranch)recentBranch, player, allOpts);
                    if (newRouteOptions.Length < allOpts.Length) SetRouteOptions(false, newRouteOptions);
                }
                if (cachedAboard && recentBranch is IBranchBusOnboard) PrunePassedOnboardStops(player);
                if (playerAboardTaxi && !ShowTaxiRoute() && player.GetRecentBranch() is IBranchTaxiOrigin) PruneUnreachableDestinationParking(player);
            }
        }

        private void CheckAutorouteValidity(Player player, bool forTaxi)
        {
            KList<IBranch> autoRoute = forTaxi ? autoRouteV : autoRouteP;
            int na = autoRoute.Count;
            if (na < 2) return;
            int deleteFromI = -1;
            if (!forTaxi) 
            {
                IBranch autoLast = autoRoute[na - 1];
                if (autoLast.GetZone() != player.GetDestination()) deleteFromI = 1;
            }
            for (int ai = 1; ai < na && deleteFromI < 0; ai++)
            {
                IBranch branchI = autoRoute[ai];
                for (int aj = 0; aj < ai && deleteFromI < 0; aj++)
                {
                    IBranch branchJ = autoRoute[aj];
                    if (branchI == branchJ) deleteFromI = ai;
                }
            }
            if (deleteFromI > 0) 
            {
                for (int ak = na - 1; ak >= deleteFromI; ak--)
                {
                    autoRoute.Remove(ak);
                }
            }
        }

        private void TakeLit(Player player, bool forTaxi)
        {
            KList<IBranch> autoRoute = forTaxi ? autoRouteV : autoRouteP;
            if (autoRoute.Count == 0) return;
            IBranch autoFirst = autoRoute[0];
            int nmo = autoFirst.MidOptions();
            if (autoFirst == ManLast(forTaxi)) TakeLitOnWalkwayWithStops(autoFirst);
            else if (nmo > 0) 
            {
                IWalkBranch wb = (IWalkBranch)autoFirst;
                int naks = ActiveStopCount(wb);
                if (naks >= 2) busTakeStopIndex = wb.IsForward() ? 0 : naks - 1;
                ManRouteAdd(player, forTaxi, autoFirst, true);
            }
            else 
                {
                    RemoveFirst(autoRoute);
                    ManRouteAdd(player, forTaxi, autoFirst, true);
                }
        }

        private void TakeLitOnWalkwayWithStops(IBranch branch)
        {
            int nmo = branch.MidOptions();
            if (nmo == 0 || !(branch is IWalkBranch)) return;
            IWalkBranch walkBranchWithStops = (IWalkBranch)branch;
            bool moveToEndOfWalk = false;
            if (nmo == 1) moveToEndOfWalk = true;
            else 
            {
                bool fwd = walkBranchWithStops.IsForward();
                int naks = ActiveStopCount(walkBranchWithStops);
                int bsti = BusTakeStopIndex();
                if (bsti < 0 || bsti >= naks) return;
                int bstiNew = bsti + (fwd ? +1 : -1);
                if (bstiNew < 0 || bstiNew >= naks) 
                {
                    moveToEndOfWalk = true;
                    BusTakeStopIndexReset();
                }
                else busTakeStopIndex = bstiNew;
            }
            if (moveToEndOfWalk) 
            {
                KList<IBranch> autoRoute = autoRouteP;
                RemoveFirst(autoRoute);
                IBranch[] exitWalks = WalksAtEndOf(walkBranchWithStops);
                IBranch newAutoFirst = autoRoute.Count > 0 ? autoRoute[0] : null;
                if (newAutoFirst is IWalkBranch) {}
                else 
                {
                    newAutoFirst = LowestCostWalkExit(exitWalks);
                    AutoRouteBuild(false, walkBranchWithStops, newAutoFirst);
                }
                SetRouteOptions(false, exitWalks);
            }
            else SetRouteOptions(false, AllOptions(false));
        }

        private void RemoveFirst(KList<IBranch> autoRoute)
        {
            autoRoute.Remove(0);
        }

        public virtual void BackOutOfStopOrBayToWalkway(IWalkBranch walkBranchWithStops)
        {
            Player player = Player.ActivePlayer();
            if (player == null) return;
            IPed playerPed = player.Ped();
            if (walkBranchWithStops.MidOptions() > 1) 
            {
                int naks = ActiveStopCount(walkBranchWithStops);
                busTakeStopIndex = walkBranchWithStops.IsForward() ? 0 : naks - 1;
            }
            NewPlayerBranch(false, walkBranchWithStops);
            RouteOptionsFrom(playerPed, false, null, walkBranchWithStops);
            manRouteP.Add(walkBranchWithStops);
            AutoRouteBuild(false, walkBranchWithStops, walkBranchWithStops);
            IBranch auto1 = autoRouteP.Count > 1 ? autoRouteP[1] : null;
            if (auto1 is IWalkBranch) playerPed.NextWalkway((IWalkBranch)auto1);
        }

        private IBranch[] WalksAtEndOf(IWalkBranch walkBranch)
        {
            KList<IBranch> endList = new KList<IBranch>();
            int nx = walkBranch.ExitsXU();
            for (int xi = 1; xi <= nx; xi++)
            {
                IBranch exit = walkBranch.ExitBranch(xi);
                if (exit is IWalkBranch) endList.Add(exit);
            }
            return ((IBranch[])endList.ToArray(ZERO_BRANCH));
        }

        private IBranch LowestCostWalkExit(IBranch[] exitWalks)
        {
            Player player = Player.ActivePlayer();
            IRouter router = game.Sim().RouterPed();
            int dzi = player.GetDestination().RouterIndex();
            int mi = player.GetMind().RouterIndex();
            return LowestCostOption(router, dzi, mi, exitWalks);
        }

        private IBranch LowestCostOption(IRouter router, int dzi, int mi, IBranch[] options)
        {
            double lowest = Price.eINF_KJ;
            IBranch best = null;
            foreach (IBranch option in options)
            {
                double cost = router.RouteCost(option, dzi, mi).KJ();
                if (cost < lowest) 
                {
                    lowest = cost;
                    best = option;
                }
            }
            return best;
        }

        private IBranch LowestCostExitOrUTurn(Player player, IRouter router, IBranch branch, int dzi, int mi, int notExit)
        {
            IBranch exit;
            if (branch is IBranchTaxiOriginLane) 
            {
                IBranch[] finiteUnsorted = FiniteCostExitsUnsorted(false, null, branch);
                IBranch[] sorted = router.SortBranchesByDistanceFromDest(dzi, mi, finiteUnsorted);
                exit = sorted.Length > 0 ? sorted[0] : null;
            }
            else exit = router.LowestCostExit(branch, dzi, mi, notExit);
            if (exit == null && branch is IWalkBranch) 
            {
                int nx = branch.Exits();
                for (int xi = 1; xi <= nx; xi++)
                {
                    exit = branch.ExitBranch(xi);
                    if (exit is IWalkBranch) return exit;
                }
                IWalkBranch wd = (IWalkBranch)branch;
                exit = wd.GetOpposite();
            }
            return exit;
        }

        private IBranch[] BarredExitsOnWalkway(IWalkBranch walkBranchWithStops)
        {
            IStop[] aksa = ActiveStops(walkBranchWithStops);
            int naks = aksa.Length;
            int nx = walkBranchWithStops.Exits();
            if (naks == 0) return new IBranch[] { walkBranchWithStops.ExitBranch(nx) };
            if (naks == 1) return new IBranch[] { aksa[0].GetBoardingBranch() };
            bool fwd = walkBranchWithStops.IsForward();
            int bsti = BusTakeStopIndex();
            if (bsti < 0 || bsti >= naks) return ZERO_BRANCH;
            else 
            {
                int nb = fwd ? 1 + bsti : naks - bsti;
                IBranch[] ba = new IBranch[nb];
                if (fwd) 
                {
                    for (int bi = 0; bi < nb; bi++)
                    {
                        ba[bi] = aksa[bi].GetBoardingBranch();
                    }
                }
                else 
                {
                    for (int bi = 0; bi < nb; bi++)
                    {
                        ba[bi] = aksa[naks - 1 - bi].GetBoardingBranch();
                    }
                }
                return ba;
            }
        }

        public virtual bool OptionsRotate(bool forTaxi)
        {
            return AvailableOptions(forTaxi).Length >= 4;
        }

        public virtual bool OptionsRotateContinuous(bool forTaxi)
        {
            switch (OptionButtonsAxilType(forTaxi))
            {
                case AxilType.TaxiDest:
                {
                    return true;
                }
                case AxilType.BusRoute:
                {
                    return true;
                }
                default:
                {
                    break;
                }
            }
            return false;
        }

        public virtual bool OptionLeftEnabled()
        {
            return (OptionButtonsEnabledMask() & OPTMASK_L) != 0;
        }

        public virtual bool OptionForwardEnabled()
        {
            return (OptionButtonsEnabledMask() & OPTMASK_F) != 0;
        }

        public virtual bool OptionRightEnabled()
        {
            return (OptionButtonsEnabledMask() & OPTMASK_R) != 0;
        }

        public virtual bool ButtonEnabledForFn(AppDigitalFn fn)
        {
            if (fn == GameDigitalFn.RouteLeft && OptionLeftEnabled()) return true;
            if (fn == GameDigitalFn.RouteForward && OptionForwardEnabled()) return true;
            if (fn == GameDigitalFn.RouteRight && OptionRightEnabled()) return true;
            return false;
        }

        public virtual IBranch OptionOnButton(AppDigitalFn fn)
        {
            bool forTaxi = ShowTaxiRoute();
            IBranch[] oba = AvailableOptions(forTaxi);
            int no = oba.Length;
            if (no == 0) return null;
            if (no == 1) return oba[0];
            if (OptionsRotate(forTaxi)) 
            {
                int loi = LitOptionIndex(forTaxi);
                if (loi < 0 || loi >= no) return null;
                if (!OptionsRotateContinuous(forTaxi)) 
                {
                    if (fn == GameDigitalFn.RouteLeft && loi == 0) return null;
                    if (fn == GameDigitalFn.RouteRight && loi == no - 1) return null;
                }
                if (fn == GameDigitalFn.RouteLeft) return oba[loi == 0 ? no - 1 : loi - 1];
                if (fn == GameDigitalFn.RouteForward) return oba[loi];
                if (fn == GameDigitalFn.RouteRight) return oba[loi == no - 1 ? 0 : loi + 1];
                return null;
            }
            if (fn == GameDigitalFn.RouteLeft) return oba[0];
            if (fn == GameDigitalFn.RouteForward) return !OptionLeftEnabled() ? oba[0] : oba[1];
            if (fn == GameDigitalFn.RouteRight) return no == 3 ? oba[2] : oba[1];
            return null;
        }

        private IBranch LastChosen(bool forTaxi)
        {
            Player player = Player.ActivePlayer();
            KList<IBranch> manRoute = forTaxi ? manRouteV : manRouteP;
            int nm = manRoute.Count;
            return nm > 0 ? manRoute[nm - 1] : player.GetRecentBranch();
        }

        public virtual KBool ApplyRequestedOption(AppDigitalFn fn, bool forTaxi)
        {
            IBranch[] avOpt = AvailableOptions(forTaxi);
            int navo = avOpt.Length;
            if (navo < 2) return null;
            if (!ButtonEnabledForFn(fn)) return null;
            IBranch newAutoFirst;
            if (OptionsRotate(forTaxi) && fn != GameDigitalFn.RouteForward) 
            {
                bool continuous = OptionsRotateContinuous(forTaxi);
                int loi = LitOptionIndex(forTaxi);
                int inc = fn == GameDigitalFn.RouteRight ? +1 : -1;
                int loi2 = loi + inc;
                if (loi2 < 0) loi2 = continuous ? navo - 1 : 0;
                else if (loi2 >= navo) loi2 = continuous ? 0 : navo - 1;
                if (loi2 == loi) return null;
                newAutoFirst = avOpt[loi2];
            }
            else 
                {
                    IBranch newLitOption = OptionOnButton(fn);
                    if (newLitOption == null) return null;
                    IBranch currentlyLit = AutoFirst(forTaxi);
                    if (newLitOption == currentlyLit) return KBool.FALSE;
                    newAutoFirst = newLitOption;
                }
            AutoRouteBuild(forTaxi, null, newAutoFirst);
            FireRouteChangedLater();
            return KBool.TRUE;
        }

        public virtual IBranch CachedPlayerBranch(bool forTaxi)
        {
            return forTaxi ? cachedTaxiBranch : cachedPedBranch;
        }

        private IBranch[] PrunePassedParkingOrStops(IWalkBranch wb, Player player, IBranch[] routeOptions)
        {
            int nnwb = 0;
            foreach (IBranch b in routeOptions)
            {
                if (!(b is IWalkBranch)) nnwb++;
            }
            if (nnwb == 0) return routeOptions;
            bool fwd = wb.IsForward();
            KHashSet<IBranch> branchesAlreadyPassed = new KHashSet<IBranch>();
            int naks = ActiveStopCount(wb);
            int nxsi = -1;
            if (naks > 0) 
            {
                IStop[] aksa = ActiveStops(wb);
                double pd = player.Ped().LocusDistance();
                if (fwd) 
                {
                    nxsi = 0;
                    foreach (IStop aks in aksa)
                    {
                        if (pd > aks.DistanceOnFootpath()) 
                        {
                            branchesAlreadyPassed.Add(aks.GetBoardingBranch());
                            nxsi++;
                        }
                        else break;
                    }
                    if (nxsi >= naks) nxsi = -1;
                }
                else 
                    {
                        double plen = wb.GetWalkway().Length();
                        nxsi = naks - 1;
                        for (int ksi = naks - 1; ksi >= 0; ksi--)
                        {
                            IStop aks = aksa[ksi];
                            if (pd > plen - aks.DistanceOnFootpath()) 
                            {
                                branchesAlreadyPassed.Add(aks.GetBoardingBranch());
                                nxsi--;
                            }
                            else break;
                        }
                    }
            }
            if (wb.HasTaxiZone()) 
            {
                IFootpath fp = (IFootpath)wb.GetWalkway();
                ITaxiZone taxiZone = fp.GetRunningLane().GetTaxiZone();
                int nb = taxiZone.Bays();
                double pd = player.Ped().LocusDistance();
                if (fwd) 
                {
                    double lastBayEnd = taxiZone.BayEndDistance(nb - 1);
                    if (pd > lastBayEnd) branchesAlreadyPassed.Add(taxiZone.GetTaxiOriginLane());
                }
                else 
                    {
                        double firstBayStart = taxiZone.BayStartDistance(0);
                        double plen = wb.GetWalkway().Length();
                        if (pd > plen - firstBayStart) branchesAlreadyPassed.Add(taxiZone.GetTaxiOriginLane());
                    }
            }
            IBranch[] filteredOptions;
            int bsti = BusTakeStopIndex();
            if (branchesAlreadyPassed.Count == 0) 
            {
                filteredOptions = routeOptions;
                if (naks > 1) 
                {
                    if (fwd && bsti < 0) busTakeStopIndex = 0;
                    if (!fwd && bsti < 0) busTakeStopIndex = naks - 1;
                }
            }
            else 
                {
                    KList<IBranch> newOptions = new KList<IBranch>();
                    int nr = routeOptions.Length;
                    for (int roi = 0; roi < nr; roi++)
                    {
                        IBranch ro = routeOptions[roi];
                        if (!branchesAlreadyPassed.Contains(ro)) newOptions.Add(ro);
                    }
                    filteredOptions = ((IBranch[])newOptions.ToArray(ZERO_BRANCH));
                    if (naks > 1) 
                    {
                        if (fwd && bsti < nxsi) busTakeStopIndex = nxsi;
                        if (!fwd && bsti < 0 || bsti > nxsi) busTakeStopIndex = nxsi;
                    }
                }
            return filteredOptions;
        }

        private void PrunePassedOnboardStops(Player player)
        {
            IBus bus = player.GetBus();
            if (bus == null) return;
            IBranch[] routeOptions = AllOptions(false);
            int nr = routeOptions.Length;
            if (nr == 0) return;
            if (!(routeOptions[0] is IBranchBusArrival)) return;
            KList<IBranch> newRouteOptions = new KList<IBranch>();
            IHalt nextHalt = bus.GetNextHalt();
            IBusroute busroute = bus.GetBusroute();
            int nsi = busroute.IndexOf(nextHalt.GetStop());
            for (int roi = 0; roi < nr && nsi >= 0; roi++)
            {
                IBranch b = routeOptions[roi];
                int si = busroute.IndexOf(((IBranchBusArrival)b).GetStop());
                if (si >= nsi) newRouteOptions.Add(b);
            }
            int nnr = newRouteOptions.Count;
            if (nnr < nr) SetRouteOptions(false, ((IBranch[])newRouteOptions.ToArray(new IBranch[nnr])));
        }

        private void PruneUnreachableDestinationParking(Player player)
        {
            ITaxi taxi = player.GetTaxi();
            if (taxi == null) return;
            IRoad currentRoad = taxi.GetRoad();
            if (currentRoad == null) return;
            IMind vm = taxi.GetMind();
            IRouter vRouter = game.Sim().RouterDrv();
            bool forTaxi = false;
            IBranch[] routeOptions = AllOptions(forTaxi);
            bool setNewOptions = false;
            KList<IBranch> reachable = new KList<IBranch>();
            foreach (IBranch b in routeOptions)
            {
                if (b is IBranchTaxiDestination) 
                {
                    if (vRouter.RouteExists(currentRoad, b.GetZone(), vm)) reachable.Add(b);
                    else setNewOptions = true;
                }
                else return;
            }
            if (setNewOptions) SetRouteOptions(false, ((IBranch[])reachable.ToArray(new IBranch[reachable.Count])));
        }

        private void ManRouteAdd(Player player, bool forTaxi, IBranch branch, bool userAction)
        {
            KList<IBranch> manRoute = forTaxi ? manRouteV : manRouteP;
            int nm = manRoute.Count;
            IBranch prev = nm > 0 ? manRoute[nm - 1] : player.GetRecentBranch();
            ManRouteAddRecurseSingleExit(forTaxi, null, prev, branch, userAction);
            nm = manRoute.Count;
            IBranch manLast = manRoute[nm - 1];
            if (nm > 1) prev = manRoute[nm - 2];
            RouteOptionsFrom(player.Ped(), forTaxi, prev, manLast);
            AutoRouteBuild(forTaxi, manLast, null);
            if (branch is IBranchBusOnboard) game.Sim().CheckBusLeg(player.Ped(), (IBranchBusOnboard)branch);
        }

        private bool ManRouteDeleteLast(Player player, bool forTaxi)
        {
            KList<IBranch> autoRoute = forTaxi ? autoRouteV : autoRouteP;
            KList<IBranch> manRoute = forTaxi ? manRouteV : manRouteP;
            int nm = manRoute.Count;
            if (nm == 0) return false;
            IBranch lastMan = manRoute[nm - 1];
            IBranch autoFirst = AutoFirst(forTaxi);
            IPed playerPed = player.Ped();
            int nmo = lastMan.MidOptions();
            bool removeLastMan = true;
            bool insertIntoAuto = true;
            if (nmo > 0) 
            {
                IWalkBranch lastManWalk = (IWalkBranch)lastMan;
                bool fwd = lastManWalk.IsForward();
                if (nmo == 1) 
                {
                    if (lastMan == autoFirst) 
                    {
                        removeLastMan = true;
                        insertIntoAuto = false;
                    }
                    else if (lastMan.IsConnectedTo(autoFirst)) 
                    {
                        removeLastMan = (lastMan == player.GetRecentBranch());
                        insertIntoAuto = true;
                    }
                    else return false;
                }
                else 
                    {
                        int bsti = BusTakeStopIndex();
                        int naks = ActiveStopCount(lastManWalk);
                        if (lastMan == autoFirst) 
                        {
                            if (bsti < 0 || bsti >= naks) return false;
                            else if ((fwd && bsti == 0) || (!fwd && bsti == naks - 1)) 
                            {
                                removeLastMan = true;
                                insertIntoAuto = false;
                                BusTakeStopIndexReset();
                            }
                            else 
                            {
                                int bstiNew = bsti + (fwd ? -1 : +1);
                                if (bstiNew < 0 || bstiNew >= naks) return false;
                                busTakeStopIndex = bstiNew;
                                removeLastMan = (lastMan == player.GetRecentBranch());
                                insertIntoAuto = false;
                            }
                        }
                        else if (lastMan.IsConnectedTo(autoFirst)) 
                            {
                                removeLastMan = (lastMan == player.GetRecentBranch());
                                insertIntoAuto = true;
                                busTakeStopIndex = fwd ? naks - 1 : 0;
                            }
                    }
            }
            if (removeLastMan) 
            {
                lastMan = ManRouteRemove(manRoute, nm - 1);
                nm--;
            }
            if (insertIntoAuto) autoRoute.Add(0, lastMan);
            IBranch preOptionsBranch;
            IBranch prev;
            if (nm > 0) 
            {
                IBranch newLastMan = manRoute[nm - 1];
                prev = nm > 1 ? manRoute[nm - 2] : CachedPlayerBranch(forTaxi);
                if (NoChoiceFromHere(forTaxi, prev, newLastMan, false)) return ManRouteDeleteLast(player, forTaxi);
                preOptionsBranch = newLastMan;
            }
            else 
                {
                    preOptionsBranch = CachedPlayerBranch(forTaxi);
                    prev = null;
                    if (NoChoiceFromHere(forTaxi, prev, preOptionsBranch, true)) 
                    {
                        RouteOptionsFrom(playerPed, forTaxi, prev, preOptionsBranch);
                        return false;
                    }
                }
            if (removeLastMan && insertIntoAuto && preOptionsBranch.MidOptions() > 0) 
            {
                IBranch[] exitWalks = WalksAtEndOf((IWalkBranch)preOptionsBranch);
                if (exitWalks.Length > 1) SetRouteOptions(forTaxi, exitWalks);
                else RouteOptionsFrom(playerPed, forTaxi, prev, preOptionsBranch);
            }
            else RouteOptionsFrom(playerPed, forTaxi, prev, preOptionsBranch);
            return true;
        }

        private IBranch ManRouteRemove(KList<IBranch> manRoute, int mi)
        {
            if (mi < 0 || mi >= manRoute.Count) return null;
            IBranch removed = manRoute.Remove(mi);
            if (removed != null) manRouteByUserRequest.Remove(removed);
            return removed;
        }

        private IBranch[] FiniteCostExitsUnsorted(bool forTaxi, IBranch optionalPrev, IBranch entry)
        {
            Player player = Player.ActivePlayer();
            if (player == null || entry == null) return ZERO_BRANCH;
            ITaxi taxi = player.GetTaxi();
            if (forTaxi && taxi == null) return ZERO_BRANCH;
            IZone dest = forTaxi ? taxi.GetDestination() : player.GetDestination();
            IMind mind = forTaxi ? taxi.GetMind() : player.GetMind();
            if (entry.GetZone() == dest) return ZERO_BRANCH;
            IRouter router = forTaxi ? (IRouter)game.Sim().RouterDrv() : (IRouter)game.Sim().RouterPed();
            int dzi = dest.RouterIndex();
            int mi = mind.RouterIndex();
            int nx = entry.ExitsXU();
            KList<IBranch> unsorted = new KList<IBranch>();
            for (int xi = 1; xi <= nx; xi++)
            {
                IBranch exit = entry.ExitBranch(xi);
                if (IgnoreExit(optionalPrev, entry, exit)) continue;
                if (entry.IsBarredTurn(exit)) continue;
                bool checkCost = true;
                if (exit is IWalkBranch) checkCost = false;
                if (exit is IBranchTaxiDestination) 
                {
                    IDrvZone drvZone = ((IBranchTaxiDestination)exit).GetDrvZone();
                    IPedZone directPedZone = drvZone.GetDirectPedZone();
                    if (directPedZone != null && directPedZone != dest) continue;
                }
                if (checkCost && router.RouteCost(exit, dzi, mi).KJ() > Price.eINF_KJ) continue;
                unsorted.Add(exit);
            }
            return ((IBranch[])unsorted.ToArray(ZERO_BRANCH));
        }

        private void RouteOptionsFromLastSelected(IPed player, bool forTaxiRoute)
        {
            KList<IBranch> manRoute = forTaxiRoute ? manRouteV : manRouteP;
            int nm = manRoute.Count;
            ITaxi taxi = player.GetTaxi();
            IBranch recent = forTaxiRoute && taxi != null ? taxi.GetRecentBranch() : player.GetRecentBranch();
            IBranch last = nm > 0 ? manRoute[nm - 1] : recent;
            IBranch prev = nm > 1 ? manRoute[nm - 2] : nm == 1 ? recent : null;
            RouteOptionsFrom(player, forTaxiRoute, prev, last);
        }

        private void RouteOptionsFrom(IPed player, bool forTaxi, IBranch optionalPrev, IBranch lastChosen)
        {
            IBranch[] finiteUnsorted = FiniteCostExitsUnsorted(forTaxi, optionalPrev, lastChosen);
            IBranch[] routeOptions;
            if (lastChosen is IBranchTaxiOrigin) 
            {
                IRouter routerP = game.Sim().RouterPed();
                int dzi = player.GetDestination().RouterIndex();
                int mi = player.GetMind().RouterIndex();
                IBranch[] sorted = routerP.SortBranchesByDistanceFromDest(dzi, mi, finiteUnsorted);
                int nFirst = 5;
                int nRandom = 2;
                routeOptions = routerP.Select(nFirst, nRandom, sorted);
            }
            else routeOptions = finiteUnsorted;
            SetRouteOptions(forTaxi, routeOptions);
        }

        private void CheckTypes(AxilType at, IBranch from, IBranch[] routeOptions, bool immediate)
        {
            switch (at)
            {
                case AxilType.WalkJunction:
                {
                    if (!(from is IWalkBranch)) throw new BranchTypeException(from);
                    break;
                }
                case AxilType.RoadJunction:
                {
                    if (!(from is IRoad)) throw new BranchTypeException(from);
                    break;
                }
                case AxilType.TaxiIn:
                {
                    if (!(from is IWalkBranch)) throw new BranchTypeException(from);
                    break;
                }
                case AxilType.TaxiDest:
                {
                    if (!(from is IBranchTaxiOrigin)) throw new BranchTypeException(from);
                    break;
                }
                case AxilType.TaxiOut:
                {
                    if (!(from is IBranchTaxiDestination)) throw new BranchTypeException(from);
                    break;
                }
                case AxilType.BusTake:
                {
                    if (!(from is IWalkBranch)) throw new BranchTypeException(from);
                    break;
                }
                case AxilType.BusRoute:
                {
                    if (!(from is IBranchBusBoarding)) throw new BranchTypeException(from);
                    break;
                }
                case AxilType.BusDest:
                {
                    if (!(from is IBranchBusOnboard)) throw new BranchTypeException(from);
                    break;
                }
                case AxilType.BusOut:
                {
                    if (!(from is IBranchBusArrival)) throw new BranchTypeException(from);
                    break;
                }
            }
            int countWalks = 0;
            foreach (IBranch exit in routeOptions)
            {
                bool isWalk = exit is IWalkBranch;
                bool isBusOn = exit is IBranchBusBoarding;
                bool isTaxiOn = exit is IBranchTaxiOrigin;
                if (isWalk) countWalks++;
                switch (at)
                {
                    case AxilType.WalkJunction:
                    {
                        if (!(isWalk || isBusOn | isTaxiOn)) throw new BranchTypeException(exit);
                        break;
                    }
                    case AxilType.RoadJunction:
                    {
                        if (!(exit is IRoad)) throw new BranchTypeException(exit);
                        break;
                    }
                    case AxilType.TaxiDest:
                    {
                        if (!(exit is IBranchTaxiDestination)) throw new BranchTypeException(exit);
                        break;
                    }
                    case AxilType.TaxiOut:
                    {
                        if (!(isWalk)) throw new BranchTypeException(exit);
                        break;
                    }
                    case AxilType.BusRoute:
                    {
                        if (!(exit is IBranchBusOnboard)) throw new BranchTypeException(exit);
                        break;
                    }
                    case AxilType.BusDest:
                    {
                        if (!(exit is IBranchBusArrival)) throw new BranchTypeException(exit);
                        break;
                    }
                    case AxilType.BusOut:
                    {
                        if (!(isWalk || exit is IBranchBusBoarding)) throw new BranchTypeException(exit);
                        break;
                    }
                    case AxilType.BusTake:
                    {
                        if (!(isBusOn || isWalk)) throw new BranchTypeException(exit);
                        break;
                    }
                    case AxilType.TaxiIn:
                    {
                        if (!(isTaxiOn || isWalk)) throw new BranchTypeException(exit);
                        break;
                    }
                }
            }
            int nro = routeOptions.Length;
            if (immediate) switch (at)
{
    case AxilType.WalkJunction:
    {
        if (!(nro == countWalks)) throw new BranchTypeException(from);
        break;
    }
    case AxilType.TaxiIn:
    {
        if (!(nro == 2 && countWalks == 1)) throw new BranchTypeException(from);
        break;
    }
    case AxilType.TaxiOut:
    {
        if (!(nro == 2 && countWalks == 2)) throw new BranchTypeException(from);
        break;
    }
    case AxilType.BusTake:
    {
        if (!(nro == 2 && countWalks == 1)) throw new BranchTypeException(from);
        break;
    }
    case AxilType.BusOut:
    {
        if (!(nro <= 3 && countWalks == 2)) throw new BranchTypeException(from);
        break;
    }
    default:
    {
        break;
    }
}
        else switch (at)
{
    case AxilType.TaxiIn:
    {
        if (!(nro == countWalks + 1)) throw new BranchTypeException(from);
        break;
    }
    case AxilType.TaxiOut:
    {
        if (!(nro == 2 && countWalks == 2)) throw new BranchTypeException(from);
        break;
    }
    case AxilType.BusTake:
    {
        if (!(countWalks >= 0 && nro > countWalks)) throw new BranchTypeException(from);
        break;
    }
    case AxilType.BusOut:
    {
        if (!(nro <= 3 && countWalks == 2)) throw new BranchTypeException(from);
        break;
    }
    default:
    {
        break;
    }
}
    }

    private IBranch[] ImmediateOptions(bool forTaxi, AxilType at, IBranch entry, IBranch[] ba)
    {
        switch (at)
        {
            case AxilType.RoadJunction:
            case AxilType.TaxiDest:
            case AxilType.TaxiOut:
            case AxilType.BusRoute:
            case AxilType.BusDest:
            case AxilType.BusOut:
            {
                return ba;
            }
            case AxilType.WalkJunction:
            {
                KList<IBranch> immediateList = new KList<IBranch>();
                foreach (IBranch br in ba)
                {
                    immediateList.Add(br);
                }
                return ((IBranch[])immediateList.ToArray(ZERO_BRANCH));
            }
            case AxilType.TaxiIn:
            {
                IBranch parking = null;
                foreach (IBranch br in ba)
                {
                    if (br is IBranchTaxiOrigin) 
                    {
                        parking = br;
                        break;
                    }
                }
                return new IBranch[] { parking, entry };
            }
            case AxilType.BusTake:
            {
                IWalkBranch wb = (IWalkBranch)entry;
                IStop[] aksa = ActiveStops(wb);
                int naks = aksa.Length;
                if (naks == 0) return new IBranch[] { entry };
                int aksi = naks == 1 ? 0 : BusTakeStopIndex();
                if (aksi < 0 || aksi >= naks) aksi = wb.IsForward() ? 0 : naks - 1;
                return new IBranch[] { aksa[aksi].GetBoardingBranch(), entry };
            }
        }
        return ZERO_BRANCH;
    }

    private Angle OptionSortAngle(AxilType at, IBranch entryBranch, IBranch exitBranch)
    {
        Angle entryAngleB = Angle.A0;
        if (at == AxilType.RoadJunction) 
        {
            IRoad rd1 = (IRoad)entryBranch;
            IRoad rd2 = (IRoad)exitBranch;
            return rd2.BearingA().Minus(rd1.BearingB()).RangeN180();
        }
        if (at == AxilType.WalkJunction) 
        {
            IWalkBranch wd1 = (IWalkBranch)entryBranch;
            IWalkBranch wd2 = (IWalkBranch)exitBranch;
            Angle directAngleToNextWalkway = wd1.FinishLocation().BearingTo(wd2.StartLocation());
            return directAngleToNextWalkway.Minus(wd1.BearingFinish()).RangeN180();
        }
        return entryBranch.FinishLocation().BearingTo(exitBranch.FinishLocation()).RangeN180();
    }

    private IBranch[] SortOptions(AxilType at, IBranch entry, IBranch[] immediateOptions)
    {
        int nio = immediateOptions.Length;
        IBranch[] sortedOptions;
        int mask;
        bool onRight = game.Map().DriveOnRight();
        if (at == AxilType.TaxiIn) 
        {
            sortedOptions = immediateOptions;
            mask = OPTMASK_L + OPTMASK_F;
            if (onRight != ((IWalkBranch)entry).IsForward()) 
            {
                sortedOptions = new IBranch[] { immediateOptions[1], immediateOptions[0] };
                mask = OPTMASK_F + OPTMASK_R;
            }
        }
        else if (at == AxilType.TaxiOut) 
            {
                sortedOptions = immediateOptions;
                mask = OPTMASK_L + OPTMASK_R;
                if (!onRight) sortedOptions = new IBranch[] { immediateOptions[1], immediateOptions[0] };
            }
            else if (at == AxilType.BusTake) 
                {
                    sortedOptions = immediateOptions;
                    mask = OPTMASK_L + OPTMASK_F;
                    if (onRight != ((IWalkBranch)entry).IsForward()) 
                    {
                        sortedOptions = new IBranch[] { immediateOptions[1], immediateOptions[0] };
                        mask = OPTMASK_F + OPTMASK_R;
                    }
                }
                else if (at == AxilType.BusRoute) 
                    {
                        sortedOptions = immediateOptions;
                        if (nio == 2) mask = OPTMASK_L + OPTMASK_R;
                        else mask = OPTMASK_L + OPTMASK_F + OPTMASK_R;
                    }
                    else if (at == AxilType.BusDest) 
                        {
                            sortedOptions = immediateOptions;
                            if (nio == 2) mask = OPTMASK_L + OPTMASK_R;
                            else mask = OPTMASK_L + OPTMASK_F + OPTMASK_R;
                        }
                        else if (at == AxilType.BusOut) 
                            {
                                if (nio == 3) 
                                {
                                    IWalkBranch walkF = (IWalkBranch)immediateOptions[0];
                                    IWalkBranch walkR = (IWalkBranch)immediateOptions[1];
                                    IBranch reBoardOption = immediateOptions[2];
                                    mask = OPTMASK_L + OPTMASK_F + OPTMASK_R;
                                    if (onRight) sortedOptions = new IBranch[] { walkF, reBoardOption, walkR };
                                    else sortedOptions = new IBranch[] { walkR, reBoardOption, walkF };
                                }
                                else if (nio == 2) 
                                    {
                                        IWalkBranch walkF = (IWalkBranch)immediateOptions[0];
                                        IWalkBranch walkR = (IWalkBranch)immediateOptions[1];
                                        mask = OPTMASK_L + OPTMASK_R;
                                        if (onRight) sortedOptions = new IBranch[] { walkF, walkR };
                                        else sortedOptions = new IBranch[] { walkR, walkF };
                                    }
                                    else return ZERO_BRANCH;
                            }
                            else 
                                {
                                    KList<Angle> optionAngles = new KList<Angle>();
                                    KList<IBranch> orderedByAngle = new KList<IBranch>();
                                    foreach (IBranch exit in immediateOptions)
                                    {
                                        Angle optionAngle = OptionSortAngle(at, entry, exit);
                                        int insertIndex = -1;
                                        int na = optionAngles.Count;
                                        for (int ai = 0; ai < na; ai++)
                                        {
                                            if (optionAngle.Degrees() < optionAngles[ai].Degrees()) 
                                            {
                                                insertIndex = ai;
                                                break;
                                            }
                                        }
                                        if (insertIndex < 0) 
                                        {
                                            optionAngles.Add(optionAngle);
                                            orderedByAngle.Add(exit);
                                        }
                                        else 
                                        {
                                            optionAngles.Add(insertIndex, optionAngle);
                                            orderedByAngle.Add(insertIndex, exit);
                                        }
                                    }
                                    sortedOptions = ((IBranch[])orderedByAngle.ToArray(ZERO_BRANCH));
                                    int nso = sortedOptions.Length;
                                    if (nso >= 4) mask = OPTMASK_L + OPTMASK_F + OPTMASK_R;
                                    else if (nso == 3) mask = OPTMASK_L + OPTMASK_F + OPTMASK_R;
                                    else mask = SetMaskByTwoAngles(optionAngles);
                                }
        if (at == AxilType.RoadJunction) optionButtonsEnabledMaskV = mask;
        else optionButtonsEnabledMaskP = mask;
        return sortedOptions;
    }

    private int SetMaskByTwoAngles(KList<Angle> angles)
    {
        double angleA = angles[0].Degrees();
        double angleB = angles[1].Degrees();
        if (angleA > -30) return OPTMASK_F + OPTMASK_R;
        if (angleB > 30) return OPTMASK_L + OPTMASK_R;
        return OPTMASK_L + OPTMASK_F;
    }

    private AxilType FindAxilType(bool forTaxi, IBranch[] routeOptions)
    {
        if (forTaxi) return AxilType.RoadJunction;
        KList<IBranch> manRoute = manRouteP;
        int nm = manRoute.Count;
        IBranch lastChosen = nm > 0 ? manRoute[nm - 1] : CachedPlayerBranch(forTaxi);
        if (lastChosen is IBranchTaxiOrigin) return AxilType.TaxiDest;
        else if (lastChosen is IBranchTaxiDestination) return AxilType.TaxiOut;
        else if (lastChosen is IBranchBusBoarding) return AxilType.BusRoute;
        else if (lastChosen is IBranchBusOnboard) return AxilType.BusDest;
        else if (lastChosen is IBranchBusArrival) return AxilType.BusOut;
        foreach (IBranch exit in routeOptions)
        {
            if (exit is IBranchTaxiOrigin) return AxilType.TaxiIn;
            if (exit is IBranchBusBoarding) return AxilType.BusTake;
        }
        return AxilType.WalkJunction;
    }

    private AxilType FindAxilType_Test(bool forTaxi, IBranch[] routeOptions)
    {
        if (forTaxi) return AxilType.RoadJunction;
        KList<IBranch> manRoute = manRouteP;
        int nm = manRoute.Count;
        IBranch lastChosen = nm > 0 ? manRoute[nm - 1] : CachedPlayerBranch(forTaxi);
        if (lastChosen is IBranchTaxiOrigin) return AxilType.TaxiDest;
        else if (lastChosen is IBranchTaxiDestination) return AxilType.TaxiOut;
        else if (lastChosen is IBranchBusBoarding) return AxilType.BusRoute;
        else if (lastChosen is IBranchBusOnboard) return AxilType.BusDest;
        else if (lastChosen is IBranchBusArrival) return AxilType.BusOut;
        IBranch autoFirst = AutoFirst(forTaxi);
        bool atEndOfWalkway = lastChosen != autoFirst && lastChosen is IWalkBranch && autoFirst is IWalkBranch;
        foreach (IBranch exit in routeOptions)
        {
            if (exit is IBranchTaxiOrigin && !atEndOfWalkway) return AxilType.TaxiIn;
            if (exit is IBranchBusBoarding && !atEndOfWalkway) return AxilType.BusTake;
        }
        return AxilType.WalkJunction;
    }

    private IBranch[] FindAvailableOptions(bool forTaxi, IBranch[] routeOptions)
    {
        KList<IBranch> autoRoute = forTaxi ? autoRouteV : autoRouteP;
        KList<IBranch> manRoute = forTaxi ? manRouteV : manRouteP;
        AxilType at = OptionButtonsAxilType(forTaxi);
        int nm = manRoute.Count;
        IBranch lastChosen = nm > 0 ? manRoute[nm - 1] : CachedPlayerBranch(forTaxi);
        IBranch[] immediateOptions = ImmediateOptions(forTaxi, at, lastChosen, routeOptions);
        if (immediateOptions.Length < 2) 
        {
            OptionButtonEnableUpOnly(forTaxi);
            return immediateOptions;
        }
        return SortOptions(at, lastChosen, immediateOptions);
    }

    private void SetRouteOptions(bool forTaxi, IBranch[] routeOptions)
    {
        if (forTaxi) allOptionsV = routeOptions;
        else allOptionsP = routeOptions;
        if (!forTaxi) optionButtonsAxilTypeP = FindAxilType(forTaxi, routeOptions);
        IBranch[] availableOptions = routeOptions.Length < 2 ? routeOptions : FindAvailableOptions(forTaxi, routeOptions);
        if (forTaxi) availableOptionsV = availableOptions;
        else availableOptionsP = availableOptions;
        if (availableOptions.Length < 2) 
        {
            OptionButtonEnableUpOnly(forTaxi);
            if (availableOptions.Length == 1) 
            {
                IBranch av0 = availableOptions[0];
                ManRouteAdd(Player.ActivePlayer(), forTaxi, av0, false);
            }
        }
        FireRouteChangedLater();
    }

    private bool IgnoreExit(IBranch optionalPrev, IBranch branch, IBranch exit)
    {
        if (optionalPrev is IBranchBusArrival && exit is IBranchBusBoarding) 
        {
            IStop arrivalStop = ((IBranchBusArrival)optionalPrev).GetStop();
            IStop boardingStop = ((IBranchBusBoarding)exit).GetStop();
            if (arrivalStop == boardingStop) return true;
            if (arrivalStop.IsAtKerb()) 
            {
                IStop[] fpsa = arrivalStop.GetFootpath().GetStops();
                for (int si = 0; si < fpsa.Length; si++)
                {
                    if (fpsa[si] == arrivalStop) break;
                    else if (fpsa[si] == boardingStop) return true;
                }
            }
        }
        if (optionalPrev is IBranchTaxiDestinationLane && exit is IBranchTaxiOriginLane) 
        {
            if (optionalPrev.GetZone() == exit.GetZone()) return true;
        }
        if (optionalPrev == branch && branch is IWalkBranch) 
        {
            if (!(exit is IWalkBranch)) return true;
        }
        return false;
    }

    private IBranch SingleExit(bool forTaxi, IBranch prev, IBranch branch)
    {
        IBranch[] possibleExits = FiniteCostExitsUnsorted(forTaxi, prev, branch);
        int nx = possibleExits.Length;
        if (nx == 1) return possibleExits[0];
        return null;
    }

    private bool ChoiceFromHere(bool forTaxi, IBranch prev, IBranch branch)
    {
        bool playerOnBranch = branch == CachedPlayerBranch(forTaxi);
        return ChoiceFromHere(forTaxi, prev, branch, playerOnBranch);
    }

    private bool ChoiceFromHere(bool forTaxi, IBranch prev, IBranch branch, bool playerOnBranch)
    {
        IBranch[] possibleExitsFromStart = FiniteCostExitsUnsorted(forTaxi, prev, branch);
        if (possibleExitsFromStart.Length < 2) return false;
        if (playerOnBranch && branch.MidOptions() > 0) 
        {
            IWalkBranch wb = (IWalkBranch)branch;
            Player player = Player.ActivePlayer();
            IBranch[] remainingExits = PrunePassedParkingOrStops(wb, player, possibleExitsFromStart);
            if (remainingExits.Length < 2) return false;
        }
        return true;
    }

    private bool NoChoiceFromHere(bool forTaxi, IBranch prev, IBranch branch, bool playerOnBranch)
    {
        return !ChoiceFromHere(forTaxi, prev, branch, playerOnBranch);
    }

    private void ManRouteAddRecurseSingleExit(bool forTaxi, KHashSet<IBranch> singleExitBranchesAdded, IBranch prev, IBranch branch, bool userAction)
    {
        KList<IBranch> manRoute = forTaxi ? manRouteV : manRouteP;
        manRoute.Add(branch);
        if (userAction) manRouteByUserRequest.Add(branch);
        IBranch singleExitBranch = SingleExit(forTaxi, prev, branch);
        if (singleExitBranch != null) 
        {
            if (singleExitBranchesAdded == null) singleExitBranchesAdded = new KHashSet<IBranch>();
            else if (singleExitBranchesAdded.Contains(singleExitBranch)) return;
            singleExitBranchesAdded.Add(singleExitBranch);
            ManRouteAddRecurseSingleExit(forTaxi, singleExitBranchesAdded, branch, singleExitBranch, userAction);
        }
    }

    public virtual IBranch TaxiDestinationInManRoute(IBranch bvo)
    {
        int nmb = ManRouteN(false);
        if (nmb == 0) return null;
        IBranch cb = CachedPlayerBranch(false);
        if (cb == bvo) 
        {
            IBranch first = ManRouteBranch(false, 0);
            if (first is IBranchTaxiDestination) return first;
            return null;
        }
        for (int mi = 0; mi < nmb - 1; mi++)
        {
            IBranch mbo = ManRouteBranch(false, mi);
            IBranch mbd = ManRouteBranch(false, mi + 1);
            if (mbo == bvo && mbd is IBranchTaxiDestination) return mbd;
        }
        return null;
    }

    public virtual int BusrouteHaltIndex(IBranch bbb)
    {
        int nmb = ManRouteN(false);
        if (nmb == 0) return 0;
        IBranch cb = CachedPlayerBranch(false);
        if (cb == bbb) 
        {
            IBranch first = ManRouteBranch(false, 0);
            return bbb.ExitIndex(first);
        }
        for (int mi = 0; mi < nmb - 1; mi++)
        {
            IBranch mbb = ManRouteBranch(false, mi);
            IBranch mbo = ManRouteBranch(false, mi + 1);
            if (mbb == bbb) return bbb.ExitIndex(mbo);
        }
        return 0;
    }

    public virtual int BusrouteOffStopIndex(IBranch bbo)
    {
        int nmb = ManRouteN(false);
        if (nmb == 0) return 0;
        IBranch cb = CachedPlayerBranch(false);
        if (cb == bbo) 
        {
            IBranch first = ManRouteBranch(false, 0);
            return bbo.ExitIndex(first);
        }
        for (int mi = 0; mi < nmb - 1; mi++)
        {
            IBranch mbo = ManRouteBranch(false, mi);
            IBranch mba = ManRouteBranch(false, mi + 1);
            if (mbo == bbo) return bbo.ExitIndex(mba);
        }
        return 0;
    }

    public virtual IBranch ChosenBranchFrom(IBot bot, IBranch routeBranch)
    {
        Player player = Player.ActivePlayer();
        if (player == null) return null;
        bool forTaxiRoute = routeBranch is IRoad;
        if (forTaxiRoute && bot != player.GetTaxi()) return null;
        if (!forTaxiRoute && bot != player.Ped()) return null;
        ITaxi taxi = forTaxiRoute ? player.GetTaxi() : null;
        if (forTaxiRoute && taxi == null) return null;
        IBranch recentBranch = forTaxiRoute ? taxi.GetRecentBranch() : player.GetRecentBranch();
        if (recentBranch == null) return null;
        if (forTaxiRoute && taxi.GetDestination() != cachedTaxiDestination) AutoRouteBuild(true, null, null);
        KList<IBranch> autoRoute = forTaxiRoute ? autoRouteV : autoRouteP;
        KList<IBranch> manRoute = forTaxiRoute ? manRouteV : manRouteP;
        int nmx = manRoute.Count;
        int nax = autoRoute.Count;
        if (routeBranch == recentBranch) 
        {
            if (routeBranch is IBranchTaxiOrigin && forTaxiRoute && ShowTaxiRoute()) return game.Sim().TaxiDestination(taxi, routeBranch);
            IBranch manFirst = manRoute.Count > 0 ? manRoute[0] : null;
            IBranch autoFirst = autoRoute.Count > 0 ? autoRoute[0] : null;
            IBranch nextBranch = manFirst != null ? manFirst : autoFirst;
            if (nextBranch != null && recentBranch.IsConnectedTo(nextBranch)) return nextBranch;
            if (nextBranch != null && nextBranch == recentBranch) 
            {
                if (nextBranch == manFirst) nextBranch = manRoute.Count > 1 ? manRoute[1] : autoFirst;
                if (nextBranch == recentBranch && nextBranch == autoFirst && autoRoute.Count > 1) nextBranch = autoRoute[1];
                if (recentBranch.IsConnectedTo(nextBranch)) return nextBranch;
            }
            if (recentBranch.GetZone() == player.GetDestination()) return null;
            if (taxi != null && recentBranch.GetZone() == taxi.GetDestination()) return null;
            KBool rebuildAuto = NewPlayerBranch(forTaxiRoute, recentBranch);
            if (ShowTaxiRoute() && player.GetTaxi() == null) ShowTaxiRoute(false);
            if (rebuildAuto != null) 
            {
                RouteOptionsFromLastSelected(player.Ped(), forTaxiRoute);
                if (rebuildAuto.BoolValue()) AutoRouteBuild(forTaxiRoute, null, null);
            }
            nextBranch = manRoute.Count > 0 ? manRoute[0] : autoRoute.Count > 0 ? autoRoute[0] : null;
            if (nextBranch != null && recentBranch.IsConnectedTo(nextBranch)) return nextBranch;
            return null;
        }
        if (recentBranch is IBranchBusBoarding && routeBranch is IWalkBranch) 
        {
            IStop stop = ((IBranchBusBoarding)recentBranch).GetStop();
            if (stop.IsAtKerb() && stop.GetFootpath() == ((IWalkBranch)routeBranch).GetWalkway()) return recentBranch;
        }
        if (recentBranch is IBranchTaxiOriginLane && routeBranch is IWalkBranch) 
        {
            if (((IBranchTaxiOriginLane)recentBranch).GetFootpath() == ((IWalkBranch)routeBranch).GetWalkway()) return recentBranch;
        }
        IBranch prev = null;
        if (manRoute.Count > 0) 
        {
            prev = manRoute[0];
            for (int mi = 1; mi < manRoute.Count; mi++)
            {
                IBranch manI = manRoute[mi];
                if (routeBranch == prev) return manI;
                prev = manI;
            }
        }
        for (int ai = 0; ai < autoRoute.Count; ai++)
        {
            IBranch autoI = autoRoute[ai];
            if (routeBranch == prev) return autoI;
            prev = autoI;
        }
        return null;
    }

    public virtual IBranch[] AltRouteBranches(bool forTaxi)
    {
        lock (this)
        {
            return ManRouteBranches(forTaxi);
        }
    }

    public virtual int ManRouteN(bool forTaxi)
    {
        return forTaxi ? manRouteV.Count : manRouteP.Count;
    }

    public virtual IBranch ManRouteLast(bool forTaxi)
    {
        int n = ManRouteN(forTaxi);
        return n == 0 ? null : forTaxi ? manRouteV[n - 1] : manRouteP[n - 1];
    }

    public virtual IBranch[] ManRouteBranches(bool forTaxi)
    {
        KList<IBranch> manRoute = forTaxi ? manRouteV : manRouteP;
        return ((IBranch[])manRoute.ToArray(new IBranch[manRoute.Count]));
    }

    public virtual IBranch ManRouteBranch(bool forTaxi, int mi)
    {
        KList<IBranch> manRoute = forTaxi ? manRouteV : manRouteP;
        return manRoute[mi];
    }

    public virtual IBranch[] AutoRouteBranches(bool forTaxi)
    {
        KList<IBranch> autoRoute = forTaxi ? autoRouteV : autoRouteP;
        return ((IBranch[])autoRoute.ToArray(new IBranch[autoRoute.Count]));
    }

    public virtual IRoad[] ManAndAutoRouteTaxiRoads()
    {
        int nrm = manRouteV.Count;
        int nra = autoRouteV.Count;
        int nr = nrm + nra;
        IRoad[] ra = new IRoad[nr];
        int ri = 0;
        while (ri < nrm)
        {
            ra[ri] = (IRoad)manRouteV[ri++];
        }
        while (ri < nr)
        {
            ra[ri] = (IRoad)autoRouteV[-nrm + ri++];
        }
        return ra;
    }

    public virtual void AppStateChanged(AppStateTransition transition)
    {
        if (transition.after == GameState.Navigating) 
        {
            activeStopsForPlayer.Clear();
            Player player = Player.ActivePlayer();
            if (player == null) return;
            IRouter router = game.Sim().RouterPed();
            foreach (IStop stop in game.Map().Stops())
            {
                if (router.RouteExists(stop, player.GetDestination(), player.GetMind())) activeStopsForPlayer.Add(stop);
            }
            activeStopsPerWalk.Clear();
            foreach (IFootpath fp in game.Map().Footpaths())
            {
                if (fp.IsPedway() || fp.OnMedianSide()) continue;
                KList<IStop> aksList = new KList<IStop>();
                IStop[] ksa = fp.GetStops();
                foreach (IStop ks in ksa)
                {
                    if (activeStopsForPlayer.Contains(ks)) aksList.Add(ks);
                }
                IStop[] aksa = ((IStop[])aksList.ToArray(IZero.ZERO_ISTOP));
                activeStopsPerWalk.Put(fp.BranchCD(), aksa);
                activeStopsPerWalk.Put(fp.BranchDC(), aksa);
            }
        }
        if (transition.after == GameState.GameOver) ClearRoute();
    }

    public virtual int ActiveStopCount(IBranch wb)
    {
        return activeStopsPerWalk[wb].Length;
    }

    public virtual IStop[] ActiveStops(IBranch wb)
    {
        return activeStopsPerWalk[wb];
    }

    public virtual bool RouteExistsForPlayer(IStop stop)
    {
        return activeStopsForPlayer.Contains(stop);
    }

    public bool ShowTaxiRoute()
    {
        return showTaxiRoute;
    }

    public void ShowTaxiRoute(bool b)
    {
        if (showTaxiRoute == b) return;
        showTaxiRoute = b;
        game.Gsc().RequestToggleControlPedTaxi(false);
    }

    public virtual int BusTakeStopIndex()
    {
        return busTakeStopIndex;
    }

    public virtual void BusTakeStopIndexReset()
    {
        busTakeStopIndex = -1;
    }

    public virtual int ManRouteIncludesFootpath(IFootpath fp)
    {
        foreach (IBranch man in manRouteP)
        {
            if (man is IWalkBranch && ((IWalkBranch)man).GetWalkway() == fp) return ((IWalkBranch)man).IsForward() ? 1 : -1;
        }
        return 0;
    }

    public virtual bool ManRouteExtendedByUserRequest()
    {
        return manRouteByUserRequest.Count > 0;
    }

    public virtual Xyzbg RouteChoicePosition()
    {
        Xyz pos;
        Angle bearing;
        bool forTaxi = ShowTaxiRoute();
        IBranch choiceBranch = PreOptionBranch(forTaxi);
        Player player = Player.ActivePlayer();
        if (choiceBranch == null || player == null || player.GetDestination() == null) 
        {
            if (player == null) 
            {
                pos = Xyz.ALLZERO;
                bearing = Angle.A0;
            }
            else 
            {
                pos = player.Ped().Centre();
                bearing = player.Ped().Forward().AsBearing();
            }
        }
        else if (choiceBranch.GetZone() == player.GetDestination()) 
            {
                pos = choiceBranch.GetZone().Location();
                bearing = choiceBranch.FinishBearing();
            }
            else if (choiceBranch.MidOptions() > 0 && choiceBranch is IWalkBranch) 
            {
                IWalkBranch wb = (IWalkBranch)choiceBranch;
                IBranch autoFirst = AutoFirst(forTaxi);
                IStop[] aksa = ActiveStops(wb);
                if (autoFirst != choiceBranch && autoFirst != null) 
                {
                    pos = autoFirst.StartLocation();
                    bearing = autoFirst.StartBearing();
                }
                else if (aksa != null && aksa.Length > 0) 
                {
                    int stpi = 0;
                    int btsi = BusTakeStopIndex();
                    if (btsi >= 0 && btsi < aksa.Length) stpi = btsi;
                    IStop stop = aksa[stpi];
                    pos = stop.Position();
                    bearing = stop.Position().Bearing();
                }
                else 
                    {
                        ITaxiZone taxiZone = null;
                        if (wb.HasTaxiZone()) 
                        {
                            IFootpath fp = (IFootpath)wb.GetWalkway();
                            taxiZone = fp.GetRunningLane().GetTaxiZone();
                        }
                        if (taxiZone != null && !taxiZone.IsDropOffOnly() && taxiZone.Bays() > 0) 
                        {
                            int nb = taxiZone.Bays();
                            int bayi = 0;
                            int pibi = player.Ped().IntendedBay();
                            if (pibi >= 0 && pibi < nb) bayi = pibi;
                            Xyzbg bayCentre = taxiZone.BayCentre(bayi);
                            pos = bayCentre;
                            bearing = bayCentre.Bearing();
                        }
                        else 
                            {
                                pos = choiceBranch.FinishLocation();
                                bearing = choiceBranch.FinishBearing();
                            }
                    }
            }
            else 
                {
                    pos = choiceBranch.FinishLocation();
                    bearing = choiceBranch.FinishBearing();
                }
        return new Xyzbg(pos, bearing, 0);
    }

    public virtual string DescribeBranch(IBranch opt)
    {
        string desc = DescribeBranchFull(opt);
        string[] lines = KTools.SplitQuick(desc, CC.NEWLN);
        int nl = lines.Length;
        if (nl == 1) 
        {
            lines = SplitLongLine(desc);
            nl = lines.Length;
        }
        string truncated = Truncate(lines[0]);
        for (int li = 1; li < nl; li++)
        {
            truncated += SC.NL + Truncate(lines[li]);
        }
        return truncated;
    }

    public virtual string DescribeBranchFull(IBranch branch)
    {
        if (branch == null) return SC.MI;
        Player aplayer = Player.ActivePlayer();
        if (aplayer == null) return SC.MI;
        IPed player = aplayer.Ped();
        bool forTaxi = ShowTaxiRoute();
        IBranch choiceBranch = PreOptionBranch(forTaxi);
        bool isChoiceBranch = branch == choiceBranch;
        bool isPrevPlayerBranch = !isChoiceBranch && branch == CachedPlayerBranch(forTaxi);
        int nmb = ManRouteN(forTaxi);
        if (branch is IRoad) 
        {
            IRoad rd = (IRoad)branch;
            string relCompass = rd.BearingA().Minus(player.Forward().AsBearing()).CardinalDirectionSymbol8();
            return KFormat.Sprintf(SC.ROUTE_DRIVE, relCompass, rd.Description());
        }
        if (branch is IWalkBranch) 
        {
            IWalkBranch wb = (IWalkBranch)branch;
            IWalkway w = wb.GetWalkway();
            if (w is ICrossing) return KFormat.Sprintf(SC.ROUTE_CROSS, ((ICrossing)w).GetRoad().Description());
            IFootpath fp = (IFootpath)w;
            string desc = fp.Description();
            if (fp.IsSidewalk() && !fp.OnMedianSide()) 
            {
                ILane lane = fp.GetRunningLane();
                if (lane != null) desc = lane.GetRoad().Description();
            }
            Angle entryAngle = wb.IsForward() ? fp.Entry().Bearing() : fp.Exit().Bearing().Plus(Angle.A180);
            string relCompass = entryAngle.Minus(player.Forward().AsBearing()).CardinalDirectionSymbol8();
            return KFormat.Sprintf(SC.ROUTE_WALK, relCompass, desc);
        }
        if (branch is IBranchBusBoarding) 
        {
            IStop stop = ((IBranchBusBoarding)branch).GetStop();
            string symbols = stop.IsOnEdge() ? SC.ON_BUS_EDGE : SC.ON_BUS;
            string str = symbols + SC.S + stop.Description();
            if (isPrevPlayerBranch) 
            {
                IBranch manFirst = nmb > 0 ? ManRouteBranch(forTaxi, 0) : null;
                if (nmb >= 2 && manFirst is IBranchBusOnboard) 
                {
                    IHalt halt = ((IBranchBusOnboard)manFirst).GetHalt();
                    str += SC.NL + halt.GetBusroute().Description();
                    str += SC.NL + halt.NextBusArrivalDescription();
                }
            }
            else if (isChoiceBranch) {}
else str += stop.DescriptionOfArrivingBuses();
            return str;
        }
        if (branch is IBranchBusOnboard) 
        {
            IBranchBusOnboard bbo = (IBranchBusOnboard)branch;
            IStop stop = bbo.GetStop();
            IHalt halt = bbo.GetHalt();
            string routeStr = halt.GetBusroute().Description();
            string arrStr = halt.NextBusArrivalDescription();
            string str = routeStr;
            if (isPrevPlayerBranch) 
            {
                if (!player.IsAboard()) str += SC.NL + arrStr;
            }
            else if (isChoiceBranch) 
                {
                    if (!player.IsAboard()) str += SC.NL + arrStr;
                }
                else str += SC.NL + arrStr;
            return str;
        }
        if (branch is IBranchBusArrival) 
        {
            IStop stop = ((IBranchBusArrival)branch).GetStop();
            IStop offStop = stop;
            string symbols = offStop.IsOnEdge() ? SC.OFF_BUS_EDGE : SC.OFF_BUS;
            return symbols + SC.S + offStop.Description();
        }
        if (branch is IBranchTaxiOrigin) 
        {
            if (branch is IBranchTaxiOriginEdge) return branch.GetZone().Description();
            string arrStr;
            if (player.IsAboard()) 
            {
                ITaxi taxi = player.GetTaxi();
                arrStr = taxi == null ? SC.N : taxi.ToString();
            }
            else 
            {
                int bay = player.IntendedBay() >= 0 ? player.IntendedBay() : -1;
                ITaxiZone taxiZone = (ITaxiZone)branch.GetZone();
                arrStr = taxiZone.DescriptionNextArriving(bay);
            }
            return SC.ON_TAXI + SC.S + arrStr;
        }
        if (branch is IBranchTaxiDestination) return SC.OFF_TAXI + SC.S + branch.Description();
        return branch.Description();
    }
    private static int STR_MAX = 18;

    private static string Truncate(string s)
    {
        return s.Length < STR_MAX ? s : uk.vroad.apk.KTools.Substring(s, 0, STR_MAX) + SC.ELLIPSIS;
    }

    private static int SplitPos(string s)
    {
        char[] ca = s.ToCharArray();
        int n = STR_MAX;
        if (ca.Length < n) return -1;
        for (int ci = n - 1; ci >= 1; ci--)
        {
            if (ca[ci] == CC.SLASH) return ci;
        }
        int h = ca.Length / 2;
        for (int di = 0; di < h - 1; di++)
        {
            if (ca[h + di] == CC.SPACE) return h + di;
            if (ca[h - di] == CC.SPACE) return h - di;
        }
        return -1;
    }

    public static string[] SplitLongLine(string desc)
    {
        if (desc.Length > STR_MAX) 
        {
            int spci = SplitPos(desc);
            if (spci > 0) 
            {
                string[] lines = new string[2];
                lines[0] = uk.vroad.apk.KTools.Substring(desc, 0, spci);
                lines[1] = uk.vroad.apk.KTools.Substring(desc, spci + 1);
                return lines;
            }
        }
        return new string[] { desc };
    }
}
}
