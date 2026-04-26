using uk.vroad.api;
using uk.vroad.rvr;
using uk.vroad.api.input;
using uk.vroad.apk;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;

namespace uk.vroad.urvr
{
    public class UGamePadRvr : MonoBehaviour
    {

        private Game game;
       
        private App App() { return game; }
        
        private static int _rumbleCountdown;
        private static int _rumbleTicks;
        private static float _rumbleStrengthL;
        private static float _rumbleStrengthR;

        private KHash<AppButton, ButtonControl> gpButtonToUnityButton = new KHash<AppButton, ButtonControl>();
        private KHash<AppAxis, StickAxis> gpAxisToUnityAxis = new KHash<AppAxis, StickAxis>();
        private KHash<AppAxis, ButtonControl[]> gpAxisToUnityButtonPair = new KHash<AppAxis, ButtonControl[]>();

        private bool gamepadSetup;


        void Awake()
        {
            game = Game.AwakeInstance();
            Gamepad gamepad = Gamepad.current;
            if (gamepad != null) SetupGamepad(gamepad);
        }

        void Update()
        {
            Gamepad gamepad = Gamepad.current;
            if (gamepad == null)
            {
                if (gamepadSetup) // previously had a gamepad
                {
                    App().Aih().FireDigitalEvent(AppDigitalFn.Pause, true);
                    
                    gpButtonToUnityButton.Clear();
                    gpAxisToUnityAxis.Clear();
                    gpAxisToUnityButtonPair.Clear();

                    gamepadSetup = false;

                    SU.Report(SU.PAD_01);

                }
            }
            else
            {
                if (!gamepadSetup)
                {
                    App().Aih().FireDigitalEvent(AppDigitalFn.Pause, true);
                    SetupGamepad(gamepad);
                }

                foreach (AppButton gpButton in gamePadButtons)
                {
                    ButtonControl bc = gpButtonToUnityButton.Get(gpButton);
                    if (bc == null) continue;

                    // Standard buttons do not have auto-repeat, but if axes are used as buttons,
                    // then we need to use was..thisFrame otherwise stick will fight with button
                    // button will report press, stick will report release, adn you will end up getting
                    // multiple presses
                    if (bc.wasPressedThisFrame) App().Aih().SetDigitalState(gpButton, true);
                    if (bc.wasReleasedThisFrame) App().Aih().SetDigitalState(gpButton, false);
                    
                    //game.Aih().SetDigitalState(gpButton, bc.isPressed);
                }
            
                foreach (AppAxis gpAxis in gamePlayAxes)
                {
                    StickAxis sa = gpAxisToUnityAxis.Get(gpAxis);
                    if (sa != null)
                    {
                        Vector2 valuesXY = sa.stickControl.ReadValue();
                        double value = sa.isHorizontal ? valuesXY.x : valuesXY.y;

                        App().Aih().SetAnalogValue(gpAxis, value);
                    }
                    else
                    {
                        ButtonControl[] ba = gpAxisToUnityButtonPair.Get(gpAxis);
                        if (ba != null && ba.Length == 2)
                        {
                            float neg = ba[0].ReadValue();
                            float pos = ba[1].ReadValue();
                            // If both buttons are pressed, choose the larger
                            if /**/ (pos > neg) App().Aih().SetAnalogValue(gpAxis, pos);
                            else if (neg > pos) App().Aih().SetAnalogValue(gpAxis, -neg);
                            else App().Aih().SetAnalogValue(gpAxis, 0);
                        }
                    }
                }
            }

        }

        
        void FixedUpdate()
        {
            var gp = Gamepad.current;
            if (gp != null)
            {
                if (_rumbleTicks > 0) _rumbleCountdown = 1;

                if (_rumbleCountdown > 0)
                {
                    if (--_rumbleCountdown == 0) InputSystem.PauseHaptics();
                }

                if (_rumbleTicks > 0)
                {
                    _rumbleCountdown = _rumbleTicks;
                    _rumbleTicks = 0;

                    gp.SetMotorSpeeds(_rumbleStrengthL, _rumbleStrengthR);
                }
            }

          
        }
        
