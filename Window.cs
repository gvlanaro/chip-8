using System.ComponentModel.DataAnnotations;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class Window(int width, int height, string title, Emulator emulator) : GameWindow(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = (width, height), Title = title })
{
    private Emulator emulator;
    protected override void OnUpdateFrame(FrameEventArgs e)
    {    
        base.OnUpdateFrame(e);
        emulator.Cycle();

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }
    }
}

//TODO https://opentk.net/learn/chapter1/2-hello-triangle.html?tabs=onload-opentk4%2Conrender-opentk4%2Cresize-opentk4
