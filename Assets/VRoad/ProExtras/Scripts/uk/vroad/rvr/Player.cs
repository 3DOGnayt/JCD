using uk.vroad.api.enums;
using uk.vroad.api.geom;
using uk.vroad.api.map;
using uk.vroad.api.route;
using uk.vroad.api.sim;
using uk.vroad.api.str;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public class Player
    {
        private const int SIM_SECONDS_BETWEEN_UTURNS = 15;
        private readonly Game game;
        private readonly IPed ped;

        public Player(Game gm, IPed p)
        {
            game = gm;
            ped = p;
            playerInstance = this;
        }

        public virtual IPed Ped()
        {
            return ped;
        }
        private static uk.vroad.rvr.Player playerInstance;

        public static void Reset()
        {
            playerInstance = null;
        }

        public static uk.vroad.rvr.Player ActivePlayer()
        {
            return playerInstance;
        }

        public static bool IsPlayer(IPed ped)
        {
            return playerInstance != null && ped == playerInstance.ped;
        }

        public static bool IsPlayerTaxi(ITaxi taxi)
        {
            return playerInstance != null && playerInstance.ped.GetTaxi() == taxi;
        }

        public static bool IsPlayerBus(IBus bus)
        {
            return playerInstance != null && playerInstance.ped.GetBus() == bus;
        }
        private bool uTurnRequested;

        public virtual void UTurnRequested(bool b)
        {
            uTurnRequested = b;
        }

        private bool UTurnRequested()
        {
            return uTurnRequested;
        }

        public virtual void FireTransferPhase()
        {
            if (GameFeatures.ViralOn()) 
            {
                if (game.Gsm().CurrentState() == GameState.Navigating && game.Pe().ViralContacts() > GameFeatures.VIRAL_CONTACT_OVERLOAD) 
                {
                    game.Gsc().GameOver(GameSimControl.GAME_OVER_VIRUS);
                    return;
                }
            }
            CheckForCollisionOnRunningRedLight();
            if (UTurnRequested()) 
            {
                if (UDbg.BUTTON) SE.Report(SE.BUTTON_05);
                if (ped.IsAboard()) {}
                else if (ped.IsOnWalkway() || ped.IsOnCorner()) 
                {
                    if (ped.CanUTurn()) ped.MakeUTurn();
                }
                UTurnRequested(false);
            }
            if (GameFeatures.ViralOn()) 
            {
                IBus bus = ped.GetBus();
                if (bus != null) 
                {
                    int nCarriers = game.Gsc().BallastViralCarriers(bus);
                    IBusRider[] bra = bus.GetBusRiderArray();
                    foreach (IBusRider br in bra)
                    {
                        IPed that = br.GetPed();
                        if (PedGameData.ViralCarrier(br.GetPed())) nCarriers++;
                    }
                    if (nCarriers > 0) game.Pe().ViralContactsInc(nCarriers);
                }
            }
        }

        private void CheckForCollisionOnRunningRedLight()
        {
            if (game.Gsm().CurrentState() != GameState.Navigating) return;
            if (game.Gsc().RunningRed()) 
            {
                ITaxi playerTaxi = ped.GetTaxi();
                if (playerTaxi == null || playerTaxi.GetStreme() == null) game.Gsc().CancelRunningRed();
                else if (playerTaxi.HasCollidedWithVehicle()) game.Gsc().GameOver(GameSimControl.GAME_OVER_COLLISION_TAXI);
            }
            if (game.Gsc().JayWalking() && ped.HasCollidedWithVehicle()) game.Gsc().GameOver(GameSimControl.GAME_OVER_COLLISION_PED);
        }

        public virtual ILocus GetLocus()
        {
            IVkl vkl = Ped().GetVkl();
            return vkl != null ? vkl.GetLocus() : ped.GetLocus();
        }

        public virtual ILocus GetNextLocus()
        {
            IVkl vkl = Ped().GetVkl();
            return vkl != null ? vkl.GetNextLocus() : ped.GetNextLocus();
        }

        public virtual double LocusDistance()
        {
            IVkl vkl = Ped().GetVkl();
            return vkl != null ? vkl.LocusDistance() : ped.LocusDistance();
        }

        public virtual bool LocusForward()
        {
            return Aboard() || ped.IsForwardOnLocus();
        }

        public virtual bool Aboard()
        {
            return ped.IsAboard();
        }

        public virtual bool WaitingAtBusStop()
        {
            return ped.IsWaitingAtBusStop();
        }

        public virtual bool WaitingAtTaxiBay()
        {
            return ped.IsWaitingAtTaxiBay();
        }

        public virtual bool WaitingAtCrossing()
        {
            return ped.IsWaitingAtCrossing();
        }

        public virtual bool StartedCrossing()
        {
            return ped.HasStartedCrossing();
        }

        public virtual bool Blocked()
        {
            return ped.IsBlocked();
        }

        public virtual bool Arrived()
        {
            return ped.HasArrived();
        }

        public virtual IVkl GetVkl()
        {
            return ped.GetVkl();
        }

        public virtual IBus GetBus()
        {
            return ped.GetBus();
        }

        public virtual ITaxi GetTaxi()
        {
            return ped.GetTaxi();
        }

        public virtual IZone GetOrigin()
        {
            return ped.GetOrigin();
        }

        public virtual IZone GetDestination()
        {
            return ped.GetDestination();
        }

        public virtual IBranch GetRecentBranch()
        {
            return ped.GetRecentBranch();
        }

        public virtual IMind GetMind()
        {
            return ped.GetMind();
        }

        public virtual ITrip GetTrip()
        {
            return ped.GetTrip();
        }

        public virtual AbandonEvent Abandon(bool isTaxi)
        {
            return ped.AbandonVehicle(isTaxi);
        }

        public virtual IWalkway Walkway()
        {
            return ped.GetWalkway();
        }

        public virtual IWalkway NextWalkway()
        {
            return ped.GetNextWalkway();
        }

        public virtual double DepartureTime()
        {
            return ped.DepartureTime();
        }

        public virtual bool Finished()
        {
            return ped.Finished();
        }

        public virtual Xyz Centre()
        {
            return ped.Centre();
        }
    }
}
