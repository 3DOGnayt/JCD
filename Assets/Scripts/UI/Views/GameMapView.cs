using KoboldUi.Element.View;

namespace UI.Views
{
    public class GameMapView : AUiAnimatedView
    {
        // map
    }
    
    public class GameSpeedometerView : AUiAnimatedView
    {
        // in canvas
        // speedometer
    }
    
    public class GameTimerView : AUiAnimatedView
    {
        // Total time / Section time (1/2/..)
    }
    
    public class StoryOpponentView : AUiAnimatedView // ???
    {
        // image + name + car vs player + car / adavntage (+/- 4.9m) /
    }
    
    public class TrainingOpponentView : AUiAnimatedView
    {
        // best time / difference total / (player + car)???
    }
    
    public class GamePauseView : AUiAnimatedView
    {
        // continue / retry / exit
    }
    
    public class GameResultView : AUiAnimatedView
    {
        // Win / Lose / total time + time by lap / move to => AfterResultView / ????????
    }
}