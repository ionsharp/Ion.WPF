using Ion.Numeral;

using System;

namespace Ion.Core;

public class AppTask(Bit dispatch, string message, Action action) : object()
{
    public readonly Action Action = action;

    public readonly bool Dispatch = dispatch;

    public readonly string Message = message;
}