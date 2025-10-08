using System;

namespace Ion;

[Flags]
public enum Option
{
    None = 0,
    Copy = 1,
    Default = 2,
    Edit = 4,
    Paste = 8,
    Replace = 16,
    Reset = 32,
    Revert = 64,
    Unset = 128,
    All = Copy | Default | Edit | Paste | Replace | Reset | Revert | Unset
}