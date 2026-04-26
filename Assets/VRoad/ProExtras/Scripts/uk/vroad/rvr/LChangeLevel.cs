using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public interface LChangeLevel
    {
        void LevelChangerSelected(bool selected);

        void LevelChangeRequested(int inc);

        void ActiveQuarterChanged();

        void ActiveQuarterChangeDisabled(int opt);
    }
}
