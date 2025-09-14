using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems
{
    public sealed class PlayerSpawnSystem : ISystem 
    {
        [Inject] public World World { get; set;}
        [Inject] private CarPreset _carPreset;
        [Inject] private DiContainer _container;
        
        private Transform _playerGroup;
        
        public void OnAwake()
        {
            SetSpawnRoot();
            SpawnPlayer();
        }

        private void SetSpawnRoot() => _playerGroup = new GameObject("Player").transform;

        private void SpawnPlayer()
        {
            var player = _carPreset.Car;
            if (player == null)
                return;

            var instance = _container.InstantiatePrefab(player, Vector3.zero, Quaternion.identity, _playerGroup);

            var entity = World.CreateEntity();
            AddGameComponents(entity);
            AddInternalComponents(entity, instance.transform);
        }

        private void AddGameComponents(Entity entity)
        {
            entity.SetComponent(new PlayerTagComponent());
            
            //var carParameters = _carPreset.CarParameters;
            
            entity.SetComponent(new SpeedComponent { Value = 100});
            entity.SetComponent(new BackSpeedComponent { Value = 40});
            entity.SetComponent(new MotorTorqueComponent { Value = 4000});
            entity.SetComponent(new AccelerationMultiplierComponent { Value = 4});
            entity.SetComponent(new DecelerationMultiplierComponent { Value = 2});
            entity.SetComponent(new SteeringAngleComponent { Value = 40});
            entity.SetComponent(new SteeringSpeedComponent { Value = 1});
            entity.SetComponent(new BrakeForceComponent { Value = 1});
            entity.SetComponent(new DriftMultiplierComponent { Value = 1});
        }

        private void AddInternalComponents(Entity entity, Transform car)
        {
            AddCommonComponents(entity, car);
            AddMainCarComponents(entity);
            AddFrontWheelComponents(entity);
            AddBackWheelComponents(entity);
        }

        private void AddCommonComponents(Entity entity, Transform car)
        {
            entity.SetComponent(new PlayerTagComponent());

            entity.SetComponent(new TransformComponent { Value = car });
            entity.SetComponent(new PositionComponent { Value = car.position });
            entity.SetComponent(new RotationComponent { Value = car.rotation });
            entity.SetComponent(new ScaleComponent { Value = 1 });
        }

        private void AddMainCarComponents(Entity entity)
        {
            var carParameters = _carPreset.CarParameters;

            entity.SetComponent(new CarMassComponent { Value = carParameters.Mass });
            entity.SetComponent(new AutomaticCenterOfMassComponent { Value = carParameters.AutomaticCenterOfMass });
            entity.SetComponent(new CenterOfMassComponent { Value = carParameters.CenterOfMass });
        }

        private void AddFrontWheelComponents(Entity entity)
        {
            var wheelInfos = _carPreset.WheelInfos;

            entity.SetComponent(new WheelInfoComponent { WheelInfo = wheelInfos });
            
            //todo: remove
            /*foreach (var wheelInfo in wheelInfos)
            {
                entity.SetComponent(new WheelInfoComponent
                {
                    WheelInfo = wheelInfo,
                    //LeftWheel = wheelInfo.LeftWheel,
                    //RightWheel = wheelInfo.RightWheel,
                    //LeftVisual = wheelInfo.LeftVisual,
                    //RightVisual = wheelInfo.RightVisual,
                    //Motor = wheelInfo.Motor,
                    //Steering = wheelInfo.Steering
                });
            }*/
            
            var frontMainWheelParameters = _carPreset.FrontWheelParameters.MainWheelParameters;
            var frontSuspensionSpring = _carPreset.FrontWheelParameters.SuspensionSpring;
            var frontForwardFriction = _carPreset.FrontWheelParameters.ForwardFriction;
            var frontSidewaysFriction = _carPreset.FrontWheelParameters.SidewaysFriction;

            entity.SetComponent(new FrontWheelMassComponent { Value = frontMainWheelParameters.Mass });
            entity.SetComponent(new FrontWheelRadiusComponent { Value = frontMainWheelParameters.Radius });
            entity.SetComponent(new FrontDampingRateComponent { Value = frontMainWheelParameters.DampingRate });
            entity.SetComponent(new FrontSuspensionDistanceComponent { Value = frontMainWheelParameters.SuspensionDistance });
            entity.SetComponent(new FrontForceAppPointDistanceComponent { Value = frontMainWheelParameters.ForceAppPointDistance });
            entity.SetComponent(new FrontWheelCenterComponent { Value = frontMainWheelParameters.Center });

            entity.SetComponent(new FrontSpringComponent { Value = frontSuspensionSpring.Spring });
            entity.SetComponent(new FrontDamperComponent { Value = frontSuspensionSpring.Damper });
            entity.SetComponent(new FrontTargetPositionComponent { Value = frontSuspensionSpring.TargetPosition });

            entity.SetComponent(new FrontExtremumSlipForwardComponent { Value = frontForwardFriction.ExtremumSlip });
            entity.SetComponent(new FrontExtremumValueForwardComponent { Value = frontForwardFriction.ExtremumValue });
            entity.SetComponent(new FrontAsymptoteSlipForwardComponent { Value = frontForwardFriction.AsymptoteSlip });
            entity.SetComponent(new FrontAsymptoteValueForwardComponent { Value = frontForwardFriction.AsymptoteValue });
            entity.SetComponent(new FrontStiffnessForwardComponent { Value = frontForwardFriction.Stiffness });

            entity.SetComponent(new FrontExtremumSlipSidewaysComponent { Value = frontSidewaysFriction.ExtremumSlip });
            entity.SetComponent(new FrontExtremumValueSidewaysComponent { Value = frontSidewaysFriction.ExtremumValue });
            entity.SetComponent(new FrontAsymptoteSlipSidewaysComponent { Value = frontSidewaysFriction.AsymptoteSlip });
            entity.SetComponent(new FrontAsymptoteValueSidewaysComponent { Value = frontSidewaysFriction.AsymptoteValue });
            entity.SetComponent(new FrontStiffnessSidewaysComponent { Value = frontSidewaysFriction.Stiffness });
        }

        private void AddBackWheelComponents(Entity entity)
        {
            var backMainWheelParameters = _carPreset.BackWheelParameters.MainWheelParameters;
            var backSuspensionSpring = _carPreset.BackWheelParameters.SuspensionSpring;
            var backForwardFriction = _carPreset.BackWheelParameters.ForwardFriction;
            var backSidewaysFriction = _carPreset.BackWheelParameters.SidewaysFriction;

            entity.SetComponent(new BackWheelMassComponent { Value = backMainWheelParameters.Mass });
            entity.SetComponent(new BackWheelRadiusComponent { Value = backMainWheelParameters.Radius });
            entity.SetComponent(new BackDampingRateComponent { Value = backMainWheelParameters.DampingRate });
            entity.SetComponent(new BackSuspensionDistanceComponent { Value = backMainWheelParameters.SuspensionDistance });
            entity.SetComponent(new BackForceAppPointDistanceComponent { Value = backMainWheelParameters.ForceAppPointDistance });
            entity.SetComponent(new BackWheelCenterComponent { Value = backMainWheelParameters.Center });

            entity.SetComponent(new BackSpringComponent { Value = backSuspensionSpring.Spring });
            entity.SetComponent(new BackDamperComponent { Value = backSuspensionSpring.Damper });
            entity.SetComponent(new BackTargetPositionComponent { Value = backSuspensionSpring.TargetPosition });

            entity.SetComponent(new BackExtremumSlipForwardComponent { Value = backForwardFriction.ExtremumSlip });
            entity.SetComponent(new BackExtremumValueForwardComponent { Value = backForwardFriction.ExtremumValue });
            entity.SetComponent(new BackAsymptoteSlipForwardComponent { Value = backForwardFriction.AsymptoteSlip });
            entity.SetComponent(new BackAsymptoteValueForwardComponent { Value = backForwardFriction.AsymptoteValue });
            entity.SetComponent(new BackStiffnessForwardComponent { Value = backForwardFriction.Stiffness });

            entity.SetComponent(new BackExtremumSlipSidewaysComponent { Value = backSidewaysFriction.ExtremumSlip });
            entity.SetComponent(new BackExtremumValueSidewaysComponent { Value = backSidewaysFriction.ExtremumValue });
            entity.SetComponent(new BackAsymptoteSlipSidewaysComponent { Value = backSidewaysFriction.AsymptoteSlip });
            entity.SetComponent(new BackAsymptoteValueSidewaysComponent { Value = backSidewaysFriction.AsymptoteValue });
            entity.SetComponent(new BackStiffnessSidewaysComponent { Value = backSidewaysFriction.Stiffness });
        }

        public void OnUpdate(float deltaTime) { }
        public void Dispose() { }
    }
}