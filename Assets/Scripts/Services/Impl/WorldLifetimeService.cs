using System;
using Scellecs.Morpeh;

namespace Services.Impl
{
    public class WorldLifetimeService : IDisposable
    {
        private readonly World _world;

        public WorldLifetimeService(World world)
        {
            _world = world;
        }

        public void Dispose()
        {
            if (_world != null && !_world.IsDisposed)
                _world.Dispose();
        }
    }
}