using System;
using uk.vroad.api;
using uk.vroad.api.etc;
using uk.vroad.api.events;
using uk.vroad.api.input;
using uk.vroad.api.str;
using uk.vroad.apk;
using uk.vroad.pac;

namespace uk.vroad.rvr
{
    public class LevelManager : LAppInput, LAppState, IEventDistributor
    {
        public const int N_LEVELS = 8;
        public const int QUARTERS_PER_LEVEL = 4;
        public const int CUSTOM_QUARTER = 5;
        private bool levelChangerSelected;
        private KBool isMapServerAlive;
        private string[][] mapNames;
        private KImage[][] snapshots;
        private string[][] credits;
        private int selectedQuarter;
        private LChangeLevel levelChangeListener;
        private IScreenGrabber screenGrabber;
        private readonly Game game;

        public static uk.vroad.rvr.LevelManager Awake(Game game)
        {
            lock (typeof(LevelManager))
            {
                return new uk.vroad.rvr.LevelManager(game);
            }
        }

        private LevelManager(Game gm)
        {
            game = gm;
            gm.AddEventDistributor(this);
            gm.AddEventConsumer(this);
        }

        public virtual void Start()
        {
            bool ok = ReadLevels();
            Reporter.Sink(ok);
            SetSelectedQuarter(game.Pe().CurrentQuarter());
        }

        public virtual void AddEventConsumer(LEvent eventListener)
        {
            if (eventListener is IScreenGrabber) screenGrabber = (IScreenGrabber)eventListener;
        }

        public virtual void RemoveEventConsumer(LEvent consumer)
        {
            lock (this)
            {
            }
        }

        public virtual bool DeregisterFireMapChange()
        {
            return false;
        }

        public virtual void Register(LChangeLevel lcl)
        {
            levelChangeListener = lcl;
        }

        public virtual bool IsLevelChangerSelected()
        {
            return levelChangerSelected;
        }
        private bool snapshotIsTemporary;

        public virtual void AppStateChanged(AppStateTransition transition)
        {
            PlayerEnergy pe = game.Pe();
            if (transition.after == AppState.WaitingForMapChoice) SetSelectedQuarter(pe.CurrentQuarter());
            bool justOpened = transition == GameStateTransition.startSimulation;
            bool chosenTrip = transition == GameStateTransition.chooseTrip;
            if (justOpened || chosenTrip) 
            {
                if (pe.CurrentQuarter() == CUSTOM_QUARTER) 
                {
                    int level = pe.CurrentLevel();
                    string prefix = CustomPrefix(level);
                    string vroad = game.GetDataFile().FileName();
                    int dot = vroad.LastIndexOf(CC.DOT);
                    string customMapName = dot > 0 ? uk.vroad.apk.KTools.Substring(vroad, 0, dot) : vroad;
                    string snapshotName = prefix + customMapName + CustomSnapSuffix();
                    KDir vroadW = new KDir(KEnv.VroadWriteDir());
                    KFile snapshotFile = new KFile(vroadW, snapshotName);
                    bool snapshotExists = snapshotFile.Exists();
                    if (!snapshotExists || (chosenTrip && snapshotIsTemporary)) 
                    {
                        if (snapshotExists && snapshotIsTemporary) snapshotFile.Delete();
                        screenGrabber.ScreenGrab(snapshotFile);
                        int qpl = uk.vroad.rvr.LevelManager.QUARTERS_PER_LEVEL;
                        snapshots[level - 1][qpl] = new KImage(snapshotFile);
                        mapNames[level - 1][qpl] = customMapName;
                        snapshotIsTemporary = justOpened;
                    }
                }
            }
            else if (transition == GameStateTransition.arrivalAtTargetLevelUp) SetSelectedQuarter(pe.CurrentQuarter());
        }

        public virtual void RenameCustomMap(string newName)
        {
            int level = game.Pe().CurrentLevel();
            int qpl = uk.vroad.rvr.LevelManager.QUARTERS_PER_LEVEL;
            mapNames[level - 1][qpl] = newName;
        }

