using uk.vroad.api.input;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public class GameAnalogFn : AppAnalogFn
    {
        public static readonly GameAnalogFn Lane = new GameAnalogFn();
        public static readonly GameAnalogFn Speed = new GameAnalogFn();
        private static bool regG = false;

        public override string ToString()
        {
            if (!regG) 
            {
                regG = true;
                ADbr.RegisterStaticObjectNames(Lane);
            }
            return base.ToString();
        }
    }
}
