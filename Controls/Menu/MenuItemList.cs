using Ion.Collect;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Ion.Controls;

/// <inheritdoc/>
public class MenuItemList : ListObservable<MenuItem>
{
    /// <inheritdoc/>
    public MenuItemList() : base() { }

    /// <inheritdoc/>
    public MenuItemList(params MenuItem[] i) : base(i) { }

    /// <inheritdoc/>
    public MenuItemList(IEnumerable<MenuItem> i) : base(i) { }
}