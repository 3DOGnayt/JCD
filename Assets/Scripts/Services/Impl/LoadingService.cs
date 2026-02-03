using System;
using KoboldUi.Services.WindowsService;
using UniRx;

namespace Services.Impl
{
    public class LoadingService : ILoadingService, IDisposable
    {
        private readonly ILocalWindowsService _localWindowsService;

        private readonly ReactiveProperty<float> _loadingProgress = new();
        private readonly ReactiveProperty<bool> _isLoadingCompleted = new();
        
        public IReactiveProperty<float> LoadingProgress => _loadingProgress;
        public IReactiveProperty<bool> IsLoadingCompleted => _isLoadingCompleted;

        public LoadingService(ILocalWindowsService localWindowsService)
        {
            _localWindowsService = localWindowsService;
        }

        public void ReloadCurrentScene()
        {
            
        }

        public void Dispose()
        {
            
        }
    }
}