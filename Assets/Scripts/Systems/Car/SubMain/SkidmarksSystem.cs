using Components;
using Scellecs.Morpeh;
using Services;
using Zenject;

namespace Systems.Car.SubMain
{
    public class SkidmarksSystem : IFixedSystem
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
                var rigidbodyComponent = _rbStash.Get(car);
                var info = _wheelInfoStash.Get(car);
                var skidFlag = _skidmarksStash.Get(car).Value;

                var rigidbody = rigidbodyComponent.Value;
                if (rigidbody == null || info.WheelInfo == null)
                    continue;

                foreach (var w in info.WheelInfo)
                {
                    if (w == null)
                        continue;

                    if (w.LeftWheel != null)
                        _skidmarksService.UpdateWheel(rigidbody, w.LeftWheel, skidFlag);

                    if (w.RightWheel != null)
                        _skidmarksService.UpdateWheel(rigidbody, w.RightWheel, skidFlag);
                }
            }
        }

        public void Dispose() { }
    }
}