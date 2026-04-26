using System;
using uk.vroad.api;
using uk.vroad.api.events;
using uk.vroad.api.input;
using uk.vroad.api.route;
using uk.vroad.rvr;
using uk.vroad.ucm;
using UnityEngine;

namespace uk.vroad.urvr
{
    public class UMiniMapCheqFlag : MonoBehaviour, LAppState, LAppView
    {
        private readonly Vector3 ABOVE_DEST_MINI = new Vector3(0, 500, 0);
        private readonly Vector3 ABOVE_DEST_SIM = new Vector3(0, 10, 0);
        
        private Vector3 location;
        private float size;
        private bool viewChanged;
        private float rot;
        private float height;
        private SpriteRenderer sr;
        private GameInputHandler gih;
        private Game game;
        
        void Awake()
        {
            game = Game.AwakeInstance();
            game.AddEventConsumer(this);
            sr = GetComponent<SpriteRenderer>();
            gih = game.Gih();
        }
        public bool DeregisterFireMapChange() { return true; }
       
        void Update()
        {
            UCamControllerMainRvr camCon = ((UCamControllerMainRvr) UCamControllerMainRvr.MostRecentInstance);
            float rotNow = (float) camCon.RotationApplied().Degrees();
            float heightNow = (float) gih.GetCameraHeight();

            if (viewChanged || Math.Abs(rotNow - rot) > 10f || Math.Abs(heightNow - height) > 5f)
            {
                viewChanged = false;
                rot = rotNow;
                height = heightNow;
                
                bool visible = gih.MapOn() && game.Ptc().PlayerTrip() != null;
                sr.enabled = visible;
                if (visible)
                {
                    bool mini = gih.ShowMiniMap();

                    float scaledSize = size * (mini ? 1f : (0.33f + (0.67f * height / 1200)));
                    transform.localScale = new Vector3(scaledSize, scaledSize, 1);
                    transform.position = location + (mini ? ABOVE_DEST_MINI : ABOVE_DEST_SIM);
                    transform.rotation = Quaternion.Euler(90, mini ? 0 : rot, 0);

                    gameObject.layer = gih.ShowMiniMap() ?  UMapMeshRvr.LAYER_MINI_MAP: UMapMeshRvr.LAYER_SIM_MAP;
                }
            }
        }

        public void FireViewChanged()
        {
            viewChanged = true;
        }

        
        public void AppStateChanged(AppStateTransition ast)
        {
            if (ast.after == GameState.ReadyToSimulate)
            {
                // For 400 hectares at 16:9, width will be 2666 + 200 = 2866
                size = 0.007f * (float) game.Map().GetWidth();
            }

            if (ast.after == GameState.TripChosen)
            {
                IPedTrip trip = game.Ptc().PlayerTrip();
                Vector3 dest = trip.GetDestination().Location().ToVector3();

                location = dest;
            }
        }

    }
}
