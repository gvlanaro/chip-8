using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using System.IO.Compression;

public class Window : GameWindow
{

    private float[] _vertices =
    {
        20.0f, 10.0f, 0.0f, // top right
        20.0f, 0.0f, 0.0f, // bottom right
        10.0f, 0.0f, 0.0f, // bottom left
        10.0f, 10.0f, 0.0f, // top left
    };

    // Then, we create a new array: indices.
    // This array controls how the EBO will use those vertices to create triangles
    private uint[] _indices =
    {
        // Note that indices start at 0!
        0, 1, 3, // The first triangle will be the top-right half of the triangle
        1, 2, 3  // Then the second will be the bottom-left half of the triangle
    };

    private const int width = 640;
    private const int height = 320;

    private int _vertexBufferObject;

    private int _vertexArrayObject;

    private Shader _shader;

    private int _elementBufferObject;

    private Matrix4 _projection;
    
    private Emulator emulator;
    public Window(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title }) 
    { 
        emulator = new Emulator("roms/ibm_logo.ch8");
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

        _vertexBufferObject = GL.GenBuffer();
        
        _vertexArrayObject = GL.GenVertexArray();

        _elementBufferObject = GL.GenBuffer();

        _shader = new Shader("shaders/shader.vert", "shaders/shader.frag");
        _projection = Matrix4.CreateOrthographicOffCenter(0.0f, width, 0.0f, height, -1.0f, 1.0f);
        _shader.SetMatrix4("projection", _projection);
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        //Code goes here.
        emulator.Cycle();

        // dopo ogni ciclo controllo la variabile Display dell'emulatore e per ogni '1' creo il 'pixel'
        // le dimensioni di display sono fisse (640,320) poi vengono scalate automaticamente tramite Viewport
        for (int x = 0; x < 64; x++)
        {   
            for (int y = 0; y < 32; y++)
            {
                // draw pixel/rectangle (white)
                if (emulator.Display[x,y] == 1)
                {
                    drawRect(x,y);
                }
            }
        }

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



        // preparations
        //_vertexBufferObject = GL.GenBuffer();
        //GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

        //_vertexArrayObject = GL.GenVertexArray();
        //GL.BindVertexArray(_vertexArrayObject);
        GL.BindVertexArray(_vertexArrayObject);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
                
        // We create/bind the Element Buffer Object EBO the same way as the VBO, except there is a major difference here which can be REALLY confusing.
        // The binding spot for ElementArrayBuffer is not actually a global binding spot like ArrayBuffer is. 
        // Instead it's actually a property of the currently bound VertexArrayObject, and binding an EBO with no VAO is undefined behaviour.
        // This also means that if you bind another VAO, the current ElementArrayBuffer is going to change with it.
        // Another sneaky part is that you don't need to unbind the buffer in ElementArrayBuffer as unbinding the VAO is going to do this,
        // and unbinding the EBO will remove it from the VAO instead of unbinding it like you would for VBOs or VAOs.
        //_elementBufferObject = GL.GenBuffer();
        //GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
        // We also upload data to the EBO the same way as we did with VBOs.
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);
        // The EBO has now been properly setup. Go to the Render function to see how we draw our rectangle now!


        _shader.Use();

        

        // Then replace your call to DrawTriangles with one to DrawElements
        // Arguments:
        //   Primitive type to draw. Triangles in this case.
        //   How many indices should be drawn. Six in this case.
        //   Data type of the indices. The indices are an unsigned int, so we want that here too.
        //   Offset in the EBO. Set this to 0 because we want to draw the whole thing.
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
            GL.DeleteVertexArray(_vertexArrayObject);
            GL.DeleteBuffer(_vertexArrayObject);
            GL.DeleteBuffer(_elementBufferObject);
            Close();
        }
    }
}
