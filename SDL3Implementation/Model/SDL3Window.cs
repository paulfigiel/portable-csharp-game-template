namespace MyGame.Sdl3.Model;

using SDL;

public struct SDL3Window
{
    public unsafe SDL_Window* window;
    public unsafe SDL_Renderer* renderer;
}