using System.Windows;

namespace Ion.Storage;

public delegate void PathChangedEventHandler(object sender, PathChangedEventArgs e);

public class PathChangedEventArgs(RoutedEvent input, string path) : RoutedEventArgs(input)
{
    public readonly string Path = path;
}