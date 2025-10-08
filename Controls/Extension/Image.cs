using Ion.Imaging;
using Ion.Reflect;
using System;

namespace Ion.Controls;

public class ImageExtension : UriExtension
{
    public Images? Key { get; set; }

    public ImageSize Size { get; set; } = ImageSize.Smallest;

    public ImageExtension() : base() { }

    public ImageExtension(string fileName) : base(Resource.PathImageMain.F(fileName), AssemblyProject.Main) { }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        Uri result = Key is not null ? Resource.GetImageUri(Key.Value, Size) : (Uri)base.ProvideValue(serviceProvider);
        return XImageSource.Convert(result);
    }
}