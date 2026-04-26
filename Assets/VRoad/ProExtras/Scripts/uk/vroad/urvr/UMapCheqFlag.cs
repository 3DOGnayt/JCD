using uk.vroad.api.map;
using uk.vroad.api.route;
using uk.vroad.api.sim;
using uk.vroad.rvr;
using uk.vroad.ucm;
using UnityEngine;

namespace uk.vroad.urvr
{
    public class UMapCheqFlag : MonoBehaviour
    {
        private Renderer flagRenderer;

        void Awake()
        {
            flagRenderer = GetComponent<Renderer>();
        }

        void Update()
        {
            Player player = Player.ActivePlayer();
            if (player == null) return;
            IPed playerPed = player.Ped();
            
            IPedZone dest = (IPedZone) playerPed.GetDestination();
            if (dest == null)
            {
                flagRenderer.enabled = false;
                return;
            }
            flagRenderer.enabled = true;

            Vector3 location = dest.Location().ToVector3();

            IBranch rb = playerPed.GetRecentBranch();
            if (dest == rb.GetZone() && ! playerPed.IsAboard() && rb is IWalkBranch wb)
            {
                IWalkway walkway = wb.GetWalkway();
                double locusP = 0;
                if (dest.GetDirectDrvZone() == null) locusP = 0.5;
                else if (wb.IsForward()) locusP = 1.0;

                location = walkway.Position(locusP * walkway.Length()).ToVector3();
            }
            
            Vector3 playerToFlag = location - playerPed.Centre().ToVector3();
            
            // The transform on this object moves it up and sideways so that the base of the flagpole is at (0,0)
            //
            // Move and rotate the parent transform so that the centre of the flag is at the target location
            Transform pt = transform.parent;
            pt.position = location;
            pt.localRotation = Quaternion.LookRotation(playerToFlag, Vector3.up);
        }

    }
}
