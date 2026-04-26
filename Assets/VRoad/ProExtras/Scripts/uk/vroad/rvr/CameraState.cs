using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public class CameraState
    {
        public static readonly uk.vroad.rvr.CameraState North = new uk.vroad.rvr.CameraState();
        public static readonly uk.vroad.rvr.CameraState Dolly = new uk.vroad.rvr.CameraState();
        public static readonly uk.vroad.rvr.CameraState Tether = new uk.vroad.rvr.CameraState();
        public static readonly uk.vroad.rvr.CameraState Route = new uk.vroad.rvr.CameraState();

        private CameraState()
        {
        }
    }
}
