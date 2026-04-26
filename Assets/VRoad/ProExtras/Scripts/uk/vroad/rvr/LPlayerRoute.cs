using uk.vroad.api.events;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public interface LPlayerRoute : LEvent
    {
        void FireRouteChanged();
    }
}
