using Ion.Reflect;
using System;

namespace Ion.Controls;

public static class Resource
{
    /// <see cref="Region.Field"/>

    public const string PathImage = "Image";

    public const string PathImageCore = PathImage + "/{0}/{1}.png";

    public const string PathImageMain = PathImage + "/{0}";

    public const string PathFont = "{0}#{1}";

    public const string Pack = "pack://application:,,,/{0};component/{1}";

    /// <see cref="Region.Method"/>

    public static Uri GetFontUri(string fontPath, string fontName, AssemblyProject assembly = AssemblyProject.WPF)
        => GetUri(assembly.GetName(), PathFont.F(fontPath, fontName));

    public static string GetImage(object name, AssemblyProject assembly = AssemblyProject.Main)
        => GetImageUri(name, assembly)?.OriginalString;

    public static string GetImage(object name, AssemblyProject assembly, ImageSize size)
        => GetImageUri(name, assembly, size)?.OriginalString;

    public static string GetImage(Images name, ImageSize size = ImageSize.Smallest)
        => GetImageUri(name, size)?.OriginalString;

    public static Uri GetImageUri(object name, AssemblyProject assembly, ImageSize size)
    {
        if (name is not null)
        {
            if (name is Images)
            {
                var i = (int)size;

                //Image/[Size]/[Size]-[Name].png

                var fileName = $"{i}-{name}";
                var folderName = $"{i}";

                //Smallest size omits prefix (optional, but requires changing file names)
                if (i == 16)
                    fileName = $"{name}";

                //All folder names have three characters (also optional, but requires changing folder names)
                if (i < 10)
                    folderName = $"00{folderName}";

                if (i > 9 && i < 100)
                    folderName = $"0{folderName}";

                return GetUri(PathImageCore.F(folderName, fileName), AssemblyProject.WPF);
            }

            if (name is string)
                return GetUri(PathImageMain.F(name), assembly);

            if (name is Uri uri)
                return uri;
        }
        return null;
    }

    public static Uri GetImageUri(object name, AssemblyProject assembly = AssemblyProject.Main)
        => GetImageUri(name, assembly, ImageSize.Smallest);

    public static Uri GetImageUri(Images name, ImageSize size = ImageSize.Smallest)
        => GetImageUri(name, AssemblyProject.WPF, size);

    public static Uri GetUri(string relativePath, AssemblyProject assembly = AssemblyProject.Main) => GetUri(assembly.GetName(), relativePath);

    public static Uri GetUri(string assemblyName, string relativePath) => new(Pack.F(assemblyName, relativePath), UriKind.Absolute);
}