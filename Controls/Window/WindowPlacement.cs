using System;

namespace Ion.Controls;

[Description("Specifies how a window is placed relative to other windows.")]
public enum WindowPlacement
{
    [Description("The window may move above or below other windows.")]
    None,
    [Description("The window stays below all other windows.")]
    Bottom,
    [Description("The window stays above all other windows.")]
    Top
}