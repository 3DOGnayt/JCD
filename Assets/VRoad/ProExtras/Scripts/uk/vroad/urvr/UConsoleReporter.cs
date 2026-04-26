using uk.vroad.ucm;
using uk.vroad.api;
using uk.vroad.api.str;
using uk.vroad.api.etc;
using uk.vroad.rvr;
using UnityEngine;

namespace uk.vroad.urvr
{
    public class UConsoleReporter: UaExternalReporter
    {
        private Game game;

        public override App App() { return game; }

        protected override void Awake()
        {
            game = Game.AwakeInstance();
            base.Awake();
        }

        public override void Report(string msg)
        {
            bool showConsole = msg.StartsWith(SC.N+CC.CARET);
            if (showConsole) msg = msg.Substring(1);
            
            msg = DecoratedMsg(msg);
            
            // SC.S+ : Text is too tight to left hand side, this only works for single line msg
            UGameConsole.Log(SC.S +  msg); 
            if (showConsole) UGameConsole.SetVisible(true);
            
#if UNITY_EDITOR
            Debug.Log(msg);
#endif
        }
    }
}