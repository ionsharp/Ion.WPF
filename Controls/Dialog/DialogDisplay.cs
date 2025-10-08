using System.Windows;

namespace Ion.Controls;

public class DisplayDialog() : Display<Window>()
{
    /// <see cref="Region.Property"/>

    private static readonly DependencyPropertyKey ActiveDialogKey = DependencyProperty.RegisterReadOnly(nameof(ActiveDialog), typeof(DialogModel), typeof(DisplayDialog), new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty ActiveDialogProperty = ActiveDialogKey.DependencyProperty;
    public DialogModel ActiveDialog
    {
        get => (DialogModel)GetValue(ActiveDialogProperty);
        set => SetValue(ActiveDialogKey, value);
    }

    private static readonly DependencyPropertyKey IsActiveKey = DependencyProperty.RegisterReadOnly(nameof(IsActive), typeof(bool), typeof(DisplayDialog), new FrameworkPropertyMetadata(false));
    public static readonly DependencyProperty IsActiveProperty = IsActiveKey.DependencyProperty;
    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveKey, value);
    }

    /// <see cref="Region.Method"/>

    internal void Close(int input)
    {
        var dialogs = XWindow.GetDialogs(Control);
        if (dialogs.Count == 0)
        {
            ActiveDialog
                = null;
            IsActive
                = false;

            XWindow.SetIsDialogActive(Control, false);
            return;
        }

        var last = dialogs.Pop();
        last.Result = input;

        ActiveDialog
            = dialogs.Count > 0 && dialogs.Peek() is DialogModel i
            ? i : null;
        IsActive
            = false;

        XWindow.SetIsDialogActive(Control, false);
        last.OnClosed?.Invoke(last.Result);
    }

    internal void Show(DialogModel input)
    {
        Control ??= this.GetParent<Window>();

        XWindow.GetDialogs(Control).Push(input);
        ActiveDialog = input;

        IsActive = true;
        XWindow.SetIsDialogActive(Control, true);
    }
}