        // ticks: Time span of rumble 
        // strengthL: Strength of low-frequency motor, 0-100
        // strengthR: Strength of high-frequency motor, 0-100
        public static void Rumble(int ticks, int strengthL, int strengthR)
        {
            _rumbleStrengthL = 0.01f * strengthL;
            _rumbleStrengthR = 0.01f * strengthR;
            _rumbleTicks = ticks;
        }

        public static void RumbleCancel()
        {
            _rumbleCountdown = 1;
            _rumbleTicks = 0;
        }
        private void SetupGamepad(Gamepad gp)
        {
            gamepadSetup = true;

            InputDeviceDescription desc = gp.description;
            if (UDbg.BUTTON) SU.Report(SU.PAD_02, gp.GetType().Name, desc.manufacturer, gp.displayName);

            gpButtonToUnityButton.Put(GamePadButtons.DPad_Up, gp.dpad.up);
            gpButtonToUnityButton.Put(GamePadButtons.DPad_Down, gp.dpad.down);
            gpButtonToUnityButton.Put(GamePadButtons.DPad_Left, gp.dpad.left);
            gpButtonToUnityButton.Put(GamePadButtons.DPad_Right, gp.dpad.right);

            gpButtonToUnityButton.Put(GamePadButtons.LStick_Up, gp.leftStick.up);
            gpButtonToUnityButton.Put(GamePadButtons.LStick_Down, gp.leftStick.down);
            gpButtonToUnityButton.Put(GamePadButtons.LStick_Left, gp.leftStick.left);
            gpButtonToUnityButton.Put(GamePadButtons.LStick_Right, gp.leftStick.right);

            gpButtonToUnityButton.Put(GamePadButtons.RStick_Up, gp.rightStick.up);
            gpButtonToUnityButton.Put(GamePadButtons.RStick_Down, gp.rightStick.down);
            gpButtonToUnityButton.Put(GamePadButtons.RStick_Left, gp.rightStick.left);
            gpButtonToUnityButton.Put(GamePadButtons.RStick_Right, gp.rightStick.right);

            gpButtonToUnityButton.Put(GamePadButtons.Yellow_Y_Triangle_Up, gp.buttonNorth); // also gp.yButton
            gpButtonToUnityButton.Put(GamePadButtons.Green_A_Cross_Down, gp.buttonSouth); // also gp.aButton
            gpButtonToUnityButton.Put(GamePadButtons.Blue_X_Square_Left, gp.buttonWest);
            gpButtonToUnityButton.Put(GamePadButtons.Red_B_Circle_Right, gp.buttonEast);

            gpButtonToUnityButton.Put(GamePadButtons.Back_Select, gp.selectButton);
            gpButtonToUnityButton.Put(GamePadButtons.Options_Start, gp.startButton);

            gpButtonToUnityButton.Put(GamePadButtons.JoyL, gp.leftStickButton);
            gpButtonToUnityButton.Put(GamePadButtons.JoyR, gp.rightStickButton);

            gpButtonToUnityButton.Put(GamePadButtons.ShoulderL, gp.leftShoulder);
            gpButtonToUnityButton.Put(GamePadButtons.ShoulderR, gp.rightShoulder);

            gpButtonToUnityButton.Put(GamePadButtons.TriggerL, gp.leftTrigger);
            gpButtonToUnityButton.Put(GamePadButtons.TriggerR, gp.rightTrigger);

            gpAxisToUnityAxis.Put(GamePadAxes.LeftH, new StickAxis(gp.leftStick, true));
            gpAxisToUnityAxis.Put(GamePadAxes.LeftV, new StickAxis(gp.leftStick, false));

            gpAxisToUnityAxis.Put(GamePadAxes.RightH, new StickAxis(gp.rightStick, true));
            gpAxisToUnityAxis.Put(GamePadAxes.RightV, new StickAxis(gp.rightStick, false));

            gpAxisToUnityButtonPair.Put(GamePadAxes.TriggerAxis, new[] {gp.leftTrigger, gp.rightTrigger,});

            // There is a playStationButton for PS4 (and PS3) but no XBox button for XInput
            //if (gp is DualShock4GamepadHID)
            //    gpButtonToUnityButton.Put(GamePadButtons.Overlay, ((DualShock4GamepadHID)gp).playStationButton);

        }
        
        
        
