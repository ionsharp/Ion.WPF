using Ion.Text;
using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

public partial class TextBullet : TextBlock
{
    public static readonly DependencyProperty BulletProperty = DependencyProperty.Register(nameof(Bullet), typeof(Bullet), typeof(TextBullet), new FrameworkPropertyMetadata(Bullet.Square));
    public Bullet Bullet
    {
        get => (Bullet)GetValue(BulletProperty);
        set => SetValue(BulletProperty, value);
    }

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(TextBullet), new FrameworkPropertyMetadata(default(double)));
    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public TextBullet() => InitializeComponent();
}