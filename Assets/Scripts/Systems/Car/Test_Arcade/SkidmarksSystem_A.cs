using Components;
using Scellecs.Morpeh;
using Zenject;

namespace Systems.Car.Test_Arcade
{
    public class SkidmarksSystem_A : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private ISkidmarksService _skidmarksService;

        private Filter _cars;
        private Stash<RigidbodyComponent> _rbStash;
        private Stash<WheelInfoComponent> _wheelInfoStash;
        private Stash<SkidmarksComponent> _skidmarksStash;

        public void OnAwake()
        {
            _cars = World.Filter
                .With<RigidbodyComponent>()
                .With<WheelInfoComponent>()
                .With<SkidmarksComponent>()
                .Build();

            _rbStash = World.GetStash<RigidbodyComponent>();
            _wheelInfoStash = World.GetStash<WheelInfoComponent>();
            _skidmarksStash = World.GetStash<SkidmarksComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var car in _cars)
            {
                var rbComp = _rbStash.Get(car);
                var info = _wheelInfoStash.Get(car);
                var skidFlag = _skidmarksStash.Get(car).Value;

                var rb = rbComp.Value;
                if (rb == null || info.WheelInfo == null)
                    continue;

                foreach (var w in info.WheelInfo)
                {
                    if (w == null)
                        continue;

                    if (w.LeftWheel != null)
                        _skidmarksService.UpdateWheel(rb, w.LeftWheel, skidFlag);

                    if (w.RightWheel != null)
                        _skidmarksService.UpdateWheel(rb, w.RightWheel, skidFlag);
                }
            }
        }

        public void Dispose() { }
    }
}