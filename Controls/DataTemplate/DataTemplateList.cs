using Ion.Collect;
using System.Collections.Generic;
using System.Windows;

namespace Ion.Controls;

/// <inheritdoc/>
public class DataTemplateList : ListObservable<DataTemplate>
{
    /// <inheritdoc/>
    public DataTemplateList() : base() { }

    /// <inheritdoc/>
    public DataTemplateList(params DataTemplate[] i) : base(i) { }

    /// <inheritdoc/>
    public DataTemplateList(IEnumerable<DataTemplate> i) : base(i) { }
}