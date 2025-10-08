using Ion.Analysis;
using System;

namespace Ion.Core;

public enum UnhandledExceptions
{
    AppDomain, Dispatcher, TaskScheduler
}

public delegate void UnhandledExceptionEventHandler(object sender, UnhandledExceptionEventArgs e);

public class UnhandledExceptionEventArgs(UnhandledExceptions type, Error error) : EventArgs()
{
    public readonly Error Error = error;

    public readonly UnhandledExceptions Type = type;
}