using uk.vroad.api;
using uk.vroad.api.events;
using uk.vroad.api.geom;
using uk.vroad.api.map;
using uk.vroad.rvr;
using uk.vroad.ucm;
using UnityEngine;


namespace uk.vroad.urvr
{
    public class UCamControllerMiniRvr : MonoBehaviour, LAppState
    {
        public static UCamControllerMiniRvr Instance;

        // This controls how close minimap is to the  corner
        private static readonly float VP_MARGIN = 0.02f; 
        private static readonly float VP_CANVAS = 1.0f - (2f * VP_MARGIN);
        private static readonly float VP_HEIGHT_FRACTION_TETHER = 0.44f; // 0.33F;

        private IMap map;

        private Game game;
        void Awake()
        {
            Instance = this;
            game = Game.AwakeInstance();
            game.AddEventConsumer(this);
        }
        private App App() { return game; }

        
        
        public bool DeregisterFireMapChange() { return true; }
       
        public void AppStateChanged(AppStateTransition ast)
        {
            if (ast.before == AppState.ReadyToSimulate)
            {
                map = App().Map();
            }

        }

        public bool CalcMapSize()
        {
            if (map == null) return false;

            float mapWidth = (float) map.GetWidth();
            float mapHeight = (float) map.GetHeight();
            float mapAspect = mapWidth / mapHeight;
            Xyz sw = map.GetSW();
            Xyz ne = map.GetNE();
            Xyz mapCentre = new Xyz(sw, ne, 0.5);

            float minimapHtFrac = VP_HEIGHT_FRACTION_TETHER;
            float xShift = 0;

            float screenPixelsH = minimapHtFrac * Screen.height;
            float screenPixelsW = mapAspect * screenPixelsH;
            float screenFractionW = screenPixelsW / Screen.width;
            float spaceW = VP_CANVAS - screenFractionW;

            Camera miniCamera = gameObject.GetComponent<Camera>();
            miniCamera.orthographicSize = (0.5f * mapHeight);
            Rect vpRect = miniCamera.rect;

            vpRect.height = minimapHtFrac;
            vpRect.width = screenFractionW;
            vpRect.x = VP_MARGIN + (xShift * 0.5f * spaceW);
            vpRect.y = VP_MARGIN;

            miniCamera.rect = vpRect;

            Vector3 cameraPos = mapCentre.ToVector3(); 

            float farClip = miniCamera.farClipPlane; // normally set to 1000
            cameraPos.y += farClip - 200f; // put camera high enough to see all, but not too high to clip

            transform.position = cameraPos;

            return true;
        }

    }
}
