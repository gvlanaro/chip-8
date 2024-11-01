using System;

namespace chip_8.Common;

public abstract class MessageBroker
{
    public static event EventHandler RestartReceived;
    public static event EventHandler PauseReceived;
    public static void SendRestart(object message)
    {
        RestartReceived?.Invoke(null, new EventArgs());
    }

    public static void SendPause(object o)
    {
        PauseReceived?.Invoke(null, new EventArgs());
    }
}