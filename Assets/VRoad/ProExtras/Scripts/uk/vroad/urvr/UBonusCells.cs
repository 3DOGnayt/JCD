using uk.vroad.api;
using uk.vroad.api.events;
using uk.vroad.api.geom;
using uk.vroad.api.str;
using uk.vroad.apk;
using uk.vroad.rvr;
using uk.vroad.ucm;
using UnityEngine;
using UnityEngine.UI;

namespace uk.vroad.urvr
{
    public class UBonusCells : MonoBehaviour, LDynamicObject, LSimTimeSec
    {
        public GameObject bonusCellPrefab;
        public GameObject bonusCellSpritePrefab;

        public Text bronzeCount;
        public Text silverCount;
        public Text goldCount;
        public Text blackCount;

        private KList<BonusCandidate> justSpawned = new KList<BonusCandidate>();
        private KList<BonusCandidate> justExpired = new KList<BonusCandidate>();
        private KList<BonusCandidate> nowFlashing = new KList<BonusCandidate>();
        private KHash<BonusCandidate, GameObject> cellHashGame = new KHash<BonusCandidate, GameObject>();
        private KHash<BonusCandidate, GameObject> cellHashMini = new KHash<BonusCandidate, GameObject>();
        private KHash<BonusCandidate, GameObject> cellHashMaxi = new KHash<BonusCandidate, GameObject>();

        private const int SECONDS_PER_FLASH = 2; // x2 per cycle

        private Game game;
        private bool spritesOn;
        private bool polesOn;
        private bool flashWarningOn;
        private int flashCountdown = SECONDS_PER_FLASH;
        void Awake()
        {
            game = Game.AwakeInstance();
            game.AddEventConsumer(this);
        }

        
        void Update()
        {
            ISim sim = game.Sim();
            if (sim == null) return;
            
            BonusCandidate[] justSpawnedArray;
            BonusCandidate[] justExpiredArray;
            BonusCandidate[] nowFlashingArray;
            
            lock (this)
            {
                justSpawnedArray = justSpawned.ToArray();
                justSpawned.Clear();
                
                justExpiredArray = justExpired.ToArray();
                justExpired.Clear();

                nowFlashingArray = nowFlashing.ToArray();
                nowFlashing.Clear();
            }

            foreach (BonusCandidate bc in justSpawnedArray)
            {
                Xyzbg loc = bc.Location();
                Vector3 pos = loc.ToVector3(2.0f);

                float rotY = 90f + (float) loc.Bearing().Degrees();
                Quaternion rot = Quaternion.Euler(0, rotY, 90f);
                GameObject bonusGameGO = Instantiate(bonusCellPrefab, pos, rot, transform);
                cellHashGame.Put(bc, bonusGameGO);
                MeshRenderer mr = bonusGameGO.GetComponent<MeshRenderer>();
                mr.material.color = RewardColor(bc.reward);

                rot = Quaternion.identity; // .Euler(0, 0, 0);
                pos = loc.ToVector3(100.0f);
                GameObject bonusMaxiGO = Instantiate(bonusCellPrefab, pos, rot, transform);
                bonusMaxiGO.transform.localScale = new Vector3(10, 100, 10);
                bonusMaxiGO.transform.GetChild(0).localScale = new Vector3(0.3F, 0.005f, 0.3F);
                cellHashMaxi.Put(bc, bonusMaxiGO);
                mr = bonusMaxiGO.GetComponent<MeshRenderer>();
                mr.material.color = RewardColor(bc.reward);
                //mr.enabled = poleCellsOn;
                bonusMaxiGO.SetActive(polesOn);
                
                pos = loc.ToVector3(15.0f);
                rot = Quaternion.Euler(90f, 0, 0);
                GameObject bonusSpriteGO = Instantiate(bonusCellSpritePrefab, pos, rot, transform);
                cellHashMini.Put(bc, bonusSpriteGO);
                SpriteRenderer sr = bonusSpriteGO.GetComponent<SpriteRenderer>();
                Color rc = RewardColor(bc.reward);
                
                sr.color = rc;
                //sr.enabled = poleCellsOn;
                bonusSpriteGO.SetActive(spritesOn);
            }

            foreach (BonusCandidate bc in justExpiredArray)
            {
                GameObject bonusCellGO = cellHashGame.Get(bc);
                Destroy(bonusCellGO);
                cellHashGame.Remove(bc);

                GameObject bonusMaxiGO = cellHashMaxi.Get(bc);
                Destroy(bonusMaxiGO);
                cellHashMaxi.Remove(bc);

                GameObject bonusSpriteGO = cellHashMini.Get(bc);
                Destroy(bonusSpriteGO);
                cellHashMini.Remove(bc);
            }

            double now = sim.TimeNow();
           
            foreach (BonusCandidate bc in nowFlashingArray)
            {
                Color rc = flashWarningOn
                    ? (bc.ShowSecondWarning(now) ? Color.blue : Color.cyan)
                    : RewardColor(bc.reward);

                if (cellHashGame.ContainsKey(bc))
                {
                    GameObject bonusCellGO = cellHashGame.Get(bc);
                    MeshRenderer mr = bonusCellGO.GetComponent<MeshRenderer>();
                    mr.material.color = rc;
                }
                if (cellHashMaxi.ContainsKey(bc))
                {
                    GameObject bonusMaxiGO = cellHashMaxi.Get(bc);
                    MeshRenderer mr = bonusMaxiGO.GetComponent<MeshRenderer>();
                    mr.material.color = rc;
                }
                if (cellHashMini.ContainsKey(bc))
                {
                    GameObject bonusSpriteGO = cellHashMini.Get(bc);
                    SpriteRenderer sr = bonusSpriteGO.GetComponent<SpriteRenderer>();
                    sr.color = rc;
                }
            }

            bool choosingTrip = game.Gsm().CurrentState() == GameState.WaitingForTripChoice;
            // Show battery poles while choosing trip up to level 4; after that you have to remember where they are
            bool polesOnWhileChoosingTrip = choosingTrip && game.Pe().CurrentLevel() <= 4;
            bool polesOnNow = !game.Gih().ShowMiniMap() || polesOnWhileChoosingTrip;
            if (polesOnNow != polesOn)
            {
                polesOn = polesOnNow;
                
                BonusCandidate[] bcax = cellHashMaxi.KeysAsArray(new BonusCandidate[cellHashMaxi.Count]);
                foreach (BonusCandidate bc in bcax)
                {
                    GameObject bonusMaxiGO = cellHashMaxi.Get(bc);
                    bonusMaxiGO.SetActive(polesOn);
                }
            }

            BonusCandidate[] bca = cellHashMini.KeysAsArray(new BonusCandidate[cellHashMini.Count]);
            bool spritesOnNow = game.Gih().ShowMiniMap();
           
            if (spritesOnNow != spritesOn)
            {
                spritesOn = spritesOnNow;
                
                foreach (BonusCandidate bc in bca)
                {
                    GameObject bonusSpriteGO = cellHashMini.Get(bc);
                    bonusSpriteGO.SetActive(spritesOn);
                }
            }

            Text[] texts = {bronzeCount, silverCount, goldCount, blackCount,};
            int nt = texts.Length;
            int[] counts = new int[nt];
            int total = 0;
            foreach (BonusCandidate bc in bca)
            {
                int tier = RewardTier(bc.reward);
                if (tier >= 0 && tier < counts.Length) counts[tier]++;
                total++;
            }

            int remaining = total;
            for (int ti = 0; ti < nt; ti++)
            {
                int n = counts[ti];
                remaining -= n;
                texts[ti].transform.parent.gameObject.SetActive(n > 0  || remaining > 0);
                texts[ti].text = SC.N + n;
            }
        }

        
        public bool DeregisterFireMapChange()
        {
            justSpawned.Clear();
            justExpired.Clear();
            nowFlashing.Clear();
            cellHashGame.Clear();
            cellHashMaxi.Clear();
            cellHashMini.Clear();
            
            return true;
        }

