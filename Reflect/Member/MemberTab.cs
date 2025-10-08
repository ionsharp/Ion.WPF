using Ion.Core;
using System;

namespace Ion.Reflect;

public record class MemberTab(Enum source, string name, string description, string image, bool? groups, Fill layout) : Namable(name)
{
    public bool? Groups { get; private set; } = groups;

    public string Description { get; private set; } = description;

    public string Image { get; private set; } = image;

    public int Index { get; private set; }

    public Fill Layout { get; private set; } = layout;

    public Enum Source { get; private set; } = source;

    public override string ToString(string format, IFormatProvider provider) => $"{Source}";
}