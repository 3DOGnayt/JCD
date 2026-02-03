using UniRx;

namespace Services
{
    public interface ILoadingService
    {
        IReactiveProperty<float> LoadingProgress { get; }
        IReactiveProperty<bool> IsLoadingCompleted { get; }
        
        void ReloadCurrentScene();
    }
}