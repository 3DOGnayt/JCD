namespace uk.vroad.urvr
{
    public class SG
    {
        // This class contains do-not-translate strings for RVR on Unity
        public const string NAVIGATION_SCENE = "GamePlayMap";
        public const string INITIAL_SCENE = "GameLoadMap";
        
        public const string buildClicked = "buildClicked";
        public const string Anim_beatMultiplier = "beatMultiplier";

        public const string QMAP = "QMap_%d_%d";
        
        // Must match list defined in  https://partner.steamgames.com/apps/achievements/1785970
        public const string STEAM_L2 = "L2";
        public const string STEAM_L4 = "L4";
        public const string STEAM_L6 = "L6";
        public const string STEAM_L8 = "L8";
        public const string STEAM_V01 = "V01";
        public const string STEAM_V03 = "V03";
        public const string STEAM_V06 = "V06";
        public const string STEAM_V12 = "V12";
        public const string STEAM_V18 = "V18";
        public const string STEAM_V24 = "V24";
        public const string STEAM_V30 = "V30";
        public const string STEAM_V36 = "V36";

        // Must match list defined in https://partner.steamgames.com/apps/stats/1785970
        public const string STAT_LEVEL = "LEVEL";
        public const string STAT_VIAL = "VIAL";
        public const string STAT_KJ = "KJ";
        public const string STAT_VIRUS = "VIRUS";
        public const string STAT_VIALS_LO = "VIALS_LO";
        public const string STAT_VIALS_HI = "VIALS_HI"; // This 4-byte mask is now also used for custom map

        // These can be used to fetch (localized) name/ description of achievement, for display in app
        public const string STEAM_ACH_NAME = "name";
        public const string STEAM_ACH_DESC = "desc";

        // If a UIText object has a name beginning with this prefix, then it will be used to find cousins with the same name
        public const string KEYS_PREFIX = "Keys";
        
       
    }
}
