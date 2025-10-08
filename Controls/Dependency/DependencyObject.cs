using Ion.Analysis;
using Ion.Core;
using Ion.Reflect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace Ion.Controls;

[Extend<DependencyObject>]
public static class XDependency
{
    /// <see cref="Region.Property"/>
    #region

    #region IsVisible

    public static readonly DependencyProperty IsVisibleProperty = DependencyProperty.RegisterAttached("IsVisible", typeof(bool), typeof(XDependency), new FrameworkPropertyMetadata(true));
    public static bool GetIsVisible(DependencyObject i) => (bool)i.GetValue(IsVisibleProperty);
    public static void SetIsVisible(DependencyObject i, bool input) => i.SetValue(IsVisibleProperty, input);

    #endregion

    #region (internal) Popup

    internal static readonly DependencyProperty PopupProperty = DependencyProperty.RegisterAttached("Popup", typeof(Popup), typeof(XDependency), new FrameworkPropertyMetadata(null));
    internal static Popup GetPopup(DependencyObject i) => (Popup)i.GetValue(PopupProperty);
    internal static void SetPopup(DependencyObject i, Popup input) => i.SetValue(PopupProperty, input);

    #endregion

    #endregion

    /// <see cref="Region.Method"/>
    #region

    #region AddChanged

    public static void AddChanged(this DependencyObject input, DependencyProperty p, EventHandler e)
        => DependencyPropertyDescriptor.FromProperty(p, input.GetType()).AddValueChanged(input, e);

    #endregion

    #region Bind

    public static BindingExpressionBase Bind(this DependencyObject source, DependencyProperty property, BindingBase binding) => BindingOperations.SetBinding(source, property, binding);

    public static BindingExpressionBase Bind(this DependencyObject input, DependencyProperty property, string path, object source, BindingMode mode = BindingMode.OneWay, IValueConverter converter = null, object converterParameter = null, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged) => input.Bind(property, new PropertyPath(path), source, mode, converter, converterParameter, updateSourceTrigger);

    public static BindingExpressionBase Bind(this DependencyObject input, DependencyProperty property, PropertyPath path, object source, BindingMode mode = BindingMode.OneWay, IValueConverter converter = null, object converterParameter = null, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged)
    {
        return BindingOperations.SetBinding(input, property, new Binding()
        {
            Converter
                = converter,
            ConverterParameter
                = converterParameter,
            Mode
                = mode,
            Path
                = path,
            Source
                = source,
            UpdateSourceTrigger
                = updateSourceTrigger
        });
    }

    #endregion

    #region Convert

    public static ValueChange<T> Convert<T>(this DependencyPropertyChangedEventArgs i)
    => new(i.OldValue is T j ? j : default, i.NewValue is T k ? k : default);

    public static ValueChange Convert(this DependencyPropertyChangedEventArgs i)
        => new(i.OldValue, i.NewValue);

    #endregion

    #region GetChild

    #region GetChildOfType

    public static T GetChildOfType<T>(this DependencyObject input) where T : DependencyObject
        => input.GetLogicalChildOfType<T>() ?? input.GetVisualChildOfType<T>();

    public static DependencyObject GetChildOfType(this DependencyObject input, Type type)
        => input.GetLogicalChildOfType(type) ?? input.GetVisualChildOfType(type);

    #endregion

    #region GetLogicalChildOfType

    public static T GetLogicalChildOfType<T>(this DependencyObject input) where T : DependencyObject
    {
        if (input is not null)
        {
            foreach (var i in LogicalTreeHelper.GetChildren(input))
            {
                if (i is DependencyObject j)
                {
                    var result = j is T k ? k : j.GetLogicalChildOfType<T>();
                    if (result != null) return result;
                }
            }
        }
        return default;
    }

    public static DependencyObject GetLogicalChildOfType(this DependencyObject input, Type type)
    {
        if (input is not null)
        {
            foreach (var i in LogicalTreeHelper.GetChildren(input))
            {
                if (i is DependencyObject j)
                {
                    var result = j.GetType().Inherits(type) || j.GetType().Implements(type) ? j : j.GetLogicalChildOfType(type);
                    if (result != null) return result;
                }
            }
        }
        return default;
    }

    #endregion

    #region GetVisualChildOfType

