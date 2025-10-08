using System.Windows.Input;
using System.Windows.Media;

namespace Ion.Controls;

public class ButtonModel() : object()
{
    public SolidColorBrush Color { get; set; }

    public ICommand Command { get; set; }

    public object CommandTarget { get; set; }

    public string Content { get; set; }

    public ImageSource Image { get; set; }

    public bool IsCancel { get; set; }

    public bool IsDefault { get; set; }

    public int Result { get; set; }

    public string Tip { get; set; }

    public ButtonModel(string content, int result, bool isDefault = false, bool isCancel = false) : this()
    {
        Content = content; IsDefault = isDefault; IsCancel = isCancel; Result = result;
    }
}