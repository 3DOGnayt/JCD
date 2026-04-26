using uk.vroad.api.map;
using uk.vroad.api.route;
using uk.vroad.api.str;
using uk.vroad.apk;
using uk.vroad.rvr;
using UnityEngine;
using UnityEngine.UI;

namespace uk.vroad.urvr
{
    public class UPlayerRoute : UPlayerRouteBase
    {
        public URouteOption optionL;
        public URouteOption optionU;
        public URouteOption optionR;
        public URouteChoice urChoice;
        public URouteCurrent urCurrent;

        public Text manRouteText;
        public Text textOptCount;
        
        public Material materialExtraOptions;

        public Sprite streetSignSprite;
        public Sprite crossingSprite;
        
        public const float MAIN_ROUTE_KW = 0.4f;
        public const float MAIN_ROUTE_MW = 0.6f;
        public const float MAIN_ROUTE_ELEV = 0.1f;

        public const float MAIN_ROUTE_KWL = 0.1f;
        public const float MAIN_ROUTE_MWL = 0.3f;
        public const float MAIN_ROUTE_KWR = 0.7f;
        public const float MAIN_ROUTE_MWR = 0.9f;

        private static UPlayerRoute _instance;
    
        private GameObject goManRouteMiddle;
        private RectTransform manRouteRect;
        
        private bool newSec;
        
        // The number of materials must be the same as the number of meshes
        private Material[] optionMaterials;
        protected override void Awake()
        {
            base.Awake();

            _instance = this;
            
            goManRouteMiddle = manRouteText.gameObject.transform.parent.gameObject;
            GameObject goManRoute = goManRouteMiddle.transform.parent.gameObject;
            manRouteRect = goManRoute.GetComponent<RectTransform>();

            MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
            optionMaterials = mr.materials;
        }

        public override void TimeSec()
        {
            newSec = true; // here we are in wrong thread to change text
        }

        protected override bool IsMini() { return false; }
        protected override float RouteWidthK() { return MAIN_ROUTE_KW; }
        protected override float RouteWidthM() { return MAIN_ROUTE_MW; }
        protected override float RouteElevation() { return MAIN_ROUTE_ELEV; }
        protected override float OptionElevation() { return 0; }

        protected override float RouteWidthOptK(int opti, int nopt)
        {
            if (nopt < 2) return MAIN_ROUTE_KWL;
            
            float p = opti / (float) (nopt - 1);

            if (game.Map().DriveOnRight()) p = 1f - p;
            
            return MAIN_ROUTE_KWL + (p * (MAIN_ROUTE_KWR - MAIN_ROUTE_KWL));
        }

        protected override float RouteWidthOptM(int opti, int nopt)
        {
            if (nopt < 2) return MAIN_ROUTE_MWR;
            
            float p = opti / (float) (nopt - 1);
            if (game.Map().DriveOnRight()) p = 1f - p;
            
            return MAIN_ROUTE_MWL + (p * (MAIN_ROUTE_MWR - MAIN_ROUTE_MWL));
        }

