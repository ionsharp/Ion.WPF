using Ion.Core;
using System.Windows;

namespace Ion.Controls;

internal sealed class DockDragReference(Content[] content, Point start)
{
    public readonly Content[] Content
            = content;

    public readonly Point Start
            = start;
}