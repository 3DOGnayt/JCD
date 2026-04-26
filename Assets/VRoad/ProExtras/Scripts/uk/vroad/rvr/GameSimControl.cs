using uk.vroad.api;
using uk.vroad.api.enums;
using uk.vroad.api.etc;
using uk.vroad.api.events;
using uk.vroad.api.input;
using uk.vroad.api.map;
using uk.vroad.api.sim;
using uk.vroad.api.str;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public class GameSimControl : AppSimControl, LSimTimeStep, LSimRewind, LAppState, LBotDepart, LBotArrive
    {
        private const double MIN_SPEED_FACTOR = 0.5;
        private const double MAX_PED_SPEED_FACTOR = 2.0;
        private const double MAX_TAXI_SPEED_FACTOR = 1.2;
        private const int GAME_OVER_COUNTDOWN_SEC = 30;
        public const int GAME_OVER_RESTART = -1;
        public const int GAME_OVER_ENERGY_EXHAUSTED = 1;
        public const int GAME_OVER_VIRUS = 2;
        public const int GAME_OVER_COLLISION_PED = 3;
        public const int GAME_OVER_COLLISION_TAXI = 4;
        private int gameOverSequenceCountdown;
        private int gameOverReason;
        private bool playerArrived;
        private bool jayWalking = false;
        private bool runningRed = false;
        private bool runningRedRequested = false;
        private double requestToChangeLane;
        private double requestToChangeSpeed;
        private int requestToChangeSpeedOnce;
        private bool requestToggleControlPedTaxi;
        private bool stickCentredAfterStop = false;
        private double topSpeedFactor;
        private int tsSincePsiChange;
        private PlayerSpeedIndicator psi = PlayerSpeedIndicator.NORM;
        private readonly uk.vroad.rvr.Game game;

        public static uk.vroad.rvr.GameSimControl Awake(uk.vroad.rvr.Game game)
        {
            lock (typeof(GameSimControl))
            {
                return new uk.vroad.rvr.GameSimControl(game);
            }
        }

        private GameSimControl(uk.vroad.rvr.Game gm)
            : base(gm)
        {
            game = gm;
        }

        private uk.vroad.rvr.Game Game()
        {
            return (uk.vroad.rvr.Game)app;
        }

        public virtual void AppStateChanged(AppStateTransition transition)
        {
            if (transition.after == GameState.GameOver) 
            {
                gameOverSequenceCountdown = GAME_OVER_COUNTDOWN_SEC;
                Player player = Player.ActivePlayer();
                if (player != null) player.Ped().Zombify(BabylKey.GameOver.ToString() + SC.RBL + GameOverReason() + SC.RBR);
                PlayerReset();
            }
            if (transition == GameStateTransition.arrivalAtTargetLevelUp) Game().Pe().IncrementAttainedLevel();
        }

        public override bool AppInputDigitalEvent(AppDigitalFn fn, bool isPressed)
        {
            if (isPressed == false) return false;
            if (fn == GameDigitalFn.PedOrTaxi) return RequestToggleControlPedTaxi(true);
            if (fn == GameDigitalFn.RunRed) return RequestSignalJump();
            if (fn == GameDigitalFn.UTurnStopped) return RequestUTurn(true);
            if (fn == GameDigitalFn.Abandon) return RequestAbandon();
            if (fn == GameDigitalFn.MenuChangeCity) return EscapeToMapChooserFromPlay();
            return base.AppInputDigitalEvent(fn, isPressed);
        }

        public override bool AppInputAnalogEvent(AppAnalogFn aFn, double value)
        {
            if (aFn == GameAnalogFn.Lane) 
            {
                double analogThreshold = 0.2;
                double laneChangeMultiplier = 0.1;
                if (value > analogThreshold || value < -analogThreshold) RequestToChangeLane(value * laneChangeMultiplier);
                return true;
            }
            if (aFn == GameAnalogFn.Speed) 
            {
                double analogThreshold = 0.2;
                double speedChangeMultiplier = 0.5;
                if (value > analogThreshold || value < -analogThreshold) RequestToChangeSpeed(value * speedChangeMultiplier);
                else RequestToChangeSpeed(0);
                return true;
            }
            return false;
        }

        public virtual void RequestToChangeLane(double requestDirection)
        {
            requestToChangeLane = requestDirection;
        }

        public virtual double LaneChangeRequested()
        {
            return requestToChangeLane;
        }

        public virtual void HandledRequestToChangeLane()
        {
            requestToChangeLane = 0;
        }

        public virtual void RequestToChangeSpeed(double requestDirection)
        {
            requestToChangeSpeed = requestDirection;
        }

        public virtual double SpeedChangeRequested()
        {
            return requestToChangeSpeed;
        }

        public virtual void RequestToChangeSpeedOnce(int requestDirection)
        {
            requestToChangeSpeed = 0;
            requestToChangeSpeedOnce = requestDirection;
        }

        public virtual int SpeedChangeRequestedOnce()
        {
            int value = requestToChangeSpeedOnce;
            requestToChangeSpeedOnce = 0;
            return value;
        }

        public virtual bool RequestToggleControlPedTaxi(bool b)
        {
            if (requestToggleControlPedTaxi == b) return false;
            requestToggleControlPedTaxi = b;
            return true;
        }

        public virtual bool ToggleControlPedTaxiRequested()
        {
            return requestToggleControlPedTaxi;
        }

        public virtual bool RequestUTurn(bool mustBeStopped)
        {
            Player player = Player.ActivePlayer();
            if (player != null && !player.Ped().IsAboard() && player.Ped().CanUTurn()) 
            {
                bool canUTurnWhileMoving = !mustBeStopped;
                bool stoppedWhileWalking = SpeedIndicator() == PlayerSpeedIndicator.STOP && StickCentredAfterStop();
                if (canUTurnWhileMoving || stoppedWhileWalking || player.WaitingAtBusStop() || player.WaitingAtTaxiBay()) 
                {
                    if (UDbg.BUTTON) SE.Report(SE.BUTTON_06);
                    player.UTurnRequested(true);
                    return true;
                }
            }
            if (mustBeStopped) return true;
            return false;
        }

        private bool RequestAbandon()
        {
            Player player = Player.ActivePlayer();
            if (player == null) return true;
            if (!player.Aboard()) return true;
            IVkl vkl = player.GetVkl();
            if (vkl == null) return true;
            GameEventWrangler gew = Game().Gew();
            if (vkl.Speed() > PlayerEnergy.MAX_SPEED_BAIL_OUT) 
            {
                gew.FireAlertEvent(AlertEvent.SpeedTooHigh);
                return false;
            }
            bool isTaxi = vkl is ITaxi;
            PlayerEnergy pe = Game().Pe();
            double charge = isTaxi ? pe.FineAbandonTaxi(player) : pe.FineAbandonBus();
            if (charge > pe.EnergyRemainingKJ()) 
            {
                gew.FireAlertEvent(AlertEvent.EnergyTooLow);
                return false;
            }
            AbandonEvent abe = player.Abandon(isTaxi);
            if (abe != null) 
            {
                AlertEvent ale = abe == AbandonEvent.NoFootpath ? AlertEvent.NoFootpath : AlertEvent.NotAboard;
                gew.FireAlertEvent(ale);
                return false;
            }
            if (charge > 0) pe.ApplyFine(isTaxi ? EnergyEvent.AbandonTaxi : EnergyEvent.AbandonBus, charge);
            return true;
        }

        private bool RequestSignalJump()
        {
            Player player = Player.ActivePlayer();
            if (player == null) return false;
            ITaxi taxi = player.GetTaxi();
            if (player.GetBus() != null) 
            {
                if (UDbg.BUTTON) SE.Report(SE.BUTTON_07A);
            }
            else if (taxi != null) 
                {
                    if (taxi.LaneRank() == 0 && taxi.IsBlockedByRedSignal()) 
                    {
                        runningRedRequested = true;
                        Game().Gew().FireAlertEvent(AlertEvent.RequestRedLightRun);
                        return true;
                    }
                    else if (UDbg.BUTTON) SE.Report(SE.BUTTON_07C, taxi.LaneRank());
                }
                else 
                    {
                        ICrossing x = null;
                        if (player.Walkway() is ICrossing) 
                        {
                            if (player.StartedCrossing()) 
                            {
                                if (UDbg.BUTTON) SE.Report(SE.BUTTON_08A);
                                return false;
                            }
                            x = (ICrossing)player.Walkway();
                        }
                        else if (player.NextWalkway() is ICrossing) x = (ICrossing)player.NextWalkway();
                        if (x != null) 
                        {
                            if (!x.IsWalkOn()) 
                            {
                                StartJayWalking(x.IsRedGreen());
                                return true;
                            }
                            else if (UDbg.BUTTON) SE.Report(SE.BUTTON_08B);
                        }
                        else if (UDbg.BUTTON) SE.Report(SE.BUTTON_08C);
                    }
            return false;
        }

        public virtual bool JayWalking()
        {
            return jayWalking;
        }

        public virtual void StartJayWalking(bool signalisedCrossing)
        {
            jayWalking = true;
            bool fined = false;
            if (signalisedCrossing) fined = ApplyFineIfCamera(EnergyEvent.FineJayWalking, Game().Pe().FineJayWalking(), Game().Pe().RiskJayWalking());
            if (!fined) Game().Gew().FireAlertEvent(AlertEvent.RequestJayWalk);
        }

        public virtual void FinishJayWalking()
        {
            jayWalking = false;
        }

        public virtual bool RunningRedRequested()
        {
            return runningRedRequested;
        }

        public virtual bool RunningRed()
        {
            return runningRed;
        }

        public virtual void CancelRunningRed()
        {
            runningRedRequested = false;
            runningRed = false;
        }

        public virtual void FinishedRunningRed()
        {
            runningRed = false;
        }

        public virtual void StartedRunningRed()
        {
            runningRed = true;
            runningRedRequested = false;
            ApplyFineIfCamera(EnergyEvent.FineRedLight, Game().Pe().FineRedLight(), Game().Pe().RiskRedLight());
        }

        public virtual bool ApplyFineIfCamera(EnergyEvent pe, double fine, double pCamera)
        {
            double p = Rng.NextDouble(Rng.Vein.INCIDENT);
            if (p <= pCamera) 
            {
                Game().Pe().ApplyFine(pe, fine);
                return true;
            }
            return false;
        }

        public virtual bool StickCentredAfterStop()
        {
            return stickCentredAfterStop;
        }

        public override void TimeStep()
        {
            base.TimeStep();
            PlayerSpeedIndicator psi1 = SpeedIndicator();
            double dsc = SpeedChangeRequested();
            if (dsc == 0) 
            {
                if (psi1 == PlayerSpeedIndicator.STOP) stickCentredAfterStop = true;
            }
            else if (psi1 != PlayerSpeedIndicator.STOP) stickCentredAfterStop = false;
            if (dsc == 0) dsc = SpeedChangeRequestedOnce();
            double PSI_FFWD_VALUE = 0.4;
            int PSI_TIMESTEPS = 20;
            PlayerSpeedIndicator psi2 = psi1;
            if (dsc > PSI_FFWD_VALUE && psi1 == PlayerSpeedIndicator.FAST) psi2 = PlayerSpeedIndicator.FFWD;
            if (dsc > 0) 
            {
                if (psi1 == PlayerSpeedIndicator.STOP) psi2 = PlayerSpeedIndicator.SLOW;
                else if (psi1 == PlayerSpeedIndicator.SLOW) psi2 = PlayerSpeedIndicator.NORM;
                else if (psi1 == PlayerSpeedIndicator.NORM) psi2 = PlayerSpeedIndicator.FAST;
            }
            else if (dsc < 0) 
                {
                    if (psi1 == PlayerSpeedIndicator.SLOW) psi2 = PlayerSpeedIndicator.STOP;
                    else if (psi1 == PlayerSpeedIndicator.NORM) psi2 = PlayerSpeedIndicator.SLOW;
                    else if (psi1 == PlayerSpeedIndicator.FAST) psi2 = PlayerSpeedIndicator.NORM;
                }
            if (dsc < PSI_FFWD_VALUE && psi1 == PlayerSpeedIndicator.FFWD) 
            {
                psi2 = PlayerSpeedIndicator.FAST;
                tsSincePsiChange = PSI_TIMESTEPS;
            }
            if (psi2 != psi1) 
            {
                if (tsSincePsiChange >= PSI_TIMESTEPS) 
                {
                    tsSincePsiChange = 0;
                    Player player = Player.ActivePlayer();
                    if (player != null) SpeedIndicator(player.Aboard(), psi2);
                }
            }
            tsSincePsiChange++;
            if (playerArrived) 
            {
                playerArrived = false;
                HandlePlayerArrived();
            }
        }

        public virtual bool IsStop()
        {
            return psi == PlayerSpeedIndicator.STOP;
        }

        public virtual bool IsFFwd()
        {
            return psi == PlayerSpeedIndicator.FFWD;
        }

        public virtual bool IsFast()
        {
            return psi == PlayerSpeedIndicator.FAST;
        }

        public virtual PlayerSpeedIndicator SpeedIndicator()
        {
            return psi;
        }

        public virtual double SpeedFactor()
        {
            return topSpeedFactor;
        }

        private void SpeedIndicator(bool aboard, PlayerSpeedIndicator p)
        {
            psi = p;
            if (psi == PlayerSpeedIndicator.STOP) topSpeedFactor = 0;
            else if (psi == PlayerSpeedIndicator.SLOW) topSpeedFactor = MIN_SPEED_FACTOR;
            else if (psi == PlayerSpeedIndicator.NORM) topSpeedFactor = 1.0;
            else topSpeedFactor = aboard ? MAX_TAXI_SPEED_FACTOR : MAX_PED_SPEED_FACTOR;
            Game().Gew().FireSpeedChanged();
        }

        public virtual void ResetSpeedControl()
        {
            SpeedIndicator(false, PlayerSpeedIndicator.NORM);
            stickCentredAfterStop = false;
            tsSincePsiChange = 0;
        }

        public virtual void PlayerReset()
        {
            Game().Ptc().PlayerTripClear();
            Player.Reset();
        }

        public virtual double GameOverSequenceCountdown()
        {
            return gameOverSequenceCountdown;
        }

        public override void TimeSec()
        {
            if (gameOverSequenceCountdown > 0) 
            {
                gameOverSequenceCountdown--;
                if (gameOverSequenceCountdown <= 0) 
                {
                    gameOverSequenceCountdown = 0;
                    Game().Sim().StopAndKill(GameStateTransition.removeGameOverMsg);
                }
            }
            base.TimeSec();
        }

        public virtual void Arrive(IBot bot, IZone dest)
        {
            if (bot is IPed && Player.IsPlayer((IPed)bot) && dest is IPedZone) 
            {
                IPedZone pDest = (IPedZone)dest;
                Game().Pe().ApplyReward();
                Game().Ptc().PlayerArrive(pDest);
                playerArrived = true;
            }
        }

        private void HandlePlayerArrived()
        {
            if (Game().Pe().EnoughEnergyForNextLevel(Game().Pe().AttainedLevel())) 
            {
                Game().Gsm().MakeTransition(GameStateTransition.arrivalAtTargetLevelUp);
                KillAllAndMakeTransition(GameStateTransition.startNewLevel);
            }
            else 
            {
                Game().Gsm().MakeTransition(GameStateTransition.arrivalAtTargetContinueThisMap);
                if (Game().Pe().HaveCollectedVial()) KillAllAndMakeTransition(GameStateTransition.escapeFromQuarterWait);
            }
            PlayerReset();
        }

        public override bool EscapeToMapChooserBySystem()
        {
            if (Game().Gsm().CurrentState() == GameState.Navigating) return EscapeToMapChooserFromPlay();
            else if (Game().Gsm().CurrentState() == GameState.WaitingForTripChoice) return EscapeToMapChooserFromWait();
            return EscapeToMapChooserFromAnyState();
        }

        public virtual bool EscapeToMapChooserFromWait()
        {
            PlayerReset();
            KillAllAndMakeTransition(GameStateTransition.escapeFromQuarterWait);
            return true;
        }

        public virtual bool EscapeToMapChooserFromPlay()
        {
            PlayerReset();
            KillAllAndMakeTransition(GameStateTransition.escapeFromQuarterPlay);
            return true;
        }

        private bool EscapeToMapChooserFromAnyState()
        {
            PlayerReset();
            KillAllAndMakeTransition(AppStateTransition.Find(Game().Gsm().CurrentState(), AppState.WaitingForMapChoice));
            return true;
        }

        public virtual void TimeRewind()
        {
            gameOverSequenceCountdown = 0;
        }

        public virtual void GameOver(int reason)
        {
            gameOverReason = reason;
            Game().Gsm().MakeTransition(GameStateTransition.gameOverWhileNavigating);
        }

        public virtual int GameOverReason()
        {
            return gameOverReason;
        }
        private KHash<IBus, BusGameData> busGameData = new KHash<IBus, BusGameData>();

        public virtual void Depart(IBot bot)
        {
            if (bot is IBus) 
            {
                IBus bus = (IBus)bot;
                int nbvc = 0;
                int nballast = bus.BallastRiderSeatNos().Length;
                for (int bi = 0; bi < nballast; bi++)
                {
                    bool vc = Game().Gf().IsViralCarrier(bus.GetTrip());
                    if (vc) nbvc++;
                }
                BusGameData bgd = new BusGameData(nbvc);
                busGameData.Put(bus, bgd);
            }
        }

        public virtual int BallastViralCarriers(IBus bus)
        {
            BusGameData bgd = busGameData[bus];
            return bgd == null ? 0 : bgd.ballastViralCount;
        }
    }
}
