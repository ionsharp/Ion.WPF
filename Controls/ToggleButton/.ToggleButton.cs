using Ion.Data;
using Ion.Numeral;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Ion.Controls;

[Extend<ToggleButton>]
public static class XToggleButton
{
    public static readonly ResourceKey StyleKey = new();

    #region BulletSize

    public static readonly DependencyProperty BulletSizeProperty = DependencyProperty.RegisterAttached("BulletSize", typeof(MSize<double>), typeof(XToggleButton), new FrameworkPropertyMetadata(null));
    [TypeConverter(typeof(DoubleSizeTypeConverter))]
    public static MSize<double> GetBulletSize(ToggleButton i) => (MSize<double>)i.GetValue(BulletSizeProperty);
    public static void SetBulletSize(ToggleButton i, MSize<double> value) => i.SetValue(BulletSizeProperty, value);

    #endregion

    #region BulletTemplate

    public static readonly DependencyProperty BulletTemplateProperty = DependencyProperty.RegisterAttached("BulletTemplate", typeof(DataTemplate), typeof(XToggleButton), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetBulletTemplate(ToggleButton i) => (DataTemplate)i.GetValue(BulletTemplateProperty);
    public static void SetBulletTemplate(ToggleButton i, DataTemplate value) => i.SetValue(BulletTemplateProperty, value);

    #endregion

    #region Image

    public static readonly DependencyProperty ImageProperty = DependencyProperty.RegisterAttached("Image", typeof(ImageSource), typeof(XToggleButton), new FrameworkPropertyMetadata(null));
    public static ImageSource GetImage(ToggleButton i) => (ImageSource)i.GetValue(ImageProperty);
    public static void SetImage(ToggleButton i, ImageSource value) => i.SetValue(ImageProperty, value);

    #endregion
}