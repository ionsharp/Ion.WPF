using System;

namespace Ion.Storage;

public enum ItemAttributes
{
    [Hide]
    None = 0,
    Hidden = 1,
    ReadOnly = 2,
    [Hide]
    All = Hidden | ReadOnly
}