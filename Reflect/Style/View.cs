using System;

namespace Ion;

[Flags]
public enum View
{
    None = 0,
    Footer = 1,
    Header = 2,
    HeaderItem = 4,
    HeaderOption = 8,
    ItemOption = 16,
    Main = 32,
    Option = 64,
    All = Header | HeaderItem | HeaderOption | ItemOption | Main | Footer | Option
}