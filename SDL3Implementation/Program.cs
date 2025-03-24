namespace MyGame.Sdl3;

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Arch.Core;
using MyGame.Model;
using static SDL.SDL3;

public static class Program
{
    public static void Main()
    {
        if (OperatingSystem.IsWindows())
            Console.OutputEncoding = Encoding.UTF8;

        SDL_SetHint(SDL_HINT_WINDOWS_CLOSE_ON_ALT_F4, "null byte \0 in string"u8);

        SDL_SetHint(SDL_HINT_WINDOWS_CLOSE_ON_ALT_F4, "1"u8);
        SDL_SetHint(SDL_HINT_WINDOWS_CLOSE_ON_ALT_F4, "1");

        var world = World.Create();
        var gameModule = new GameModule(world);
        var sdl3Module = new Sdl3Module(world);
        var quitQuery = new QueryDescription().WithAll<QuitEvent>();

        var desiredDelta = TimeSpan.FromSeconds(1f / 60);
        double deltaTime = 0;

        var stopwatch = new Stopwatch();
        while (world.CountEntities(quitQuery) == 0)
        {
            stopwatch.Restart();
            sdl3Module.Update();
            gameModule.Update(deltaTime);
            var diff = desiredDelta - stopwatch.Elapsed;
            var shouldWait = diff > TimeSpan.Zero;
            if (shouldWait)
                Thread.Sleep(diff);

            Console.WriteLine($"Frame time is {stopwatch.Elapsed.Milliseconds}ms");
            deltaTime = stopwatch.Elapsed.TotalSeconds;
        }
    }
}