    public static T GetVisualChildOfType<T>(this DependencyObject input) where T : DependencyObject
    {
        if (input is not null)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(input); i++)
            {
                var child = VisualTreeHelper.GetChild(input, i);
                var result = child is T j ? j : child.GetVisualChildOfType<T>();
                if (result != null) return result;
            }
        }
        return default;
    }

    public static DependencyObject GetVisualChildOfType(this DependencyObject input, Type type)
    {
        if (input is not null)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(input); i++)
            {
                var j = VisualTreeHelper.GetChild(input, i);
                var result = j.GetType().Inherits(type) || j.GetType().Implements(type) ? j : j.GetVisualChildOfType(type);
                if (result != null) return result;
            }
        }
        return default;
    }

    #endregion

    #region GetLogicalChildren

    public static IEnumerable<T> GetLogicalChildren<T>(this DependencyObject input, bool recursive = true) where T : DependencyObject
    {
        if (input is null)
            yield break;

        if (input is T t)
            yield return t;

        foreach (var i in LogicalTreeHelper.GetChildren(input).OfType<DependencyObject>())
        {
            if (recursive)
            {
                foreach (var j in i.GetLogicalChildren<T>())
                {
                    if (j is not null)
                        yield return j;
                }
            }
            else if (i is T k)
                yield return k;
        }
    }

    #endregion

    #region GetVisualChildren

    public static IEnumerable<T> GetVisualChildren<T>(this DependencyObject input, bool recursive = true) where T : DependencyObject
    {
        if (input != null)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(input); i++)
            {
                var j = VisualTreeHelper.GetChild(input, i);
                if (j != null && j is T t)
                    yield return t;

                if (recursive)
                {
                    foreach (var k in j.GetVisualChildren<T>())
                        yield return k;
                }
            }
        }
        yield break;
    }

    #endregion

    #endregion

    #region GetParent

    #region GetParent

    /// <summary>Attempts to find parent for specified object in following order: VisualParent -> LogicalParent -> LogicalTemplatedParent.</summary>
    /// <remarks>Visual, FrameworkElement, and FrameworkContentElement types are supported. If the logical parent is not found, we try TemplatedParent.</remarks>
    /// <param name="input">The object to get the parent for.</param>
    public static DependencyObject GetParent(this DependencyObject input)
    {
        if (input is Popup)
        {
            //Case 126732 : To correctly detect parent of a popup we must do that exception case
            if (input is Popup popup && popup.PlacementTarget != null)
                return popup.PlacementTarget;
        }

        var parent = input is not Visual visual ? null : VisualTreeHelper.GetParent(visual);
        if (parent is null)
        {
            //No visual parent, check logical tree.
            if (input is FrameworkElement frameworkElement)
            {
                parent = frameworkElement.Parent;
                parent ??= frameworkElement.TemplatedParent;
            }
            else
            {
                if (input is FrameworkContentElement frameworkContentElement)
                {
                    parent = frameworkContentElement.Parent;
                    parent ??= frameworkContentElement.TemplatedParent;
                }
            }
        }
        return parent;
    }

    /// <summary>Attempts to find parent for specified object in following order: VisualParent -> LogicalParent -> LogicalTemplatedParent.</summary>
    public static T GetParent<T>(this DependencyObject input, Predicate<T> predicate = null) where T : DependencyObject
    {
        var parent = input;
        while (parent != null)
        {
            parent = parent.GetParent();
            if (parent is T t)
            {
                if (predicate?.Invoke(t) != false)
                    break;
            }
        }
        return parent is T result ? result : default;
    }

    /// <summary>Attempts to find parent for specified object in following order: VisualParent -> LogicalParent -> LogicalTemplatedParent.</summary>
    public static DependencyObject GetParent(this DependencyObject input, Type type)
    {
        var parent = input.GetParent();
        while (parent != null && parent.GetType() != type)
        {
            if (parent is ContextMenu contextMenu)
                parent = contextMenu.PlacementTarget;

            else parent = parent.GetParent();
        }
        return parent;
    }

    #endregion

    #region GetLogicalParent

    public static DependencyObject GetLogicalParent(this DependencyObject input) => LogicalTreeHelper.GetParent(input);

    public static T GetLogicalParent<T>(this DependencyObject input) where T : DependencyObject
    {
        do
        {
            if (input is T)
                return (T)input;

            input = input.GetLogicalParent();
        }
        while (input != null);
        return null;
    }

    #endregion

    #region GetVisualParent

    public static DependencyObject GetVisualParent(this DependencyObject input) => VisualTreeHelper.GetParent(input);

    public static T GetVisualParent<T>(this DependencyObject input) where T : DependencyObject
    {
        if (input is not null)
        {
            do
            {
                if (input is T i)
                    return i;

                if (input is Visual)
                    input = input.GetVisualParent();
                else break;
            }
            while (input != null);
        }
        return null;
    }

    #endregion

    #endregion

    #region GetDependencyProperty

    public static DependencyProperty GetDependencyProperty(this DependencyObject input, string propertyName)
    {
        var type = input.GetType();
        DependencyProperty result(string i)
        {
            var field = type.GetField(i, BindingFlags.Public | BindingFlags.Static);
            return field?.GetValue(null) as DependencyProperty ?? (field?.GetValue(null) as DependencyPropertyGeneric)?.Property;
        }
        return result($"{propertyName}Property") ?? result(propertyName);
    }

    #endregion

    #region GetValueOrSetDefault

    public static T GetValueOrSetDefault<T>(this DependencyObject input, DependencyProperty property, Func<T> action)
    {
        if (input.GetValue(property) is T j)
            return j;

        j = action();
        input.SetValue(property, j);
        return j;
    }

    public static T GetValueOrSetDefault<T>(this DependencyObject input, DependencyPropertyKey key, Func<T> action)
    {
        if (input.GetValue(key.DependencyProperty) is T j)
            return j;

        j = action();
        input.SetValue(key, j);
        return j;
    }

    #endregion

    #region HasDependencyProperty

    public static bool HasDependencyProperty(this DependencyObject input, string propertyName) => input.GetDependencyProperty(propertyName) != null;

    #endregion

    #region MultiBind

    public static BindingExpressionBase MultiBind(this DependencyObject input, DependencyProperty property, IMultiValueConverter converter, object converterParameter, params Binding[] bindings)
    {
        var result = new MultiBinding()
        {
            Converter = converter,
            ConverterParameter = converterParameter,
            Mode = BindingMode.OneWay,
        };
        if (bindings?.Length > 0)
            bindings.ForEach(i => result.Bindings.Add(i));

        return input.Bind(property, result);
    }

    public static BindingExpressionBase MultiBind(this DependencyObject input, DependencyProperty property, IMultiValueConverter converter, object source, params PropertyPath[] paths)
    {
        var result = new MultiBinding()
        {
            Converter = converter,
            Mode = BindingMode.OneWay,
        };

        if (paths?.Length > 0)
        {
            foreach (var i in paths)
            {
                var j = new Binding
                {
                    Path = i,
                    Mode
                    = BindingMode.OneWay,
                    Source
                    = source
                };

                result.Bindings.Add(j);
            }
        }
        return input.Bind(property, result);
    }

    public static BindingExpressionBase MultiBind(this DependencyObject input, DependencyProperty property, MultiBinding binding, params Binding[] bindings)
    {
        bindings.ForEach(binding.Bindings.Add);
        return input.Bind(property, binding);
    }

    #endregion

    #region RemoveChanged

    public static void RemoveChanged(this DependencyObject input, DependencyProperty p, EventHandler e)
        => DependencyPropertyDescriptor.FromProperty(p, input.GetType()).RemoveValueChanged(input, e);

    #endregion

    #region Select

    public static void Select(this DependencyObject input, bool select)
    {
        //ListBox/DataGrid
        if (input is ListBoxItem || input is DataGridRow)
            Selector.SetIsSelected(input, select);

        //TreeView
        else if (input is TreeViewItem item)
            XTreeViewItem.SetIsSelected(item, select);

        //ItemsControl
        else if (input is ContentPresenter content && content.DataContext is ISelect iSelect)
            iSelect.IsSelected = select;

        else throw new NotSupportedException();
    }

    public static Result TrySelect(this DependencyObject input, bool select) => Try.Do(() => input.Select(select));

    #endregion

    #region Unbind

    public static void Unbind(this DependencyObject input, DependencyProperty property) => BindingOperations.ClearBinding(input, property);

    #endregion

    #endregion
}