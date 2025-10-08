using GongSolutions.Wpf.DragDrop;
using Ion.Input;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace Ion;

/// <see cref="Region.Constructor"/>
public class DropHandler<Element, Data>(Element parent) : IDropTarget where Element : UIElement
{
    /// <see cref="Region.Field"/>

    public readonly Element Parent = parent;

    /// <see cref="Region.Method.Protected"/>
    #region

    protected virtual DragDropEffects DragOver(IEnumerable<Data> source, UIElement target)
    {
        if (ModifierKeys.Control.Pressed() || ModifierKeys.Shift.Pressed())
            return DragDropEffects.Copy;

        return DragDropEffects.Move;
    }

    protected virtual void Drop(IEnumerable<Data> source, UIElement target) { }

    protected virtual IEnumerable<Data> From(object x)
    {
        if (x is Data)
        {
            yield return (Data)x;
        }
        else
        if (x is IList<Data> y && y.Any())
        {
            foreach (var i in y)
                yield return i;
        }
        else
        if (x is IEnumerable<Data> z && z.Any())
        {
            foreach (var i in z)
                yield return i;
        }
    }

    protected virtual IEnumerable<Data> From(DataObject dataObject) { yield break; }

    protected virtual bool Droppable(UIElement input) => input is Element;

    #endregion

    /// <see cref="Region.Method.Public"/>

    public void DragOver(IDropInfo info)
    {
        var source = info.Data is DataObject i ? From(i) : From(info.Data);
        if (source.Any())
        {
            if (Droppable(info.VisualTarget))
            {
                var result = DragOver(source, info.VisualTarget);
                info.Effects = result;
                return;
            }
        }
        info.Effects = DragDropEffects.None;
    }

    public void Drop(IDropInfo info)
    {
        var source = info.Data is DataObject i ? From(i) : From(info.Data);
        if (source.Any())
        {
            if (Droppable(info.VisualTarget))
                Drop(source, info.VisualTarget);
        }
    }
}