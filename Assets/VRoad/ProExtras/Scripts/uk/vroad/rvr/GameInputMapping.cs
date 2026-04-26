using uk.vroad.api.input;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public class GameInputMapping : AppInputMapping
    {
        public static readonly uk.vroad.rvr.GameInputMapping ChooseMap = new uk.vroad.rvr.GameInputMapping(SGM.AIM_CHOOSE_MAP);
        public static readonly uk.vroad.rvr.GameInputMapping Browser = new uk.vroad.rvr.GameInputMapping(SGM.AIM_BROWSER);
        public static readonly uk.vroad.rvr.GameInputMapping ChooseTrip = new uk.vroad.rvr.GameInputMapping(SGM.AIM_CHOOSE_TRIP);
        public static readonly uk.vroad.rvr.GameInputMapping PlayingGame = new uk.vroad.rvr.GameInputMapping(SGM.AIM_PLAYING_GAME);
        public static readonly uk.vroad.rvr.GameInputMapping PlayingDolly = new uk.vroad.rvr.GameInputMapping(SGM.AIM_PLAYING_DOLLY);

        protected internal GameInputMapping(string name)
            : base(name)
        {
            switch (name)
            {
                case SGM.AIM_CHOOSE_MAP:
                {
                    StoreMapping(GamePadButtons.DPad_Up, GameDigitalFn.CityUp);
                    StoreMapping(GamePadButtons.DPad_Left, GameDigitalFn.CityLeft);
                    StoreMapping(GamePadButtons.DPad_Right, GameDigitalFn.CityRight);
                    StoreMapping(GamePadButtons.DPad_Down, GameDigitalFn.CityDown);
                    StoreMapping(GamePadButtons.TriggerL, GameDigitalFn.CitySelect);
                    StoreMapping(GamePadButtons.TriggerR, GameDigitalFn.CitySelect);
                    StoreMapping(GamePadButtons.Options_Start, AppDigitalFn.Pause);
                    StoreMapping(GamePadButtons.LStick_Up, GameDigitalFn.CityUp);
                    StoreMapping(GamePadButtons.LStick_Right, GameDigitalFn.CityRight);
                    StoreMapping(GamePadButtons.LStick_Left, GameDigitalFn.CityLeft);
                    StoreMapping(GamePadButtons.LStick_Down, GameDigitalFn.CityDown);
                    StoreMapping(GamePadButtons.RStick_Up, GameDigitalFn.CityUp);
                    StoreMapping(GamePadButtons.RStick_Right, GameDigitalFn.CityRight);
                    StoreMapping(GamePadButtons.RStick_Left, GameDigitalFn.CityLeft);
                    StoreMapping(GamePadButtons.RStick_Down, GameDigitalFn.CityDown);
                    break;
                }
                case SGM.AIM_BROWSER:
                {
                    StoreMapping(GamePadButtons.DPad_Up, AppDigitalFn.PanN);
                    StoreMapping(GamePadButtons.DPad_Left, AppDigitalFn.PanE);
                    StoreMapping(GamePadButtons.DPad_Right, AppDigitalFn.PanW);
                    StoreMapping(GamePadButtons.DPad_Down, AppDigitalFn.PanS);
                    StoreMapping(GamePadButtons.TriggerL, GameDigitalFn.BrowserSelect);
                    StoreMapping(GamePadButtons.TriggerR, GameDigitalFn.BrowserSelect);
                    StoreMapping(GamePadAxes.LeftH, AppAnalogFn.PanX);
                    StoreMapping(GamePadAxes.LeftV, AppAnalogFn.PanY);
                    StoreMapping(GamePadAxes.RightV, AppAnalogFn.Zoom);
                    StoreMapping(GamePadButtons.Options_Start, AppDigitalFn.Pause);
                    break;
                }
                case SGM.AIM_CHOOSE_TRIP:
                {
                    StoreMapping(GamePadButtons.DPad_Up, GameDigitalFn.TripUp);
                    StoreMapping(GamePadButtons.DPad_Left, GameDigitalFn.TripLeft);
                    StoreMapping(GamePadButtons.DPad_Right, GameDigitalFn.TripRight);
                    StoreMapping(GamePadButtons.DPad_Down, GameDigitalFn.TripDown);
                    StoreMapping(GamePadButtons.TriggerL, GameDigitalFn.TripSelect);
                    StoreMapping(GamePadButtons.Red_B_Circle_Right, GameDigitalFn.TripNewDeal);
                    StoreMapping(GamePadButtons.Options_Start, AppDigitalFn.Pause);
                    break;
                }
                case SGM.AIM_PLAYING_GAME:
                {
                    PlayingGameCommon();
                    StoreMapping(GamePadAxes.RightH, GameAnalogFn.Lane);
                    StoreMapping(GamePadAxes.RightV, GameAnalogFn.Speed);
                    break;
                }
                case SGM.AIM_PLAYING_DOLLY:
                {
                    PlayingGameCommon();
                    StoreMapping(GamePadAxes.RightV, AppAnalogFn.Tilt);
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

        private void PlayingGameCommon()
        {
            StoreMapping(GamePadButtons.DPad_Up, GameDigitalFn.RouteForward);
            StoreMapping(GamePadButtons.DPad_Left, GameDigitalFn.RouteLeft);
            StoreMapping(GamePadButtons.DPad_Right, GameDigitalFn.RouteRight);
            StoreMapping(GamePadButtons.DPad_Down, GameDigitalFn.RouteUndoUTurn);
            StoreMapping(GamePadButtons.TriggerL, GameDigitalFn.RouteSelect);
            StoreMapping(GamePadButtons.TriggerR, GameDigitalFn.RunRed);
            StoreMapping(GamePadButtons.Blue_X_Square_Left, GameDigitalFn.PedOrTaxi);
            StoreMapping(GamePadButtons.Yellow_Y_Triangle_Up, GameDigitalFn.Abandon);
            StoreMapping(GamePadButtons.ShoulderL, GameDigitalFn.VolumeDown);
            StoreMapping(GamePadButtons.ShoulderR, GameDigitalFn.VolumeUp);
            StoreMapping(GamePadButtons.JoyL, GameDigitalFn.CameraCycle);
            StoreMapping(GamePadButtons.JoyR, GameDigitalFn.MapCycle);
            StoreMapping(GamePadAxes.LeftH, AppAnalogFn.Rotate);
            StoreMapping(GamePadAxes.LeftV, AppAnalogFn.Zoom);
            StoreMapping(GamePadButtons.Options_Start, AppDigitalFn.Pause);
        }
    }
}
