using System;
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
    public class PlayerTripChooser : LSimTimeStep, LAppInput
    {
        private static readonly TripCandidate[] CANDIDATES_NONE = new TripCandidate[0];
        private LTripCandidate listener;
        private bool newDealRequired = true;
        private bool takeLitCandidate = false;
        private int countdownInitial;
        private double countdownSeconds;
        private IPedZone previousDestination;
        private TripCandidate[] candidates = CANDIDATES_NONE;
        private TripCandidate candidateLit = null;
        private int quartileOffset = 0;
        private int newDealCharge = PlayerEnergy.CHARGE_NEW_DEAL_INITIAL;
        private PlayerTripChooser.TripOriginCandidate cachedTripOriginCandidate;
        private readonly Game game;

        public static uk.vroad.rvr.PlayerTripChooser Awake(Game game)
        {
            lock (typeof(PlayerTripChooser))
            {
                return new uk.vroad.rvr.PlayerTripChooser(game);
            }
        }

        private PlayerTripChooser(Game gm)
        {
            game = gm;
            gm.AddEventConsumer(this);
        }

        public virtual void Init()
        {
            previousDestination = null;
            countdownSeconds = 0;
            ClearCandidates();
            NewDealRequired(true);
        }
        private IPedTrip playerTrip;

        public virtual IPedTrip PlayerTrip()
        {
            return playerTrip;
        }

        public virtual void PlayerTrip(IPedTrip trip)
        {
            playerTrip = trip;
        }

        public virtual void PlayerTripClear()
        {
            playerTrip = null;
        }

        public virtual bool IsPlayerTrip(ITrip trip)
        {
            return trip == PlayerTrip();
        }

        public virtual void NewDealRequired(bool b)
        {
            newDealRequired = b;
        }

        private bool NewDealRequired()
        {
            return newDealRequired;
        }

        public virtual bool DeregisterFireMapChange()
        {
            quartileOffset = 0;
            cachedTripOriginCandidate = null;
            listener = null;
            return false;
        }

        private TripCandidate[] TakeFirst(TripCandidate[] tca, int nFirst)
        {
            if (nFirst < 1) return tca;
            if (tca.Length <= nFirst) return tca;
            TripCandidate[] ftca = new TripCandidate[nFirst];
            for (int i = 0; i < ftca.Length; i++)
            {
                ftca[i] = tca[i];
            }
            return ftca;
        }

        private void ShuffleInPlace(TripCandidate[] tca)
        {
            int n = tca.Length;
            int swaps = n * 4;
            Rng.Vein vein = Rng.Vein.PLAYER;
            for (int si = 0; si < swaps; si++)
            {
                int ai = Rng.NextInt(vein, n);
                int bi = Rng.NextInt(vein, n);
                if (ai == bi) continue;
                TripCandidate tmp = tca[ai];
                tca[ai] = tca[bi];
                tca[bi] = tmp;
            }
        }

        private TripCandidate SwapForAnother(TripCandidate[] tca, int tci)
        {
            return tci == 0 ? tca[1] : tca[tci - 1];
        }
        private static readonly int[] AVAILABLE_VIALS = new int[] { 4, 8, 13, 18, 23, 28, 33, 38 };

        private TripCandidate[] TakeCandidateInQuartiles123(TripCandidate[] tca, int qo)
        {
            int n = tca.Length;
            if (n < 4) return tca;
            int cpq = n / 4;
            int hi = qo % cpq;
            int mi = cpq + (((cpq / 2) + qo) % cpq);
            int li = (3 * cpq) - hi - 1;
            TripCandidate high = tca[hi];
            TripCandidate medium = tca[mi];
            TripCandidate low = tca[li];
            PlayerEnergy pe = game.Pe();
            int nv = pe.VialsCount();
            int atl = pe.AttainedLevel();
            if (atl < 1) atl = 1;
            if (atl < LevelManager.N_LEVELS && nv >= AVAILABLE_VIALS[atl - 1] - 1) 
            {
                IPedZone vialZone = game.Vg().VialZone();
                if (high.Destination() == vialZone) high = SwapForAnother(tca, hi);
                if (medium.Destination() == vialZone) medium = SwapForAnother(tca, mi);
                if (low.Destination() == vialZone) low = SwapForAnother(tca, li);
            }
            if (UDbg.SHOW_TRIP_HML) 
            {
                IPedZone vialZone = game.Vg().VialZone();
                int vi = -1;
                for (int tci = 0; tci < tca.Length; tci++)
                {
                    if (tca[tci].Destination() == vialZone) 
                    {
                        vi = tci;
                        break;
                    }
                }
                SE.Report(SE.TRIP_HML, n, cpq, vi, hi, mi, li, high.Destination(), medium.Destination(), low.Destination());
            }
            return new TripCandidate[] { high, medium, low };
        }
        public class TripOriginCandidate
        {
            public readonly IPedZone origin;
            private readonly TripCandidate[] tca;

            public TripOriginCandidate(PlayerTripChooser _enclosing, IPedZone pzo, KSortList<TripCandidate> sortedList)
            {
                this._enclosing = _enclosing;
                this.origin = pzo;
                this.tca = sortedList.ToArray(TripCandidate.ZERO);
            }

            public virtual TripCandidate[] Candidates()
            {
                TripCandidate[] tcaCopy = new TripCandidate[this.tca.Length];
                for (int i = 0; i < this.tca.Length; i++)
                {
                    tcaCopy[i] = this.tca[i];
                }
                return tcaCopy;
            }

            public override string ToString()
            {
                return this.origin.ToString() + SC.S + CC.SQBL + SC.S + this.tca.Length + SC.S + CC.SQBR;
            }
            private readonly PlayerTripChooser _enclosing;
        }
        private const int MIN_DESTINATIONS = 3;

        private PlayerTripChooser.TripOriginCandidate SuitableAsOrigin(IPedZone originCandidate, int minimumDests)
        {
            if (originCandidate == null) return null;
            if (originCandidate.DepartureBranches().Length == 0) return null;
            if (originCandidate.GetDirectDrvZone() != null && originCandidate.GetDirectDrvZone().DepartureBranches().Length == 0 && originCandidate.DepartureBranches
                ().Length == 1) return null;
            IRouter routerP = game.Sim().RouterPed();
            KSortList<TripCandidate> sortedList = new KSortList<TripCandidate>();
            int mi = game.Sim().AddedPedTripMind().RouterIndex();
            foreach (IPedZone dest in game.Map().PedZones())
            {
                if (dest == originCandidate) continue;
                if (dest.ArrivalBranches().Length == 0) continue;
                if (dest.DepartureBranches().Length == 0) continue;
                if (originCandidate.IsOnEdge() && dest.IsOnEdge()) continue;
                double rewardKJ = PlayerReward(routerP, originCandidate, dest.RouterIndex(), mi);
                if (rewardKJ > Price.eINF_KJ) continue;
                TripCandidate tc = new TripCandidate(game, originCandidate, dest, rewardKJ);
                sortedList.Add(tc);
            }
            if (sortedList.Count < minimumDests) 
            {
                sortedList.Clear();
                return null;
            }
            return new PlayerTripChooser.TripOriginCandidate(this, originCandidate, sortedList);
        }

        private double PlayerReward(IRouter routerP, IPedZone origin, int dzi, int mi)
        {
            IBranch[] dba = origin.DepartureBranches();
            if (dba == null || dba.Length == 0) return Price.INF_KJ;
            if (dba.Length == 1) return routerP.RouteCost(dba[0], dzi, mi).KJ();
            bool canDepartByTaxi = CanDepartByTaxi(dba);
            double lowest = Price.INF_KJ;
            foreach (IBranch branch in dba)
            {
                double cost = routerP.RouteCost(branch, dzi, mi).KJ();
                if (canDepartByTaxi && branch is IWalkBranch) cost += IncreasedCostToWalkAtEdge();
                if (cost < lowest) lowest = cost;
            }
            return lowest;
        }

        public virtual double CostVariationOnRelease(ITripOD trip, IBranch first)
        {
            if (game.IsPlayerTrip(trip) && first is IWalkBranch && CanDepartByTaxi(trip.GetOrigin().DepartureBranches())) return IncreasedCostToWalkAtEdge();
            return 0;
        }

        public virtual bool CanDepartByTaxi(IBranch[] dba)
        {
            foreach (IBranch db in dba)
            {
                if (db is IBranchDirectDepartureByTaxi) return true;
            }
            return false;
        }

        public virtual double IncreasedCostToWalkAtEdge()
        {
            return +500;
        }

        private PlayerTripChooser.TripOriginCandidate RandomSuitableOrigin()
        {
            KList<PlayerTripChooser.TripOriginCandidate> tripOriginCandidates = new KList<PlayerTripChooser.TripOriginCandidate>();
            foreach (IPedZone originCandidate in game.Map().PedZones())
            {
                PlayerTripChooser.TripOriginCandidate toc = SuitableAsOrigin(originCandidate, MIN_DESTINATIONS);
                if (toc != null) tripOriginCandidates.Add(toc);
            }
            int noc = tripOriginCandidates.Count;
            if (noc == 0) return null;
            int ci = Rng.NextInt(Rng.Vein.PLAYER, noc);
            PlayerTripChooser.TripOriginCandidate randomOrigin = tripOriginCandidates[ci];
            return randomOrigin;
        }

        private PlayerTripChooser.TripOriginCandidate NearestSuitableOrigin(IPedZone firstChoiceOrigin)
        {
            if (cachedTripOriginCandidate != null && cachedTripOriginCandidate.origin == firstChoiceOrigin) return cachedTripOriginCandidate;
            PlayerTripChooser.TripOriginCandidate best = SuitableAsOrigin(firstChoiceOrigin, MIN_DESTINATIONS);
            if (best == null) 
            {
                Xyz firstChoiceLocation = firstChoiceOrigin.Location();
                double nearestD = double.MaxValue;
                foreach (IPedZone originCandidate in game.Map().PedZones())
                {
                    if (originCandidate == firstChoiceOrigin) continue;
                    PlayerTripChooser.TripOriginCandidate toc = SuitableAsOrigin(originCandidate, MIN_DESTINATIONS);
                    if (toc == null) continue;
                    double d = originCandidate.Location().DistanceXY(firstChoiceLocation);
                    if (d < nearestD) 
                    {
                        nearestD = d;
                        best = toc;
                    }
                }
            }
            return best;
        }

        public virtual void TimeStep()
        {
            if (game.Gsm().CurrentState() != GameState.WaitingForTripChoice) return;
            double dt = game.Sim().TimeStep();
            if (NewDealRequired()) 
            {
                IPedZone origin = PreviousDestination();
                PlayerTripChooser.TripOriginCandidate toc = origin == null ? RandomSuitableOrigin() : NearestSuitableOrigin(origin);
                if (toc == null) 
                {
                    game.Gsc().EscapeToMapChooserFromWait();
                    return;
                }
                cachedTripOriginCandidate = toc;
                TripCandidate[] allTripcandidates = toc.Candidates();
                TripCandidate[] strategyCandidates = TakeCandidateInQuartiles123(allTripcandidates, quartileOffset++);
                if (strategyCandidates.Length == 0) 
                {
                    game.Gsc().EscapeToMapChooserFromWait();
                    return;
                }
                candidates = strategyCandidates;
                candidateLit = candidates[0];
                countdownInitial = CountdownInitial(game.Pe().CurrentLevel());
                countdownSeconds = countdownInitial;
                takeLitCandidate = false;
                NewDealRequired(false);
                FireNewCandidatesAvailable();
            }
            else if (countdownInitial > 0 && countdownSeconds > 0) 
                {
                    countdownSeconds -= dt;
                    if (countdownSeconds < 0.001) 
                    {
                        countdownSeconds = 0;
                        takeLitCandidate = true;
                    }
                }
            if (takeLitCandidate) 
            {
                takeLitCandidate = false;
                IPedTrip trip = game.Sim().AddPedTrip(candidateLit.Origin(), candidateLit.Destination());
                PlayerTrip(trip);
                game.Pe().RewardOnArrival(candidateLit.Reward());
                game.Gsm().MakeTransition(GameStateTransition.chooseTrip);
                FireCandidateChosen();
                ClearCandidates();
            }
        }
        private static readonly int[] COUNTDOWN = new int[] { 0, 240, 180, 120 };

        private static int CountdownInitial(int level)
        {
            if (level < 1) return COUNTDOWN[0];
            if (level > COUNTDOWN.Length) return COUNTDOWN[COUNTDOWN.Length - 1];
            return COUNTDOWN[level - 1];
        }

        public virtual TripCandidate[] Candidates()
        {
            return candidates;
        }

        public virtual int NCandidates()
        {
            return candidates.Length;
        }

        private void ClearCandidates()
        {
            candidates = CANDIDATES_NONE;
            candidateLit = null;
        }

        public virtual void PlayerArrive(IPedZone dest)
        {
            ClearCandidates();
            previousDestination = dest;
            NewDealRequired(true);
        }

        public virtual TripCandidate CandidateLit()
        {
            return candidateLit;
        }

        private void CandidateLit(TripCandidate tc)
        {
            candidateLit = tc;
        }

        private bool FireCandidateChangeXY(int moveX, int moveY)
        {
            TripCandidate currentlyLit = CandidateLit();
            if (currentlyLit == null) return false;
            TripCandidate next = null;
            Xyz cLoc = currentlyLit.Destination().Location();
            double cx = cLoc.X();
            double cy = cLoc.Y();
            Xyz nLoc = null;
            foreach (TripCandidate tc in Candidates())
            {
                if (tc == currentlyLit) continue;
                if (next == null) 
                {
                    next = tc;
                    continue;
                }
                Xyz tcLoc = tc.Destination().Location();
                nLoc = next.Destination().Location();
                if (moveX > 0 && tcLoc.X() > cLoc.X() && (tcLoc.X() < nLoc.X() || nLoc.X() < cLoc.X())) next = tc;
                else if (moveX > 0 && tcLoc.X() < cLoc.X() && (tcLoc.X() < nLoc.X() && nLoc.X() < cLoc.X())) next = tc;
                else if (moveX < 0 && tcLoc.X() < cLoc.X() && (tcLoc.X() > nLoc.X() || nLoc.X() > cLoc.X())) next = tc;
                else if (moveX < 0 && tcLoc.X() > cLoc.X() && (tcLoc.X() > nLoc.X() && nLoc.X() > cLoc.X())) next = tc;
                else if (moveY > 0 && tcLoc.Y() > cLoc.Y() && (tcLoc.Y() < nLoc.Y() || nLoc.Y() < cLoc.Y())) next = tc;
                else if (moveY > 0 && tcLoc.Y() < cLoc.Y() && (tcLoc.Y() < nLoc.Y() && nLoc.Y() < cLoc.Y())) next = tc;
                else if (moveY < 0 && tcLoc.Y() < cLoc.Y() && (tcLoc.Y() > nLoc.Y() || nLoc.Y() > cLoc.Y())) next = tc;
                else if (moveY < 0 && tcLoc.Y() > cLoc.Y() && (tcLoc.Y() > nLoc.Y() && nLoc.Y() > cLoc.Y())) next = tc;
            }
            if (next == currentlyLit) return false;
            CandidateLit(next);
            if (listener != null) listener.CandidateLitChanged();
            return true;
        }

        public virtual bool FireNewDeal()
        {
            if (NewDealRequired()) return false;
            NewDealRequired(true);
            double charge = NewDealCharge();
            PlayerEnergy pe = game.Pe();
            if (charge > pe.EnergyRemainingKJ()) 
            {
                NewDealRequired(false);
                game.Gew().FireAlertEvent(AlertEvent.EnergyTooLow);
                return false;
            }
            pe.ApplyCharge(EnergyEvent.RefreshTrips, charge);
            NewDealChargeIncrease();
            countdownInitial = CountdownInitial(pe.CurrentLevel());
            countdownSeconds = countdownInitial;
            if (listener != null) listener.CandidateLitChanged();
            return true;
        }

        public virtual bool OnSelectDestination(int index)
        {
            TripCandidate[] tca = Candidates();
            int ntc = tca.Length;
            if (index < 0 || index >= ntc) return false;
            TripCandidate tc = tca[index];
            if (CandidateLit() == tc) 
            {
                takeLitCandidate = true;
                return true;
            }
            CandidateLit(tc);
            if (listener != null) listener.CandidateLitChanged();
            return true;
        }

        public virtual bool AppInputDigitalEvent(AppDigitalFn fn, bool isPressed)
        {
            if (!isPressed) return false;
            if (game.Gsm().CurrentState() != GameState.WaitingForTripChoice) return false;
            if (fn == GameDigitalFn.MenuChangeCity) return game.Gsc().EscapeToMapChooserFromWait();
            if (fn == GameDigitalFn.TripNewDeal) return FireNewDeal();
            if (fn == GameDigitalFn.TripSelect) 
            {
                takeLitCandidate = true;
                return true;
            }
            if (fn == GameDigitalFn.TripUp) return FireCandidateChangeXY(0, 1);
            if (fn == GameDigitalFn.TripDown) return FireCandidateChangeXY(0, -1);
            if (fn == GameDigitalFn.TripLeft) return FireCandidateChangeXY(-1, 0);
            if (fn == GameDigitalFn.TripRight) return FireCandidateChangeXY(1, 0);
            return false;
        }

        public virtual bool AppInputAnalogEvent(AppAnalogFn axis, double value)
        {
            return false;
        }

        public virtual double Countdown()
        {
            return countdownSeconds;
        }

        public virtual int CountdownInitial()
        {
            return countdownInitial;
        }

        public virtual void SetListener(LTripCandidate ltc)
        {
            listener = ltc;
        }

        private void FireNewCandidatesAvailable()
        {
            if (listener != null) listener.NewCandidatesAvailable();
        }

        private void FireCandidateChosen()
        {
            if (listener != null) listener.CandidateChosen();
        }

        public virtual IPedZone PreviousDestination()
        {
            if (previousDestination != null) return previousDestination;
            previousDestination = game.Pe().SavedDestination();
            return previousDestination;
        }

        public static int IncreaseCharge(int currentCharge)
        {
            int multipleMin = PlayerEnergy.CHARGE_NEW_DEAL_MULTIPLE;
            int multiple = currentCharge < 500 ? multipleMin : currentCharge < 1000 ? 2 * multipleMin : 4 * multipleMin;
            double increaseFactor = PlayerEnergy.CHARGE_NEW_DEAL_INCREASE_FACTOR;
            double newCharge = increaseFactor * (double)currentCharge;
            int newMinimum = currentCharge + multiple;
            int newChargeRoundedToMultiple = multiple * KTools.RoundToInt(newCharge / (double)multiple);
            return System.Math.Max(newMinimum, newChargeRoundedToMultiple);
        }

        private void NewDealChargeIncrease()
        {
            int currentCharge = NewDealCharge();
            NewDealCharge(IncreaseCharge(currentCharge));
        }

        public virtual void NewDealCharge(int ndc)
        {
            newDealCharge = ndc;
            if (listener != null) listener.NewDealPriceChanged(ndc);
        }

        public virtual int NewDealCharge()
        {
            return newDealCharge;
        }
    }
}
