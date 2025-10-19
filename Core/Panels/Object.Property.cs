using Ion.Collect;
using Ion.Controls;
using Ion.Input;
using Ion.Reflect;
using System;
using System.Windows.Input;

namespace Ion.Core;

/// <inheritdoc/>
[Image(Images.Properties)]
[Name("Properties")]
public record class PropertyPanel() : ObjectPanel()
{
    /// <see cref="TargetAttribute"/>
    #region

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class TargetAttribute : Attribute
    {
        public TargetAttribute() : base() { }
    }

    #endregion

    /// <see cref="Region.Field"/>
    #region

    public static readonly ResourceKey TabTemplate = new();

    public static readonly new ResourceKey Template = new();

    #endregion

    /// <see cref="Region.Property"/>
    #region

    [NonSerializable]
    public virtual object SelectedTab { get => Get<object>(null, false); set => Set(value, false); }

    [NonSerializable]
    public virtual int SelectedTabIndex { get => Get(-1, false); set => Set(value, false); }

    [NonSerializable]
    public virtual ListObservable Sources { get => Get<ListObservable>([], false); set => Set(value, false); }

    #endregion

    /// <see cref="Region.Method"/>
    #region

    private static object GetTargetValue(object target)
    {
        object result = null;
        Instance.EachProperty(target , (i, j) =>
        {
            if (Instance.HasAttribute<TargetAttribute>(i))
                result = i.GetValue(target) ?? result;
        });
        return result;
    }

    private void OnActiveContentPropertyChanged(IPropertySet sender, PropertySetEventArgs e)
    {
        var property = sender.GetType().GetProperty(e.PropertyName);
        if (property.HasAttribute<TargetAttribute>())
            GetTargetValue(sender).IfNotNull(Add);
    }

    private void OnDataPanelSelectionChanged(IPropertySet sender, PropertySetEventArgs e)
    {
        if (e.PropertyName == nameof(DataPanel.SelectedItems))
        {
            if (sender is DataPanel dataPanel)
            {
                if (dataPanel.CanEdit)
                    Add(dataPanel.SelectedItems);
            }
        }
    }

    protected override void OnActiveContentChanged(Value<Content> value)
    {
        base.OnActiveContentChanged(value);
        if (value.OldValue is DataPanel oldPanel)
        {
            if (oldPanel.CanEdit)
                oldPanel.PropertySet -= OnDataPanelSelectionChanged;
        }

        if (value.OldValue is not null)
            value.OldValue.PropertySet -= OnActiveContentPropertyChanged;

        if (value.NewValue is DataPanel dataPanel)
        {
            if (dataPanel.CanEdit)
            {
                Add(dataPanel.SelectedItems);
                dataPanel.PropertySet += OnDataPanelSelectionChanged;
            }
        }

        if (value.NewValue is not null)
        {
            if (Instance.HasAttribute<TargetAttribute>(value.NewValue))
                Add(value.NewValue);

            else GetTargetValue(value.NewValue).IfNotNull(Add);
            value.NewValue.PropertySet += OnActiveContentPropertyChanged;
        }
    }

    public void Add(object a)
    {
        if (a is not null)
        {
            if (a is object[] m)
                a = m[0];

            var x = a as Type ?? (a as IGeneric)?.GetGenericType();
            for (var i = 0; i <= Sources.Count; i++)
            {
                if (i == Sources.Count)
                {
                    Sources.Add(a);
                    SelectedTabIndex = Sources.Count - 1;
                    break;
                }
                else
                {
                    var b = Sources[i];
                    if (b is object[] n)
                        b = n[0];

                    if (!Equals(a, b))
                    {
                        var y = b as Type ?? (b as IGeneric)?.GetGenericType();
                        if (x == y)
                        {
                            Sources.RemoveAt(i);
                            Sources.Insert(i, a);
                            SelectedTabIndex = i;
                            break;
                        }
                    }
                    else
                    {
                        SelectedTabIndex = i;
                        break;
                    }
                }
            }
        }
    }

    #endregion

    /// <see cref="ICommand"/>
    #region

    private ICommand removeCommand;
    public ICommand RemoveCommand => removeCommand ??= new RelayCommand<object>(i => Sources.Remove(i), i => i is not null && Sources.Contains(i));

    private ICommand selectTabCommand;
    public ICommand SelectTabCommand => selectTabCommand ??= new RelayCommand<object>(i => SelectedTab = i, i => i is not null);

    #endregion
}