using System.Windows;

namespace Ion.Controls;

public interface IControl
{
    bool IsEnabled { get; set; }

    object GetValue(DependencyProperty property);

    void SetValue(DependencyProperty property, object value);

    void SetValue(DependencyPropertyKey property, object value);
}