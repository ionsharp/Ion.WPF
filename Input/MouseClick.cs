using System;

namespace Ion.Input;

[Flags]
public enum MouseClick
{
    None = 0,
    Single = 1,
    Double = 2,
    Triple = 4,
    All = Single | Double | Triple
}