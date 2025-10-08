using Ion.Reflect;

namespace Ion;

public static class AssemblyPath
{
    public const string Xmlns = "http://i.on/wpf";

    public const string Behavior
        = XAssembly.Name + "." + nameof(Ion.Behavior);

    public const string Colors
        = XAssembly.Name + "." + nameof(Ion.Colors);

    public const string Controls
        = XAssembly.Name + "." + nameof(Ion.Controls);

    public const string Core
        = XAssembly.Name + "." + nameof(Ion.Core);

    public const string Data
        = XAssembly.Name + "." + nameof(Ion.Data);

    public const string Effects
        = XAssembly.Name + "." + nameof(Ion.Effects);

    public const string Input
        = XAssembly.Name + "." + nameof(Ion.Input);

    public const string Media
        = XAssembly.Name + "." + nameof(Ion.Media);

    public const string Numeral
        = XAssembly.Name + "." + nameof(Ion.Numeral);

    public const string Reflect
        = XAssembly.Name + "." + nameof(Ion.Reflect);

    public const string Storage
        = XAssembly.Name + "." + nameof(Ion.Storage);

    public const string Text
        = XAssembly.Name + "." + nameof(Ion.Text);

    public const string Validation
        = XAssembly.Name + "." + nameof(Ion.Validation);
}