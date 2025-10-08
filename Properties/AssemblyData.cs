using Ion.Reflect;

namespace Ion;

internal static class AssemblyData
{
    public const string Name = XAssembly.Name + "." + "WPF";

    public static readonly string[] ProjectNames =
    [
        XAssembly.Name,
        $"{XAssembly.Name}.Color",
        $"{XAssembly.Name}.{nameof(Effects)}",
        $"{XAssembly.Name}.{nameof(Imaging)}",
        $"{XAssembly.Name}.{nameof(Input)}",
        $"{XAssembly.Name}.{nameof(Windows)}", 
        Name,
    ];
}