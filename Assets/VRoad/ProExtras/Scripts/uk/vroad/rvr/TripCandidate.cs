using System;
using uk.vroad.api;
using uk.vroad.api.geom;
using uk.vroad.api.map;
using uk.vroad.api.str;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public class TripCandidate : IComparable<uk.vroad.rvr.TripCandidate>
    {
        public static readonly uk.vroad.rvr.TripCandidate[] ZERO = new uk.vroad.rvr.TripCandidate[0];
        private readonly IPedZone orig;
        private readonly IPedZone dest;
        private readonly Xy origLocRel;
        private readonly Xy destLocRel;
        private readonly int reward;

        public TripCandidate(Game game, IPedZone zo, IPedZone zd, double kj)
        {
            orig = zo;
            dest = zd;
            reward = KTools.RoundToInt(kj);
            IMap map = game.Map();
            double bb = map.GetBorder();
            Xyz sw = map.GetSW();
            Xyz ne = map.GetNE();
            double mapWidth = map.GetWidth();
            double mapHeight = map.GetHeight();
            double orx = (orig.Location().X() - sw.X() + bb) / mapWidth;
            double ory = (orig.Location().Y() - sw.Y() + bb) / mapHeight;
            origLocRel = new Xy(orx, ory);
            double drx = (dest.Location().X() - sw.X() + bb) / mapWidth;
            double dry = (dest.Location().Y() - sw.Y() + bb) / mapHeight;
            destLocRel = new Xy(drx, dry);
        }

        public override string ToString()
        {
            return orig.ToString() + SC.RT_ARROW + dest.ToString() + SC.SEQS + reward;
        }

        public virtual IPedZone Origin()
        {
            return orig;
        }

        public virtual IPedZone Destination()
        {
            return dest;
        }

        public virtual int Reward()
        {
            return reward;
        }

        public virtual Xy OriginLocationRelative()
        {
            return origLocRel;
        }

        public virtual Xy DestinationLocationRelative()
        {
            return destLocRel;
        }

        public virtual int CompareTo(uk.vroad.rvr.TripCandidate that)
        {
            if (that == this) return 0;
            if (this.reward > that.reward) return -1;
            if (this.reward < that.reward) return 1;
            int destCI = string.CompareOrdinal(this.dest.ToString(), that.dest.ToString());
            if (destCI != 0) return destCI;
            int origCI = string.CompareOrdinal(this.orig.ToString(), that.orig.ToString());
            if (origCI != 0) return origCI;
            return 0;
        }
    }
}
