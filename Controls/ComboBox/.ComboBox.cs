using Ion;
using Ion.Collect;
using Ion.Input;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Ion.Controls;

[Extend<ComboBox>]
public static class XComboBox
{
    public static readonly ResourceKey MenuHeaderPatternKey = new();

    public static readonly ResourceKey ToggleButtonStyleKey = new();

    #region Properties

    #region Flags

    public static readonly DependencyProperty FlagsProperty = DependencyProperty.RegisterAttached("Flags", typeof(object), typeof(XComboBox), new FrameworkPropertyMetadata(null, OnFlagsChanged));
    public static object GetFlags(ComboBox i) => i.GetValue(FlagsProperty);
    public static void SetFlags(ComboBox i, object input) => i.SetValue(FlagsProperty, input);

    #endregion

    #region LeftContent

    public static readonly DependencyProperty LeftContentProperty = DependencyProperty.RegisterAttached("LeftContent", typeof(object), typeof(XComboBox), new FrameworkPropertyMetadata(null));
    public static object GetLeftContent(ComboBox i) => i.GetValue(LeftContentProperty);
    public static void SetLeftContent(ComboBox i, object input) => i.SetValue(LeftContentProperty, input);

    #endregion

    #region LeftContentTemplate

    public static readonly DependencyProperty LeftContentTemplateProperty = DependencyProperty.RegisterAttached("LeftContentTemplate", typeof(DataTemplate), typeof(XComboBox), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetLeftContentTemplate(ComboBox i) => (DataTemplate)i.GetValue(LeftContentTemplateProperty);
    public static void SetLeftContentTemplate(ComboBox i, DataTemplate input) => i.SetValue(LeftContentTemplateProperty, input);

    #endregion

    #region (private) HandleSelectedItems

    private static readonly DependencyProperty HandleSelectedItemsProperty = DependencyProperty.RegisterAttached("HandleSelectedItems", typeof(Handle), typeof(XComboBox), new FrameworkPropertyMetadata(null));

    private static Handle GetHandleSelectedItems(this ComboBox i) => i.GetValueOrSetDefault<Handle>(HandleSelectedItemsProperty, () => false);

    #endregion

    #region MenuAnimation

    public static readonly DependencyProperty MenuAnimationProperty = DependencyProperty.RegisterAttached("MenuAnimation", typeof(PopupAnimation), typeof(XComboBox), new FrameworkPropertyMetadata(PopupAnimation.Fade));
    public static PopupAnimation GetMenuAnimation(ComboBox i) => (PopupAnimation)i.GetValue(MenuAnimationProperty);
    public static void SetMenuAnimation(ComboBox i, PopupAnimation input) => i.SetValue(MenuAnimationProperty, input);

    #endregion

    #region MenuFooter

    public static readonly DependencyProperty MenuFooterProperty = DependencyProperty.RegisterAttached("MenuFooter", typeof(object), typeof(XComboBox), new FrameworkPropertyMetadata(null));
    public static object GetMenuFooter(ComboBox i) => i.GetValue(MenuFooterProperty);
    public static void SetMenuFooter(ComboBox i, object input) => i.SetValue(MenuFooterProperty, input);

    #endregion

    #region MenuFooterTemplate

    public static readonly DependencyProperty MenuFooterTemplateProperty = DependencyProperty.RegisterAttached("MenuFooterTemplate", typeof(DataTemplate), typeof(XComboBox), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetMenuFooterTemplate(ComboBox i) => (DataTemplate)i.GetValue(MenuFooterTemplateProperty);
    public static void SetMenuFooterTemplate(ComboBox i, DataTemplate input) => i.SetValue(MenuFooterTemplateProperty, input);

    #endregion

    #region MenuHeader

    public static readonly DependencyProperty MenuHeaderProperty = DependencyProperty.RegisterAttached("MenuHeader", typeof(object), typeof(XComboBox), new FrameworkPropertyMetadata(null));
    public static object GetMenuHeader(ComboBox i) => i.GetValue(MenuHeaderProperty);
    public static void SetMenuHeader(ComboBox i, object input) => i.SetValue(MenuHeaderProperty, input);

    #endregion

    #region MenuHeaderIcon

    public static readonly DependencyProperty MenuHeaderIconProperty = DependencyProperty.RegisterAttached("MenuHeaderIcon", typeof(ImageSource), typeof(XComboBox), new FrameworkPropertyMetadata(null));
    public static ImageSource GetMenuHeaderIcon(ComboBox i) => (ImageSource)i.GetValue(MenuHeaderIconProperty);
    public static void SetMenuHeaderIcon(ComboBox i, ImageSource input) => i.SetValue(MenuHeaderIconProperty, input);

    #endregion

    #region MenuHeaderTemplate

    public static readonly DependencyProperty MenuHeaderTemplateProperty = DependencyProperty.RegisterAttached("MenuHeaderTemplate", typeof(DataTemplate), typeof(XComboBox), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetMenuHeaderTemplate(ComboBox i) => (DataTemplate)i.GetValue(MenuHeaderTemplateProperty);
    public static void SetMenuHeaderTemplate(ComboBox i, DataTemplate input) => i.SetValue(MenuHeaderTemplateProperty, input);

    #endregion

    #region MenuPlacement

    public static readonly DependencyProperty MenuPlacementProperty = DependencyProperty.RegisterAttached("MenuPlacement", typeof(PlacementMode), typeof(XComboBox), new FrameworkPropertyMetadata(PlacementMode.Bottom));
    public static PlacementMode GetMenuPlacement(ComboBox i) => (PlacementMode)i.GetValue(MenuPlacementProperty);
    public static void SetMenuPlacement(ComboBox i, PlacementMode input) => i.SetValue(MenuPlacementProperty, input);

    #endregion

    #region MinDropDownHeight

    public static readonly DependencyProperty MinDropDownHeightProperty = DependencyProperty.RegisterAttached("MinDropDownHeight", typeof(double), typeof(XComboBox), new FrameworkPropertyMetadata(64.0));
    public static double GetMinDropDownHeight(ComboBox i) => (double)i.GetValue(MinDropDownHeightProperty);
    public static void SetMinDropDownHeight(ComboBox i, double input) => i.SetValue(MinDropDownHeightProperty, input);

    #endregion

    #region (readonly) IsSelectionEmpty

    private static readonly DependencyPropertyKey IsSelectionEmptyKey = DependencyProperty.RegisterAttachedReadOnly("IsSelectionEmpty", typeof(bool), typeof(XComboBox), new FrameworkPropertyMetadata(true));
    public static readonly DependencyProperty IsSelectionEmptyProperty = IsSelectionEmptyKey.DependencyProperty;
    public static bool GetIsSelectionEmpty(ComboBox i) => (bool)i.GetValue(IsSelectionEmptyProperty);
    private static void SetIsSelectionEmpty(ComboBox i, bool input) => i.SetValue(IsSelectionEmptyKey, input);

    #endregion

    #region Placeholder

    public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.RegisterAttached("Placeholder", typeof(object), typeof(XComboBox), new FrameworkPropertyMetadata(null));
    public static object GetPlaceholder(ComboBox i) => i.GetValue(PlaceholderProperty);
    public static void SetPlaceholder(ComboBox i, object input) => i.SetValue(PlaceholderProperty, input);

    #endregion

    #region PlaceholderTemplate

    public static readonly DependencyProperty PlaceholderTemplateProperty = DependencyProperty.RegisterAttached("PlaceholderTemplate", typeof(DataTemplate), typeof(XComboBox), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetPlaceholderTemplate(ComboBox i) => (DataTemplate)i.GetValue(PlaceholderTemplateProperty);
    public static void SetPlaceholderTemplate(ComboBox i, DataTemplate input) => i.SetValue(PlaceholderTemplateProperty, input);

    #endregion

    #region PlaceholderTemplateSelector

    public static readonly DependencyProperty PlaceholderTemplateSelectorProperty = DependencyProperty.RegisterAttached("PlaceholderTemplateSelector", typeof(DataTemplateSelector), typeof(XComboBox), new FrameworkPropertyMetadata(null));
    public static DataTemplateSelector GetPlaceholderTemplateSelector(ComboBox i) => (DataTemplateSelector)i.GetValue(PlaceholderTemplateSelectorProperty);
    public static void SetPlaceholderTemplateSelector(ComboBox i, DataTemplateSelector input) => i.SetValue(PlaceholderTemplateSelectorProperty, input);

    #endregion

    #region RightContent

    public static readonly DependencyProperty RightContentProperty = DependencyProperty.RegisterAttached("RightContent", typeof(object), typeof(XComboBox), new FrameworkPropertyMetadata(null));
    public static object GetRightContent(ComboBox i) => i.GetValue(RightContentProperty);
    public static void SetRightContent(ComboBox i, object input) => i.SetValue(RightContentProperty, input);

    #endregion

    #region RightContentTemplate

    public static readonly DependencyProperty RightContentTemplateProperty = DependencyProperty.RegisterAttached("RightContentTemplate", typeof(DataTemplate), typeof(XComboBox), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetRightContentTemplate(ComboBox i) => (DataTemplate)i.GetValue(RightContentTemplateProperty);
    public static void SetRightContentTemplate(ComboBox i, DataTemplate input) => i.SetValue(RightContentTemplateProperty, input);

    #endregion

    #region SelectionMode

    public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.RegisterAttached("SelectionMode", typeof(Select), typeof(XComboBox), new FrameworkPropertyMetadata(Select.OneOrNone, OnSelectionModeChanged));
    public static Select GetSelectionMode(ComboBox i) => (Select)i.GetValue(SelectionModeProperty);
    public static void SetSelectionMode(ComboBox i, Select input) => i.SetValue(SelectionModeProperty, input);

    private static void OnSelectionModeChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is ComboBox)
        {
            if ((Select)e.NewValue == Select.One)
            {
            }
        }
    }

    #endregion

    #region (readonly) SelectedItems

    private static readonly DependencyPropertyKey SelectedItemsKey = DependencyProperty.RegisterAttachedReadOnly("SelectedItems", typeof(IListObservable), typeof(XComboBox), new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty SelectedItemsProperty = SelectedItemsKey.DependencyProperty;
    public static IListObservable GetSelectedItems(ComboBox i) => i.GetValueOrSetDefault(SelectedItemsKey, () => new ListObservable<object>());

    #endregion

    #region SelectedItemTemplate

    public static readonly DependencyProperty SelectedItemTemplateProperty = DependencyProperty.RegisterAttached("SelectedItemTemplate", typeof(DataTemplate), typeof(XComboBox), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetSelectedItemTemplate(ComboBox i) => (DataTemplate)i.GetValue(SelectedItemTemplateProperty);
    public static void SetSelectedItemTemplate(ComboBox i, DataTemplate input) => i.SetValue(SelectedItemTemplateProperty, input);

    #endregion

    #region SelectedItemTemplateSelector

    public static readonly DependencyProperty SelectedItemTemplateSelectorProperty = DependencyProperty.RegisterAttached("SelectedItemTemplateSelector", typeof(DataTemplateSelector), typeof(XComboBox), new FrameworkPropertyMetadata(default(DataTemplateSelector)));
    public static DataTemplateSelector GetSelectedItemTemplateSelector(ComboBox i) => (DataTemplateSelector)i.GetValue(SelectedItemTemplateSelectorProperty);
    public static void SetSelectedItemTemplateSelector(ComboBox i, DataTemplateSelector input) => i.SetValue(SelectedItemTemplateSelectorProperty, input);

    #endregion

    #region SelectionButton

    public static readonly DependencyProperty SelectionButtonProperty = DependencyProperty.RegisterAttached("SelectionButton", typeof(MouseButton), typeof(XComboBox), new FrameworkPropertyMetadata(MouseButton.Left));
    public static MouseButton GetSelectionButton(ComboBox i) => (MouseButton)i.GetValue(SelectionButtonProperty);
    public static void SetSelectionButton(ComboBox i, MouseButton input) => i.SetValue(SelectionButtonProperty, input);

    #endregion

    #region SelectionButtonState

    public static readonly DependencyProperty SelectionButtonStateProperty = DependencyProperty.RegisterAttached("SelectionButtonState", typeof(MouseButtonState), typeof(XComboBox), new FrameworkPropertyMetadata(MouseButtonState.Released));
    public static MouseButtonState GetSelectionButtonState(ComboBox i) => (MouseButtonState)i.GetValue(SelectionButtonStateProperty);
    public static void SetSelectionButtonState(ComboBox i, MouseButtonState input) => i.SetValue(SelectionButtonStateProperty, input);

    #endregion

    #region SelectionModifier

    public static readonly DependencyProperty SelectionModifierProperty = DependencyProperty.RegisterAttached("SelectionModifier", typeof(ModifierKeys), typeof(XComboBox), new FrameworkPropertyMetadata(ModifierKeys.None));
    public static ModifierKeys GetSelectionModifier(ComboBox i) => (ModifierKeys)i.GetValue(SelectionModifierProperty);
    public static void SetSelectionModifier(ComboBox i, ModifierKeys input) => i.SetValue(SelectionModifierProperty, input);

    #endregion

    #region StaysOpen

    public static readonly DependencyProperty StaysOpenProperty = DependencyProperty.RegisterAttached("StaysOpen", typeof(bool), typeof(XComboBox), new FrameworkPropertyMetadata(false));
    public static bool GetStaysOpen(ComboBox i) => (bool)i.GetValue(StaysOpenProperty);
    public static void SetStaysOpen(ComboBox i, bool input) => i.SetValue(StaysOpenProperty, input);

    #endregion

    #endregion

    #region XComboBox

    private static readonly Dictionary<IListObservable, ComboBox> Selections = [];

    ///

    static XComboBox()
    {
        /*
        EventManager.RegisterClassHandler(typeof(ComboBox), ComboBox.LoadedEvent,
            new RoutedEventHandler(OnLoaded), true);
        EventManager.RegisterClassHandler(typeof(ComboBox), ComboBox.UnloadedEvent,
            new RoutedEventHandler(OnUnloaded), true);
        EventManager.RegisterClassHandler(typeof(ComboBox), ComboBox.SelectionChangedEvent,
            new SelectionChangedEventHandler(OnSelectionChanged), true);
        */
        EventManager.RegisterClassHandler(typeof(ComboBox), UIElement.PreviewMouseDownEvent,
            new MouseButtonEventHandler(OnPreviewMouseDownUp), true);
        EventManager.RegisterClassHandler(typeof(ComboBox), UIElement.PreviewMouseUpEvent,
            new MouseButtonEventHandler(OnPreviewMouseDownUp), true);
    }

    ///

    private static Enum ConvertFlags(this ComboBox input)
    {
        if (GetSelectionMode(input) == Select.OneOrMore)
        {
            var selection = GetSelectedItems(input);

            Enum result;
            if (selection.Count > 0)
            {
                result = selection.First<Enum>();
                for (var i = 1; i < selection.Count; i++)
                    result = result.AddFlag((Enum)selection[i]);
            }
            else
            {
                result = (Enum)GetFlags(input);
                foreach (Enum i in input.Items)
                    result = result.RemoveFlag(i);
            }
            return result;
        }
        return null;
    }

    ///

    /// <summary>
    /// Occurs when <see cref="FlagsProperty"/> changes.
    /// </summary>
    private static void OnFlagsChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is ComboBox box)
        {
            box.GetHandleSelectedItems().DoInternal(() =>
            {
                if (GetSelectionMode(box) == Select.OneOrMore)
                {
                    if (e.NewValue is Enum newValue)
                    {
                        var selection = GetSelectedItems(box);
                        foreach (Enum i in box.Items)
                        {
                            if (newValue.HasFlag(i))
                            {
                                //(1) XComboBox.SelectedItems
                                if (!selection.Contains(i))
                                    selection.Add(i);

                                //(2) XComboBoxItem.IsSelected
                                box.SelectInternal(i, true);
                            }
                            else
                            {
                                //(1) XComboBox.SelectedItems
                                selection.Remove(i);
                                //(2) XComboBoxItem.IsSelected
                                box.SelectInternal(i, false);
                            }
                        }
                        //(3) ComboBox.SelectedItem
                        if (selection.Count > 0)
                            box.SetCurrentValue(Selector.SelectedItemProperty, selection[0]);
                    }
                }
            });
        }
    }

    /// <summary>
    /// Occurs upon loading.
    /// </summary>
    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is ComboBox box)
        {
            SetIsSelectionEmpty(box, GetSelectedItems(box).Count == 0);

            Selections.Add(GetSelectedItems(box), box);
            GetSelectedItems(box).CollectionChanged += OnSelectedItemsChanged;
        }
    }

    /// <summary>
    /// Occurs when the mouse is pressed and released.
    /// </summary>
    private static void OnPreviewMouseDownUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is ComboBox box)
        {
            if (box.CanSelect(e))
            {
                if ((e.OriginalSource as ComboBoxItem ?? e.OriginalSource.As<DependencyObject>().GetParent<ComboBoxItem>()) is ComboBoxItem container)
                {
                    box.GetHandleSelectedItems().Do(() =>
                    {
                        var item = box.GetItem(container);

                        var selection = GetSelectedItems(box);
                        switch (GetSelectionMode(box))
                        {
                            case Select.OneOrMore:
                                if (GetSelectionModifier(box) == ModifierKeys.None || GetSelectionModifier(box).Pressed())
                                {
                                    //(1) XComboBoxItem.IsSelected
                                    container.SelectInverse();
                                    if (container.isSelected())
                                    {
                                        //(2) ComboBox.SelectedItem
                                        box.SetCurrentValue(Selector.SelectedItemProperty, item);

                                        //(3) XComboBox.SelectedItems
                                        if (!selection.Contains(item))
                                            selection.Add(item);
                                    }
                                    else
                                    {
                                        //(2) ComboBox.SelectedItem
                                        if (ReferenceEquals(container, box.ItemContainerGenerator.ContainerFromItem(box.SelectedItem)))
                                            box.SetCurrentValue(Selector.SelectedItemProperty, null);

                                        //(3) XComboBox.SelectedItems
                                        selection.Remove(item);
                                    }

                                    //(4) XComboBox.Flags
                                    SetFlags(box, box.ConvertFlags());
                                }
                                break;

                            case Select.One:
                                //(1) XComboBoxItem.IsSelected
                                container.Select(true);
                                box.UnselectInternal(container, i => selection.Remove(i));

                                //(2) XComboBox.SelectedItems
                                if (!selection.Contains(item))
                                    selection.Add(item);

                                //(3) ComboBox.SelectedItem
                                box.SetCurrentValue(Selector.SelectedItemProperty, item);
                                break;

                            case Select.OneOrNone:
                                //(1) XComboBoxItem.IsSelected
                                container.SelectInverse();
                                if (container.isSelected())
                                {
                                    box.UnselectInternal(container, i => selection.Remove(i));
                                    //(2) XComboBox.SelectedItems
                                    if (!selection.Contains(item))
                                        selection.Add(item);

                                    //(3) ComboBox.SelectedItem
                                    box.SetCurrentValue(Selector.SelectedItemProperty, item);
                                }
                                else
                                {
                                    //(2) XComboBox.SelectedItems
                                    selection.Remove(item);
                                    //(3) ComboBox.SelectedItem
                                    if (ReferenceEquals(container, box.ItemContainerGenerator.ContainerFromItem(box.SelectedItem)))
                                        box.SetCurrentValue(Selector.SelectedItemProperty, null);
                                }
                                break;
                        }
                    });
                    e.Handled = true;
                    if (!GetStaysOpen(box))
                        box.SetCurrentValue(ComboBox.IsDropDownOpenProperty, false);
                }
            }
        }
    }

    /// <summary>
    /// Occurs when <see cref="SelectedItemsProperty"/> changes.
    /// </summary>
    private static void OnSelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is IListObservable selection)
            SetIsSelectionEmpty(Selections[selection], selection.Count == 0);

        if (Selections[sender as IListObservable] is ComboBox box)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    box.GetHandleSelectedItems().DoInternal(() =>
                    {
                        //(1) XComboBoxItem.IsSelected
                        box.SelectInternal(e.NewItems[0], true);
                        //(2) ComboBox.SelectedItem
                        box.SetCurrentValue(Selector.SelectedItemProperty, e.NewItems[0]);
                        //(3) XComboBox.Flags
                        SetFlags(box, box.ConvertFlags());
                    });
                    break;

                case NotifyCollectionChangedAction.Remove:
                    box.GetHandleSelectedItems().DoInternal(() =>
                    {
                        //(1) XComboBoxItem.IsSelected
                        box.SelectInternal(e.OldItems[0], false);
                        //(2) ComboBox.SelectedItem
                        if (ReferenceEquals(box.GetContainer(box.SelectedItem), box.GetContainer(e.OldItems[0])))
                            box.SetCurrentValue(Selector.SelectedItemProperty, null);

                        //(3) XComboBox.Flags
                        SetFlags(box, box.ConvertFlags());
                    });
                    break;
            }
        }
    }

    /// <summary>
    /// Occurs when <see cref="XComboBoxItem.IsSelectedProperty"/> changes.
    /// </summary>
    internal static void OnSelected(this ComboBox input, ComboBoxItem container) => input.GetHandleSelectedItems().DoInternal(() =>
    {
        var selection = GetSelectedItems(input);

        var item = input.GetItem(container);
        if (container.isSelected())
        {
            //(1) XComboBox.SelectedItems
            if (!selection.Contains(item))
                selection.Add(item);

            //(2) ComboBox.SelectedItem
            input.SetCurrentValue(Selector.SelectedItemProperty, item);
        }
        else
        {
            //(1) XComboBox.SelectedItems
            selection.Remove(item);
            //(2) ComboBox.SelectedItem
            if (ReferenceEquals(input.GetContainer(input.SelectedItem), container))
                input.SetCurrentValue(Selector.SelectedItemProperty, null);
        }

        //(3) XComboBox.Flags
        SetFlags(input, input.ConvertFlags());
    });

    /// <summary>
    /// Occurs when <see cref="Selector.SelectedIndex"/> or <see cref="Selector.SelectedItem"/> changes.
    /// </summary>
    private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox box)
        {
            box.GetHandleSelectedItems().DoInternal(() =>
            {
                //(1) XComboBox.SelectedItems
                var selection = GetSelectedItems(box);
                if (!selection.Contains(box.SelectedItem))
                    selection.Add(box.SelectedItem);

                //(2) XComboBoxItem.IsSelected
                box.SelectInternal(box.SelectedItem, true);
                box.UnselectInternal(box.GetContainer(box.SelectedItem), i => selection.Remove(i));

                //(3) XComboBox.Flags
                SetFlags(box, box.ConvertFlags());
            });
        }
    }

    private static void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (sender is ComboBox box)
        {
            GetSelectedItems(box).CollectionChanged -= OnSelectedItemsChanged;
            Selections.Remove(GetSelectedItems(box));
        }
    }

    #endregion

    #region Methods

    private static bool CanSelect(this ComboBox input, MouseButtonEventArgs e)
    {
        return GetSelectionButton(input) switch
        {
            MouseButton.Left => e.LeftButton == GetSelectionButtonState(input),
            MouseButton.Middle => e.MiddleButton == GetSelectionButtonState(input),
            MouseButton.Right => e.RightButton == GetSelectionButtonState(input),
            MouseButton.XButton1 => e.XButton1 == GetSelectionButtonState(input),
            MouseButton.XButton2 => e.XButton2 == GetSelectionButtonState(input),
            _ => throw new NotSupportedException(),
        };
    }

    private static void SelectInternal(this ComboBox input, object item, bool select)
    {
        if (input.ItemContainerGenerator.ContainerFromItem(item) is ComboBoxItem i)
            XComboBoxItem.Select(i, select);
    }

    private static void UnselectInternal(this ComboBox input, ComboBoxItem except, Action<object> action = null)
    {
        for (var i = input.Items.Count - 1; i >= 0; i--)
        {
            if (input.ItemContainerGenerator.ContainerFromItem(input.Items[i]) is ComboBoxItem j)
            {
                if (!ReferenceEquals(except, j))
                {
                    j.Select(false);
                    action?.Invoke(input.Items[i]);
                }
            }
        }
    }

    ///

    public static ComboBoxItem GetContainer(this ComboBox input, object item) => input.ItemContainerGenerator.ContainerFromItem(item) as ComboBoxItem;

    public static object GetItem(this ComboBox input, ComboBoxItem item) => input.ItemContainerGenerator.ItemFromContainer(item);

    public static void ClearSelection(this ComboBox input)
    {
        foreach (var i in input.Items)
        {
            if (input.ItemContainerGenerator.ContainerFromItem(i) is ComboBoxItem item)
                XComboBoxItem.SetIsSelected(item, false);
        }
    }

    #endregion
}