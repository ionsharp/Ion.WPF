using System;

namespace Ion;

[Flags]
public enum Filter
{
    None = 0,
    Group = 1,
    Route = 2,
    Search = 4,
    Show = 8,
    Sort = 16,
    All = Group | Route | Search | Show | Sort
}