        public virtual KImage Snapshot(int qi)
        {
            int lvi = CurrentLevel();
            return snapshots[CurrentLevel() - 1][qi - 1];
        }

        public virtual string MapName(int qi)
        {
            return mapNames[CurrentLevel() - 1][qi - 1];
        }

        public virtual string Credit(int qi)
        {
            return credits[CurrentLevel() - 1][qi - 1];
        }

        public virtual string Credit(int li, int qi)
        {
            return credits[li - 1][qi - 1];
        }

        public virtual bool AppInputAnalogEvent(AppAnalogFn afn, double value)
        {
            return false;
        }

        public virtual bool AppInputDigitalEvent(AppDigitalFn fn, bool isPressed)
        {
            if (!isPressed) return false;
            GameStateMachine gsm = game.Gsm();
            AppState currentState = gsm.CurrentState();
            if (currentState == AppState.MenuAtMapChoice) 
            {
                if (fn == GameDigitalFn.MenuResume) 
                {
                    gsm.MakeTransition(AppStateTransition.closeMapMenu);
                    return true;
                }
                if (fn == GameDigitalFn.MenuExit) 
                {
                    game.Gsc().Exit();
                    return true;
                }
                return false;
            }
            if (currentState == GameState.MenuWhileBrowsing) 
            {
                if (fn == GameDigitalFn.MenuResume) 
                {
                    gsm.MakeTransition(GameStateTransition.closeBrowseMenu);
                    return true;
                }
                if (fn == GameDigitalFn.MenuExit) 
                {
                    game.Gsc().Exit();
                    return true;
                }
                return false;
            }
            if (currentState == GameState.Browsing) 
            {
                if (fn == GameDigitalFn.Pause) 
                {
                    gsm.MakeTransition(GameStateTransition.openBrowseMenu);
                    return true;
                }
                return false;
            }
            if (currentState != AppState.WaitingForMapChoice) return false;
            if (fn == GameDigitalFn.Pause) 
            {
                gsm.MakeTransition(AppStateTransition.openMapMenu);
                return true;
            }
            int opt;
            if (fn == GameDigitalFn.CityLeft) opt = 1;
            else if (fn == GameDigitalFn.CityUp) opt = 2;
            else if (fn == GameDigitalFn.CityRight) opt = 3;
            else if (fn == GameDigitalFn.CityDown) opt = 4;
            else return false;
            bool q2disabled = false;
            bool qAllDisabled = false;
            int sel = SelectedQuarter();
            if (levelChangerSelected) 
            {
                if (opt == 4) 
                {
                    if (!HaveCollectedVial(2)) opt = 2;
                    else if (!HaveCollectedVial(5) && CustomQuarterActive()) opt = 5;
                    else if (!HaveCollectedVial(4)) opt = 4;
                    else if (!HaveCollectedVial(1)) opt = 1;
                    else if (!HaveCollectedVial(3)) opt = 3;
                    else 
                    {
                        if (levelChangeListener != null) levelChangeListener.ActiveQuarterChangeDisabled(0);
                        return false;
                    }
                    levelChangerSelected = false;
                    if (levelChangeListener != null) levelChangeListener.LevelChangerSelected(false);
                }
                else 
                    {
                        if (levelChangeListener != null) 
                        {
                            if (opt == 1) levelChangeListener.LevelChangeRequested(-1);
                            else if (opt == 3) levelChangeListener.LevelChangeRequested(+1);
                        }
                        opt = 0;
                    }
            }
            else 
                {
                    if (CustomQuarterActive() && !HaveCollectedVial(5)) 
                    {
                        if (sel == 3 && opt == 1) opt = CUSTOM_QUARTER;
                        else if (sel == 1 && opt == 3) opt = CUSTOM_QUARTER;
                        else if (sel == 2 && opt == 4) opt = CUSTOM_QUARTER;
                        else if (sel == 4 && opt == 2) opt = CUSTOM_QUARTER;
                    }
                    if (opt == 2) 
                    {
                        if (HaveCollectedVial(2)) 
                        {
                            if (sel == 4 && CustomQuarterActive() && !HaveCollectedVial(5)) {}
                            else 
                            {
                                opt = 0;
                                if (sel == 4 && CustomQuarterActive() && HaveCollectedVial(5)) qAllDisabled = true;
                                else q2disabled = true;
                            }
                        }
                        else if (sel == 2) opt = 0;
                        if (opt == 0) 
                        {
                            levelChangerSelected = true;
                            if (levelChangeListener != null) levelChangeListener.LevelChangerSelected(true);
                        }
                    }
                }
            if (opt != sel) 
            {
                SetSelectedQuarter(opt);
                if (levelChangeListener != null) 
                {
                    if (HaveCollectedVial(opt)) levelChangeListener.ActiveQuarterChangeDisabled(opt);
                    else levelChangeListener.ActiveQuarterChanged();
                }
            }
            if (opt == 0 && levelChangeListener != null) 
            {
                if (qAllDisabled) levelChangeListener.ActiveQuarterChangeDisabled(0);
                else if (q2disabled) levelChangeListener.ActiveQuarterChangeDisabled(2);
            }
            return true;
        }

