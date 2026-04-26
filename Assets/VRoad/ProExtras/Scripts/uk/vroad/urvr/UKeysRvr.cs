using uk.vroad.api;
using uk.vroad.api.input;
using uk.vroad.api.events;
using uk.vroad.apk;
using uk.vroad.rvr;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace uk.vroad.urvr
{
    public class UKeysRvr : MonoBehaviour, LAppState
    {
        /// <summary> A global static flag to disable keys, to use for example when typing into a text field </summary>
        public static void IgnoreKeyInput(bool v) { ignoreKeyInput = v;}
        private static bool ignoreKeyInput;

        private Game game;
        
        private App App() { return game; }
        public bool DeregisterFireMapChange()  { return true; }

        void Awake()
        {
            game = Game.AwakeInstance();
            App().AddEventConsumer(this);
        }
        void Start()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;
            
            //
            // On Windows, this returns the keyboard layout as defined by the keyboard input method bar, and does not
            // appear to change if a different physical keyboard is plugged in.
            //
            // So it seems that it is not possible to detect from 'kb' if the physical keyboard has a NumPad
            //
            // Keyboard layout names on Windows are a combination of language and region
            // The lowest 10 bits are language, and higher bits are region
            // English is language #9, region 1 is US, region 2 is UK
            // 0x 0809 = English UK = 2057 decimal         (1024 * 2) + 9
            // 0x 0409 = English US = 1033                 (1024 * 1) + 9
            //
            //  Keyboard.KeyCount; // This is constant, always 110 

            

            SetKeysForGameState(AppState.WaitingForMapChoice);
            
           
        }

       
        public void AppStateChanged(AppStateTransition ast)
        {
            changeKeysOnUpdateToNewState = ast.after;
        }

        void Update()
        {
            if (ignoreKeyInput) return; // for example while editing name of map

            Keyboard kb = Keyboard.current;
            if (kb != null) HandleKeyboard(kb);
        }

        
        private void HandleKeyboard(Keyboard kb)
        {
            if (changeKeysOnUpdateToNewState != AppState.WildCard)
            {
                SetKeysForGameState(changeKeysOnUpdateToNewState);
                changeKeysOnUpdateToNewState = AppState.WildCard;
            }

            AppInputHandler aih = App().Aih();

            foreach (KeyControl kc in keyPressToFunctionOn.Keys)
            {
                if (kc.wasPressedThisFrame)
                {
                    AppDigitalFn fn = keyPressToFunctionOn.Get(kc);

                    aih.FireDigitalEvent(fn, true);
                }
            }

            foreach (KeyControl kc in keyPressToButtonOn.Keys)
            {
                if (kc.wasPressedThisFrame)
                {
                    AppButton button = keyPressToButtonOn.Get(kc);

                    aih.FireDigitalEvent(button, true);
                }
            }

            foreach (KeyControl kc in keyReleaseToFunctionOff.Keys)
            {
                if (kc.wasReleasedThisFrame)
                {
                    aih.FireDigitalEvent(keyReleaseToFunctionOff.Get(kc), false);
                }
            }


            foreach (AppAnalogFn afn in functionToKeyPair.Keys)
            {
                KeyPair keyPair = functionToKeyPair.Get(afn);

                // This sets the value to zero if no keys pressed, overriding any value from gamepad
                // gplay.FireAnalogEvent(afn, keyPair.posKey.isPressed ? 1.0 : keyPair.negKey.isPressed ? -1.0 : 0);

                if (keyPair.posKey.isPressed) aih.FireAnalogEvent(afn, 1.0);
                else if (keyPair.negKey.isPressed) aih.FireAnalogEvent(afn, -1.0);
                //else if (Gamepad.current == null) gplay.FireAnalogEvent(afn, 0);
                else
                {
                    if (keyPair.posKey.wasReleasedThisFrame) aih.FireAnalogEvent(afn, 0);
                    if (keyPair.negKey.wasReleasedThisFrame) aih.FireAnalogEvent(afn, 0);
                }
            }


            if (kb.ctrlKey.isPressed)
            {
                foreach (KeyControl kc in ctrlKeyPressToFunctionOn.Keys)
                {
                    if (kc.wasPressedThisFrame)
                    {
                        AppDigitalFn fn = ctrlKeyPressToFunctionOn.Get(kc);
                        aih.FireDigitalEvent(fn, true);
                    }
                }
            }
        }
        
        protected AppState changeKeysOnUpdateToNewState = AppState.WildCard;
        
        protected KHash<KeyControl, AppButton> keyPressToButtonOn = new KHash<KeyControl, AppButton>();
        protected KHash<KeyControl, AppDigitalFn> keyPressToFunctionOn = new KHash<KeyControl, AppDigitalFn>();
        protected KHash<KeyControl, AppDigitalFn> keyReleaseToFunctionOff = new KHash<KeyControl, AppDigitalFn>();
        protected KHash<KeyControl, AppDigitalFn> ctrlKeyPressToFunctionOn = new KHash<KeyControl, AppDigitalFn>();
        protected KHash<AppAnalogFn, KeyPair> functionToKeyPair = new KHash<AppAnalogFn, KeyPair>();

        protected virtual void SetKeysForGameState(AppState currentState)
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;

            keyPressToFunctionOn.Clear();
            keyReleaseToFunctionOff.Clear();
            keyPressToButtonOn.Clear();
            functionToKeyPair.Clear();

            bool wasdToDPad = false;
            bool arrowsToDPad = false;
            bool pause = false;
            bool resume = false;
            bool spaceToTriggerL = false;
            bool enterToTriggerR = false;

            if (currentState == AppState.WaitingForMapChoice)
            {
                pause = true;
                wasdToDPad = true;
                arrowsToDPad = true;
                spaceToTriggerL = true;
                enterToTriggerR = true;
            }

            
            else if (currentState == AppState.MenuAtMapChoice )
            {
                wasdToDPad = true;
                arrowsToDPad = true;
                resume = true;
                spaceToTriggerL = true;
                enterToTriggerR = true;
            }


            if (pause)
            {
                keyPressToFunctionOn.Put(kb.escapeKey, AppDigitalFn.Pause);
            }

            if (resume)
            {
                keyPressToFunctionOn.Put(kb.escapeKey, AppDigitalFn.MenuResume);
            }
            
            if (arrowsToDPad)
            {
                keyPressToButtonOn.Put(kb.upArrowKey, GamePadButtons.DPad_Up);
                keyPressToButtonOn.Put(kb.leftArrowKey, GamePadButtons.DPad_Left);
                keyPressToButtonOn.Put(kb.rightArrowKey, GamePadButtons.DPad_Right);
                keyPressToButtonOn.Put(kb.downArrowKey, GamePadButtons.DPad_Down);
            }

            if (wasdToDPad)
            {
                keyPressToButtonOn.Put(kb.wKey, GamePadButtons.DPad_Up);
                keyPressToButtonOn.Put(kb.aKey, GamePadButtons.DPad_Left);
                keyPressToButtonOn.Put(kb.dKey, GamePadButtons.DPad_Right);
                keyPressToButtonOn.Put(kb.sKey, GamePadButtons.DPad_Down);
            }
            
            if (spaceToTriggerL) keyPressToButtonOn.Put(kb.spaceKey, GamePadButtons.TriggerL);
            if (enterToTriggerR) keyPressToButtonOn.Put(kb.enterKey, GamePadButtons.TriggerR);

             
            
            ctrlKeyPressToFunctionOn.Put(kb.f2Key, AppDigitalFn.Console);

            ctrlKeyPressToFunctionOn.Put(kb.cKey, AppDigitalFn.CopyConsoleToClipboard);
       
          if (currentState == GameState.WaitingForTripChoice)
          {
                pause = true;
                wasdToDPad = true;
                arrowsToDPad = true;
                spaceToTriggerL = true;
                //enterToTriggerR = false;
                keyPressToButtonOn.Put(kb.enterKey, GamePadButtons.TriggerL);
                keyPressToButtonOn.Put(kb.numpadEnterKey, GamePadButtons.TriggerL);

                keyPressToFunctionOn.Put(kb.f3Key, GameDigitalFn.TripNewDeal);
          }


          else if (currentState == GameState.Navigating)
          {
                pause = true;
                wasdToDPad = true;
                //arrowsToDPad = false;

                functionToKeyPair.Put(AppAnalogFn.Zoom, new KeyPair(kb.upArrowKey, kb.downArrowKey));
                functionToKeyPair.Put(AppAnalogFn.Rotate, new KeyPair(kb.rightArrowKey, kb.leftArrowKey));

                // Can't have two key pairs assigned to the same analog fn, the second will override the first
                //
                //functionToKeyPair.Put(GameAnalogFn.Speed, new KeyPair(kb.numpad8Key, kb.numpad2Key));
                //functionToKeyPair.Put(GameAnalogFn.Lane, new KeyPair(kb.numpad6Key, kb.numpad4Key));

                functionToKeyPair.Put(GameAnalogFn.Speed, new KeyPair(kb.f10Key, kb.f9Key));
                functionToKeyPair.Put(GameAnalogFn.Lane, new KeyPair(kb.f8Key, kb.f7Key));

                //spaceToTriggerL = true;
                keyPressToFunctionOn.Put(kb.spaceKey, GameDigitalFn.RouteSelect);
                keyPressToFunctionOn.Put(kb.vKey, GameDigitalFn.PedOrTaxi);
                keyPressToFunctionOn.Put(kb.mKey, GameDigitalFn.MapCycle);

                keyPressToFunctionOn.Put(kb.backspaceKey, GameDigitalFn.Abandon);
                keyPressToFunctionOn.Put(kb.enterKey, GameDigitalFn.RunRed);
                keyPressToFunctionOn.Put(kb.deleteKey, GameDigitalFn.CameraCycle);

                //keyPressToFunctionOn.Put(kb.numpad2Key, GameDigitalFn.UTurnStopped);

                ctrlKeyPressToFunctionOn.Put(kb.nKey, GameDigitalFn.CameraNorth);
                ctrlKeyPressToFunctionOn.Put(kb.dKey, GameDigitalFn.CameraDolly);

          }
          else if (currentState == GameState.MenuWhileBrowsing)
          {
              wasdToDPad = true;
              arrowsToDPad = true;
              resume = true;
              spaceToTriggerL = true;
              enterToTriggerR = true;
          }
          else if (currentState == GameState.Browsing)
          {
              pause = true;

              // The browser uses arrow keys for panning and +/- keys for zooming (main keyboard or numpad)

              keyPressToFunctionOn.Put(kb.wKey, AppDigitalFn.PanN);
              keyPressToFunctionOn.Put(kb.aKey, AppDigitalFn.PanW);
              keyPressToFunctionOn.Put(kb.dKey, AppDigitalFn.PanE);
              keyPressToFunctionOn.Put(kb.sKey, AppDigitalFn.PanS);

              keyReleaseToFunctionOff.Put(kb.wKey, AppDigitalFn.PanN);
              keyReleaseToFunctionOff.Put(kb.aKey, AppDigitalFn.PanW);
              keyReleaseToFunctionOff.Put(kb.dKey, AppDigitalFn.PanE);
              keyReleaseToFunctionOff.Put(kb.sKey, AppDigitalFn.PanS);

              spaceToTriggerL = true;
              enterToTriggerR = true;
          }
          else if (currentState == GameState.PausedWhileNavigating ||
                   currentState == GameState.PausedAtTripChoice)
            {
                wasdToDPad = true;
                arrowsToDPad = true;
                resume = true;
                spaceToTriggerL = true;
                enterToTriggerR = true;
            }


            if (pause)
            {
                keyPressToFunctionOn.Put(kb.escapeKey, AppDigitalFn.Pause);
            }

            if (resume)
            {
                keyPressToFunctionOn.Put(kb.escapeKey, AppDigitalFn.MenuResume);
            }
            
            if (arrowsToDPad)
            {
                keyPressToButtonOn.Put(kb.upArrowKey, GamePadButtons.DPad_Up);
                keyPressToButtonOn.Put(kb.leftArrowKey, GamePadButtons.DPad_Left);
                keyPressToButtonOn.Put(kb.rightArrowKey, GamePadButtons.DPad_Right);
                keyPressToButtonOn.Put(kb.downArrowKey, GamePadButtons.DPad_Down);
            }

            if (wasdToDPad)
            {
                keyPressToButtonOn.Put(kb.wKey, GamePadButtons.DPad_Up);
                keyPressToButtonOn.Put(kb.aKey, GamePadButtons.DPad_Left);
                keyPressToButtonOn.Put(kb.dKey, GamePadButtons.DPad_Right);
                keyPressToButtonOn.Put(kb.sKey, GamePadButtons.DPad_Down);
            }
            
            if (spaceToTriggerL) keyPressToButtonOn.Put(kb.spaceKey, GamePadButtons.TriggerL);
            if (enterToTriggerR) keyPressToButtonOn.Put(kb.enterKey, GamePadButtons.TriggerR);
            
            // if (volumeKeys) // always true
            {
                keyPressToFunctionOn.Put(kb.numpadPlusKey, GameDigitalFn.VolumeUp);
                keyReleaseToFunctionOff.Put(kb.numpadPlusKey, GameDigitalFn.VolumeUp);
                keyPressToFunctionOn.Put(kb.numpadMinusKey, GameDigitalFn.VolumeDown);
                keyReleaseToFunctionOff.Put(kb.numpadMinusKey, GameDigitalFn.VolumeDown);
                
                keyPressToFunctionOn.Put(kb.equalsKey, GameDigitalFn.VolumeUp);
                keyReleaseToFunctionOff.Put(kb.equalsKey, GameDigitalFn.VolumeUp);
                keyPressToFunctionOn.Put(kb.minusKey, GameDigitalFn.VolumeDown);
                keyReleaseToFunctionOff.Put(kb.minusKey, GameDigitalFn.VolumeDown);

                keyPressToFunctionOn.Put(kb.numpadMultiplyKey, GameDigitalFn.VolumeMute);
            }
           
        }

        
        
        public class KeyPair
        {
            public readonly KeyControl posKey;
            public readonly KeyControl negKey;

            internal KeyPair(KeyControl p, KeyControl n)
            {
                posKey = p; negKey = n; 
            }
        }

    }
    
    
   

}
