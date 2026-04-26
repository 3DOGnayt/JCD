using System;
using uk.vroad.api;
using uk.vroad.api.enums;
using uk.vroad.api.geom;
using uk.vroad.api.input;
using uk.vroad.api.str;
using uk.vroad.apk;
using uk.vroad.rvr;
using uk.vroad.ucm;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace uk.vroad.urvr
{
    public class UCamControllerMainRvr : UaCamControllerMain, LPlayerRoute, LAppView
    {
        public GameObject cameraPlayerIcon;
        public GameObject cameraRouteIcon;
        public GameObject mapRouteIcon;
        public GameObject mapTaxiIcon;
        public GameObject mapBusIcon;
        public GameObject mapCellsIcon;

        public Text mapCostText;

        
        private const float FOCUS_HEIGHT = 1.8f;
        private const float MIN_CAMERA_HEIGHT_ABOARD = 3.5f;
        private const float MIN_CAMERA_HEIGHT_ROUTE = 30f;
        private const float MIN_TETHER_DISTANCE = 4f; 
        private const float MAX_TETHER_DISTANCE = 400f; 
        private const float ROUTE_DOLLY_RATIO = 0.5f;
        private const float DOWN_TO_TETHER_SPEED = 0.96f; // was 0.98
        /** The speed at which the height increases when resetting view to see whole model */
        private const float RECENTRE_HEIGHT_FACTOR = 1.1f; 
        /** The speed at which the focus pans back to the centre when resetting view to see whole model */
        private const float RECENTRE_FOCUS_FACTOR = 0.1f; 
        /** When moving to model centre, the height fraction at which the view starts to rotate to north */
        private const float RECENTRE_HEIGHT_THRESHOLD = 0.96f;
        private const float MIN_DOLLY_RATIO = 0.01f;
        private const float MAX_DOLLY_RATIO = 4f;
        private const float MAX_DOLLY_DISTANCE = 800f; // for clipping plane
        private const float SPIN_HT = 0.1f;
        private const float ROUTE_FOCUS_SLOW = 0.02f;
        private const float ROUTE_FOCUS_FAST = 0.05f;

        private Game game;
        private PlayerRouteBuilder prb;
        private GameInputHandler gih;
        private bool playerAboard;
        private float dolly;
        private float spin;

        private float spinP; // This stops jitter on spin, by applying it only when player moves
        private float dollyRatio = MIN_DOLLY_RATIO;
        private float dollySpin;
        private bool spunToNorth = true;
        private bool viewChanged;  // to access text widgets in correct thread

        private Vector3 routeFocus = Vector3.zero;
        private Vector3 routeFocusNew = Vector3.zero;
        private Angle routeBearing = Angle.A0;
        private Vector3 playerFocus = Vector3.zero;
        private bool playerIsActive;
        private Angle playerBearing= Angle.A0;
        private float playerSpeed;
        private Angle rotApplied = Angle.A0;

        protected override void Awake()
        {
            game = Game.AwakeInstance();
            base.Awake();
         
            prb = game.Prb();
            gih = game.Gih();
        }
        void Start()
        {
            SetButtonText();
        }


        protected override App App() { return game; }

        private void SetButtonText()
        {
            CameraState cs = game.Gih().GetCameraState();
            MapState ms = game.Gih().GetMapState();
            
            cameraPlayerIcon.SetActive(cs == CameraState.Tether);
            cameraRouteIcon.SetActive(cs == CameraState.Route);

            mapRouteIcon.SetActive(ms != MapState.None);
            mapCellsIcon.SetActive(ms != MapState.None);
            mapTaxiIcon.SetActive(ms == MapState.XRay);
            mapBusIcon.SetActive(ms == MapState.XRay);

            mapCostText.text = SC.N + game.Pe().MapCostPowerW() + SC.S + SI.W;

        }
        public void FireRouteChanged()
        {
            Xyzbg pos = prb.RouteChoicePosition();
            routeFocusNew = pos.ToVector3();
            routeBearing = pos.Bearing();
            
            if (game.Gih().GetCameraState() != CameraState.Dolly) dollySpin = 0;
        }

        private bool IsRouteFocusFast()
        {
            return game.Gsc().IsFFwd();
        }
       
        public void MapCycle()
        {
            game.Gih().MapCycle();
        }

        public virtual void CameraCycle()
        {
            game.Gih().CameraCycle();
        }

        public Angle RotationApplied() { return rotApplied; }

        protected override void SetCameraHeight()
        {
            base.SetCameraHeight();
            
            float minH = gih.FocusOnRoute()? MIN_CAMERA_HEIGHT_ROUTE:
                playerAboard? MIN_CAMERA_HEIGHT_ABOARD: MIN_CAMERA_HEIGHT;
                    
            if (zoom > 0.1 || zoom < -0.1)
            {
                gih.ResetNewlyTethered();
                
            }
                    
            else if (gih.IsNewlyTethered())
            {
                cameraHeight = Mathf.Max(minH, cameraHeight * DOWN_TO_TETHER_SPEED); // move camera slowly down
                if (cameraHeight < minH + 0.1) gih.ResetNewlyTethered();
            }
        }

        public override bool AppInputAnalogEvent(AppAnalogFn afn, double value)
        {
            if (afn == AppAnalogFn.Zoom)
            {
                zoom = (float) value; 

                return true;
            }

            if (afn == AppAnalogFn.Rotate)
            {
                spin = (float) value;

                return true;
            }
            
            if (gih.IsCameraTethered())
            {
                return false;
            }
            
            if (afn == AppAnalogFn.Tilt)
            {
                dolly = (float) value; 
                return true;
            }

            return false;
        }

        protected override void Update()
        {
            if (!mapLoaded) return;
            
            base.Update();
            
            if (viewChanged)
            {
                viewChanged = false;
                SetButtonText();
            }

            
            UCamControllerMiniRvr ccMini = UCamControllerMiniRvr.Instance;
            bool showMiniMap = gih.ShowMiniMap(); 
            
            if (showMiniMap != ccMini.gameObject.activeSelf)
            {
                if (showMiniMap && ccMini.CalcMapSize()) ccMini.gameObject.SetActive(true);

                if (!showMiniMap) ccMini.gameObject.SetActive(false);
            }
        }
        
        
        public override void PlayerArrived()
        {
            playerIsActive = false;
        }

        public override void PlayerPosition(Vector3 pos, Angle bearing, double speed, bool aboard)
        {
            playerFocus = pos + (Vector3.up * FOCUS_HEIGHT);
            playerBearing = bearing.RangeN180();
            playerSpeed = (float) speed;
            playerAboard = aboard;
            spinP = spin;

            // Automatically re-tether to new player
            // if (!playerIsActive && !gsc.IsCameraTethered()) gsc.SetCameraState(CameraState.Tether);

            playerIsActive = true;
            
        }

        protected override void SetRotation(Angle a)
        {
            // ignore external rotation
        }

        protected override void LateUpdate()
        {
            if (!mapLoaded) return;

            base.LateUpdate();
            
            if (playerIsActive)
            {
                if (gih.IsCameraTethered())
                {
                    TetherCameraMove();
                }
                else 
                {
                    DollyCameraMove();
                }
            }
            else // There is no player: move to trip-choice position
            {
                SlowlyMoveToModelCentre();
            }

            gih.SetCameraHeight(cameraHeight);
        }

        public void FireViewChanged()
        {
            viewChanged = true;
        }

        private void PositionCameraOnTether(Vector3 tetherRope, float spin)
        {
            tetherRope.y = 0;
            tetherRope.Normalize();

            // If player is stopped, tether rope can be zero here, having been straight above player (0, Y, 0)
            if (tetherRope.magnitude < 0.01)  tetherRope = - playerBearing.UnitVectorXY().ToVector3();
            
            float tetherDistance = TetherDistance(cameraHeight);
            
            if (spin > 0.1 || spin < -0.1)
            {
                float spinF = spin * (float) Math.Sqrt(SPIN_HT / cameraHeight);
                Vector3 sideways = spinF * (new Vector3(tetherRope.z, 0f, -tetherRope.x));
                tetherRope += sideways;
                
                // Compensate for forward movement of player by also moving camera forward when spinning
                // This compensation is greatest when tether distance is shortest
                float pfd = playerSpeed * (float) game.Sim().TimeStep();
                Vector3 fwd = playerBearing.UnitVectorXY().ToVector3() * pfd / tetherDistance;
                tetherRope += fwd;
                
                tetherRope.Normalize();
            }

            transform.position = cameraFocus + tetherRope * tetherDistance + Vector3.up * (cameraHeight - FOCUS_HEIGHT);
            transform.LookAt(cameraFocus);

            dollyRatio = tetherDistance / cameraHeight;
            if (dollyRatio < MIN_DOLLY_RATIO) dollyRatio = MIN_DOLLY_RATIO;
        }

        private float TetherDistance(float height)
        {
            float minT = MIN_TETHER_DISTANCE;
            float rangeT = MAX_TETHER_DISTANCE - minT;

            float minH = MIN_CAMERA_HEIGHT;
            float maxH = cameraHeightSeeWholeModel;
            float rangeH = maxH - minH;

            float qH = Math.Min(Math.Max(0, (maxH - height) / rangeH), 1);
            qH = (float) Math.Pow(qH, 2);
            float qT = (float) Math.Sin(qH * Math.PI);

            return minT + (qT * rangeT);
        }

        private void SlowlyMoveToModelCentre()
        {
            // slowly move camera up to height at which whole model is visible
            cameraHeight = Mathf.Min(cameraHeightSeeWholeModel, cameraHeight * RECENTRE_HEIGHT_FACTOR);
            playerBearing = Angle.A0;
            
            // Three stages
            // - move camera up
            // - rotate camera to north
            // - pan camera to centre of map

            Vector3 tetherRope;
            if (cameraHeight < RECENTRE_HEIGHT_THRESHOLD * cameraHeightSeeWholeModel)
            {
                // stage 1, while moving up, retain player focus, and bearing
                spunToNorth = false;
                cameraFocus = playerFocus;
                tetherRope = transform.position - cameraFocus;
            }
            else if (!spunToNorth)
            {
                Angle newRot = Angle.A0;
                rotApplied = SmoothTransition(rotApplied, newRot, 1.0f);
                
                // stage 2: spin camera back to north

                cameraFocus = playerFocus;
                tetherRope = rotApplied.UnitVectorXY().Negative().ToVector3();

                spunToNorth = rotApplied.Degrees() < 5f;
            }
            else
            {
                // Stage 3: once camera is almost at north, move position to map centre
                cameraFocus += RECENTRE_FOCUS_FACTOR * (mapCentre - cameraFocus);

                tetherRope = new Vector3(0, 0, -1);
            }



            PositionCameraOnTether(tetherRope, 0);
        }

       
        
        private void TetherCameraMove()
        {
            SetCameraHeight();

            // This is for tether only: moving up beyond map height switches map on
            // if (!gsc.MapOn() && zoom < -0.1 && cameraHeight > MIN_CAMERA_HEIGHT_WITH_MAP * 1.2f) gsc.MapCycle();
            
            dollySpin = 0;
            cameraFocus = playerFocus; 
            routeFocus = playerFocus;
            Vector3 tetherRope = transform.position - cameraFocus;

            PositionCameraOnTether(tetherRope, spinP);
            spinP = 0;
            rotApplied = tetherRope.ToXyz().Negative().AsBearing();
        }


        protected void DollyCameraMove()
        {
            SetCameraHeight();

            Angle newRot;
            float dollyDistance;
            
            if (gih.FocusOnRoute())
            {
                if (routeFocus.sqrMagnitude < 1) routeFocus = playerFocus;
                if (routeFocusNew.sqrMagnitude < 1) routeFocus = playerFocus;
                else routeFocus += (routeFocusNew - routeFocus) * (IsRouteFocusFast() ? ROUTE_FOCUS_FAST : ROUTE_FOCUS_SLOW);
                
                cameraFocus = routeFocus + Vector3.up * MIN_CAMERA_HEIGHT;
                
                dollySpin += 2f * spinP;
                newRot = new AngleDeg(routeBearing.Degrees() + dollySpin);

                dollyDistance = cameraHeight * ROUTE_DOLLY_RATIO;
            }
            else if (gih.FaceCameraNorth())
            {
                routeFocus = playerFocus;
                cameraFocus = playerFocus;
                newRot = Angle.A0;
                dollyDistance = 1.0f; 
            }
            else
            {
                dollySpin += 2f * spinP;
                
                // newRot = new AngleDeg(playerBearing.Degrees() + dollySpin);
                newRot = new AngleDeg(dollySpin);

                cameraFocus = playerFocus;
                routeFocus = playerFocus;
                
                dollyDistance = (dollyRatio * cameraHeight) - dolly;

                if (dollyDistance > MAX_DOLLY_DISTANCE) dollyDistance = MAX_DOLLY_DISTANCE;

                dollyRatio = dollyDistance / cameraHeight;
                if (dollyRatio < MIN_DOLLY_RATIO) dollyRatio = MIN_DOLLY_RATIO;
                else if (dollyRatio > MAX_DOLLY_RATIO) dollyRatio = MAX_DOLLY_RATIO;


                dollyDistance = dollyRatio * cameraHeight;
            }

            spinP = 0;

            rotApplied = SmoothTransition(rotApplied, newRot, 0.1f);
            
            // Forward vector based on current angle of rotation
            Vector3 forward = rotApplied.UnitVectorXY().ToVector3();
            //Vector3 right = XV.ToVector(rotApplied.Plus(Angle.A90).UnitVectorXY());

            transform.position = cameraFocus + Vector3.up * (cameraHeight - FOCUS_HEIGHT) - dollyDistance * forward;
            transform.LookAt(cameraFocus);
        }

        //////////////////////////////////////////////////////////////////////////////////////
       
        private Angle SmoothTransition(Angle from, Angle to, float rotMult)
        {
            Angle diff = to.Minus(from).RangeN180();

            float diffCap = Time.deltaTime * rotMult * (float) diff.Degrees();
            return (new AngleDeg(from.Degrees() + diffCap)).RangeN180();
        }
        
        public string metrics()
        {
            return KFormat.Sprintf(SU.METRICS_01 + SU.METRICS_02 + SU.METRICS_03 ,
                cameraHeight, TetherDistance(cameraHeight), dollyRatio,
                zoom, spin, dolly,
                playerSpeed, playerBearing, rotApplied);
                
      
        }

    }
    
    
}
