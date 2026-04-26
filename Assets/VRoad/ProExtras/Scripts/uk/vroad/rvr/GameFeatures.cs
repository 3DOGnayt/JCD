using uk.vroad.api.etc;
using uk.vroad.api.events;
using uk.vroad.api.map;
using uk.vroad.api.route;
using uk.vroad.api.str;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public class GameFeatures : LEvent
    {
        public static double VIRAL_TRANSMISSION_DISTANCE = 2.0;
        public static double VIRAL_CARRIERS_PER_1000_DEFAULT = 25.0;
        public static int VIRAL_CONTACT_OVERLOAD = 5000;
        public static double INCIDENTS_PER_HOUR = 4.0;
        public static int INCIDENT_PROBABILITY_BREAKDOWN = 75;
        private static bool playing = true;
        private static bool bonusOn = true;
        private static bool virusOn = true;
        private static bool incidentsOn = true;

        public static bool PlayingNow()
        {
            return playing;
        }

        public static bool BonusOn()
        {
            return bonusOn;
        }

        public static bool ViralOn()
        {
            return virusOn;
        }

        public static bool IncidentsOn()
        {
            return incidentsOn;
        }

        public static void Testing(bool bonus, bool virus, bool incidents)
        {
        }

        public static void Playing()
        {
        }
        private double viralCarriersPer1000 = -1;

        public static uk.vroad.rvr.GameFeatures Awake(Game game)
        {
            lock (typeof(GameFeatures))
            {
                return new uk.vroad.rvr.GameFeatures(game);
            }
        }
        private readonly Game game;

        private GameFeatures(Game gm)
        {
            game = gm;
            gm.AddEventConsumer(this);
        }

        public virtual bool DeregisterFireMapChange()
        {
            viralCarriersPer1000 = -1;
            return false;
        }

        public virtual bool IsViralCarrier(ITrip trip)
        {
            if (!ViralOn()) return false;
            double rngv = Strand.RngDouble(trip, Strand.HEALTH, 1000);
            return rngv < ViralCarriersPer1000(game);
        }

        public virtual double ViralCarriersPer1000(Game game)
        {
            if (viralCarriersPer1000 < 0) 
            {
                ICouple vlc = game.Map().Couple(SF.VIRAL_LOAD);
                if (vlc != null && vlc.Value() != null) viralCarriersPer1000 = KTools.ParseDoubleX0(vlc.Value());
                if (viralCarriersPer1000 <= 0) 
                {
                    viralCarriersPer1000 = VIRAL_CARRIERS_PER_1000_DEFAULT * game.Pe().CurrentLevel() * (0.5 + Rng.NextDouble(Rng.Vein.VIRUS));
                    if (viralCarriersPer1000 < 0) viralCarriersPer1000 = 0;
                }
            }
            return viralCarriersPer1000;
        }
    }
}
