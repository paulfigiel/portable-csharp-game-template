namespace MyGame.Sdl3;

using System;
using Arch.Core;
using MyGame.Model;
using MyGame.Sdl3.Model;
using SDL;
using static SDL.SDL3;

public unsafe class Sdl3Module
{
    private const SDL_InitFlags InitFlags = SDL_InitFlags.SDL_INIT_VIDEO | SDL_InitFlags.SDL_INIT_GAMEPAD;

    private const int EventsPerPeep = 64;
    private static readonly SDL_Event[] Events = new SDL_Event[EventsPerPeep];
    private readonly QueryDescription _eventsQuery;
    private readonly QueryDescription _inputsQuery;
    private readonly QueryDescription _rectangleQuery;
    private readonly QueryDescription _windowQuery;

    private readonly World _world;

    public Sdl3Module(World world)
    {
        _world = world;
        _windowQuery = new QueryDescription().WithAll<Window>().WithNone<SDL3Window>();
        _eventsQuery = new QueryDescription().WithAll<InputSettings>();
        _rectangleQuery = new QueryDescription().WithAll<Rectangle, Position>();
        _inputsQuery = new QueryDescription().WithAll<PlayerInput>();

        if (!SDL_InitSubSystem(InitFlags))
            throw new InvalidOperationException($"failed to initialise SDL. Error: {SDL_GetError()}");
    }

    public void Update()
    {
        _world.Query(in _windowQuery,
                     (Entity entity, ref Window pos) =>
                     {
                         Console.WriteLine($"Creating window {pos.Title}");
                         var window = SDL_CreateWindow(pos.Title,
                                                       pos.Width,
                                                       pos.Height,
                                                       SDL_WindowFlags.SDL_WINDOW_RESIZABLE |
                                                       SDL_WindowFlags.SDL_WINDOW_HIGH_PIXEL_DENSITY);

                         var renderer = SDL_CreateRenderer(window, (Utf8String)null);
                         entity.Add(entity, new SDL3Window { window = window, renderer = renderer });
                     });

        _world.Query(_eventsQuery,
                     (Entity entity, ref InputSettings _) =>
                     {
                         SDL_PumpEvents();

                         int eventsRead;

                         do
                         {
                             eventsRead = SDL_PeepEvents(Events,
                                                         SDL_EventAction.SDL_GETEVENT,
                                                         SDL_EventType.SDL_EVENT_FIRST,
                                                         SDL_EventType.SDL_EVENT_LAST);

                             float horizontal = 0;
                             float vertical = 0;
                             for (var i = 0; i < eventsRead; i++)
                             {
                                 switch (Events[i].Type)
                                 {
                                     case SDL_EventType.SDL_EVENT_QUIT: _world.Create(new QuitRequestEvent()); break;
                                     case SDL_EventType.SDL_EVENT_KEY_DOWN:
                                         switch (Events[i].key.key)
                                         {
                                             case SDL_Keycode.SDLK_UP: vertical += 1; break;
                                             case SDL_Keycode.SDLK_DOWN: vertical -= 1; break;
                                             case SDL_Keycode.SDLK_RIGHT: horizontal += 1; break;
                                             case SDL_Keycode.SDLK_LEFT: horizontal -= 1; break;
                                         }

                                         break;
                                     case SDL_EventType.SDL_EVENT_KEY_UP:
                                         switch (Events[i].key.key)
                                         {
                                             case SDL_Keycode.SDLK_UP: vertical -= 1; break;
                                             case SDL_Keycode.SDLK_DOWN: vertical += 1; break;
                                             case SDL_Keycode.SDLK_RIGHT: horizontal -= 1; break;
                                             case SDL_Keycode.SDLK_LEFT: horizontal += 1; break;
                                         }

                                         break;
                                 }
                             }

                             _world.Query(_inputsQuery,
                                          (ref PlayerInput input) =>
                                          {
                                              input.Horizontal = Math.Clamp(input.Horizontal + horizontal, -1, 1);
                                              input.Vertical = Math.Clamp(input.Vertical + vertical, -1, 1);
                                          });
                         } while (eventsRead == EventsPerPeep);
                     });

        _world.Query(new QueryDescription().WithAll<Window>(),
                     (ref Window window, ref SDL3Window sdl3Window) =>
                     {
                         var width = 0;
                         var height = 0;
                         SDL_GetWindowSizeInPixels(sdl3Window.window, &width, &height);
                         window.Width = width;
                         window.Height = height;
                     });

        _world.Query(new QueryDescription().WithAll<QuitRequestEvent>(),
                     (ref QuitRequestEvent e) =>
                     {
                         SDL_QuitSubSystem(InitFlags);
                         SDL_Quit();
                         _world.Create(new QuitEvent());
                     });

        _world.Query(new QueryDescription().WithAll<SDL3Window>(),
                     (ref SDL3Window w) =>
                     {
                         SDL_SetRenderDrawColor(w.renderer, 68, 174, 197, SDL_ALPHA_OPAQUE);
                         SDL_RenderClear(w.renderer);
                     });

        SDL_Renderer* renderer = null;
        SDL_Window* window = null;
        _world.Query(new QueryDescription().WithAll<SDL3Window>(),
                     (ref SDL3Window w) =>
                     {
                         renderer = w.renderer;
                         window = w.window;
                     });

        _world.Query(_rectangleQuery,
                     (ref Rectangle r, ref Position pos) =>
                     {
                         var width = 0;
                         var height = 0;
                         SDL_GetWindowSizeInPixels(window, &width, &height);
                         var rect = new SDL_FRect
                         {
                             h = r.Height, w = r.Width, x = pos.X, y = height - pos.Y - r.Height
                         };

                         SDL_SetRenderDrawColor(renderer, 255, 255, 255, SDL_ALPHA_OPAQUE);
                         SDL_RenderFillRect(renderer, &rect);
                     });

        _world.Query(new QueryDescription().WithAll<SDL3Window>(),
                     static (ref SDL3Window w) => { SDL_RenderPresent(w.renderer); });
    }
}