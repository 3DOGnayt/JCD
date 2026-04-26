using uk.vroad.api.input;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public class GamePauseMenu : AppPauseMenu
    {
        private static readonly AppDigitalFn[] MAP_FNS = new AppDigitalFn[] { AppDigitalFn.MenuResume, GameDigitalFn.MenuControls, GameDigitalFn.MenuStory, GameDigitalFn
            .MenuAvatar, GameDigitalFn.MenuGraphics, GameDigitalFn.MenuCredits, GameDigitalFn.MenuReset, AppDigitalFn.MenuExit };
        private static readonly AppDigitalFn[] BROWSE_FNS = new AppDigitalFn[] { AppDigitalFn.MenuResume, GameDigitalFn.MenuControls, AppDigitalFn.MenuExit };
        private static readonly AppDigitalFn[] PLAY_FNS = new AppDigitalFn[] { AppDigitalFn.MenuResume, GameDigitalFn.MenuControls, GameDigitalFn.MenuTips, GameDigitalFn
            .MenuChangeCity, GameDigitalFn.MenuGraphics, AppDigitalFn.MenuExit };
        private static readonly AppDigitalFn[] COMPLETE_FNS = new AppDigitalFn[] { GameDigitalFn.MenuStory, GameDigitalFn.MenuCredits, GameDigitalFn.MenuReset, 
            AppDigitalFn.MenuExit };
        public static readonly uk.vroad.rvr.GamePauseMenu MapMenu = new uk.vroad.rvr.GamePauseMenu(SGM.APM_MAP_MENU, GameDigitalFn.MenuControls, MAP_FNS);
        public static readonly uk.vroad.rvr.GamePauseMenu TripMenu = new uk.vroad.rvr.GamePauseMenu(SGM.APM_TRIP_MENU, GameDigitalFn.MenuControls, PLAY_FNS);
        public static readonly uk.vroad.rvr.GamePauseMenu BrowseMenu = new uk.vroad.rvr.GamePauseMenu(SGM.APM_BROWSE_MENU, GameDigitalFn.MenuControls, BROWSE_FNS
            );
        public static readonly uk.vroad.rvr.GamePauseMenu CompleteMenu = new uk.vroad.rvr.GamePauseMenu(SGM.APM_COMPLETE_MENU, GameDigitalFn.MenuStory, COMPLETE_FNS
            );
        public static readonly uk.vroad.rvr.GamePauseMenu PlayMenu = new uk.vroad.rvr.GamePauseMenu(SGM.APM_PLAY_MENU, GameDigitalFn.MenuControls, PLAY_FNS);

        private GamePauseMenu(string name, AppDigitalFn init, AppDigitalFn[] fna)
            : base(name, init, fna)
        {
        }
    }
}