        public void TimeSec()
        {
            if (--flashCountdown <= 0)
            {
                flashCountdown = SECONDS_PER_FLASH;
                flashWarningOn = !flashWarningOn;
                
                double now = game.Sim().TimeNow();
                BonusCandidate[] bca = cellHashMini.KeysAsArray(new BonusCandidate[cellHashMini.Count]);
                foreach (BonusCandidate bc in bca)
                {
                    if (bc.ShowFirstWarning(now))
                    {
                        newlyFlashing(bc);
                    }
                }
            }
        }


        private static Color ColorByRGB(int rgb)
        {
            float r = ((rgb & 0xff0000) >> 16) / 255f;
            float g = ((rgb & 0x00ff00) >>  8) / 255f;
            float b = ( rgb & 0x0000ff)        / 255f;

            return new Color(r, g, b);
        }

        private static Color BRONZE = ColorByRGB(0xE76413); //  new Color(0.906f, 0.391f, 0.073f);
        private static Color SILVER = ColorByRGB(0xB7B7B7);
        private static Color GOLD   = ColorByRGB(0xEFC12A);
        private static Color BLACK  = ColorByRGB(0x24242C);

        public static Color RewardColor(double reward)
        {
            int[] rwa = BonusGenerator.REWARDS;
            if (rwa.Length < 4) return BRONZE;
            
            if (reward <   rwa[0]+1) return BRONZE;
            if (reward <   rwa[1]+1) return SILVER;
            if (reward <   rwa[2]+1) return GOLD;

            return BLACK;
        }
        public static int RewardTier(double reward)
        {
            int[] rwa = BonusGenerator.REWARDS;
            if (rwa.Length < 4) return 0;
            
            if (reward <   rwa[0]+1) return 0;
            if (reward <   rwa[1]+1) return 1;
            if (reward <   rwa[2]+1) return 2;

            return 3;
        }

        private void newlyFlashing(BonusCandidate bc) // Called from TimeSec() in Simulation Thread
        {
            lock (this)
            {
                nowFlashing.Add(bc);
            }
        }
        public void ObjectCreated(object obj) // Called from Simulation Thread
        {
            if (obj is BonusCandidate)
            {
                lock (this)
                {
                    justSpawned.Add((BonusCandidate) obj);
                }
            }
        }

        public void ObjectDeleted(object obj) // Called from Simulation Thread
        {
            if (obj is BonusCandidate)
            {
                lock (this)
                {
                    justExpired.Add((BonusCandidate) obj);
                }
            }
        }

    }
}
