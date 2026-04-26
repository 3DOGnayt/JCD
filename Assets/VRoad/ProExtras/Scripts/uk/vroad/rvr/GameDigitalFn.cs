using uk.vroad.api.input;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public class GameDigitalFn : AppDigitalFn
    {
        public static readonly uk.vroad.rvr.GameDigitalFn RouteForward = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn RouteLeft = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn RouteRight = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn RouteUndoUTurn = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn RouteSelect = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn TripUp = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn TripDown = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn TripLeft = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn TripRight = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn TripNewDeal = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn TripSelect = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn VolumeUp = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn VolumeDown = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn VolumeMute = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn UTurnStopped = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn PedOrTaxi = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn RunRed = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn Abandon = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn BrowserSelect = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn CityUp = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn CityDown = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn CityLeft = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn CityRight = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn CitySelect = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn MapCycle = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn CameraCycle = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn MenuControls = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn MenuGraphics = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn MenuStory = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn MenuTips = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn MenuCredits = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn MenuChangeCity = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn MenuAvatar = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn MenuReset = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn CameraDolly = new uk.vroad.rvr.GameDigitalFn();
        public static readonly uk.vroad.rvr.GameDigitalFn CameraNorth = new uk.vroad.rvr.GameDigitalFn();

        protected internal GameDigitalFn()
        {
        }
        private static bool regG = false;

        public override string ToString()
        {
            if (!regG) 
            {
                regG = true;
                ADbr.RegisterStaticObjectNames(RouteForward);
            }
            return base.ToString();
        }
    }
}
