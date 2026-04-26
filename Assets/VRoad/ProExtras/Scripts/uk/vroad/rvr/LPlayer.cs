using uk.vroad.api.events;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public interface LPlayer : LEvent
    {
        void EnergyChanged(double energyNow);

        void EnergyEventOccurred(EnergyEvent pe, double deltaJoules);

        void SpeedChanged();

        void Alert(AlertEvent we);
    }
}
