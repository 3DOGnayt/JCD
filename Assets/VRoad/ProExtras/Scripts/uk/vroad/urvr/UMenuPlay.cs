using uk.vroad.api;
using uk.vroad.api.input;
using uk.vroad.rvr;
using uk.vroad.ucm;
using UnityEngine;
using UnityEngine.UI;

namespace uk.vroad.urvr
{
    public class UMenuPlay : UaGameMenu
    {
        public Text helpText;
        public GameObject helpTipsPanel;
        
        protected override Text ControlsHelpText() { return  helpText; }

        protected override AppState MenuState() { return GameState.PausedWhileNavigating; }

        protected override App App() { return game; }
        private Game game;

        protected override void Awake()
        {
            game = Game.AwakeInstance();
            base.Awake();
        }
        

        protected override bool MenuItemPressed(AppDigitalFn fn)
        {
            if (base.MenuItemPressed(fn)) return true;
            
            if (fn == GameDigitalFn.MenuChangeCity)
            {
                // GameSimControl sets GameStateHandler.killAllAndMakeThisTransition = escapeFromQuarterPlay
                game.Ew().FireAppDigitalEvent(AppDigitalFn.MenuResume, true);
            }
            return false;
        }

        protected override void UpdateMenuFunction(AppDigitalFn fn, bool isSel)
        {
            base.UpdateMenuFunction(fn, isSel);
            
            if (fn == GameDigitalFn.MenuTips) helpTipsPanel.SetActive(isSel);

        }
        protected override void RebuildMenu()
        {
            if (helpTipsPanel.activeSelf) helpTipsPanel.SetActive(false);
            
            base.RebuildMenu();
        }
    }
}