        public void ButtonOptionUp()
        {
            App().Aih().FireDigitalEvent(GamePadButtons.DPad_Up, true);
        }
        
        public void ButtonOptionLeft()
        {
            App().Aih().FireDigitalEvent(GamePadButtons.DPad_Left, true);
        }
        public void ButtonOptionRight()
        {
            App().Aih().FireDigitalEvent(GamePadButtons.DPad_Right, true);
        }
        public void ButtonOptionDown()
        {
            App().Aih().FireDigitalEvent(GamePadButtons.DPad_Down, true);
        }
        public void Pause()
        {
            App().Aih().FireDigitalEvent(AppDigitalFn.Pause, true);
        }

        
        
        public void ButtonTripL()
        {
            game.Ptc().OnSelectDestination(0);
        }
        public void ButtonTripM()
        {
            game.Ptc().OnSelectDestination(1);
        }
        public void ButtonTripS()
        {
            game.Ptc().OnSelectDestination(2);
        }
        public void ButtonTripNewDeal()
        {
            game.Ptc().FireNewDeal();
        }
      
        public void ButtonSlower()
        {
            GameSimControl gsc = game.Gsc();
            
            if (gsc.IsStop()) game.Aih().FireDigitalEvent(GameDigitalFn.UTurnStopped, true);
            else gsc.RequestToChangeSpeedOnce(-1);
        }
        public void ButtonFaster()
        {
            GameSimControl gsc = game.Gsc();
            
            if (gsc.IsFast()) gsc.RequestToChangeSpeed(+1);
            // else if (gsc.IsFFwd()) gsc.RequestToChangeSpeed(+1);
            else gsc.RequestToChangeSpeedOnce(+1);
        }

        public void ButtonRunRed()
        {
            game.Aih().FireDigitalEvent(GameDigitalFn.RunRed, true);
        }

        public void ButtonAbandon()
        {
            game.Aih().FireDigitalEvent(GameDigitalFn.Abandon, true);
        }

        public void ButtonToggleTaxiRoute()
        {
            game.Aih().FireDigitalEvent(GameDigitalFn.PedOrTaxi, true);
        }

        

        private static AppButton[] gamePadButtons =
        {
            GamePadButtons.DPad_Up,
            GamePadButtons.DPad_Down,
            GamePadButtons.DPad_Left,
            GamePadButtons.DPad_Right,
            GamePadButtons.Yellow_Y_Triangle_Up,
            GamePadButtons.Green_A_Cross_Down,
            GamePadButtons.Blue_X_Square_Left,
            GamePadButtons.Red_B_Circle_Right,
            GamePadButtons.Back_Select,
            GamePadButtons.Options_Start,
            GamePadButtons.JoyL,
            GamePadButtons.JoyR,
            GamePadButtons.ShoulderL,
            GamePadButtons.ShoulderR,
            GamePadButtons.LStick_Up,
            GamePadButtons.LStick_Down,
            GamePadButtons.LStick_Left,
            GamePadButtons.LStick_Right,
            GamePadButtons.RStick_Up,
            GamePadButtons.RStick_Down,
            GamePadButtons.RStick_Left,
            GamePadButtons.RStick_Right,
            GamePadButtons.TriggerL,
            GamePadButtons.TriggerR,
        };

        private static AppAxis[] gamePlayAxes =
        {
            GamePadAxes.LeftH,
            GamePadAxes.LeftV,
            GamePadAxes.RightH,
            GamePadAxes.RightV,
            GamePadAxes.TriggerAxis,
        };


        public class StickAxis
        {
            public readonly StickControl stickControl;
            public readonly bool isHorizontal;

            public StickAxis(StickControl stcl, bool horizontal)
            {
                stickControl = stcl;
                isHorizontal = horizontal;
            }
        }

    }
}
