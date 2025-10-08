using System.Windows.Controls;

namespace Ion.Controls;

public class TokenBoxButton() : Button()
{
    internal TokenBoxButton(object content) : this() => Content = content;
}