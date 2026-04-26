using System;
using uk.vroad.api;
using uk.vroad.api.etc;
using uk.vroad.api.events;
using uk.vroad.api.geom;
using uk.vroad.api.map;
using uk.vroad.api.str;
using uk.vroad.apk;
using uk.vroad.rvr;
using UnityEngine;
using UnityEngine.UI;

namespace uk.vroad.urvr
{
    public class UTripChooser : MonoBehaviour, LAppState, LTripCandidate
    {
        public UMapMeshRvr mapMesh;
        public GameObject vialIconL;
        public GameObject vialIconM;
        public GameObject vialIconS;

        public GameObject utripCandidatePrefab;
        public Text newDealText;
        public Text tripCountText;
        public Text originText;

        private UTripCandidate[] displayedCandidates;

        private const string KJ_FMT = SC.MI + SC.S + SC.SFD + SC.S + SI.KJ;

        private bool isWaitingForTripChoice;
        private bool newCandidatesWaiting;
        private bool candidateLitChanged;
        private bool candidateChosen;
        
        private GameObject tripUI;
        
        private GameObject tripO;
        private GameObject tripL;
        private GameObject tripM;
        private GameObject tripS;
        
        private RectTransform rectO;
        private RectTransform rectL;
        private RectTransform rectM;
        private RectTransform rectS;
        
        private Image tripSignL;
        private Image tripSignM;
        private Image tripSignS;
       
        private Text tripTextL;
        private Text tripTextM;
        private Text tripTextS;
        
        private AudioSource tripAudio;
        private Game game;
        void Awake()
        {
            game = Game.AwakeInstance();
            game.AddEventConsumer(this);

            GameObject tmpGo = vialIconL.transform.parent.gameObject;
            tripTextL = tmpGo.GetComponent<Text>();
            tmpGo = tmpGo.transform.parent.gameObject;
            tripSignL = tmpGo.GetComponent<Image>();
            tripL = tmpGo.transform.parent.gameObject;
            rectL = tripL.GetComponent<RectTransform>();
            
            tmpGo = vialIconM.transform.parent.gameObject;
            tripTextM = tmpGo.GetComponent<Text>();
            tmpGo = tmpGo.transform.parent.gameObject;
            tripSignM = tmpGo.GetComponent<Image>();
            tripM = tmpGo.transform.parent.gameObject;
            rectM = tripM.GetComponent<RectTransform>();
            
            tmpGo = vialIconS.transform.parent.gameObject;
            tripTextS = tmpGo.GetComponent<Text>();
            tmpGo = tmpGo.transform.parent.gameObject;
            tripSignS = tmpGo.GetComponent<Image>();
            tripS = tmpGo.transform.parent.gameObject;
            rectS = tripS.GetComponent<RectTransform>();

            tripUI = tripS.transform.parent.gameObject;

            tripO = originText.transform.parent.parent.gameObject;
            rectO = tripO.GetComponent<RectTransform>();
            
            tripAudio = tripUI.GetComponent<AudioSource>();

            tripO.SetActive(false);
            tripL.SetActive(false);
            tripM.SetActive(false);
            tripS.SetActive(false);

        }

        public bool DeregisterFireMapChange() { return true; }
        
        public void AppStateChanged(AppStateTransition ast)
        {
            if (ast.after == GameState.WaitingForTripChoice)
            {
                game.Ptc().SetListener(this);
                isWaitingForTripChoice = true;
            }
            else
            {
                isWaitingForTripChoice = false;
            }
        }