        public virtual bool IsLevelComplete()
        {
            if (!HaveCollectedVial(1)) return false;
            if (!HaveCollectedVial(2)) return false;
            if (!HaveCollectedVial(3)) return false;
            if (!HaveCollectedVial(4)) return false;
            if (CustomQuarterActive() && !HaveCollectedVial(5)) return false;
            return true;
        }

        public virtual bool CustomQuarterPossible_OLD()
        {
            int CUSTOM_QUARTER_MIN_LEVEL = 3;
            int level = CurrentLevel();
            return level >= CUSTOM_QUARTER_MIN_LEVEL;
        }

        public virtual bool CustomQuarterPossible()
        {
            return VRoad.GotRvr() && (CurrentLevel() % 2 == 0);
        }

        public virtual bool CustomQuarterActive()
        {
            return CustomQuarterPossible() && (CustomBuildExists() || CanBuildCustomMap(CurrentLevel()));
        }

        private bool CanBuildCustomMap(int level)
        {
            return IsMapServerAlive() && GotVialsForCustomMap(level) && !AlreadyBuiltCustomMap(level);
        }

        public virtual int CustomMapNeedVials()
        {
            if (CustomQuarterActive()) return 0;
            if (!CustomQuarterPossible()) return 0;
            if (CustomBuildExists()) return 0;
            int level = CurrentLevel();
            if (AlreadyBuiltCustomMap(level)) return -888;
            if (!IsMapServerAlive()) return -999;
            int vialsNeeded = RequiredVialsForCustomMap(level) - game.Pe().VialsCount();
            return vialsNeeded > 0 ? vialsNeeded : 0;
        }

        public virtual string CustomMapHiddenReason()
        {
            if (CustomQuarterActive()) return null;
            if (!CustomQuarterPossible()) return null;
            if (CustomBuildExists()) return null;
            int level = CurrentLevel();
            if (AlreadyBuiltCustomMap(level)) return Babyl.Translation(BabylKey.AlreadyBuiltMap);
            int vialsNeeded = RequiredVialsForCustomMap(level) - game.Pe().VialsCount();
            if (vialsNeeded > 0) return KFormat.Sprintf(Babyl.Translation(BabylKey.MoreVialsNeeded), vialsNeeded);
            if (!IsMapServerAlive()) return Babyl.Translation(BabylKey.NoMapServer);
            return null;
        }

        private bool IsMapServerAlive()
        {
            if (isMapServerAlive == null) isMapServerAlive = KBool.Get(IsMapServerOK());
            return isMapServerAlive.BoolValue();
        }

