using uk.vroad.api.str;
using uk.vroad.apk;
using uk.vroad.pac;

namespace uk.vroad.rvr
{
    public class PlayerScore
    {
        private const int MASK_INITIAL = 0;
        private const int ENERGY_INITIAL = 1000;
        public const int LEVEL_INITIAL = 1;
        private const int VIRUS_INITIAL = 0;

        public static uk.vroad.rvr.PlayerScore ReadFromPrefs()
        {
            string hsp = KPrefs.GetString(SF.PREFS_HIGHSCORE, SC.N);
            string[] hsParts = KTools.SplitQuick(hsp, CC.UNDER);
            if (hsParts.Length >= 4) 
            {
                int part = 0;
                long mask = SP.Lh(hsParts[part++]);
                int energy = SP.I(hsParts[part++]);
                int level = SP.I(hsParts[part++]);
                int virus = SP.I(hsParts[part++]);
                uk.vroad.rvr.PlayerScore phs = new uk.vroad.rvr.PlayerScore(mask, energy, level, virus);
                if (phs.VerifyScore(hsParts[hsParts.Length - 1])) return phs;
            }
            uk.vroad.rvr.PlayerScore ps = new uk.vroad.rvr.PlayerScore(MASK_INITIAL, ENERGY_INITIAL, LEVEL_INITIAL, VIRUS_INITIAL);
            ps.Save();
            return ps;
        }
        public readonly long mask;
        public readonly int vialsCount;
        public readonly int energy;
        public readonly int level;
        public readonly int virus;

        public PlayerScore(long msk, int en, int lv, int vr)
        {
            mask = msk;
            vialsCount = PlayerEnergy.VialsCount(VialsMask());
            energy = en;
            level = lv;
            virus = vr;
        }

        public virtual long VialsMask()
        {
            return PlayerEnergy.VialMask(mask);
        }

        public virtual int MapMask()
        {
            return PlayerEnergy.MapMask(mask);
        }

        private static string MaskStr(long mask, int lo, int hi)
        {
            string s = SC.N;
            for (int bit = lo; bit <= hi; bit++)
            {
                s = ((0 != (mask & (1L << bit))) ? CC.CROSS : CC.UNDER) + s;
            }
            return s;
        }

        public override string ToString()
        {
            return KFormat.Sprintf(SGM.PHS_STR, vialsCount, MaskStr(VialsMask(), 0, 39), energy, level, virus, MaskStr(MapMask(), 0, 7));
        }

        public virtual bool IsHigherThan(uk.vroad.rvr.PlayerScore that)
        {
            return that == null || this.vialsCount > that.vialsCount || this.level > that.level || (this.vialsCount == that.vialsCount && this.energy > that.energy
                );
        }

        public virtual void Save()
        {
            string value = KFormat.Sprintf(SGM.PHS_SAVE, mask, energy, level, virus, VRoad.Jumble(ScoreAsHex()));
            KPrefs.SetString(SF.PREFS_HIGHSCORE, value);
        }

        public virtual bool VerifyScore(string jumbledString)
        {
            return VRoad.VerifyJumbled(ScoreAsHex(), jumbledString);
        }

        private string ScoreAsHex()
        {
            long xVials = (mask + 511L) * 127;
            int xEnergy = (energy + 773) * 31;
            int xLevel = (level + 37) * 31;
            int xVirus = (virus + 387) * 23;
            return KFormat.Sprintf(SGM.PHS_PLAIN, xVials, xVirus, xEnergy, xLevel);
        }
    }
}
