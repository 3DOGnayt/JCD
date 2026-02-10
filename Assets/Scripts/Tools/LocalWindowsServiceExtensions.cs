using System;
using System.Reflection;
using KoboldUi.Services.WindowsService;
using UI.Animations;
using UnityEngine;
using Zenject;

namespace Tools
{
    public static class LocalWindowsServiceExtensions
    {
        public static void AnimateWindow<TWindow>(this ILocalWindowsService service, Vector2 offset)
        {
            if (service == null)
            {
                Debug.LogWarning("[UI] AnimateWindow failed: service is null.");
                return;
            }

            var container = TryGetContainer(service);
            if (container == null)
            {
                Debug.LogWarning("[UI] AnimateWindow failed: DiContainer not available.");
                return;
            }

            if (!TryResolveWindow(container, out TWindow window))
            {
                Debug.LogWarning($"[UI] AnimateWindow failed: {typeof(TWindow).Name} not resolved.");
                return;
            }

            var animator = FindAnimator(window);
            if (animator == null)
            {
                Debug.LogWarning($"[UI] AnimateWindow failed: {typeof(TWindow).Name} has no IWindowAnimator.");
                return;
            }

            animator.Animate(offset);
        }

        private static DiContainer TryGetContainer(ILocalWindowsService service)
        {
            const string fieldName = "_diContainer";
            var type = service.GetType();

            while (type != null)
            {
                var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                    return field.GetValue(service) as DiContainer;

                type = type.BaseType;
            }

            return null;
        }

        private static bool TryResolveWindow<TWindow>(DiContainer container, out TWindow window)
        {
            window = default;
            try
            {
                window = container.Resolve<TWindow>();
                return window != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static IWindowAnimator FindAnimator<TWindow>(TWindow window)
        {
            if (window is not Component component)
                return null;

            var behaviours = component.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var behaviour in behaviours)
            {
                if (behaviour is IWindowAnimator animator)
                    return animator;
            }

            return null;
        }
    }
}
