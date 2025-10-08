using Ion.Collect;
using Ion.Text;
using System.Windows;

namespace Ion.Controls;

/// <inheritdoc/>
public partial class FileView : ViewControl
{
    public static readonly DependencyProperty BulletProperty = DependencyProperty.Register("Bullet", typeof(Bullet), typeof(FileView), new PropertyMetadata(Bullet.Circle));
    public Bullet Bullet
    {
        get => (Bullet)GetValue(BulletProperty);
        set => SetValue(BulletProperty, value);
    }

    public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(ListObservableOfString), typeof(FileView), new PropertyMetadata(null));
    public ListObservableOfString Items
    {
        get => (ListObservableOfString)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    /// <inheritdoc/>
    public FileView() : base() { }
}