using uk.vroad.api;
using uk.vroad.api.input;
using uk.vroad.api.map;
using uk.vroad.api.route;
using uk.vroad.api.sim;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public class Game : App
    {
        public static uk.vroad.rvr.Game AwakeInstance()
        {
            lock (typeof(Game))
            {
                if (instance == null) instance = new uk.vroad.rvr.Game();
                return instance;
            }
        }
        private static uk.vroad.rvr.Game instance;
        private readonly GameStateMachine gsm;
        private readonly LevelManager lm;
        private readonly PlayerEnergy pe;
        private readonly GameSimControl gsc;
        private readonly GameEventWrangler gew;
        private readonly GameInputHandler gih;
        private readonly GameFeatures gf;
        private readonly PlayerRouteBuilder prb;
        private readonly PlayerTripChooser ptc;
        private readonly BonusGenerator bg;
        private readonly IncidentGenerator ig;
        private readonly VialGenerator vg;

        private Game()
            : base()
        {
            gsm = GameStateMachine.Awake(this);
            gsc = GameSimControl.Awake(this);
            gew = GameEventWrangler.Awake(this);
            gih = GameInputHandler.Awake(this);
            gf = GameFeatures.Awake(this);
            prb = PlayerRouteBuilder.Awake(this);
            ptc = PlayerTripChooser.Awake(this);
            pe = PlayerEnergy.Awake(this);
            lm = LevelManager.Awake(this);
            bg = BonusGenerator.Awake(this);
            ig = IncidentGenerator.Awake(this);
            vg = VialGenerator.Awake(this);
            lm.Start();
        }

        public override AppStateMachine GetAppStateMachine()
        {
            return gsm;
        }

        public override AppSimControl GetAppSimControl()
        {
            return gsc;
        }

        public override AppInputHandler GetAppInputHandler()
        {
            return gih;
        }

        public virtual GameStateMachine Gsm()
        {
            return gsm;
        }

        public virtual GameSimControl Gsc()
        {
            return gsc;
        }

        public virtual GameInputHandler Gih()
        {
            return gih;
        }

        public virtual GameEventWrangler Gew()
        {
            return gew;
        }

        public virtual GameFeatures Gf()
        {
            return gf;
        }

        public virtual LevelManager Lm()
        {
            return lm;
        }

        public virtual PlayerEnergy Pe()
        {
            return pe;
        }

        public virtual PlayerRouteBuilder Prb()
        {
            return prb;
        }

        public virtual PlayerTripChooser Ptc()
        {
            return ptc;
        }

        public virtual BonusGenerator Bg()
        {
            return bg;
        }

        public virtual IncidentGenerator Ig()
        {
            return ig;
        }

        public virtual VialGenerator Vg()
        {
            return vg;
        }

        public override void FireAppStart()
        {
            bg.Clear();
            ig.Clear();
            vg.Clear();
        }

        public override void FireAppInit()
        {
            Player.Reset();
            ptc.Init();
        }

        public override void Running(bool v)
        {
            if (v && Gsm().CurrentState() == AppState.ReadyToSimulate) Gsm().MakeTransition(GameStateTransition.startSimulation);
        }

        public override void FirePlayerBoardedTaxi(IPed player)
        {
            gsc.RequestToggleControlPedTaxi(true);
        }

        public override void FirePlayerTaxiLaneChangeCompleted(ITaxi playerTaxi)
        {
            gsc.HandledRequestToChangeLane();
        }

        public override double GetPlayerTaxiLaneChangeDirectionRequested(ITaxi playerTaxi)
        {
            return prb.ShowTaxiRoute() ? gsc.LaneChangeRequested() : 0;
        }

        public override double GetPlayerTaxiSpeedFactor(ITaxi playerTaxi)
        {
            return gsc.SpeedFactor();
        }

        public override bool IsPlayerTaxiRunningRedLight(ITaxi playerTaxi)
        {
            return gsc.RunningRed();
        }

        public override void FirePlayerTaxiRunningRedCompleted(ITaxi playerTaxi)
        {
            gsc.FinishedRunningRed();
        }

        public override bool IsPlayerTaxiRunningRedRequested(ITaxi playerTaxi)
        {
            return gsc.RunningRedRequested();
        }

        public override void FirePlayerTaxiStartedRunningRed(ITaxi playerTaxi)
        {
            gsc.StartedRunningRed();
        }

        public override void FirePlayerTaxiCancelRunningRed(ITaxi playerTaxi)
        {
            gsc.CancelRunningRed();
        }

        public override IBranch GetRouteBranch(IBot bot, IBranch here)
        {
            return prb.ChosenBranchFrom(bot, here);
        }

        public override IPedTrip InsertPedTrip()
        {
            if (gsm.CurrentState() == GameState.TripChosen) 
            {
                IPedTrip trip = ptc.PlayerTrip();
                if (trip != null) 
                {
                    gsm.MakeTransition(GameStateTransition.queueRelease);
                    return trip;
                }
            }
            return null;
        }

        public override bool IsPlayerRouteExisting(IStop stop)
        {
            return prb.RouteExistsForPlayer(stop);
        }

        public override bool IsPlayerTrip(ITrip trip)
        {
            return ptc.IsPlayerTrip(trip);
        }

        public override bool IsPlayer(IPed ped)
        {
            return Player.IsPlayer(ped);
        }

        public override bool IsPlayerTaxi(ITaxi taxi)
        {
            return Player.IsPlayerTaxi(taxi);
        }

        public override bool IsPlayerBus(IBus bus)
        {
            return Player.IsPlayerBus(bus);
        }

        public override void FireBotDepart(IBot bot)
        {
            base.FireBotDepart(bot);
            if (bot is IPed && Player.IsPlayer((IPed)bot)) FirePlayerDepart();
        }

        private void FirePlayerDepart()
        {
            prb.Depart();
        }

        public override void FireTransferPhase()
        {
            Player player = Player.ActivePlayer();
            if (player != null) player.FireTransferPhase();
        }

        public override void FirePedInitialise(IPed ped, IBranch wd, ITaxi taxi, IHalt halt)
        {
            ITrip trip = ped.GetTrip();
            bool isPlayer = trip == ptc.PlayerTrip();
            if (isPlayer) 
            {
                Player player = new Player(this, ped);
                ped.AuxData(player);
                if (halt != null) Sim().ForceNextDepartureNow(halt);
                Gsc().ResetSpeedControl();
                Gsm().MakeTransition(GameStateTransition.startNavigating);
            }
            else 
                {
                    bool vc = Gf().IsViralCarrier(ped.GetTrip());
                    ped.AuxData(new PedGameData(vc));
                }
        }

        public override void FirePedOnWalkwayChange(IPed ped, bool onWalk)
        {
            if (!onWalk && Gsc().JayWalking()) Gsc().FinishJayWalking();
        }

        public override void FirePedPassingAnother(IPed ped1, IPed ped2, double distance)
        {
            if (GameFeatures.ViralOn() && Player.IsPlayer(ped1) && distance < GameFeatures.VIRAL_TRANSMISSION_DISTANCE && PedGameData.ViralCarrier(ped2)) Pe().ViralContactsInc
    (1);
        }

        public override bool IsPlayerJayWalking(IPed ped)
        {
            return Gsc().JayWalking();
        }

        public override bool IsPlayerStopped(IPed ped)
        {
            return Gsc().IsStop();
        }

        public override double GetPlayerSidewaysNudgeRequested(IPed ped)
        {
            return Gsc().LaneChangeRequested();
        }

        public override double GetPlayerSpeedFactor(IPed ped)
        {
            return Gsc().SpeedFactor();
        }

        public override void FirePedArriveAtHalt(IPed ped, IHalt halt, IBus bus)
        {
            Gsc().ResetSpeedControl();
        }

        public override int GetPlayerForwardOnFootpathOnArrivalAtZone(IPed player, IFootpath footpath)
        {
            Gsc().ResetSpeedControl();
            return Prb().ManRouteIncludesFootpath(footpath);
        }

        public override IBranch GetPlayerTaxiDestination(IPed ped, IBranch bvo)
        {
            return Prb().TaxiDestinationInManRoute(bvo);
        }

        public override int GetPlayerBusrouteIndex(IPed ped, IBranch bbb)
        {
            return Prb().BusrouteHaltIndex(bbb);
        }

        public override int GetPlayerBusOffStopIndex(IPed ped, IBranch bbo)
        {
            return Prb().BusrouteOffStopIndex(bbo);
        }

        public override void FirePlayerAbandon(IPed player)
        {
            Prb().OnBailOut();
            Gsc().RequestToChangeSpeedOnce(1);
        }

        public override void FirePlayerUTurn(IPed player)
        {
            Prb().OnUTurn1();
            Gsc().ResetSpeedControl();
        }

        public override void FirePlayerUTurnOnWalkway(IPed player, IWalkBranch wb)
        {
            Prb().OnUTurn2(wb);
        }

        public override void FirePlayerUTurnBackOutOfStopOrBayToWalkway(IPed player, IWalkBranch wb)
        {
            Prb().BackOutOfStopOrBayToWalkway(wb);
        }

        public override double GetTripCostVariationOnRelease(ITripOD trip, IBranch first)
        {
            return Ptc().CostVariationOnRelease(trip, first);
        }
    }
}
