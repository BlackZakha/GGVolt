using System;
using Avalonia.Threading;

namespace GGVolt.Client.Helpers;

public static class MainThreadHelper
{
    public static void ExecuteOnUIThread(Action action)
    {
        Dispatcher.UIThread.Post(action);
    }
}