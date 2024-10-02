using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class Window : GameWindow
{

    private float[] _vertices =
    {
        0.5f,  0.5f, 0.0f, // top right
        0.5f, -0.5f, 0.0f, // bottom right
        -0.5f, -0.5f, 0.0f, // bottom left
        -0.5f,  0.5f, 0.0f, // top left
    };

    // Then, we create a new array: indices.
    // This array controls how the EBO will use those vertices to create triangles
    private uint[] _indices =
    {
        // Note that indices start at 0!
        0, 1, 3, // The first triangle will be the top-right half of the triangle
        1, 2, 3  // Then the second will be the bottom-left half of the triangle
    };

    private int _vertexBufferObject;

    private int _vertexArrayObject;

    private Shader _shader;

    private int _elementBufferObject;

    private Emulator emulator;
    public Window(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title }) 
    { 
        emulator = new Emulator("roms/ibm_logo.ch8");
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

        //Code goes here

        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

        _vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayObject);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        // We create/bind the Element Buffer Object EBO the same way as the VBO, except there is a major difference here which can be REALLY confusing.
        // The binding spot for ElementArrayBuffer is not actually a global binding spot like ArrayBuffer is. 
        // Instead it's actually a property of the currently bound VertexArrayObject, and binding an EBO with no VAO is undefined behaviour.
        // This also means that if you bind another VAO, the current ElementArrayBuffer is going to change with it.
        // Another sneaky part is that you don't need to unbind the buffer in ElementArrayBuffer as unbinding the VAO is going to do this,
        // and unbinding the EBO will remove it from the VAO instead of unbinding it like you would for VBOs or VAOs.
        _elementBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
        // We also upload data to the EBO the same way as we did with VBOs.
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);
        // The EBO has now been properly setup. Go to the Render function to see how we draw our rectangle now!

        _shader = new Shader("shaders/shader.vert", "shaders/shader.frag");
        _shader.Use();
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        _shader.Dispose();
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        //Code goes here.
        _shader.Use();

        // Because ElementArrayObject is a property of the currently bound VAO,
        // the buffer you will find in the ElementArrayBuffer will change with the currently bound VAO.
        GL.BindVertexArray(_vertexArrayObject);

        // Then replace your call to DrawTriangles with one to DrawElements
        // Arguments:
        //   Primitive type to draw. Triangles in this case.
        //   How many indices should be drawn. Six in this case.
        //   Data type of the indices. The indices are an unsigned int, so we want that here too.
        //   Offset in the EBO. Set this to 0 because we want to draw the whole thing.
        GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

        SwapBuffers();
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
