using uk.vroad.api.events;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public interface LAppView : LEvent
    {
        void FireViewChanged();
    }
}
