using System;

namespace Ion.Core;

[Serializable]
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