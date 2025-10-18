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

        //TODO: need update
        public void OnUpdate(float deltaTime)
        {
            foreach (var car in _cars)
            {
                var carSetupAspect = _carSetupAspect.Get(car);
                ref var motorTorque = ref carSetupAspect.MotorTorque;
                ref var speed = ref carSetupAspect.Speed;

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    motorTorque.Value += 500;
                    speed.Value += 50;
                }
                else if (Input.GetKeyDown(KeyCode.C))
                {
                    motorTorque.Value -= 500;
                    speed.Value -= 20;
                }
                
                _signalBus.Fire(new ComponentChangeSignal<CarSetupAspect> 
                { 
                    Entity = car, 
                    Component = carSetupAspect
                });
            }
        }

        public void Dispose() { }
    }
}