using Ion.Collect;
using System.Collections.Generic;

namespace Ion.Controls;

/// <inheritdoc/>
public class ButtonList : ListObservable<ButtonModel>
{    
    /// <inheritdoc/>
    public ButtonList() : base() { }

    /// <inheritdoc/>
    public ButtonList(params ButtonModel[] i) : base(i) { }

    /// <inheritdoc/>
    public ButtonList(IEnumerable<ButtonModel> i) : base(i) { }
}