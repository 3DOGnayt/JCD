using uk.vroad.rvr;

namespace uk.vroad.urvr
{
    public class UPlayerCurrentMini : UPlayerCurrentBranch
    {
        private GameInputHandler gih;

        protected override void Awake()
        {
            base.Awake();
            gih = game.Gih();
        }

        protected override float RouteWidthK()
        {
            return gih.ShowMiniMap() ? UPlayerRouteMini.MINI_ROUTE_KW_MINI : UPlayerRouteMini.MINI_ROUTE_KW_SIM;
        }
        protected override float RouteWidthM()  { return UPlayerRouteMini.MINI_ROUTE_MW; }
        protected override float RouteElevation() { return UPlayerRouteMini.MINI_ROUTE_ELEV; }
        protected override bool DrawPyramids() { return false; }
        protected override bool IsMini() { return true; }

        protected override void Update()
        {
            base.Update();

            gameObject.layer = gih.ShowMiniMap() ? UMapMeshRvr.LAYER_MINI_MAP : UMapMeshRvr.LAYER_SIM_MAP;
        }

        protected override bool IsActive(Player player)
        {
            if (!base.IsActive(player)) return false;
            if (!gih.MapOn()) return false;

            return true;
        }
    }
}
