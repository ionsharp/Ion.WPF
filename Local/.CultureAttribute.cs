using System;

namespace Ion.Local;

[AttributeUsage(AttributeTargets.Field)]
public sealed class CultureAttribute(string code) : Attribute()
{
    public readonly string Code = code;
}