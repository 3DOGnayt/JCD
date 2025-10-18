using Components;
using Scellecs.Morpeh;
using Signals;
using UnityEngine;
using Zenject;

namespace Systems
{
    public class SpeedSystem : ISystem
    {
        [Inject] public World World { get; set;}
        [Inject] private SignalBus _signalBus;
        
        private Filter _cars;
        private AspectFactory<CarSetupAspect> _carSetupAspect;

        public void OnAwake()
        {
            _cars = World.Filter.Extend<CarSetupAspect>().Build();
            _carSetupAspect = World.GetAspectFactory<CarSetupAspect>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var car in _cars)
            {
                var carSetupAspect = _carSetupAspect.Get(car);
                ref var motorTorque = ref carSetupAspect.MotorTorque;
                ref var speed = ref carSetupAspect.Speed;

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    speed.Value += 50;
                }
                else if (Input.GetKeyDown(KeyCode.C))
                {
                    speed.Value -= 20;
                }
                
                Debug.Log($"AAA: speed = {speed.Value}");
                
                _signalBus.Fire(new ComponentChangeSignal<SpeedComponent> 
                { 
                    Entity = car, 
                    Component = speed 
                });
            }
        }

        public void Dispose() { }
    }
}