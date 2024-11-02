using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Input.TextInput;
using chip_8.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace chip_8;

public class MainWindowGLRendering : BaseTkOpenGlControl
{
    private const int width = 640;
    private const int height = 320;

    public static Emulator emulator;
    private readonly uint[] _indices = [0, 1, 3, 1, 2, 3];
    private Matrix4 _projection;
    private Shader _shader;
    public string beep_sound;
    private float[] bg_color;
    private int EBO;
    private bool isWindows;
    private float[] pixel_color;

    public string rom_path;
    public bool sound_toggle;
    private int VAO;
    private int VBO;
    private bool pause;
    public MainWindowGLRendering()
    {
        MessageBroker.RestartReceived += OnRestartReceived;
        MessageBroker.PauseReceived += OnPauseReceived;
    }

    private void OnPauseReceived(object? sender, EventArgs e)
    {
        PauseEmulator();
    }

    private void OnRestartReceived(object? sender, EventArgs e)
    {
        RestartEmulator();
    }

    protected override void OpenTkInit()
    {
        bg_color = new float[3];
        pixel_color = new float[3];

        // used for beep sound
        isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        // loads user settings
        rom_path = "assets/ibm_test.ch8";
        bg_color = [0.0f, 0.0f, 0.0f];
        pixel_color = [1.0f, 1.0f, 1.0f];
        beep_sound = "assets/beep.wav";
        sound_toggle = true;
        
        emulator = new Emulator(rom_path);
        _shader = new Shader("shaders/shader.vert", "shaders/shader.frag");

        GL.ClearColor(bg_color[0], bg_color[1], bg_color[2], 1.0f);

        VBO = GL.GenBuffer();
        VAO = GL.GenVertexArray();
        EBO = GL.GenBuffer();

        // convert opengl coordinates (-1 to 1) to (0 to width/height)
        _projection = Matrix4.CreateOrthographicOffCenter(0.0f, width, height, 0.0f, -1.0f, 1.0f);
        _shader.SetMatrix4("projection", _projection);
        _shader.SetVector3("color", new Vector3(pixel_color[0], pixel_color[1], pixel_color[2]));
    }

    //OpenTkRender (OnRenderFrame) is called once a frame. The aspect ratio and keyboard state are configured prior to this being called.
    protected override void OpenTkRender()
    {
        if (pause)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            DrawEmulatorScreen();
            _shader.SetVector3("color", new Vector3(pixel_color[0], pixel_color[1], pixel_color[2]));
            GL.ClearColor(bg_color[0], bg_color[1], bg_color[2], 1.0f);
            return;
        }
        
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        

        
        // chip8 runs at 600Hz (in this case 10 every frame with 60fps)
        for (var i = 0; i < 10; i++) emulator.Cycle();

        CheckSoundTimer();
        DrawEmulatorScreen();
        


        _shader.SetVector3("color", new Vector3(pixel_color[0], pixel_color[1], pixel_color[2]));
        GL.ClearColor(bg_color[0], bg_color[1], bg_color[2], 1.0f);
    }

    private void DrawEmulatorScreen()
    {
        for (var x = 0; x < 64; x++)
        for (var y = 0; y < 32; y++)
            if (emulator.Display[x, y] == 1)
                DrawRect(x, y);
    }

    private void CheckSoundTimer()
    {
        if (emulator.Sound_timer > 0 && sound_toggle) Beep_wav();
    }

    private void DrawRect(int x, int y)
    {
        // 64 to 640 and 32 to 320
        x *= 10;
        y *= 10;

        // preparations
        // _vertices length = 12 (top right, bottom right, bottom left, top left)
        GL.BufferData(BufferTarget.ArrayBuffer, 12 * sizeof(float),
            [x + 10, y + 10, 0.0f, x + 10, y, 0.0f, x, y, 0.0f, x, y + 10, 0.0f], BufferUsageHint.StaticDraw);
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

        GL.BindVertexArray(VAO);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices,
            BufferUsageHint.StaticDraw);

        _shader.Use();

        GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
    }

    private void Beep_wav()
    {
        if (isWindows)
        {
            // disabled by default because it stops the process for too long
            //Console.Beep();
        }
        else
        {
            try
            {
                Process.Start("aplay", beep_sound);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    public void RestartEmulator()
    {
        emulator = new Emulator(rom_path);
    }

    public void ChangeBgColor(float[] color)
    {
        bg_color = color;
    }

    public void ChangePixelColor(float[] color)
    {
        pixel_color = color;
    }

    public void PauseEmulator()
    {
        pause = !pause;
    }
}