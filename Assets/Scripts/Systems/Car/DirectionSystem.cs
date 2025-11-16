using Scellecs.Morpeh;
using Zenject;

namespace Systems.Car
{
    public class DirectionSystem : IFixedSystem
    {
        [Inject] public World World { get; set;}

        public void OnAwake()
        {
            
        }

        public void OnUpdate(float deltaTime)
        {
            
        }

        public void Dispose() { }
    }
}