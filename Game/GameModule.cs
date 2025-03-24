namespace MyGame
{
    using System;
    using Arch.Core;
    using MyGame.Model;

    public class GameModule
    {
        private readonly SimpleModule _module;
        private readonly World _world;

        public GameModule(World world)
        {
            _world = world;
            _module = new SimpleModule(_world);
            _world.Create(new InputSettings());
            _world.Create(new PlayerInput());

            var rnd = new Random();
            for (var i = 0; i < 5000; i++)
            {
                _world.Create(new Rectangle { Width = 5, Height = 5 },
                              new Position { X = i, Y = i },
                              new Velocity
                              {
                                  X = (float)rnd.NextDouble() * 500 - 250, Y = (float)rnd.NextDouble() * 500 - 250
                              });
            }

            _world.Create(new Rectangle { Width = 100, Height = 100 },
                          new Position { X = 0, Y = 0 },
                          new Velocity(),
                          new PlayerInput());

            _world.Create(new Window { Title = "Hello World!", Width = 1280, Height = 720 });
        }

        public void Update(double deltaTime)
        {
            _module.Update(deltaTime);
        }
    }
}