using uk.vroad.api;
using uk.vroad.api.enums;
using uk.vroad.api.etc;
using uk.vroad.api.events;
using uk.vroad.api.map;
using uk.vroad.api.sim;
using uk.vroad.api.str;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public class PlayerEnergy : LAppState, LSimTimeStep, LSimRewind
    {
        private const int MAP_COST_DETAILED = 250;
        private const int MAP_COST_STANDARD = 150;
        public const int QUARTER_INITIAL = 2;
        public const int N_SPARE_BATTERIES = 9;
        private const int VIALS_COMPLETE = 36;
        public const int VIALS_ON_EDGE_FROM_LEVEL = 9;
        public const int CHARGE_NEW_DEAL_INITIAL = 25;
        public const int CHARGE_NEW_DEAL_MULTIPLE = 25;
        public const double CHARGE_NEW_DEAL_INCREASE_FACTOR = 1.2;
        private const double JAYWALKING_CAMERA_PROBABILITY = 0.25;
        private const double RED_LIGHT_CAMERA_PROBABILITY = 0.10;
        private const int FINE_BAIL_OUT_TAXI_BASE = 150;
        private const int FINE_BAIL_OUT_TAXI_INC = 50;
        private const int FINE_BAIL_OUT_BUS_BASE = 25;
        private const int FINE_BAIL_OUT_BUS_INC = 25;
        private const double FINE_JAYWALKING_BASE = 75;
        private const double FINE_JAYWALKING_INC = 25;
        private const double FINE_RED_LIGHT_BASE = 300;
        private const double FINE_RED_LIGHT_INC = 100;
        public const double MAX_SPEED_BAIL_OUT = 2.5;
        private double bonusAccumulator;
        private double finesAccumulator;
        private double rewardOnArrival;
        private double energyUsedKJ;
        private double energyRemainingKJ;
        private double energyAfterLastReward;
        private double energyNetGain;
        private double energyRemainingOnSlider;
        private double powerCurrently;
        private double energyChangeIncrement;
        private double energyChangeRemaining;
        private int currentLevel = PlayerScore.LEVEL_INITIAL;
        private int attainedLevel = PlayerScore.LEVEL_INITIAL;
        private int currentQuarter = QUARTER_INITIAL;
        private int range;
        private int spareBatteries;
        private int rewards;
        private int viralContacts;
        private int vialsCount;
        private long mask;
        private double prevWalkDistance;
        private double prevBikeDistance;
        private double prevBusDistance;
        private double prevTaxiDistance;
        private readonly Game game;
        private PlayerScore scoreActive;
        private string mrOriginZone;
        private bool needSaveScore;
        private bool needSaveQZone;
        private bool needSaveLevel;

        public static uk.vroad.rvr.PlayerEnergy Awake(Game game)
        {
            lock (typeof(PlayerEnergy))
            {
                return new uk.vroad.rvr.PlayerEnergy(game);
            }
        }

        private PlayerEnergy(Game gm)
        {
            game = gm;
            gm.AddEventConsumer(this);
            InitDistances();
        }

        public virtual bool DeregisterFireMapChange()
        {
            return false;
        }

        public virtual void AppStateChanged(AppStateTransition transition)
        {
            if (transition == GameStateTransition.startNavigating) game.Gew().FireAlertEvent(AlertEvent.Launch);
            if (transition == GameStateTransition.removeGameOverMsg) InitFromSavedScore();
            if (transition == GameStateTransition.arrivalAtTargetContinueThisMap || transition == GameStateTransition.arrivalAtTargetLevelUp) SaveHighScore();
            if (transition == AppStateTransition.finishBuildStartSim) 
            {
                int level = CurrentLevel();
                CustomMapBuilt(level, true);
            }
        }

        public virtual PlayerScore ScoreActive()
        {
            return scoreActive;
        }

        private void ScoreActive(PlayerScore ps)
        {
            scoreActive = ps;
        }

        private void SaveHighScore()
        {
            int attainedLevel = AttainedLevel();
            while (EnoughEnergyForNextLevel(attainedLevel))
            {
                attainedLevel++;
            }
            PlayerScore scoreNow = new PlayerScore(ScoreMask(), EnergyRemainingAndPending(), attainedLevel, ViralContacts());
            if (scoreNow.IsHigherThan(ScoreActive())) 
            {
                ScoreActive(scoreNow);
                needSaveScore = true;
            }
            IPedZone dest = game.Ptc().PreviousDestination();
            if (dest != null) 
            {
                string destName = dest.ToString();
                if (!destName.Equals(MrOriginZone())) 
                {
                    MrOriginZone(destName);
                    needSaveQZone = true;
                }
            }
        }

        public virtual void LoadScoreFromCloud(PlayerScore cloudScore)
        {
            ScoreActive(cloudScore);
            needSaveScore = true;
        }

        private void MrOriginZone(string s)
        {
            if (s != null && s.Trim().Length > 0) mrOriginZone = s;
        }

        private string MrOriginZone()
        {
            if (mrOriginZone == null && game.Map() != null) 
            {
                ICouple mroc = game.Map().Couple(SF.ORIGIN_ZONE);
                if (mroc != null) MrOriginZone(mroc.Value());
            }
            return mrOriginZone;
        }

        public virtual IPedZone SavedDestination()
        {
            return game.Map().PedZone(MrOriginZone());
        }

        private void InitFromSavedScore()
        {
            if (ScoreActive() == null) return;
            InitMask(ScoreActive().mask);
            InitEnergy(ScoreActive().energy);
            AttainedLevel(ScoreActive().level);
            InitViralContacts(ScoreActive().virus);
            while (EnoughEnergyForNextLevel(AttainedLevel()))
            {
                IncrementAttainedLevel();
            }
        }

        public virtual void ReadPrefsInSuitableThread()
        {
            if (UDbg.SET_SCORE) 
            {
                UDbg.SetNewScore();
                ScoreActive(new PlayerScore(UDbg.cachedScoreMask, UDbg.cachedScoreEnergy, UDbg.cachedScoreLevel, UDbg.cachedScoreVirus));
                InitFromSavedScore();
                needSaveScore = true;
            }
            if (ScoreActive() == null) 
            {
                ScoreActive(PlayerScore.ReadFromPrefs());
                InitFromSavedScore();
                int currLevel = KPrefs.GetInt(SF.PREFS_LEVEL, 1);
                int minLevel = 1;
                if (currLevel < minLevel || currLevel > AttainedLevel()) currLevel = AttainedLevel();
                if (currLevel > 0) CurrentLevel(currLevel);
            }
            string mroz = KPrefs.GetString(KFormat.Sprintf(SF.PREFS_LQ_ZONE, CurrentLevel(), CurrentQuarter()), null);
            MrOriginZone(mroz);
            int ndc = KPrefs.GetInt(KFormat.Sprintf(SF.PREFS_LQ_DEAL, CurrentLevel(), CurrentQuarter()), CurrentLevel() * uk.vroad.rvr.PlayerEnergy.CHARGE_NEW_DEAL_INITIAL
                );
            game.Ptc().NewDealCharge(ndc);
        }

        public virtual bool SaveScoreEtcIfRequired()
        {
            bool saveLocal = false;
            bool saveRemote = false;
            if (needSaveScore) 
            {
                needSaveScore = false;
                ScoreActive().Save();
                saveLocal = true;
                saveRemote = true;
            }
            if (needSaveLevel) 
            {
                needSaveLevel = false;
                KPrefs.SetInt(SF.PREFS_LEVEL, CurrentLevel());
                saveLocal = true;
            }
            if (needSaveQZone) 
            {
                needSaveQZone = false;
                KPrefs.SetString(KFormat.Sprintf(SF.PREFS_LQ_ZONE, CurrentLevel(), CurrentQuarter()), MrOriginZone());
                KPrefs.SetInt(KFormat.Sprintf(SF.PREFS_LQ_DEAL, CurrentLevel(), CurrentQuarter()), game.Ptc().NewDealCharge());
                saveLocal = true;
            }
            if (saveLocal) KPrefs.Save();
            return saveRemote;
        }

        public virtual void CurrentQuarter(int q)
        {
            currentQuarter = q;
        }

        public virtual void TimeRewind()
        {
        }

        private void InitEnergy(double initKJ)
        {
            EnergyRemainingKJ(initKJ);
            energyAfterLastReward = initKJ;
            energyUsedKJ = 0;
            rewardOnArrival = 0;
            rewards = 0;
            FireEnergyChanged();
        }

        private void InitDistances()
        {
            prevWalkDistance = 0;
            prevBikeDistance = 0;
            prevBusDistance = 0;
            prevTaxiDistance = 0;
        }

        public virtual void TimeStep()
        {
            double dt = game.Sim().TimeStep();
            Player player = Player.ActivePlayer();
            double joulesUsed;
            double wattsAtRestSitting = 95.0;
            if (player == null || player.Ped().Finished()) joulesUsed = wattsAtRestSitting * dt;
            else 
            {
                IPed playerPed = player.Ped();
                bool isWaiting = false;
                bool isWalking = false;
                bool isInTaxi = false;
                bool isInBus = false;
                bool isBiking = false;
                if (playerPed.GetBus() != null) isInBus = true;
                else if (playerPed.IsAboard()) isInTaxi = true;
                else if (playerPed.IsWaiting()) isWaiting = true;
                else isWalking = true;
                double mass = playerPed.Mass();
                double wattsAtRestUpright = Price.PowerUsedToWalk(mass, 0);
                if (isWalking) 
                {
                    double thisWalkDistance = playerPed.DistanceTravelled(BranchMode.Walking);
                    double dd = thisWalkDistance - prevWalkDistance;
                    double vMPS = dd / dt;
                    double vKMH = vMPS * 3.6;
                    double watts = Price.PowerUsedToWalk(mass, vKMH);
                    joulesUsed = watts * dt;
                    prevWalkDistance = (double)thisWalkDistance;
                }
                else if (isWaiting) joulesUsed = wattsAtRestUpright * dt;
                else if (isBiking) 
                {
                    double totalBikeDistance = 0;
                    double dd = totalBikeDistance - prevBikeDistance;
                    prevBikeDistance = totalBikeDistance;
                    double v = dd / dt;
                    if (v < 0) v = -v;
                    if (v > 15) v = 15;
                    double A = 0.5;
                    double C_D = 1.0;
                    double ro = 1.225;
                    double watts_air = 0.5 * ro * (v * v * v) * C_D * A;
                    double j_air = watts_air * dt;
                    double mass_inc_bike = mass + 10;
                    double g = 9.81;
                    double C_rr = 0.005;
                    double watts_roll = mass_inc_bike * g * C_rr * v;
                    double j_roll = watts_roll * dt;
                    joulesUsed = j_air + j_roll;
                }
                else if (isInBus) 
                    {
                        double thisTransitDistance = playerPed.DistanceTravelled(BranchMode.OnBus);
                        double dd = thisTransitDistance - prevBusDistance;
                        prevBusDistance = thisTransitDistance;
                        joulesUsed = dd * 200;
                        joulesUsed += dt * wattsAtRestSitting;
                    }
                    else 
                    {
                        Reporter.Sink(isInTaxi);
                        double thisTaxiDistance = playerPed.DistanceTravelled(BranchMode.InTaxi);
                        double dd = thisTaxiDistance - prevTaxiDistance;
                        prevTaxiDistance = thisTaxiDistance;
                        double drivingJoulesPerM = 240;
                        double commissionRate = 0.10f;
                        double drivingTargetSpeedMPS = 8.0f;
                        double wattsDriving = commissionRate * drivingJoulesPerM * drivingTargetSpeedMPS;
                        joulesUsed = dd * drivingJoulesPerM;
                        joulesUsed += dt * wattsDriving;
                        joulesUsed += dt * wattsAtRestSitting;
                    }
            }
            joulesUsed += dt * (double)MapCostPowerW();
            if (joulesUsed < 0) joulesUsed = 0;
            double kjUsedDT = 0.001f * (double)joulesUsed;
            energyUsedKJ += kjUsedDT;
            EnergyRemainingKJ(EnergyRemainingKJ() - kjUsedDT);
            powerCurrently = 1000.0 * kjUsedDT / dt;
            EnergyChangeTimeStep();
            FireEnergyChanged();
        }

        public virtual int MapCostPowerW()
        {
            Player player = Player.ActivePlayer();
            MapState mapState = game.Gih().GetMapState();
            int mapUsagePower = (player == null || player.Ped().Finished()) ? MAP_COST_STANDARD : mapState == MapState.XRay ? MAP_COST_DETAILED : mapState == MapState
                .Base ? MAP_COST_STANDARD : 0;
            return mapUsagePower;
        }

        public virtual bool EnergyChangeGradual()
        {
            bool changed = EnergyChangeTimeStep();
            if (changed) CalcBatteryEnergy();
            return changed;
        }

        private void FireEnergyChanged()
        {
            CalcBatteryEnergy();
            double energy = EnergyRemainingKJ();
            game.Gew().FireEnergyChanged(energy);
            if (energy <= 0) 
            {
                AppState state = game.Gsm().CurrentState();
                if (state == GameState.Navigating || state == GameState.WaitingForTripChoice) game.Gsc().GameOver(GameSimControl.GAME_OVER_ENERGY_EXHAUSTED);
            }
        }

        private void CalcBatteryEnergy()
        {
            double remaining = EnergyRemainingKJ();
            range = RangeForEnergy(remaining);
            int energySingleBatteryMax = SINGLE_BATTERY_MAX[range];
            spareBatteries = (((int)remaining) - 1) / energySingleBatteryMax;
            if (spareBatteries > N_SPARE_BATTERIES) spareBatteries = N_SPARE_BATTERIES;
            if (spareBatteries < 0) spareBatteries = 0;
            energyRemainingOnSlider = remaining - (spareBatteries * energySingleBatteryMax);
        }

        private int RangeForEnergy(double kj)
        {
            int n = SINGLE_BATTERY_MAX.Length;
            int kji = (int)kj;
            for (int pi = 0; pi < n; pi++)
            {
                int max = SINGLE_BATTERY_MAX[pi];
                if (kji <= 10 * max) return pi;
            }
            return n;
        }

        public virtual int Range()
        {
            return range;
        }

        public virtual int SpareBatteries()
        {
            return spareBatteries;
        }

        public virtual double EnergyRemainingOnSlider()
        {
            return energyRemainingOnSlider;
        }

        public double EnergyUsedKJ()
        {
            return energyUsedKJ;
        }

        public double EnergyRemainingKJ()
        {
            return energyRemainingKJ;
        }

        public int EnergyRemainingAndPending()
        {
            return KTools.RoundToInt(EnergyRemainingKJ() + energyChangeRemaining);
        }

        public double EnergyAfterLastReward()
        {
            return energyAfterLastReward;
        }

        public double EnergyNetGain()
        {
            return energyNetGain;
        }

        public double PowerCurrently()
        {
            return powerCurrently;
        }

        public double Bonus()
        {
            return bonusAccumulator;
        }

        public double Fines()
        {
            return finesAccumulator;
        }

        public double RewardOnArrival()
        {
            return rewardOnArrival;
        }

        public int RewardsReceived()
        {
            return rewards;
        }

        public int CurrentQuarter()
        {
            return currentQuarter;
        }

        public void EnergyRemainingKJ(double p)
        {
            energyRemainingKJ = p > 0 ? p : 0;
        }
        private static readonly int[] SINGLE_BATTERY_MAX = new int[] { (int)1e3, (int)1e4, (int)1e5, (int)1e6 };

        public virtual void RewardOnArrival(double rewardKJ)
        {
            rewardOnArrival = rewardKJ;
        }

        private void EnergyChangeGradual(double delta)
        {
            lock (this)
            {
                energyChangeRemaining += delta;
                energyChangeIncrement = energyChangeRemaining / 25.0;
            }
        }

        private void EnergyChangeInstant(double delta)
        {
            lock (this)
            {
                EnergyRemainingKJ(EnergyRemainingKJ() + delta);
            }
        }

        private bool EnergyChangeTimeStep()
        {
            bool changed = false;
            if (energyChangeIncrement != 0) 
            {
                EnergyRemainingKJ(EnergyRemainingKJ() + energyChangeIncrement);
                energyChangeRemaining -= energyChangeIncrement;
                if ((energyChangeIncrement > 0 && energyChangeRemaining < 0.5 * energyChangeIncrement) || (energyChangeIncrement < 0 && energyChangeRemaining > 0.5 * energyChangeIncrement
                    )) 
                {
                    energyChangeRemaining = 0;
                    energyChangeIncrement = 0;
                }
                changed = true;
            }
            return changed;
        }

        public virtual void ApplyReward()
        {
            double reward = rewardOnArrival;
            rewardOnArrival = 0;
            EnergyChangeGradual(reward);
            rewardOnArrival = 0;
            rewards++;
            energyNetGain += (EnergyRemainingKJ() + reward - energyAfterLastReward);
            energyAfterLastReward = EnergyRemainingKJ() + reward;
            InitDistances();
            game.Gew().FireEnergyEvent(EnergyEvent.Reward, reward);
        }

        public virtual void ApplyBonus(double bonusReward)
        {
            bonusAccumulator += bonusReward;
            EnergyChangeGradual(bonusReward);
            game.Gew().FireEnergyEvent(EnergyEvent.Bonus, bonusReward);
        }

        public virtual void ApplyFine(EnergyEvent pe, double fine)
        {
            finesAccumulator += fine;
            EnergyChangeGradual(-fine);
            FireEnergyChanged();
            game.Gew().FireEnergyEvent(pe, -fine);
        }

        public virtual void ApplyCharge(EnergyEvent ee, double charge)
        {
            EnergyChangeInstant(-charge);
            FireEnergyChanged();
            game.Gew().FireEnergyEvent(ee, -charge);
        }

        private void InitViralContacts(int vc)
        {
            viralContacts = vc;
        }

        public virtual void ViralContactsInc(int n)
        {
            viralContacts += n;
        }

        public virtual int ViralContacts()
        {
            return viralContacts;
        }

        public virtual void IncrementAttainedLevel()
        {
            AttainedLevel(AttainedLevel() + 1);
            CurrentLevel(AttainedLevel());
            UpdateScore();
        }

        public virtual bool EnoughEnergyForNextLevel(int lowerLevel)
        {
            if (lowerLevel >= LevelManager.N_LEVELS) return false;
            double minimumForNextlevel = (lowerLevel + 1) * 1000;
            return EnergyRemainingKJ() + energyChangeRemaining > minimumForNextlevel;
        }

        public virtual void CurrentLevel(int lv)
        {
            if (lv == currentLevel) return;
            currentLevel = lv;
            needSaveLevel = true;
        }

        private void AttainedLevel(int lv)
        {
            attainedLevel = lv;
        }

        public virtual int CurrentLevel()
        {
            return currentLevel;
        }

        public virtual int AttainedLevel()
        {
            return attainedLevel;
        }

        public virtual void TestingSetLevel(int newLevel)
        {
        }

        public virtual bool CurrentLevelUpIsEnabled()
        {
            return CurrentLevel() < AttainedLevel();
        }

        public virtual bool CurrentLevelDownIsEnabled()
        {
            return CurrentLevel() > 1;
        }

        public virtual void CurrentLevelUp()
        {
            if (CurrentLevelUpIsEnabled()) CurrentLevel(CurrentLevel() + 1);
        }

        public virtual void CurrentLevelDown()
        {
            if (CurrentLevelDownIsEnabled()) CurrentLevel(CurrentLevel() - 1);
        }
        private const long VIAL_BITS = unchecked((long)(0x000000FFFFFFFFFFL));
        private const long MAP_BITS = unchecked((long)(0x0000FF0000000000L));
        private const int MAP_SHIFT = 40;

        public static long VialMask(long mask)
        {
            return mask & VIAL_BITS;
        }

        public static int MapMask(long mask)
        {
            long mapBits = mask & MAP_BITS;
            return (int)(mapBits >> MAP_SHIFT);
        }

        public static int VialsCount(long vMask)
        {
            int count = 0;
            while (vMask != 0)
            {
                vMask = vMask & (vMask - 1);
                count++;
            }
            return count;
        }

        private void InitMask(long m)
        {
            mask = m;
            vialsCount = VialsCount(VialMask());
        }

        private long VialMask()
        {
            return VialMask(ScoreMask());
        }

        private long ScoreMask()
        {
            return mask;
        }

        public virtual bool CustomMapBuilt(int level)
        {
            int mapMask = MapMask(ScoreMask());
            int mapMaskBit = level - 1;
            return (mapMask & (1 << mapMaskBit)) != 0;
        }

        public virtual void CustomMapBuilt(int level, bool v)
        {
            int fullMaskBit = MAP_SHIFT + level - 1;
            long bitMask = 1L << fullMaskBit;
            if (v) mask |= bitMask;
            else mask &= ~bitMask;
            UpdateScore();
        }

        private void UpdateScore()
        {
            ScoreActive(new PlayerScore(ScoreMask(), EnergyRemainingAndPending(), AttainedLevel(), ViralContacts()));
            needSaveScore = true;
        }

        private long VialBitMask(int level, int quarter)
        {
            int bit = -1;
            if (level >= 1 && level <= LevelManager.N_LEVELS) 
            {
                if (quarter >= 1 && quarter <= 4) bit = ((level - 1) * 4) + (quarter - 1);
                else if (quarter == 5) bit = 32 + (level - 1);
            }
            return bit >= 0 ? 1L << bit : 0;
        }

        public virtual bool HaveCollectedVial(int level, int quarter)
        {
            long bitMask = VialBitMask(level, quarter);
            return (VialMask() & bitMask) != 0;
        }

        public virtual bool HaveCollectedVial()
        {
            return HaveCollectedVial(CurrentLevel(), CurrentQuarter());
        }

        public virtual bool CollectVial()
        {
            long bitMask = VialBitMask(CurrentLevel(), CurrentQuarter());
            if (bitMask != 0) 
            {
                if ((VialMask() & bitMask) == 0) 
                {
                    bitMask &= VIAL_BITS;
                    mask |= bitMask;
                    vialsCount = VialsCount(VialMask());
                    UpdateScore();
                    game.Gew().FireAlertEvent(AlertEvent.CollectVial);
                    return true;
                }
            }
            return false;
        }

        public virtual int VialsCount()
        {
            return vialsCount;
        }

        public virtual int VialsCountComplete()
        {
            return VIALS_COMPLETE;
        }

        public virtual bool VialsComplete()
        {
            return VialsCount() >= VIALS_COMPLETE;
        }

        public virtual double RiskRedLight()
        {
            return RED_LIGHT_CAMERA_PROBABILITY;
        }

        public virtual double FineRedLight()
        {
            return FINE_RED_LIGHT_BASE + (FINE_RED_LIGHT_INC * (CurrentLevel() - 1));
        }

        public virtual double RiskJayWalking()
        {
            return JAYWALKING_CAMERA_PROBABILITY;
        }

        public virtual double FineJayWalking()
        {
            return FINE_JAYWALKING_BASE + (FINE_JAYWALKING_INC * (CurrentLevel() - 1));
        }

        public virtual double FineAbandonTaxi(Player player)
        {
            if (game.Sim() != null) 
            {
                double timeSinceStart = game.Sim().TimeNow() - player.DepartureTime();
                if (timeSinceStart < 60) return 0;
            }
            return FINE_BAIL_OUT_TAXI_BASE + (FINE_BAIL_OUT_TAXI_INC * (CurrentLevel() - 1));
        }

        public virtual double FineAbandonBus()
        {
            return FINE_BAIL_OUT_BUS_BASE + (FINE_BAIL_OUT_BUS_INC * (CurrentLevel() - 1));
        }
    }
}