        protected override void UpdateOnRouteChanged()
        {
            bool forTaxi = prb.ShowTaxiRoute();
            IBranch playerBranch = prb.CachedPlayerBranch(forTaxi);
            IBranch[] mba = prb.ManRouteBranches(forTaxi);
            IBranch[] aba = prb.AutoRouteBranches(forTaxi);
            int nm = mba.Length;
            int na = aba.Length;
            IBranch auto0 = na == 0 ? null : aba[0];
            int aoi = prb.OptionIndex(forTaxi, auto0);
            IBranch choiceBranch = nm > 0 ? mba[nm - 1] : playerBranch;
            IBranch[] avOpts = prb.AvailableOptions(forTaxi);
            int nAvOpt = avOpts.Length;


            if (prb.ButtonEnabledForFn(GameDigitalFn.RouteLeft))
            {
                IBranch branchL = prb.OptionOnButton(GameDigitalFn.RouteLeft);
                ShowRouteOption(optionL, forTaxi, branchL, aoi);
            }
            else HideRouteOption(optionL);

            if (prb.ButtonEnabledForFn(GameDigitalFn.RouteForward))
            {
                IBranch branchU = prb.OptionOnButton(GameDigitalFn.RouteForward);
                ShowRouteOption(optionU, forTaxi, branchU, aoi);
            }
            else HideRouteOption(optionU);

            if (prb.ButtonEnabledForFn(GameDigitalFn.RouteRight))
            {
                IBranch branchR = prb.OptionOnButton(GameDigitalFn.RouteRight);
                ShowRouteOption(optionR, forTaxi, branchR, aoi);
            }
            else HideRouteOption(optionR);

            textOptCount.text = SC.N + (1 + aoi) + SC.S + CC.SLASH + SC.S + nAvOpt;
            Color manColor = optionMaterials[matManual].color;

            if (choiceBranch != null) // choice branch == player branch if no manual route
            {
                ShowRouteImageLabel(urChoice, choiceBranch);

                urChoice.label.color = nm > 0 ? manColor : Color.black;
            }
            else HideRouteImageLabel(urChoice);

            urChoice.button.gameObject.SetActive(prb.PruneAvailable());

            if (nm >= 2)
            {
                goManRouteMiddle.SetActive(true);

                bool ignoreManFirst = mba[0] == playerBranch;
                int manFirst = ignoreManFirst ? 1 : 0;
                int nLines = nm - 1 - manFirst;
                float[] bgSize = {40, 24,}; // cannot use Vector2, it is a struct, passed by value
                float cw = 10f; // character width

                string ms = SC.N;
                if (nLines > 5)
                {
                    ms += CountChars(prb.DescribeBranchFull(mba[nm - 2]), cw, bgSize) + SC.NL;
                    ms += CountChars(prb.DescribeBranchFull(mba[nm - 3]), cw, bgSize) + SC.NL;
                    ms += KFormat.Sprintf(SC.MAN_MIDDLE, nLines - 4) + SC.NL;
                    ms += CountChars(prb.DescribeBranchFull(mba[manFirst + 1]), cw, bgSize) + SC.NL;
                    ms += CountChars(prb.DescribeBranchFull(mba[manFirst]), cw, bgSize);

                    nLines = 5;
                }
                else
                {
                    for (int mi = nm - 2; mi >= manFirst; mi--)
                    {
                        ms += CountChars(prb.DescribeBranchFull(mba[mi]), cw, bgSize);
                        if (mi > manFirst) ms += SC.NL;
                    }
                }

                bgSize[1] = 8 + (nLines * 16);
                manRouteRect.sizeDelta = new Vector2(bgSize[0], bgSize[1]);

                manRouteText.text = ms;
                manRouteText.color = manColor;
            }

            else
            {
                goManRouteMiddle.SetActive(false);
                manRouteRect.sizeDelta = new Vector2(160, 0);
            }

            if (playerBranch != null && playerBranch != choiceBranch)
            {
                ShowRouteImageLabel(urCurrent, playerBranch);
            }
            else HideRouteImageLabel(urCurrent);
        }

        private void ShowRouteOption(URouteOption uro, bool forTaxi, IBranch branch, int aoi)
        {
            int boi = prb.OptionIndex(forTaxi, branch);
            int dci = boi - aoi;

            uro.button.gameObject.SetActive(true);
            uro.button.interactable = true;

            ShowRouteImageLabel(uro, branch);
            
            uro.busIconImage.gameObject.SetActive(branch is IBranchBusBoarding || branch is IBranchBusOnboard || branch is IBranchBusArrival);
            uro.taxiIconImage.gameObject.SetActive(branch is IBranchTaxiOrigin || branch is IBranchTaxiDestination);

            Material mat = OptionMaterial(dci);
            if (dci != 0 && branch is IBranchTaxiOrigin) mat = optionMaterials[6];  // taxi colour if option (not auto)
            if (dci != 0 && branch is IBranchBusBoarding) mat = optionMaterials[7]; // bus colour if option (not auto)
            
            Color optionColor = mat.color;
            uro.buttonImage.color = optionColor;
            if (uro.busIconImage.gameObject.activeSelf) uro.busIconImage.color = optionColor;
            if (uro.taxiIconImage.gameObject.activeSelf) uro.taxiIconImage.color = optionColor;
        }

