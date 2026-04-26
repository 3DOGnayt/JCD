using uk.vroad.rvr;

namespace uk.vroad.urvr
{

    public class UPlayerRouteMini : UPlayerRouteBase
    {
        public const float MINI_ROUTE_KW_MINI = -2.5f; 
        public const float MINI_ROUTE_KW_SIM = -1f; 
        public const float MINI_ROUTE_MW = 1f; 
        public const float MINI_ROUTE_ELEV = 10f;
        private const float MINI_ROUTE_OPTION_ELEV = 1f;
        
        private GameInputHandler gih;
        
        protected override void Awake()
        {
            base.Awake();
            gih = game.Gih();
        }

        protected override bool IsMini() { return true; }
        protected override float RouteWidthK() { return gih.ShowMiniMap() ? MINI_ROUTE_KW_MINI: MINI_ROUTE_KW_SIM ; }
        protected override float RouteWidthM() { return MINI_ROUTE_MW; }
        protected override float RouteElevation() { return MINI_ROUTE_ELEV; }
        protected override float OptionElevation() { return MINI_ROUTE_OPTION_ELEV; }

        protected override float RouteWidthOptK(int opti, int nopt)  { return RouteWidthK() ; }
        protected override float RouteWidthOptM(int opti, int nopt)  { return MINI_ROUTE_MW; }
        
        protected override void UpdateOnRouteChanged()  {}

        protected override void Update()
        {
            base.Update();

            gameObject.layer = gih.ShowMiniMap() ? UMapMeshRvr.LAYER_MINI_MAP: UMapMeshRvr.LAYER_SIM_MAP ;
        }
        protected override bool IsActive(Player player)
        {
            if (!base.IsActive(player)) return false;
            if (!gih.MapOn()) return false;

            return true;
        }
    }
}
