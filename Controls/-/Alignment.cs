using Ion.Core;
using Ion.Reflect;
using System;
using System.Windows;

namespace Ion;

[Styles.Object(GroupName = MemberGroupName.None)]
[Serializable]
public record class Alignment() : Model()
{
    public static Alignment Center => new(HorizontalAlignment.Center, VerticalAlignment.Center);

    [Style(Orientation = Orient.Horizontal)]
    public HorizontalAlignment Horizontal { get => Get(HorizontalAlignment.Left); set => Set(value); }

    [Style(Orientation = Orient.Horizontal)]
    public VerticalAlignment Vertical { get => Get(VerticalAlignment.Top); set => Set(value); }

    public Alignment(HorizontalAlignment horizontal, VerticalAlignment vertical) : this()
    {
        Horizontal = horizontal; Vertical = vertical;
    }
}