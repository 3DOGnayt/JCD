using uk.vroad.api.str;

namespace uk.vroad.ucm
{
    public static class SA
    {
        public const string COPY_TO_CLIPBOARD = "> Ctrl-C [Contents Copied to Clipboard]";
        public const string NO_BOT_PARENTS = "Did not find parent objects for peds, cars, coaches, trucks, taxis, buses";

        public const string UNEXPECTED_PKG_LOC = "VRoad package is not in expected location: ";
        
        // --------- Strings requiring translations above this line -------------------------
        
      
        public const string SONY = "sony"; // to recognize a PS4-layout gamepad vs XBox layout
        public const string MOUSE_X = "Mouse X";
        public const string MOUSE_Y = "Mouse Y";

        public const string Anim_isIdleActive = "isIdleActive";
        public const string Anim_isWalking = "isWalking";

        public const string ASSETS_DIR = "Assets";              // *** No trailing slash
        public const string EXPECTED_PKG_LOC = "Assets/VRoad"; 
        public const string MESH_GEN_DIR = "Models/MapMeshes"; 
        public const string PREFAB_GEN_DIR = "Prefabs/Maps";
        public const string SUFFIX_ASSET = ".asset";
        public const string SUFFIX_PREFAB = ".prefab";
        public const string TERRAIN = "UnityTerrain";
        public const string PREFAB_MESH_NAME_FMT = "%s_%04d";
        public const string UNITY_RUNNER = "UnityRunner";
        public const string DOT_BINX = ".bin";
        
#if UNITY_EDITOR_WIN
        public const string ARCH_DIR = SF.ARCH_WIN64;
        public const string DOT_BIN = ".exe";
#endif
    }
}