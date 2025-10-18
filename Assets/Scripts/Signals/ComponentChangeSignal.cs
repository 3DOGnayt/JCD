using Scellecs.Morpeh;

namespace Signals
{
    public class ComponentChangeSignal<T>
    {
        public Entity Entity;
        public T Component;
    }
}