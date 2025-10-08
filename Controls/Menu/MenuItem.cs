using System;
using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

[Extend<MenuItem>]
public static class XMenuItem
{
    public static readonly ResourceKey Style = new();

    public static readonly ResourceKey Template = new();

    #region Enum

    public static readonly DependencyProperty EnumProperty = DependencyProperty.RegisterAttached("Enum", typeof(Enum), typeof(XMenuItem), new FrameworkPropertyMetadata(default(Enum), OnEnumChanged));
    public static Enum GetEnum(MenuItem i) => (Enum)i.GetValue(EnumProperty);
    public static void SetEnum(MenuItem i, Enum input) => i.SetValue(EnumProperty, input);

    private static void OnEnumChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is MenuItem item)
        {
            item.AddHandlerAttached(e.NewValue is not null, GroupNameProperty, i =>
            {
                if (Equals(GetEnum(item), GetEnumSource(item)))
                    i.IsChecked = true;

                i.Checked
                    += Enum_Checked;
            }, i =>
            {
                i.Checked
                    -= Enum_Checked;
            });
        }
    }

    private static void Enum_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem item)
        {
            SetEnumSource(item, GetEnum(item));

            //Uncheck others...
            var parent = item.GetVisualParent();
            if (parent is StackPanel panel)
            {
                foreach (var i in panel.Children)
                {
                    if (i is MenuItem j && Equals(GetGroupName(j), GetGroupName(item)) && !ReferenceEquals(j, item))
                        j.SetCurrentValue(MenuItem.IsCheckedProperty, false);
                }
            }
            else if (parent is ItemsControl control)
            {
                foreach (var i in control.Items)
                {
                    var j = i as MenuItem ?? control.GetContainer(i) as MenuItem;
                    if (j is not null && Equals(GetGroupName(j), GetGroupName(item)) && !ReferenceEquals(j, item))
                        j.SetCurrentValue(MenuItem.IsCheckedProperty, false);
                }
            }
        }
    }

    #endregion

    #region EnumSource

    public static readonly DependencyProperty EnumSourceProperty = DependencyProperty.RegisterAttached("EnumSource", typeof(Enum), typeof(XMenuItem), new FrameworkPropertyMetadata(default(Enum), OnEnumSourceChanged));
    public static Enum GetEnumSource(MenuItem i) => (Enum)i.GetValue(EnumSourceProperty);
    public static void SetEnumSource(MenuItem i, Enum input) => i.SetValue(EnumSourceProperty, input);

    private static void OnEnumSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is MenuItem)
        {
        }
    }

    #endregion

    #region EnumFlag

    public static readonly DependencyProperty EnumFlagProperty = DependencyProperty.RegisterAttached("EnumFlag", typeof(Enum), typeof(XMenuItem), new FrameworkPropertyMetadata(default(Enum), OnEnumFlagChanged));
    public static Enum GetEnumFlag(MenuItem i) => (Enum)i.GetValue(EnumFlagProperty);
    public static void SetEnumFlag(MenuItem i, Enum input) => i.SetValue(EnumFlagProperty, input);

    private static void OnEnumFlagChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is MenuItem item)
        {
            item.AddHandlerAttached(e.NewValue is not null, GroupNameProperty, i =>
            {
                var flag = GetEnumFlag(item);
                if (GetEnumFlagSource(item).HasFlag(flag))
                    i.IsChecked = true;

                i.Checked
                    += EnumFlag_Checked;
                i.Unchecked
                    += EnumFlag_Unchecked;
            }, i =>
            {
                i.Checked
                    -= EnumFlag_Checked;
                i.Unchecked
                    -= EnumFlag_Unchecked;
            });
        }
    }

    private static void EnumFlag_Unchecked(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem item)
        {
            var flag = GetEnumFlag(item);
            if (GetEnumFlagSource(item).HasFlag(flag))
                SetEnumFlagSource(item, GetEnumFlagSource(item).RemoveFlag(flag));
        }
    }

    private static void EnumFlag_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem item)
        {
            var flag = GetEnumFlag(item);
            if (!GetEnumFlagSource(item).HasFlag(flag))
                SetEnumFlagSource(item, GetEnumFlagSource(item).AddFlag(flag));
        }
    }

    #endregion

    #region EnumFlagSource

    public static readonly DependencyProperty EnumFlagSourceProperty = DependencyProperty.RegisterAttached("EnumFlagSource", typeof(Enum), typeof(XMenuItem), new FrameworkPropertyMetadata(default(Enum), OnEnumFlagSourceChanged));
    public static Enum GetEnumFlagSource(MenuItem i) => (Enum)i.GetValue(EnumFlagSourceProperty);
    public static void SetEnumFlagSource(MenuItem i, Enum input) => i.SetValue(EnumFlagSourceProperty, input);

    private static void OnEnumFlagSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is MenuItem)
        {
        }
    }

    #endregion

    #region Equals

    public static readonly DependencyProperty EqualsProperty = DependencyProperty.RegisterAttached("Equals", typeof(object), typeof(XMenuItem), new FrameworkPropertyMetadata(null, OnEqualsChanged));
    public static object GetEquals(MenuItem i) => i.GetValue(EqualsProperty);
    public static void SetEquals(MenuItem i, object input) => i.SetValue(EqualsProperty, input);

    private static void OnEqualsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is MenuItem item)
            Equals_Update(item);
    }

    private static void Equals_Update(MenuItem item)
        => GetEqualsHandle(item).DoInternal(() => item.IsChecked = GetEquals(item) == GetEqualsParameter(item));

    #endregion

    #region (private) EqualsHandle

    private static readonly DependencyProperty EqualsHandleProperty = DependencyProperty.RegisterAttached("EqualsHandle", typeof(Handle), typeof(XMenuItem), new FrameworkPropertyMetadata(null));

    private static Handle GetEqualsHandle(MenuItem i) => i.GetValueOrSetDefault(EqualsHandleProperty, () => new Handle());

    #endregion

    #region EqualsParameter

    public static readonly DependencyProperty EqualsParameterProperty = DependencyProperty.RegisterAttached("EqualsParameter", typeof(object), typeof(XMenuItem), new FrameworkPropertyMetadata(null, OnEqualsParameterChanged));
    public static object GetEqualsParameter(MenuItem i) => i.GetValue(EqualsParameterProperty);
    public static void SetEqualsParameter(MenuItem i, object input) => i.SetValue(EqualsParameterProperty, input);

    private static void OnEqualsParameterChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is MenuItem item)
        {
            item.AddHandlerAttached(e.NewValue != null, EqualsParameterProperty, i =>
            {
                i.Checked += EqualsParameter_Checked;
                Equals_Update(i);
            },
            i => i.Checked -= EqualsParameter_Checked);
            Equals_Update(item);
        }
    }

    private static void EqualsParameter_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem item)
            GetEqualsHandle(item).DoInternal(() => SetEquals(item, GetEqualsParameter(item)));
    }

    #endregion

    #region GroupName

    public static readonly DependencyProperty GroupNameProperty = DependencyProperty.RegisterAttached("GroupName", typeof(string), typeof(XMenuItem), new FrameworkPropertyMetadata(string.Empty));
    public static string GetGroupName(MenuItem i) => (string)i.GetValue(GroupNameProperty);
    public static void SetGroupName(MenuItem i, string input) => i.SetValue(GroupNameProperty, input);

    #endregion

    #region HeaderVisibility

    public static readonly DependencyProperty HeaderVisibilityProperty = DependencyProperty.RegisterAttached("HeaderVisibility", typeof(Visibility), typeof(XMenuItem), new FrameworkPropertyMetadata(Visibility.Visible));
    public static Visibility GetHeaderVisibility(MenuItem i) => (Visibility)i.GetValue(HeaderVisibilityProperty);
    public static void SetHeaderVisibility(MenuItem i, Visibility input) => i.SetValue(HeaderVisibilityProperty, input);

    #endregion

    #region IconTemplate

    public static readonly DependencyProperty IconTemplateProperty = DependencyProperty.RegisterAttached("IconTemplate", typeof(DataTemplate), typeof(XMenuItem), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetIconTemplate(FrameworkElement i) => (DataTemplate)i.GetValue(IconTemplateProperty);
    public static void SetIconTemplate(FrameworkElement i, DataTemplate input) => i.SetValue(IconTemplateProperty, input);

    #endregion

    #region IconTemplateSelector

    public static readonly DependencyProperty IconTemplateSelectorProperty = DependencyProperty.RegisterAttached("IconTemplateSelector", typeof(DataTemplateSelector), typeof(XMenuItem), new FrameworkPropertyMetadata(null));
    public static DataTemplateSelector GetIconTemplateSelector(FrameworkElement i) => (DataTemplateSelector)i.GetValue(IconTemplateSelectorProperty);
    public static void SetIconTemplateSelector(FrameworkElement i, DataTemplateSelector input) => i.SetValue(IconTemplateSelectorProperty, input);

    #endregion

    #region IconVisibility

    public static readonly DependencyProperty IconVisibilityProperty = DependencyProperty.RegisterAttached("IconVisibility", typeof(Visibility), typeof(XMenuItem), new FrameworkPropertyMetadata(Visibility.Visible));
    public static Visibility GetIconVisibility(MenuItem i) => (Visibility)i.GetValue(IconVisibilityProperty);
    public static void SetIconVisibility(MenuItem i, Visibility input) => i.SetValue(IconVisibilityProperty, input);

    #endregion

    #region InputGestureTextTemplate

    public static readonly DependencyProperty InputGestureTextTemplateProperty = DependencyProperty.RegisterAttached("InputGestureTextTemplate", typeof(DataTemplate), typeof(XMenuItem), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetInputGestureTextTemplate(MenuItem i) => (DataTemplate)i.GetValue(InputGestureTextTemplateProperty);
    public static void SetInputGestureTextTemplate(MenuItem i, DataTemplate input) => i.SetValue(InputGestureTextTemplateProperty, input);

    #endregion
}