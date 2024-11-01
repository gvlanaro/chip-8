using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia.Input;
using chip_8.Common;
using Newtonsoft.Json;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace chip_8
{
    public class MainWindowGLRendering : BaseTkOpenGlControl
    {

        private const int width = 640;
        private const int height = 320;
        private int VBO;
        private int VAO;
        private int EBO;
        private Shader _shader;
        private Matrix4 _projection;
        public static Emulator emulator;
        private bool isWindows;
        private Dictionary<Key, byte> Emu_Keys = new Dictionary<Key, byte>
        {
            {Key.D1, 0x1},
            {Key.D2, 0x2},
            {Key.D3, 0x3},
            {Key.D4, 0xC},
            {Key.Q, 0x4},
            {Key.W, 0x5},
            {Key.E, 0x6},
            {Key.R, 0xD},
            {Key.A, 0x7},
            {Key.S, 0x8},
            {Key.D, 0x9},
            {Key.F, 0xE},
            {Key.Z, 0xA},
            {Key.X, 0x0},
            {Key.C, 0xB},
            {Key.V, 0xF}
        };

        public string rom_path;
        private float[] bg_color;
        private float[] pixel_color;
        public string beep_sound;

        public MainWindowGLRendering()
        {
            MessageBroker.RestartReceived += OnRestartReceived;
        }

        private void OnRestartReceived(object? sender, EventArgs e)
        {
            RestartEmulator();
        }

        private void OnKeyUpReceived(object? sender, Key key)
        {
            emulator.Keys[Emu_Keys[key]] = false;
        }

        private void OnKeyDownReceived(object? sender, Key key)
        {
            if (key == Key.P)
            {
                RestartEmulator();
            }
            emulator.Keys[Emu_Keys[key]] = true;
        }
        protected override void OpenTkInit()
        {
            bg_color = new float[3];
            pixel_color = new float[3];

            // used for beep sound
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                isWindows = true;
            }
            else
            {
                isWindows = false;
            }

            // loads user settings
            LoadJson();

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
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // chip8 runs at 600Hz (in this case 10 every frame with 60fps)
            for (int i = 0; i < 10; i++)
            {
                emulator.Cycle();
            }

            CheckSoundTimer();
            DrawEmulatorScreen();
            
            _shader.SetVector3("color", new Vector3(pixel_color[0], pixel_color[1], pixel_color[2]));
            GL.ClearColor(bg_color[0], bg_color[1], bg_color[2], 1.0f);
        }
        public void DrawEmulatorScreen()
        {
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
        }
        private void CheckSoundTimer()
        {
            if (emulator.Sound_timer > 0)
            {
                //Beep_wav();
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
        public void Beep_wav()
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
        public void LoadJson()
        {
            using (StreamReader r = new StreamReader("settings.json"))
            {
                string json = r.ReadToEnd();
                dynamic array = JsonConvert.DeserializeObject(json);
                rom_path = array["rom_path"];

                for (int i = 0; i < 3; i++)
                {
                    bg_color[i] = array["bg_color"][i];
                    pixel_color[i] = array["pixel_color"][i];
                }
                beep_sound = array["beep_sound"];
            }
        }
        public void RestartEmulator()
        {
            emulator = new Emulator(rom_path);
        }
        public void changeBgColor(float[] color)
        {
            bg_color = color;
        }

        public void changePixelColor(float[] color)
        {
            pixel_color = color;
        }
    }
}
