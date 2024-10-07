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
    public Window(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = (width, height), Title = title }) 
    { 
        emulator = new Emulator("roms/ibm_logo.ch8");
        _shader = new Shader("shaders/shader.vert", "shaders/shader.frag");
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

        emulator.Cycle();
        
        for (int x = 0; x < 64; x++)
            for (int y = 0; y < 32; y++)
                if (emulator.Display[x,y] == 1)
                    drawRect(x,y);
        SwapBuffers();
    }

    private void drawRect(int x, int y)
    {
        // 64 to 640 and 32 to 320
        x*=10;
        y*=10;

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
            GL.DeleteVertexArray(VAO);
            GL.DeleteBuffer(VAO);
            GL.DeleteBuffer(EBO);
            Close();
        }
    }
}
