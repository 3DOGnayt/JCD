using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public class MapState
    {
        public static readonly uk.vroad.rvr.MapState None = new uk.vroad.rvr.MapState();
        public static readonly uk.vroad.rvr.MapState Base = new uk.vroad.rvr.MapState();
        public static readonly uk.vroad.rvr.MapState XRay = new uk.vroad.rvr.MapState();

        private MapState()
        {
        }
    }
}
