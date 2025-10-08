using System.Windows.Controls;

namespace Ion.Controls;

[Extend<Label>]
public static class XLabel
{
    public static readonly ResourceKey Accent = new();

    public static readonly ResourceKey AccentLight = new();

    public static readonly ResourceKey AccentLightMedium = new();

    public static readonly ResourceKey AccentMediumLight = new();

    public static readonly ResourceKey AccentMedium = new();

    public static readonly ResourceKey AccentMediumDark = new();

    public static readonly ResourceKey AccentDarkMedium = new();

    public static readonly ResourceKey AccentDark = new();

    public static readonly ResourceKey Header = new();

    public static readonly ResourceKey Header1 = new();

    public static readonly ResourceKey Header2 = new();

    public static readonly ResourceKey Header3 = new();

    public static readonly ResourceKey Header4 = new();

    public static readonly ResourceKey Header5 = new();

    public static readonly ResourceKey Header6 = new();

    public static readonly ResourceKey Header7 = new();
}