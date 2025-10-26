using Scellecs.Morpeh;

namespace Signals
{
    public class ComponentChangeSignal<T>
    {
        public Entity Entity; // TODO: mb don't need
        public T Component;
    }
}