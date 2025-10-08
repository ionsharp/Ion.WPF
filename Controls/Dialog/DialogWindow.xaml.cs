using System.Windows;

namespace Ion.Controls;

public enum DialogWindowButton
{
    Done,
    YesNo
}

public partial class DialogWindow : Window
{
    public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(nameof(Image), typeof(Images), typeof(DialogWindow), new FrameworkPropertyMetadata(Images.Info));
    public Images Image
    {
        get => (Images)GetValue(ImageProperty);
        set => SetValue(ImageProperty, value);
    }

    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(nameof(Message), typeof(object), typeof(DialogWindow), new FrameworkPropertyMetadata(null));
    public object Message
    {
        get => (object)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly DependencyProperty MessageTemplateProperty = DependencyProperty.Register(nameof(MessageTemplate), typeof(DataTemplate), typeof(DialogWindow), new FrameworkPropertyMetadata(null));
    public DataTemplate MessageTemplate
    {
        get => (DataTemplate)GetValue(MessageTemplateProperty);
        set => SetValue(MessageTemplateProperty, value);
    }

    public DialogWindow() : base()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void Done(object sender, RoutedEventArgs e)
    {
        Close();
    }

    public static int Show(object message, object title, Images image, DialogWindowButton button = DialogWindowButton.Done)
    {
        var window = new DialogWindow() { Image = image, Message = message, Title = title?.ToString() };
        window.ShowDialog();
        return 0;
    }
}