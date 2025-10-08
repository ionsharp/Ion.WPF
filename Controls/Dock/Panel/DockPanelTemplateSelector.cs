using System.Windows;
using System.Windows.Controls.Primitives;

namespace Ion.Controls;

public class DockPanelTemplateSelector : TypeTemplateSelector, IDockSelector
{
    public sealed override Matches Match => Matches.Equal;

    private static DockControl GetParent(DependencyObject i)
        => i.GetParent<Popup>() is Popup j
         ? j.PlacementTarget.GetVisualParent<DockRootControl>()?.DockControl
         : i.GetParent<DockRootControl>()?.DockControl;

    public sealed override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        var result = base.SelectTemplate(item, container);
        if (ReferenceEquals(result, Default))
            return GetParent(container)?.DefaultPanelTemplate ?? Default;

        return result;
    }
}