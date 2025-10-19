using System;

namespace Ion.Core;

public enum AppExitMethod
{
    None,
    Exit,
    Hibernate,
    Lock,
    LogOff,
    Restart,
    Shutdown,
    Sleep
}