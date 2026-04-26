using uk.vroad.api;
using uk.vroad.rvr;
using uk.vroad.ucm;
using UnityEngine.UI;

namespace uk.vroad.urvr
{
    public class UMenuBrowse : UaGameMenu
    {
        public Text helpText;
        
        private Game game;
        protected override void Awake()
        {
            game = Game.AwakeInstance();
            base.Awake();
        }
        protected override App App() { return game; }
        
        protected override AppState MenuState() { return GameState.MenuWhileBrowsing; }
        protected override Text ControlsHelpText() { return  helpText; }

    }
}