        private int RequiredVialsForCustomMap(int level)
        {
            return 4 * (level - 1);
        }

        private bool GotVialsForCustomMap(int level)
        {
            return game.Pe().VialsCount() >= RequiredVialsForCustomMap(level);
        }

        private bool AlreadyBuiltCustomMap(int level)
        {
            if ((new KFile(KEnv.VroadWriteDir(), SGM.XN_MAP)).Exists()) return false;
            return game.Pe().CustomMapBuilt(level);
        }

        public virtual bool HaveCollectedVial(int q)
        {
            return game.Pe().HaveCollectedVial(CurrentLevel(), q);
        }

        public virtual void SetSelectedQuarter(int q)
        {
            if (q == CUSTOM_QUARTER && !CustomQuarterActive()) q = PlayerEnergy.QUARTER_INITIAL;
            if (q > 0 && HaveCollectedVial(q)) return;
            selectedQuarter = q;
            if (q > 0) game.Pe().CurrentQuarter(q);
        }

        public virtual int SelectedQuarter()
        {
            return selectedQuarter;
        }

        public virtual bool ValidSelectedQuarter()
        {
            return CustomQuarterSelected() || (SelectedQuarter() >= 1 && SelectedQuarter() <= 4);
        }

        public virtual bool CustomQuarterSelected()
        {
            return CustomQuarterActive() && SelectedQuarter() == CUSTOM_QUARTER;
        }

        public virtual bool CustomBuildExists()
        {
            KFile quarterFile = QuarterFileInCurrentLevel(CUSTOM_QUARTER);
            return (quarterFile != null && quarterFile.Exists());
        }

        public virtual void OpenSelectedQuarter()
        {
            lock (this)
            {
                if (ValidSelectedQuarter()) 
                {
                    int sel = SelectedQuarter();
                    SetSelectedQuarter(0);
                    KFile quarterFile = QuarterFileInCurrentLevel(sel);
                    if (quarterFile != null && quarterFile.Exists()) VRoad.Load(game, quarterFile);
                }
            }
        }

        private KFile QuarterFileInCurrentLevel(int qi)
        {
            KFile quarterFile;
            if (qi > 0 && qi < CUSTOM_QUARTER) 
            {
                string stem = KEnv.VroadReadDir() + MapName(qi);
                quarterFile = new KFile(stem + SC.SUFFIX_DOT_VROAD);
                if (!quarterFile.Exists()) quarterFile = new KFile(stem + SC.SUFFIX_DOT_VROAD_LEGACY);
            }
            else if (qi == CUSTOM_QUARTER) 
                {
                    quarterFile = new KFile(KEnv.VroadWriteDir() + MapName(qi) + SC.SUFFIX_DOT_VROAD);
                    if (!quarterFile.Exists()) quarterFile = new KFile(KEnv.VroadLegacyWriteDir() + MapName(qi) + SC.SUFFIX_DOT_VROAD_LEGACY);
                }
                else quarterFile = null;
            return quarterFile;
        }

        public virtual void ResetCustomMaps()
        {
            KDir vroadW = new KDir(KEnv.VroadWriteDir());
            KDir newLoc = new KDir(vroadW.Parent(), KDate.DateAndTimeNow());
            vroadW.MoveTo(newLoc);
            vroadW = new KDir(KEnv.VroadWriteDir());
            vroadW.Create();
        }

        private bool Fail(KFile lvFile, string msg, params object[] args)
        {
            return false;
        }

