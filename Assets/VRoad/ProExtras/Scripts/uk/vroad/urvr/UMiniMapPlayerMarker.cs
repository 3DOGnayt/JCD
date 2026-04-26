using uk.vroad.api;
using uk.vroad.api.events;
using uk.vroad.api.geom;

using uk.vroad.rvr;
using uk.vroad.ucm;
using UnityEngine;

namespace uk.vroad.urvr
{
    public class UMiniMapPlayerMarker : MonoBehaviour, LAppState
    {
        private const double MAX_ROT_PER_TICK = 1f;
        private readonly Vector3 ABOVE_PLAYER_MINI = new Vector3(0, 500, 0);
        private readonly Vector3 ABOVE_PLAYER = new Vector3(0, 10, 0);
        private GameInputHandler gih;
        private SpriteRenderer sr;
        private Xyz playerCentre;
        private Angle rotSmoothed;
        private float size;
        private Game game;
        
        void Awake()
        {
            game = Game.AwakeInstance();
            game.AddEventConsumer(this);
            gih = game.Gih();
            sr = GetComponent<SpriteRenderer>();
        }
        public bool DeregisterFireMapChange() { return true; }
        
        void Update()
        {
            sr.enabled = gih.MapOn() && playerCentre != null && rotSmoothed != null;

            if (sr.enabled)
            {
                Vector3 c = playerCentre.ToVector3();
                bool mini = gih.ShowMiniMap();
                
                gameObject.layer =  mini? UMapMeshRvr.LAYER_MINI_MAP: UMapMeshRvr.LAYER_SIM_MAP;
                float scaledSize = size * (mini ? 2f: (float) (0.33 + (0.67 * gih.GetCameraHeight() / 1200)));
                transform.position = c + (mini? ABOVE_PLAYER_MINI: ABOVE_PLAYER);
                transform.rotation = Quaternion.Euler(90, (float) rotSmoothed.Degrees(), 0);
                transform.localScale = new Vector3(scaledSize, scaledSize, 1);
            }
        }

        void FixedUpdate()
        {
            Player player = Player.ActivePlayer();
            if (player == null) { playerCentre = null; rotSmoothed = null; return; }

            playerCentre = player.Ped().Centre();
            Angle fwd = player.Ped().Forward().AsBearing();
            if (rotSmoothed == null) rotSmoothed = fwd;
            else if (player.Aboard()) rotSmoothed = fwd;
            else
            {
                Angle delta = fwd.Minus(rotSmoothed).RangeN180();
                if (delta.DegreesAbs() > MAX_ROT_PER_TICK)
                {
                    double deltaCapped = delta.Degrees() > 0 ? MAX_ROT_PER_TICK : -MAX_ROT_PER_TICK;
                    rotSmoothed = new AngleDeg(rotSmoothed.Degrees() + deltaCapped);
                }
                else rotSmoothed = fwd;
            }

        }
        public void AppStateChanged(AppStateTransition ast)
        {
            if (ast.after == GameState.ReadyToSimulate)
            {
                // For 400 hectares at 16:9, w2e will be 2666 + 200 = 2866
                size = 0.004f * (float) game.Map().GetWidth();
            }
        }
    }
}
