using uk.vroad.api;
using uk.vroad.rvr;

namespace uk.vroad.urvr
{
    
    public class UMenuComplete : UMenuMap
    {
        // This has the dummy image/objects CompleteControlsBlank set as the image for Kbd/Xbox and PS4 images
        // because this menu does not have a controls option. It needs to have a separate image,
        // otherwise when it is inactive it would switch off the image from another menu
        
        protected override AppState MenuState() { return GameState.MenuComplete; }
        
    }
}
