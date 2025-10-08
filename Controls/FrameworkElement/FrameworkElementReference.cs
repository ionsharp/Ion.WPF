using System.Windows;

namespace Ion.Controls;

public interface IFrameworkElementReference
{
    void SetReference(IFrameworkElementKey key, FrameworkElement element);
}