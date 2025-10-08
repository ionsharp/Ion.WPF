using Ion.Collect;
using Ion.Core;
using Ion.Input;
using Ion.Reflect;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

public class FlagButton : Button, ISubscribe
{
    [Serializable]
    public enum Views { Default, Menu }

    /// <see cref="Region.Field"/>

    private readonly Handle handleValue = false;
    private readonly ListObservable<Checkable<object>> items = [];

    /// <see cref="Region.Property"/>
    #region

    public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(FlagButton), new FrameworkPropertyMetadata(false));
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(FlagButton), new FrameworkPropertyMetadata(false));
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    private static readonly DependencyPropertyKey ItemsKey = DependencyProperty.RegisterReadOnly(nameof(Items), typeof(object), typeof(FlagButton), new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty ItemsProperty = ItemsKey.DependencyProperty;
    public object Items
    {
        get => GetValue(ItemsProperty);
        private set => SetValue(ItemsKey, value);
    }

    public static readonly DependencyProperty ItemStyleProperty = DependencyProperty.Register(nameof(ItemStyle), typeof(Style), typeof(FlagButton), new FrameworkPropertyMetadata(null));
    public Style ItemStyle
    {
        get => (Style)GetValue(ItemStyleProperty);
        set => SetValue(ItemStyleProperty, value);
    }

    public static readonly DependencyProperty ItemPanelProperty = DependencyProperty.Register(nameof(ItemPanel), typeof(ItemsPanelTemplate), typeof(FlagButton), new FrameworkPropertyMetadata(null));
    public ItemsPanelTemplate ItemPanel
    {
        get => (ItemsPanelTemplate)GetValue(ItemPanelProperty);
        set => SetValue(ItemPanelProperty, value);
    }

    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(FlagButton), new FrameworkPropertyMetadata(null));
    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(nameof(Placeholder), typeof(object), typeof(FlagButton), new FrameworkPropertyMetadata(null));
    public object Placeholder
    {
        get => (object)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly DependencyProperty PlaceholderTemplateProperty = DependencyProperty.Register(nameof(PlaceholderTemplate), typeof(DataTemplate), typeof(FlagButton), new FrameworkPropertyMetadata(null));
    public DataTemplate PlaceholderTemplate
    {
        get => (DataTemplate)GetValue(PlaceholderTemplateProperty);
        set => SetValue(PlaceholderTemplateProperty, value);
    }

    public static readonly DependencyProperty PlaceholderTemplateSelectorProperty = DependencyProperty.Register(nameof(PlaceholderTemplateSelector), typeof(DataTemplateSelector), typeof(FlagButton), new FrameworkPropertyMetadata(null));
    public DataTemplateSelector PlaceholderTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(PlaceholderTemplateSelectorProperty);
        set => SetValue(PlaceholderTemplateSelectorProperty, value);
    }

    public static readonly DependencyProperty StaysOpenProperty = DependencyProperty.Register(nameof(StaysOpen), typeof(bool), typeof(FlagButton), new FrameworkPropertyMetadata(true));
    public bool StaysOpen
    {
        get => (bool)GetValue(StaysOpenProperty);
        set => SetValue(StaysOpenProperty, value);
    }

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(object), typeof(FlagButton), new FrameworkPropertyMetadata(null, OnSourceChanged));
    public object Source
    {
        get => (object)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }
    private static void OnSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => sender.As<FlagButton>().OnSourceChanged();

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(object), typeof(FlagButton), new FrameworkPropertyMetadata(null, OnValueChanged));
    public object Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => sender.As<FlagButton>().OnValueChanged(new Value<object>(e.OldValue, e.NewValue));

    public static readonly DependencyProperty ValueTemplateProperty = DependencyProperty.Register(nameof(ValueTemplate), typeof(DataTemplate), typeof(FlagButton), new FrameworkPropertyMetadata(null));
    public DataTemplate ValueTemplate
    {
        get => (DataTemplate)GetValue(ValueTemplateProperty);
        set => SetValue(ValueTemplateProperty, value);
    }

    public static readonly DependencyProperty ValueTemplateSelectorProperty = DependencyProperty.Register(nameof(ValueTemplateSelector), typeof(DataTemplateSelector), typeof(FlagButton), new FrameworkPropertyMetadata(null));
    public DataTemplateSelector ValueTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(ValueTemplateSelectorProperty);
        set => SetValue(ValueTemplateSelectorProperty, value);
    }

    public static readonly DependencyProperty ViewProperty = DependencyProperty.Register(nameof(View), typeof(Views), typeof(FlagButton), new FrameworkPropertyMetadata(Views.Default));
    public Views View
    {
        get => (Views)GetValue(ViewProperty);
        set => SetValue(ViewProperty, value);
    }

    #endregion

    /// <see cref="Region.Constructor"/>

    public FlagButton() : base()
    {
        Items = items;
        this.AddHandler(OnLoaded, OnUnloaded);
    }

    /// <see cref="Region.Method.Private"/>
    #region

    private void OnChecked(object sender, CheckEventArgs e)
    {
        if (e.IsChecked == true)
        {
            handleValue.DoInternal(() =>
            {
                if (sender is Checkable<object> i)
                {
                    if (Value is Enum e)
                    {
                        if (i.Value is Enum f)
                        {
                            if (!e.HasFlag(f))
                                SetCurrentValue(ValueProperty, e.AddFlag(f));
                        }
                    }
                    if (Value is Flag g)
                    {
                        if (!g.Has(i.Value))
                            SetCurrentValue(ValueProperty, g.AddFlag(i.Value));
                    }
                }
            });
        }
        else if (e.IsChecked == false)
        {
            handleValue.DoInternal(() =>
            {
                if (sender is Checkable<object> i)
                {
                    if (Value is Enum e)
                    {
                        if (i.Value is Enum f)
                        {
                            if (e.HasFlag(f))
                                SetCurrentValue(ValueProperty, e.RemoveFlag(f));
                        }
                    }
                    if (Value is Flag g)
                    {
                        if (g.Has(i.Value))
                            SetCurrentValue(ValueProperty, g.RemoveFlag(i.Value));
                    }
                }
            });
        }
    }

    ///

    private void OnLoaded() => Subscribe();

    private void OnUnloaded() => Unsubscribe();

    #endregion

    /// <see cref="Region.Method.Protected"/>

    protected virtual void OnSourceChanged()
    {
        Unsubscribe();

        items.Clear();
        if (Source is Flag flag)
        {
            flag.Each((i, j) =>
            {
                var result = new Checkable<object>(i) { IsChecked = flag.Has(i) };
                items.Add(result);
            });
        }

        Type type = null;
        if (Source is Enum e)
            type = e.GetType();

        if (Source is Type f)
            type = f;

        type?.GetEnumValues(Browse.Visible).ForEach(i =>
            {
                var result = new Checkable<object>(i) { IsChecked = Value.As<Enum>()?.HasFlag(i) ?? false };
                items.Add(result);
            });

        Subscribe();
    }

    protected virtual void OnValueChanged(Value<object> input)
    {
        handleValue.Do(() => items.ForEach(i =>
        {
            if (input.NewValue is Enum e)
                i.IsChecked = e.HasFlag(i.Value as Enum);

            if (input.NewValue is Flag f)
                i.IsChecked = f.Has(i.Value);
        }));
    }

    /// <see cref="Region.Method.Public"/>

    public void Subscribe()
    {
        items.ForEach(i => i.Checked += OnChecked);
    }

    public void Unsubscribe()
    {
        items.ForEach(i => i.Checked -= OnChecked);
    }
}