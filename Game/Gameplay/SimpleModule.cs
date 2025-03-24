namespace MyGame
{
    using System;
    using Arch.Core;
    using MyGame.Model;

    public class SimpleModule
    {
        private readonly QueryDescription _player;
        private readonly QueryDescription _velocity;
        private readonly QueryDescription _velocityAndPosition;
        private readonly QueryDescription _velocityAndPositionWithoutPlayer;
        private readonly World _world;

        public SimpleModule(World world)
        {
            _world = world;
            _velocityAndPosition = new QueryDescription().WithAll<Position, Velocity>();
            _velocityAndPositionWithoutPlayer =
                new QueryDescription().WithAll<Position, Velocity>().WithNone<PlayerInput>();

            _velocity = new QueryDescription().WithAll<Velocity>();
            _player = new QueryDescription().WithAll<Position, Velocity, PlayerInput>();
        }

        public void Update(double deltaTime)
        {
            // Damping
            _world.Query(in _velocity,
                         (ref Velocity vel) =>
                         {
                             vel.X *= 0.99f;
                             vel.Y *= 0.99f;
                         });

            var window = new Window();
            _world.Query(new QueryDescription().WithAll<Window>(),
                         (ref Window w) => { window = w; });

            // Bounce
            _world.Query(in _velocityAndPosition,
                         (Entity entity, ref Position pos, ref Velocity vel) =>
                         {
                             if (pos.Y < 0)
                             {
                                 pos.Y = 0;
                                 vel.Y = -vel.Y;
                             }

                             if (pos.X < 0)
                             {
                                 pos.X = 0;
                                 vel.X = -vel.X;
                             }

                             if (pos.X > window.Width)
                             {
                                 pos.X = window.Width;
                                 vel.X = -vel.X;
                             }

                             if (pos.Y > window.Height)
                             {
                                 pos.Y = window.Height;
                                 vel.Y = -vel.Y;
                             }
                         });

            Entity player = default;
            // Player
            _world.Query(in _player,
                         (Entity e, ref Position pos, ref Velocity vel, ref PlayerInput input) =>
                         {
                             vel.Y += input.Vertical * 100;
                             vel.X += input.Horizontal * 100;
                             // Damping
                             vel.Y *= 0.9f;
                             vel.X *= 0.9f;
                             player = e;
                         });

            var playerPos = _world.Get<Position>(player);
            // Repel
            _world.Query(in _velocityAndPositionWithoutPlayer,
                         (Entity entity, ref Position pos, ref Velocity vel) =>
                         {
                             var squaredDistance =
                                 Math.Clamp(Math.Pow(pos.X - playerPos.X, 2) + Math.Pow(pos.Y - playerPos.Y, 2),
                                            1,
                                            float.MaxValue);

                             vel.X += 1f / (float)squaredDistance * 500 * (pos.X - playerPos.X);
                             vel.Y += 1f / (float)squaredDistance * 500 * (pos.Y - playerPos.Y);
                         });

            // Move
            _world.Query(in _velocityAndPosition,
                         (Entity entity, ref Position pos, ref Velocity vel) =>
                         {
                             pos.X += vel.X * (float)deltaTime;
                             pos.Y += vel.Y * (float)deltaTime;
                         });
        }
    }
}