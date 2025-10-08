using System.Windows.Input;

namespace Ion.Input;

[Extend<ModifierKeys>]
public static class XModifierKeys
{
    public static bool Pressed(this ModifierKeys i) => (Keyboard.Modifiers & i) != 0;
}