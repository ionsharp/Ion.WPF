using Ion.Collect;
using System.Collections.Generic;
using System.Windows;

namespace Ion.Controls;

/// <inheritdoc/>
public class FrameworkElementList : ListObservable<FrameworkElement>
{
    /// <inheritdoc/>
    public FrameworkElementList() : base() { }

    /// <inheritdoc/>
    public FrameworkElementList(params FrameworkElement[] i) : base(i) { }

    /// <inheritdoc/>
    public FrameworkElementList(IEnumerable<FrameworkElement> i) : base(i) { }
}