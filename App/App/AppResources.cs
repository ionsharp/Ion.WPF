using Ion.Analysis;
using Ion.Controls;
using Ion.Reflect;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;

namespace Ion.Core;

public sealed class AppResources : ResourceDictionary
{
    public const string FormatStyle = "Style/{0}.xaml";

    public const string FormatTheme = "Theme/Default/{0}.xaml";

    /// <see cref="ThemeDictionary"/>
    #region

    [Styles.Object(Image = Images.Palette, Name = "Theme")]
    public class ThemeDictionary : ResourceDictionary { }

    #endregion

    /// <see cref="ThemeResource"/>
    #region

    private class ThemeResource : Dictionary<DefaultThemes, Uri>
    {
        public ThemeResource(string assemblyName) => typeof(DefaultThemes).GetEnumValues().Cast<DefaultThemes>().ForEach(i => Add(i, Resource.GetUri(assemblyName, FormatTheme.F(i))));
    }

    #endregion

    /// <see cref="Region.Property"/>
    #region

    public ThemeDictionary ActiveTheme { get; private set; }

    private Collection<ThemeResource> DefaultResources { get; set; } = [];

    #endregion

    /// <see cref="Region.Constructor"/>
    #region

    public AppResources() : base()
    {
        /// Themes
        DefaultResources.Add(new(AssemblyData.Name));

        /// Styles
        Application.Current.Resources.MergedDictionaries.Add(this);
        typeof(StyleKeys).GetEnumValues().Cast<StyleKeys>().ForEach(i => Application.Current.Resources.MergedDictionaries.Add(New(AssemblyData.Name, FormatStyle.F(i))));
    }

    #endregion

    /// <see cref="Region.Method"/>
    #region

    public void LoadTheme(DefaultThemes theme) => LoadTheme($"{theme}");

    public void LoadTheme(string theme)
    {
        BeginInit();
        MergedDictionaries.Clear();

        if (!Enum.TryParse(theme, out DefaultThemes type))
        {
            XResourceDictionary.TryDeserialize(theme, out ResourceDictionary result);
            if (result != null)
            {
                MergedDictionaries.Add(result);
                goto End;
            }
        }
        foreach (var i in DefaultResources)
        {
            var result = new ThemeDictionary { Source = i[type] };
            MergedDictionaries.Add(result);
        }

    End:
        {
            EndInit();
            ActiveTheme = MergedDictionaries.Count > 0 ? (ThemeDictionary)MergedDictionaries[0] : null;
        }
    }

    #endregion

    /// <see cref="Region.Method.Static"/>
    #region

    public static ResourceDictionary Load(string filePath)
    {
        var result = default(ResourceDictionary);
        using (var fileStream = File.OpenRead(filePath))
        {
            fileStream.Seek(0, SeekOrigin.Begin);
            result = (ResourceDictionary)XamlReader.Load(fileStream);
        }
        return result;
    }

    public static ResourceDictionary Load(Uri fileUri)
    {
        using var stream = Application.GetResourceStream(fileUri).Stream;
        return (ResourceDictionary)XamlReader.Load(stream);
    }

    public static Result TryLoad(string filePath, out ResourceDictionary result)
    {
        try
        {
            result = Load(filePath);
            return new Success();
        }
        catch (Exception e)
        {
            result = default;
            return new Error(e);
        }
    }

    ///

    public static ResourceDictionary New(string assemblyName, string relativePath) => new() { Source = Resource.GetUri(assemblyName, relativePath) };

    public static void Save(string assemblyName, string resourcePath, string destinationPath)
    {
        using var fileStream = File.Create(destinationPath);
        using var stream = Application.GetResourceStream(Resource.GetUri(assemblyName, resourcePath)).Stream;
        stream.Seek(0, SeekOrigin.Begin);
        stream.CopyTo(fileStream);
    }

    ///

    public static Stream GetStream(Uri uri) => Application.GetResourceStream(uri).Stream;

    public static string GetText(string relativePath, AssemblyProject assembly = AssemblyProject.Main)
    {
        var uri = Resource.GetUri(relativePath, assembly);

        string result = default;
        using (var stream = Application.GetResourceStream(uri).Stream)
        {
            using var reader = new StreamReader(stream);
            string line;
            while ((line = reader.ReadLine()) != null)
                result += $"{line}\n";
        }
        return result;
    }

    #endregion
}