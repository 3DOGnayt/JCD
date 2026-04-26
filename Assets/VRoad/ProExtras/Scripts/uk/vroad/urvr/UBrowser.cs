using uk.vroad.api.input;
using UnityEngine;

#if VROAD_RVR_RELEASE
using System;
using UnityEngine.InputSystem;
using ZenFulcrum.EmbeddedBrowser;
using uk.vroad.api.str;
using uk.vroad.rvr;
#endif

namespace uk.vroad.urvr
{
    public class UBrowser : MonoBehaviour, LAppInput
    {
        public bool DeregisterFireMapChange() { return true; }

#if VROAD_RVR_RELEASE
        private Browser browser;
        private PointerUIGUI pointerUI;
        private KeyCode repeatedKeyCode = KeyCode.None;
        private KeyCode fixedUpdateKeyCode = KeyCode.None;

        private Vector2 padMouse;
        private Vector2 padMouseDelta;
        private Vector2 padScroll; // Only Y is used see https://docs.unity3d.com/ScriptReference/Input-mouseScrollDelta.html
        private bool padPanningX;
        private bool padPanningY;
        private bool wasPanning;
        
        void Awake()
        {
            Game game = Game.AwakeInstance();
            game.AddEventConsumer(this);
            browser = GetComponent<Browser>();
            pointerUI = GetComponent<PointerUIGUI>();

            browser.RegisterFunction(SG.buildClicked, args => BrowserCallback(SC.N+args[0]));
           
            pointerUI.onHandlePointers += HandleGamepadAsPointer;
            // pointerUI.InputSettings.scrollSpeed = 10; // not useful
        }

        public static void BrowserCallback(string latLong)
        {
            UChooseQuarter.InstanceQ().BuildNewModel(latLong);
        }
        private void HandleGamepadAsPointer()
        {
            bool padPanning = padPanningX || padPanningY;
            if (padPanning && !wasPanning)
            {
                padMouse = Input.mousePosition;
            }
           
            wasPanning = padPanning;

            bool button1 = padPanning;
            bool button2 = padScroll.y != 0;
            bool button3 = repeatedKeyCode != KeyCode.None;
            
            float mouseSpeed = 0.5f;
            
            Vector2 mousePos = (padMouse - (padMouseDelta * mouseSpeed));
            //Mouse.current.WarpCursorPosition(mousePos); 

            pointerUI.FeedPointerState(new PointerUIBase.PointerState
            {
                id = 1001,
                is2D = true,
                position2D = mousePos,
                activeButtons = button1 ? MouseButton.Left : button2 ? MouseButton.Middle : button3? MouseButton.Right: 0,
                scrollDelta = padScroll,
            });
            padMouse = mousePos;
           
        }

        void Start()
        {
            RectTransform rTransform = gameObject.GetComponentInParent<RectTransform>();
            Rect rect = rTransform.rect;
            Vector2 centre1 = new Vector2(0.5f * rect.width, 0.5f * rect.height);
            Mouse.current.WarpCursorPosition(centre1);
           
            // pointerUI.enableMouseInput = false;
            
        }

        // Note; was browser.RegisterFunction(SG.buildClicked, (JSONNode args) => UTestLauncher.Instance().BuildNewModel(SC.N+(string) args[0]));

        void Update()
        {
            if (repeatedKeyCode != KeyCode.None) pointerUI.keyEvents.Press(repeatedKeyCode);
        }

        void FixedUpdate()
        {
            if (fixedUpdateKeyCode != KeyCode.None)
            {
                pointerUI.keyEvents.Press(fixedUpdateKeyCode);

                fixedUpdateKeyCode = KeyCode.None;
            }
        }

        
        public bool AppInputDigitalEvent(AppDigitalFn fn, bool isOn)
        {
           
            if (!isOn)
            {
                repeatedKeyCode = KeyCode.None;
                return false;
            }
            

            KeyCode onceKeyCode = KeyCode.None;

            if (fn == GameDigitalFn.PanW) repeatedKeyCode = KeyCode.RightArrow; // inverted?
            if (fn == GameDigitalFn.PanE) repeatedKeyCode = KeyCode.LeftArrow;
            if (fn == GameDigitalFn.PanN) repeatedKeyCode = KeyCode.UpArrow;
            if (fn == GameDigitalFn.PanS) repeatedKeyCode = KeyCode.DownArrow;

            if (fn == GameDigitalFn.BrowserSelect) onceKeyCode = KeyCode.KeypadEnter;  // like pressing build button on web page
            
            if (onceKeyCode != KeyCode.None) pointerUI.keyEvents.Press(onceKeyCode);
            
            return (onceKeyCode != KeyCode.None) || (repeatedKeyCode != KeyCode.None);

        }

        public bool AppInputAnalogEvent(AppAnalogFn afn, double value)
        {
            bool used = false;
            
            if (afn == GameAnalogFn.Zoom)
            {
                float t = 0.5f;
                float s = value < 0 ? -1f : 1f;
                float av = (float) Math.Abs(value);
                bool aboveT =  av > t;
                float v = av > t ? s * (av - t): 0;
                padScroll.y = v * 0.2f; // v * 0.02f;
                used = aboveT;
            }
            else if (afn == GameAnalogFn.PanX)
            {
                float pv = panValueZF(value);
                padMouseDelta.x = pv;
                padPanningX = pv != 0;
                used = pv != 0;
                
            }
            else if (afn == GameAnalogFn.PanY)
            {
                float pv = panValueZF(value);
                padMouseDelta.y = pv;
                padPanningY = pv != 0;
                used = pv != 0;;
            }

            
            return used;
        }
        
        private float panValueZF(double v1)
        {
            float t = 0.1f;
            float av1 = (float) Math.Abs(v1);
            if (av1 < t) return 0;
            float s = v1 < 0 ? -1f : 1f;
           
            float v2 = (av1 - t) / (1f - t);  // normalize to 0 .. 1
            float v3 = v2 > 0.95f? (v2 - 0.95f) * 100f: v2;
            return s * v3;
        }
#else
        public bool AppInputDigitalEvent(AppDigitalFn fn, bool isOn)  { return false; }
        public bool AppInputAnalogEvent(AppAnalogFn afn, double value)  { return false; }
#endif
        
    }

   

}
