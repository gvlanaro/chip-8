using System;
using Avalonia.OpenGL;
using OpenTK;

namespace chip_8;

/// <summary>
///     Wrapper to expose GetProcAddress from Avalonia in a manner that OpenTK can consume.
/// </summary>
internal class AvaloniaTkContext : IBindingsContext
{
    private readonly GlInterface _glInterface;

    public AvaloniaTkContext(GlInterface glInterface)
    {
        _glInterface = glInterface;
    }

    public IntPtr GetProcAddress(string procName)
    {
        return _glInterface.GetProcAddress(procName);
    }
}