        private bool ReadLevels()
        {
            SetSelectedQuarter(0);
            KDir vroadR = new KDir(KEnv.VroadReadDir());
            KFile lvFile = new KFile(vroadR, SC.RBVL_FILE);
            if (!vroadR.Exists()) return Fail(lvFile, SE.RBVL_ERROR_02, vroadR);
            KReader reader = KReader.NewZReader(lvFile);
            int nlv = uk.vroad.rvr.LevelManager.N_LEVELS;
            int qpl = uk.vroad.rvr.LevelManager.QUARTERS_PER_LEVEL;
            int nBlobsExpected = nlv * qpl;
            byte[][] blobs = new byte[nBlobsExpected][];
            int nBlobsRead = reader.ReadByteBlocks(blobs);
            string metadata = reader.ReadStringToEnd();
            if (metadata == null) return Fail(lvFile, SE.RBVL_ERROR_03, lvFile);
            metadata = metadata.Trim();
            string[] perQuarterData = KTools.SplitQuick(metadata, CC.COMMA);
            int npqd = perQuarterData.Length;
            if (metadata.EndsWith(SC.QM)) npqd--;
            KDir vroadW = new KDir(KEnv.VroadWriteDir());
            snapshots = new KImage[nlv][];
            mapNames = new string[nlv][];
            credits = new string[nlv][];
            string[] mapNameResult = new string[1];
            int bmax = System.Math.Min(npqd, nBlobsRead);
            int bi = 0;
            int qplx = qpl;
            if (VRoad.GotRvr()) qplx++;
            for (int lvi = 0; lvi < nlv; lvi++)
            {
                snapshots[lvi] = new KImage[qplx];
                mapNames[lvi] = new string[qplx];
                credits[lvi] = new string[qplx];
                for (int qi = 0; qi < qpl && bi < bmax; qi++)
                {
                    snapshots[lvi][qi] = new KImage(blobs[bi]);
                    string[] parts = KTools.SplitQuick(perQuarterData[bi], CC.DOT);
                    string mapRoot = parts[0];
                    KFile mapFile = vroadR.FirstFileMatching(mapRoot, SC.SUFFIX_DOT_VROAD, mapNameResult);
                    if (mapFile == null) mapFile = vroadR.FirstFileMatching(mapRoot, SC.SUFFIX_DOT_VROAD_LEGACY, mapNameResult);
                    string mapName = mapFile != null ? mapRoot + mapNameResult[0] : mapRoot;
                    mapNames[lvi][qi] = mapName;
                    string credit = parts.Length >= 3 ? parts[1] : SC.S;
                    credit = credit.Replace(CC.MINUS, CC.SPACE);
                    credit = credit.Replace(CC.UNDER, CC.SPACE);
                    credits[lvi][qi] = credit;
                    bi++;
                }
                if (VRoad.GotRvr()) 
                {
                    string[] customMapName = new string[1];
                    string prefix = CustomPrefix(lvi + 1);
                    KFile snapshotCustomFile = vroadW.FirstFileMatching(prefix, CustomSnapSuffix(), customMapName);
                    if (snapshotCustomFile != null) 
                    {
                        snapshots[lvi][qpl] = new KImage(snapshotCustomFile);
                        mapNames[lvi][qpl] = customMapName[0];
                    }
                }
            }
            if (nBlobsRead != nBlobsExpected) return Fail(lvFile, SE.RBVL_ERROR_04, nBlobsExpected, nBlobsRead);
            if (npqd < nBlobsExpected) return Fail(lvFile, SE.RBVL_ERROR_05, nBlobsExpected, npqd);
            SetSelectedQuarter(PlayerEnergy.QUARTER_INITIAL);
            return true;
        }

        public virtual int CurrentLevel()
        {
            return game.Pe().CurrentLevel();
        }

        public static string CustomPrefix(int level)
        {
            return SF.CUSTOM + SC.UL + level + SC.UL;
        }

        public static string CustomSnapSuffix()
        {
            return SC.SUFFIX_DOT_PNG;
        }

        public static bool IsMapServerOK()
        {
            string response = KHttp.HttpQuery(SGM.BROWSER_ALIVE_URL, null, false);
            bool ok = response == null ? false : response.Trim().Equals(SGM.BROWSER_ALIVE_EXPECTED);
            return ok;
        }
    }
}
