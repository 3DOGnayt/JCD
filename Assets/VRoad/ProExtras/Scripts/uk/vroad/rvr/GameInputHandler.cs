using uk.vroad.api;
using uk.vroad.api.events;
using uk.vroad.api.input;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public class GameInputHandler : AppInputHandler, IEventDistributor
    {
        public static uk.vroad.rvr.GameInputHandler Awake(Game game)
        {
            lock (typeof(GameInputHandler))
            {
                return new uk.vroad.rvr.GameInputHandler(game);
            }
        }
        private readonly KList<LAppView> appViewListeners = new KList<LAppView>();
        private double cameraHeight = 500;

        private GameInputHandler(Game gm)
            : base(gm, GameInputMapping.ChooseMap)
        {
            gm.AddEventDistributor(this);
        }

        public virtual void AddEventConsumer(LEvent consumer)
        {
            if (consumer is LAppView) appViewListeners.Add((LAppView)consumer);
        }

        public virtual void RemoveEventConsumer(LEvent consumer)
        {
            lock (this)
            {
                if (consumer is LAppView) appViewListeners.Remove((LAppView)consumer);
            }
        }

        protected internal virtual void FireViewChanged()
        {
            foreach (LAppView avl in appViewListeners)
            {
                avl.FireViewChanged();
            }
        }

        public override AppStateTransition MenuClosingTransition()
        {
            if (CurrentMenu() == GamePauseMenu.MapMenu) return AppStateTransition.closeMapMenu;
            if (CurrentMenu() == GamePauseMenu.TripMenu) return GameStateTransition.resumeToWaitingForTripChoice;
            if (CurrentMenu() == GamePauseMenu.PlayMenu) return GameStateTransition.resumeToNavigating;
            if (CurrentMenu() == GamePauseMenu.CompleteMenu) return GameStateTransition.resumeToNavigating;
            return null;
        }

        public override void AppStateChanged(AppStateTransition transition)
        {
            if (transition == GameStateTransition.chooseTrip) 
            {
                if (GetCameraState() == CameraState.Tether) SetNewlyTethered(true);
            }
            AppState after = transition.after;
            if (after == AppState.WaitingForMapChoice) 
            {
                SetCurrentMapping(GameInputMapping.ChooseMap);
                SetCurrentMenu(AppPauseMenu.NoMenu);
            }
            else if (after == GameState.Browsing) 
            {
                SetCurrentMapping(GameInputMapping.Browser);
                SetCurrentMenu(AppPauseMenu.NoMenu);
            }
            else if (after == GameState.WaitingForTripChoice) 
            {
                SetCurrentMapping(GameInputMapping.ChooseTrip);
                SetCurrentMenu(AppPauseMenu.NoMenu);
            }
            else if (after == GameState.Navigating) 
            {
                SetCurrentMapping(GameInputMapping.PlayingGame);
                SetCurrentMenu(AppPauseMenu.NoMenu);
            }
            else if (after == GameState.PausedAtTripChoice) 
            {
                SetCurrentMapping(AppInputMapping.Paused);
                SetCurrentMenu(GamePauseMenu.TripMenu);
            }
            else if (after == GameState.PausedWhileNavigating) 
            {
                SetCurrentMapping(AppInputMapping.Paused);
                SetCurrentMenu(GamePauseMenu.PlayMenu);
            }
            else if (after == AppState.MenuAtMapChoice) 
            {
                SetCurrentMapping(AppInputMapping.Paused);
                SetCurrentMenu(GamePauseMenu.MapMenu);
            }
            else if (after == GameState.MenuComplete) 
            {
                SetCurrentMapping(AppInputMapping.Paused);
                SetCurrentMenu(GamePauseMenu.CompleteMenu);
            }
            else if (after == GameState.MenuWhileBrowsing) 
            {
                SetCurrentMapping(AppInputMapping.Paused);
                SetCurrentMenu(GamePauseMenu.BrowseMenu);
            }
            else base.AppStateChanged(transition);
        }

        public override bool AppInputDigitalEvent(AppDigitalFn dfn, bool isPressed)
        {
            if (isPressed == false) return false;
            if (dfn == GameDigitalFn.MapCycle) return MapCycle();
            if (dfn == GameDigitalFn.CameraCycle) return CameraCycle();
            if (dfn == GameDigitalFn.CameraDolly) 
            {
                SetCameraState(CameraState.Dolly);
                return true;
            }
            if (dfn == GameDigitalFn.CameraNorth) 
            {
                SetCameraState(CameraState.North);
                return true;
            }
            return base.AppInputDigitalEvent(dfn, isPressed);
        }
        private MapState mapState = MapState.Base;
        private CameraState cameraState = CameraState.Tether;
        private bool newlyTethered = true;

        public virtual bool IsCameraTethered()
        {
            return GetCameraState() == CameraState.Tether;
        }

        public virtual CameraState GetCameraState()
        {
            return cameraState;
        }

        public virtual void SetCameraState(CameraState cs)
        {
            if (cs == GetCameraState()) return;
            if (GetCameraState() == CameraState.Dolly) SetCurrentMapping(GameInputMapping.PlayingGame);
            cameraState = cs;
            if (GetCameraState() == CameraState.Tether) newlyTethered = true;
            if (GetCameraState() == CameraState.Dolly) SetCurrentMapping(GameInputMapping.PlayingDolly);
            FireViewChanged();
        }

        public virtual bool CameraCycle()
        {
            if (GetCameraState() == CameraState.Tether) SetCameraState(CameraState.Route);
            else SetCameraState(CameraState.Tether);
            return true;
        }

        public virtual bool MapOn()
        {
            return GetMapState() != MapState.None;
        }

        public virtual MapState GetMapState()
        {
            return mapState;
        }

        public virtual void SetMapState(MapState ms)
        {
            if (ms == mapState) return;
            mapState = ms;
            FireViewChanged();
        }

        public virtual bool MapCycle()
        {
            if (GetMapState() == MapState.None) SetMapState(MapState.Base);
            else if (GetMapState() == MapState.Base) SetMapState(MapState.XRay);
            else SetMapState(MapState.None);
            return true;
        }

        public virtual bool FaceCameraNorth()
        {
            return GetCameraState() == CameraState.North;
        }

        public virtual bool FocusOnRoute()
        {
            return GetCameraState() == CameraState.Route;
        }
        private const double MINI_MAP_HT = 50;

        public virtual void SetCameraHeight(float ch)
        {
            bool viewChange = (ch <= MINI_MAP_HT && GetCameraHeight() > MINI_MAP_HT) || (ch > MINI_MAP_HT && GetCameraHeight() <= MINI_MAP_HT);
            cameraHeight = ch;
            if (viewChange) FireViewChanged();
        }

        public virtual double GetCameraHeight()
        {
            return cameraHeight;
        }

        public virtual bool ShowMiniMap()
        {
            return MapOn() && GetCameraHeight() <= MINI_MAP_HT;
        }

        public virtual bool ShowTransparentBuildings()
        {
            return GetMapState() == MapState.XRay;
        }

        public virtual bool IsNewlyTethered()
        {
            return newlyTethered;
        }

        public virtual void ResetNewlyTethered()
        {
            newlyTethered = false;
        }

        public virtual void SetNewlyTethered(bool v)
        {
            newlyTethered = v;
        }
    }
}
