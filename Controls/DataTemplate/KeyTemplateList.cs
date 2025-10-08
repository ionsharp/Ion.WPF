using Ion.Collect;
using System.Collections.Generic;

namespace Ion.Controls;

/// <inheritdoc/>
public class KeyTemplateList : ListObservable<KeyTemplate>
{
    /// <inheritdoc/>
    public KeyTemplateList() : base() { }

    /// <inheritdoc/>
    public KeyTemplateList(params KeyTemplate[] i) : base(i) { }

    /// <inheritdoc/>
    public KeyTemplateList(IEnumerable<KeyTemplate> i) : base(i) { }
}