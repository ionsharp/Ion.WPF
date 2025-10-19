using System;

namespace Ion.Storage;

[Flags]
public enum ItemProperty
{
    None = 0,
    Accessed = 1,
    Created = 2,
    Modified = 4,
    Name = 8,
    Type = 16,
    Size = 32
}