        void Update()
        {
            if (candidateChosen)
            {
                candidateChosen = false;
                DestroyCandidates();
                if (tripAudio.isPlaying) tripAudio.Pause();

                UGamePadRvr.RumbleCancel();
            }
            else if (!tripUI.activeSelf)
            {
            }
            else if (isWaitingForTripChoice)
            {
                PlayerTripChooser ptc = game.Ptc();

                float cmax = ptc.CountdownInitial();
                float count = (float) ptc.Countdown();

                if (cmax > 0)
                {
                    float vol = count < 0.5 || count > cmax ? 0 : (cmax - count) / cmax;
                    vol *= UAudioController.MasterVolume();
                    tripAudio.volume = vol;
                    //tripCountText.text = KFormat.Sprintf(SC.SFZ52F, count);
                    tripCountText.text = (new TimeHMS(count, TimeHMS.MMSS)).ToString(false);
                    tripCountText.gameObject.transform.parent.gameObject.SetActive(true);

                    if (!tripAudio.isPlaying) tripAudio.Play();
                }
                else
                {
                    tripCountText.text = SC.N;
                    tripCountText.gameObject.transform.parent.gameObject.SetActive(false);
                }
                
                if (newCandidatesWaiting)
                {
                    newCandidatesWaiting = false;

                    DestroyCandidates();

                    TripCandidate[] tca = ptc.Candidates();
                    int ntc = tca.Length;
                    displayedCandidates = new UTripCandidate[ntc];

                    for (int ci = 0; ci < ntc; ci++)
                    {
                        TripCandidate tc = tca[ci];
                        GameObject utcGO = Instantiate(utripCandidatePrefab, transform);
                        UTripCandidate utc = utcGO.GetComponent<UTripCandidate>();
                        displayedCandidates[ci] = utc;
                        utc.Build(game, tc, ci);
                    }

                    double mapAspect = game.Map().GetWidth() / game.Map().GetHeight();
                    if (ntc > 0)
                    {
                        tripO.SetActive(true);
                        rectO.anchorMax = rectO.anchorMin = ClampedLocation(mapAspect, tca[0].OriginLocationRelative());
                        originText.text = originText.text = tca[0].Origin().Description();
                    }
                    else tripO.SetActive(false);

                    
                    tripL.SetActive(ntc > 0);
                    tripM.SetActive(ntc > 1);
                    tripS.SetActive(ntc > 2);

                    if (ntc > 0) rectL.anchorMax = rectL.anchorMin = ClampedLocation(mapAspect,tca[0].DestinationLocationRelative());
                    if (ntc > 1) rectM.anchorMax = rectM.anchorMin = ClampedLocation(mapAspect,tca[1].DestinationLocationRelative());
                    if (ntc > 2) rectS.anchorMax = rectS.anchorMin = ClampedLocation(mapAspect,tca[2].DestinationLocationRelative());
                    
                    IPedZone vialZone = game.Vg().VialZone();

                    bool showVialL = ntc > 0 && vialZone == tca[0].Destination();
                    bool showVialM = ntc > 1 && vialZone == tca[1].Destination();
                    bool showVialR = ntc > 2 && vialZone == tca[2].Destination();
                    vialIconL.SetActive(showVialL);
                    vialIconM.SetActive(showVialM);
                    vialIconS.SetActive(showVialR);
                    bool vialShownOnTrip = showVialL || showVialM || showVialR;
                    mapMesh.ShowVialOnMap( ! vialShownOnTrip);
                    
                    tripTextL.text = ntc > 0? TripCandidateLabel(tca[0]): SC.N;
                    tripTextM.text = ntc > 1? TripCandidateLabel(tca[1]): SC.N;
                    tripTextS.text = ntc > 2? TripCandidateLabel(tca[2]): SC.N;

                    newDealText.text = KFormat.Sprintf(KJ_FMT, ptc.NewDealCharge());
                    
                    candidateLitChanged = true;

                    
                    // UGamePad.Rumble(50, 0, 50);
                }


                if (candidateLitChanged)
                {
                    candidateLitChanged = false;
                    if (displayedCandidates == null) return;

                    TripCandidate[] tca = ptc.Candidates();
                    int ntc = tca.Length;
                    TripCandidate litCandidate = ptc.CandidateLit();

                    // Trips retain their colour when selection is changed
                    // When unlit:
                    //   - the left (medium) trip has ci==0 and uses material -1 (Prev) mauve
                    //   - the central (long) trip has ci==1 and uses material 3 (Other) grey
                    //   - the right (short) trip has ci==2 and uses material 1 (Next) green
                    //
                    // The lit trip uses material 0 (Lit) gold
                    
                    for (int ci = 0; ci < displayedCandidates.Length; ci++)
                    {
                        UTripCandidate utc = displayedCandidates[ci];

                        bool lit = utc.TripCandidate() == litCandidate;
                        int matdci = lit ? 0 : 3;  // dci == 0 => auto (gold)     dci == 3 => other/grey
                        utc.CreateMesh(lit, matdci);

                        int bi = ntc == 1 ? 1 : ci;

                        Image sign = bi == 0 ? tripSignL : bi == 1 ? tripSignM : tripSignS;

                        Color optColor = UPlayerRoute.OptionMaterialStatic(matdci).color;
                        sign.GetComponent<Image>().color = optColor;

                        //Text text = bi == 0 ? tripTextL : bi == 1 ? tripTextM : tripTextS;
                        //text.color = Color.black;
                    }
                }
            }
            else // game state might be paused
            {
                if (tripAudio.isPlaying) tripAudio.Pause(); 

            }
        }

        private Vector2 ClampedLocation(double mapAspect, Xy relativeLocationOnMap)
        {
            double sx = Screen.width;
            double sy = Screen.height;
            double sa = sx / sy;

            double wMax = mapAspect > sa ? 1.0 : mapAspect / sa;
            double hMax = mapAspect > sa ? sa / mapAspect : 1.0;
           
            // translate relative location on map to relative location on screen, centering
            double rsx = (relativeLocationOnMap.X() * wMax) + (0.5 * (1 - wMax));
            double rsy = (relativeLocationOnMap.Y() * hMax) + (0.5 * (1 - hMax));
            
            double rx = Math.Min(Math.Max(100.0 / sx, rsx), 1.0 - (80.0 / sx));
            double ry = Math.Min(Math.Max(100.0 / sy, rsy), 1.0 - (50.0 / sy));

            return new Vector2((float) rx, (float)ry);
        }
        private string TripCandidateLabel(TripCandidate tc)
        {
            string desc = tc.Destination().Description();
            string[] lines = PlayerRouteBuilder.SplitLongLine(desc);

            string edgeStr = tc.Destination().IsOnEdge() ? SC.S+SC.RT_ARROW + SC.PP : SC.N;

            desc = lines[0] + edgeStr + SC.NL;
            if (lines.Length >= 2) desc += lines[1] + SC.NL;

            string rs = SC.N + CC.PLUS + tc.Reward() + SI.KJ;
            return desc + rs;
        }
       

        // If there was a previously selected candidate it may still be visible, destroy it here
        private void DestroyCandidates()
        {
            if (displayedCandidates == null) return;

            foreach (UTripCandidate utc in displayedCandidates)
            {
                Destroy(utc.gameObject);
            }

            displayedCandidates = null;
        }

        public void RibbonsEnabled(bool v)
        {
            if (displayedCandidates == null) return;

            foreach (UTripCandidate utc in displayedCandidates)
            {
                utc.gameObject.SetActive(v);
            }

        }
        public void NewCandidatesAvailable()
        {
            newCandidatesWaiting = true;
            candidateChosen = false;
        }
        public void CandidateChosen()
        {
            candidateChosen = true;
        }

        public void CandidateLitChanged()
        {
            candidateLitChanged = true;
        }

        public void NewDealPriceChanged(int newPrice)
        {
            newDealText.text = KFormat.Sprintf(KJ_FMT, newPrice);
        }
    }
}
