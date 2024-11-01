using System;

namespace chip_8.Common;

public class MessageBroker
{
    public static event EventHandler RestartReceived;

    public static void SendRestart(object message)
    {
        RestartReceived?.Invoke(null, new EventArgs());
    }
}