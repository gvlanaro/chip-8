using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
public class Window : GameWindow
{
    private const int width = 640;
    private const int height = 320;
    private int VBO;
    private int VAO;
    private int EBO;
    private Shader _shader;
    private Matrix4 _projection;
    private Emulator emulator;
    private Dictionary<int, byte> Emu_Keys = new Dictionary<int, byte>
    {
        {10, 0x1},
        {11, 0x2},
        {12, 0x3},
        {13, 0xC},
        {24, 0x4},
        {25, 0x5},
        {26, 0x6},
        {27, 0xD},
        {38, 0x7},
        {39, 0x8},
        {40, 0x9},
        {41, 0xE},
        {52, 0xA},
        {53, 0x0},
        {54, 0xB},
        {55, 0xF}
    };
    public Window(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = (width, height), Title = title })
    {
        emulator = new Emulator("roms/test5.ch8");
        _shader = new Shader("shaders/shader.vert", "shaders/shader.frag");
    }

    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        emulator.Keys[Emu_Keys[e.ScanCode]] = true;
    }

    protected override void OnKeyUp(KeyboardKeyEventArgs e)
    {
        emulator.Keys[Emu_Keys[e.ScanCode]] = false;
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

        VBO = GL.GenBuffer();
        VAO = GL.GenVertexArray();
        EBO = GL.GenBuffer();

        _projection = Matrix4.CreateOrthographicOffCenter(0.0f, width, height, 0.0f, -1.0f, 1.0f);
        _shader.SetMatrix4("projection", _projection);
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        for (int i = 0; i < 10; i++)
        {
            emulator.Cycle();
        }

        CheckSoundTimer();

        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                if (emulator.Display[x, y] == 1)
                {
                    DrawRect(x, y);
                }
            }
        }

        SwapBuffers();
    }

    private void CheckSoundTimer()
    {
        if (emulator.Sound_timer > 0)
        {
            // TODO: doesn't work on linux
            Console.Beep();
        }
    }

    private void DrawRect(int x, int y)
    {
        // 64 to 640 and 32 to 320
        x *= 10;
        y *= 10;

        float[] _vertices =
        {
            x+10, y+10, 0.0f, // top right
            x+10, y,    0.0f, // bottom right
            x,    y,    0.0f, // bottom left
            x,    y+10, 0.0f, // top left
        };

        uint[] _indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        // preparations
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

        GL.BindVertexArray(VAO);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

        _shader.Use();

        GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
    }

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);
    }
    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }
    }
}