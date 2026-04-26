using uk.vroad.api.sim;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public class PedGameData
    {
        private readonly bool viralCarrier;

        public PedGameData(bool vc)
        {
            viralCarrier = vc;
        }

        public bool ViralCarrier()
        {
            return viralCarrier;
        }

        public static bool ViralCarrier(IPed ped)
        {
            if (Player.IsPlayer(ped)) return false;
            object gd = ped.AuxData();
            return gd is uk.vroad.rvr.PedGameData ? ((uk.vroad.rvr.PedGameData)gd).ViralCarrier() : false;
        }
    }
}
