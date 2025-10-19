using System;

namespace Ion.Controls;

[Flags]
public enum PopupTriggers
{
    [Hide]
    None = 0,
    GotFocus = 1,
    GotKeyboardFocus = 2,
    TextChanged = 4,
    [Hide]
    All = GotFocus | GotKeyboardFocus | TextChanged
}