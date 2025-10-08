using System;

namespace Ion.Reflect;

[Flags]
public enum MemberLogType
{
    None = 0,
    Style = 1,
    StyleModel = 2,
    Value = 4,
    All = Style | StyleModel | Value
}