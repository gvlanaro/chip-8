﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Rendering;
using Avalonia.Threading;
using chip_8.Common;
using OpenTK.Graphics.OpenGL4;

namespace chip_8;

public abstract class BaseTkOpenGlControl : OpenGlControlBase, ICustomHitTest
{
    private AvaloniaTkContext? _avaloniaTkContext;

    private readonly Dictionary<Key, byte> Emu_Keys = new()
    {
        { Key.D1, 0x1 },
        { Key.D2, 0x2 },
        { Key.D3, 0x3 },
        { Key.D4, 0xC },
        { Key.Q, 0x4 },
        { Key.W, 0x5 },
        { Key.E, 0x6 },
        { Key.R, 0xD },
        { Key.A, 0x7 },
        { Key.S, 0x8 },
        { Key.D, 0x9 },
        { Key.F, 0xE },
        { Key.Z, 0xA },
        { Key.X, 0x0 },
        { Key.C, 0xB },
        { Key.V, 0xF }
    };

    private GlInterface gl;

    /// <summary>
    ///     KeyboardState provides an easy-to-use, stateful wrapper around Avalonia's Keyboard events, as OpenTK keyboard
    ///     states are not handled.
    ///     You can access full keyboard state for both the current frame and the previous one through this object.
    /// </summary>
    public AvaloniaKeyboardState KeyboardState = new();

    public bool HitTest(Point point)
    {
        return Bounds.Contains(point);
    }

    /// <summary>
    ///     OpenTkRender is called once a frame to draw to the control.
    ///     You can do anything you want here, but make sure you undo any configuration changes after, or you may get weirdness
    ///     with other controls.
    /// </summary>
    protected virtual void OpenTkRender()
    {
    }

    /// <summary>
    ///     OpenTkInit is called once when the control is first created.
    ///     At this point, the GL bindings are initialized and you can invoke GL functions.
    ///     You could use this function to load and compile shaders, load textures, allocate buffers, etc.
    /// </summary>
    protected virtual void OpenTkInit()
    {
    }

    /// <summary>
    ///     OpenTkTeardown is called once when the control is destroyed.
    ///     Though GL bindings are still valid, as OpenTK provides no way to clear them, you should not invoke GL functions
    ///     after this function finishes executing.
    ///     At best, they will do nothing, at worst, something could go wrong.
    ///     You should use this function as a last chance to clean up any GL resources you have allocated - delete buffers,
    ///     vertex arrays, programs, and textures.
    /// </summary>
    protected virtual void OpenTkTeardown()
    {
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        this.gl = gl;

        //Update last key states
        KeyboardState.OnFrame();

        var size = GetPixelSize();

        //Set up the aspect ratio so shapes aren't stretched.
        GL.Viewport(0, 0, size.Width, size.Height);

        //Tell our subclass to render
        if (Bounds.Width != 0 && Bounds.Height != 0) OpenTkRender();

        //Schedule next UI update with avalonia
        Dispatcher.UIThread.Post(RequestNextFrameRendering, DispatcherPriority.Background);
    }


    protected sealed override void OnOpenGlInit(GlInterface gl)
    {
        //Initialize the OpenTK<->Avalonia Bridge
        _avaloniaTkContext = new AvaloniaTkContext(gl);
        GL.LoadBindings(_avaloniaTkContext);
        //Invoke the subclass' init function
        OpenTkInit();
    }

    //Simply call the subclass' teardown function
    protected sealed override void OnOpenGlDeinit(GlInterface gl)
    {
        OpenTkTeardown();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!IsEffectivelyVisible)
            return;

        try
        {
            if (e.Key == Key.P) MessageBroker.SendPause(null);
            if (e.Key == Key.O) MessageBroker.SendRestart(null);
            MainWindowGLRendering.emulator.Keys[Emu_Keys[e.Key]] = true;
        }
        catch (Exception)
        {
            Debug.WriteLine("wrong button up");
        }

        KeyboardState.SetKey(e.Key, true);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (!IsEffectivelyVisible)
            return;

        try
        {
            MainWindowGLRendering.emulator.Keys[Emu_Keys[e.Key]] = false;
        }
        catch (Exception)
        {
            Debug.WriteLine("wrong button up");
        }

        KeyboardState.SetKey(e.Key, false);
    }


    public GlInterface getGLInterface()
    {
        return gl;
    }

    private PixelSize GetPixelSize()
    {
        var scaling = TopLevel.GetTopLevel(this).RenderScaling;
        return new PixelSize(Math.Max(1, (int)(Bounds.Width * scaling)),
            Math.Max(1, (int)(Bounds.Height * scaling)));
    }
}