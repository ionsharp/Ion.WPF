using Ion.Reflect;
using System;
using System.Collections;
using System.Windows.Controls;

namespace Ion.Controls;

public class ObjectMenu() : ContextMenu(), IObjectControl<ContextMenu>
{
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        base.OnItemsSourceChanged(oldValue, newValue);
        if (newValue is not null)
        {
            var a
                = XObjectControl.GetSourceModel(this);
            var b
                = a?.StyleModel as TemplateModelObject;

            if (!ReferenceEquals(newValue, b?.Default.View))
                throw new NotSupportedException();
        }
    }
}