        private static readonly Color SIGN_COLOUR_ROAD = new Color(0, 0.6f, 1, 0.4f);
        private static readonly Color SIGN_COLOUR_WALK = new Color(1, 1, 1, 0.4f);

        private void ShowRouteImageLabel(URouteCurrent urc, IBranch branch)
        {
            bool isCrossing = branch is IWalkBranch xwd && xwd.GetWalkway() is ICrossing;
            urc.wayImage.sprite = isCrossing ? crossingSprite : streetSignSprite;
            urc.wayImage.color = branch is IRoad ? SIGN_COLOUR_ROAD : SIGN_COLOUR_WALK;
            urc.wayImage.gameObject.SetActive(true);
            
            //bool isStreet = !isCrossing && (branch is Road || branch is WalkBranch);
            //urc.crossingImage.gameObject.SetActive(isCrossing);
            //urc.streetSignImage.gameObject.SetActive(isStreet));
            
            urc.label.text = prb.DescribeBranch(branch); // text color always black
        }
        private void HideRouteOption(URouteOption uro)
        {
            
            HideRouteImageLabel(uro);
            uro.button.gameObject.SetActive(false);
            uro.busIconImage.gameObject.SetActive(false);
            uro.taxiIconImage.gameObject.SetActive(false);
        }

        private void HideRouteImageLabel(URouteCurrent urc)
        {
            urc.wayImage.gameObject.SetActive(false);
            //urc.crossingImage.gameObject.SetActive(false);
            //urc.streetSignImage.gameObject.SetActive(false);
            urc.label.text = SC.N;
        }
        private string CountChars(string s, float cw, float[] size)
        {
            float widthRequired = s.Length * cw;
            if (widthRequired > size[0]) size[0] = widthRequired;
            return s;
        }
        private bool HasVariableLabel(IBranch b)
        {
            if (b is IBranchBusBoarding) return true; // countdown time
            if (b is IBranchBusOnboard) return true;// countdown time
            if (b is IBranchTaxiOriginLane) return true;// countdown time
            if (b is IRoad)  return true; // compass bearing
            if (b is IWalkBranch)  return true; // compass bearing
            
            return false;
        }
        public void FixedUpdate()
        {
            if (newSec)
            {
                newSec = false;
                
                // This updates the 'countdown' part of bus/taxi arrival route/option labels 

                if (prb.ButtonEnabledForFn(GameDigitalFn.RouteLeft))
                {
                    IBranch optL = prb.OptionOnButton(GameDigitalFn.RouteLeft);
                    if (HasVariableLabel(optL)) optionL.label.text = prb.DescribeBranch(optL);
                }

                if (prb.ButtonEnabledForFn(GameDigitalFn.RouteForward))
                {
                    IBranch optU = prb.OptionOnButton(GameDigitalFn.RouteForward);
                    if (HasVariableLabel(optU)) optionU.label.text = prb.DescribeBranch(optU);
                }

                if (prb.ButtonEnabledForFn(GameDigitalFn.RouteRight))
                {
                    IBranch optR = prb.OptionOnButton(GameDigitalFn.RouteRight);
                    if (HasVariableLabel(optR)) optionR.label.text = prb.DescribeBranch(optR);
                }

                bool forTaxi = prb.ShowTaxiRoute();
                IBranch chBr = prb.PreOptionBranch(forTaxi);
                if (HasVariableLabel(chBr))  urChoice.label.text = prb.DescribeBranch(chBr);

                IBranch plBr = prb.CachedPlayerBranch(forTaxi);
                if (plBr != chBr && HasVariableLabel(plBr))  urCurrent.label.text = prb.DescribeBranch(plBr);

            }
        }

        // Given the difference between the lit option index, return a material
        public Material OptionMaterial(int dci)
        {
            switch (dci)
            {
                case 0:  return optionMaterials[1];  // lit option = auto
                case -1: return optionMaterials[2];  // morePrev 
                case +1: return optionMaterials[3];  // moreNext 
                case -2: return optionMaterials[4];  // morePrev2
                case +2: return optionMaterials[5];  // moreNext2

                default: return materialExtraOptions;  // others (grey?)
            }
        }

        public static Material OptionMaterialStatic(int dci)
        {
            return _instance.OptionMaterial(dci);
        }
    }




}
