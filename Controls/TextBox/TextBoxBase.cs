﻿using Ion;
using Ion.Collect;
using Ion.Data;
using Ion.Input;
using Ion.Numeral;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Ion.Controls;

[Extend<TextBoxBase>]
public static class XTextBoxBase
{
    public const string DefaultMouseTarget = "System.Windows.Controls.TextBoxView";

    public static readonly ResourceKey ValidationErrorTemplate = new();

    public static readonly ResourceKey ValidationMessageTemplate = new();

    public static readonly ResourceKey ValidationSuccessTemplate = new();

    public static readonly ResourceKey ValidationWarningTemplate = new();

    public static readonly ResourceKey ValidationIconTemplate = new();

    #region Properties

    #region Allow

    /// <summary>Gets or sets an array of characters to allow when typing. All characters are allowed if nothing is specified.</summary>
    public static readonly DependencyProperty AllowProperty = DependencyProperty.RegisterAttached("Allow", typeof(IReadOnlyCollection<char>), typeof(XTextBoxBase), new FrameworkPropertyMetadata(null));
    [TypeConverter(typeof(CharArrayTypeConverter))]
    public static IReadOnlyCollection<char> GetAllow(TextBoxBase i) => (IReadOnlyCollection<char>)i.GetValue(AllowProperty);
    public static void SetAllow(TextBoxBase i, IReadOnlyCollection<char> input) => i.SetValue(AllowProperty, input);

    #endregion

    #region CanClearSuggestions

    public static readonly DependencyProperty CanClearSuggestionsProperty = DependencyProperty.RegisterAttached("CanClearSuggestions", typeof(bool), typeof(XTextBoxBase), new FrameworkPropertyMetadata(false));
    public static bool GetCanClearSuggestions(TextBoxBase i) => (bool)i.GetValue(CanClearSuggestionsProperty);
    public static void SetCanClearSuggestions(TextBoxBase i, bool input) => i.SetValue(CanClearSuggestionsProperty, input);

    #endregion

    #region CaretIndex

    public static readonly DependencyProperty CaretIndexProperty = DependencyProperty.RegisterAttached("CaretIndex", typeof(int), typeof(XTextBoxBase), new FrameworkPropertyMetadata(0, OnCaretIndexChanged));
    public static int GetCaretIndex(TextBoxBase i) => (int)i.GetValue(CaretIndexProperty);
    public static void SetCaretIndex(TextBoxBase i, int input) => i.SetValue(CaretIndexProperty, input);

    private static void OnCaretIndexChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is TextBox box)
        {
            GetHandleSelection(box).DoInternal(() =>
            {
                box.CaretIndex = (int)e.NewValue;
                box.Focus();

                SetSelectionLength
                    (box, box.SelectionLength);
                SetSelectionStart
                    (box, box.SelectionStart);
            });
        }
    }

    #endregion

    #region ClearCommand

    public static readonly RoutedUICommand ClearCommand = new(nameof(ClearCommand), nameof(ClearCommand), typeof(XTextBoxBase));

    private static void ClearCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is RichTextBox a)
            a.Document.Blocks.Clear();

        if (sender is TextBox b)
            b.SetCurrentValue(TextBox.TextProperty, string.Empty);
    }

    private static void ClearCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is TextBoxBase control)
            e.CanExecute = control.IsEnabled && !control.IsReadOnly && !GetIsEmpty(control);
    }

    #endregion

    #region ClearSuggestionsCommand

    public static readonly RoutedUICommand ClearSuggestionsCommand = new(nameof(ClearSuggestionsCommand), nameof(ClearSuggestionsCommand), typeof(XTextBoxBase));

    private static void ClearSuggestionsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is TextBoxBase control)
        {
            GetSuggestions(control).Clear();
            UpdateSuggestions(control);
        }
    }

    private static void ClearSuggestionsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = GetSuggestions(sender as TextBoxBase)?.Count > 0;

    #endregion

    #region EnableCopyCommand

    public static readonly DependencyProperty EnableCopyCommandProperty = DependencyProperty.RegisterAttached("EnableCopyCommand", typeof(bool), typeof(XTextBoxBase), new FrameworkPropertyMetadata(true, OnEnableCopyCommandChanged));
    public static bool GetEnableCopyCommand(TextBoxBase i) => (bool)i.GetValue(EnableCopyCommandProperty);
    public static void SetEnableCopyCommand(TextBoxBase i, bool input) => i.SetValue(EnableCopyCommandProperty, input);
    private static void OnEnableCopyCommandChanged(object sender, DependencyPropertyChangedEventArgs e) => OnEnableCommandChanged(sender as UIElement, OnPreviewCopyExecuted, (bool)e.NewValue);

    private static void OnPreviewCopyExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Command == ApplicationCommands.Copy)
            e.Handled = true;
    }

    #endregion

    #region EnableCutCommand

    public static readonly DependencyProperty EnableCutCommandProperty = DependencyProperty.RegisterAttached("EnableCutCommand", typeof(bool), typeof(XTextBoxBase), new FrameworkPropertyMetadata(true, OnEnableCutCommandChanged));
    public static bool GetEnableCutCommand(TextBoxBase i) => (bool)i.GetValue(EnableCutCommandProperty);
    public static void SetEnableCutCommand(TextBoxBase i, bool input) => i.SetValue(EnableCutCommandProperty, input);
    private static void OnEnableCutCommandChanged(object sender, DependencyPropertyChangedEventArgs e) => OnEnableCommandChanged(sender as UIElement, OnPreviewCutExecuted, (bool)e.NewValue);

    private static void OnPreviewCutExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Command == ApplicationCommands.Cut)
            e.Handled = true;
    }

    #endregion

    #region EnablePasteCommand

    public static readonly DependencyProperty EnablePasteCommandProperty = DependencyProperty.RegisterAttached("EnablePasteCommand", typeof(bool), typeof(XTextBoxBase), new FrameworkPropertyMetadata(true, OnEnablePasteCommandChanged));
    public static bool GetEnablePasteCommand(TextBoxBase i) => (bool)i.GetValue(EnablePasteCommandProperty);
    public static void SetEnablePasteCommand(TextBoxBase i, bool input) => i.SetValue(EnablePasteCommandProperty, input);
    private static void OnEnablePasteCommandChanged(object sender, DependencyPropertyChangedEventArgs e) => OnEnableCommandChanged(sender as UIElement, OnPreviewPasteExecuted, (bool)e.NewValue);

    private static void OnPreviewPasteExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Command == ApplicationCommands.Paste)
            e.Handled = true;
    }

    #endregion

    #region Ignore

    /// <summary>Gets or sets an array of characters to ignore when typing. No characters are ignored if nothing is specified.</summary>
    public static readonly DependencyProperty IgnoreProperty = DependencyProperty.RegisterAttached("Ignore", typeof(IReadOnlyCollection<char>), typeof(XTextBoxBase), new FrameworkPropertyMetadata(null));
    [TypeConverter(typeof(CharArrayTypeConverter))]
    public static IReadOnlyCollection<char> GetIgnore(TextBoxBase i) => (IReadOnlyCollection<char>)i.GetValue(IgnoreProperty);
    public static void SetIgnore(TextBoxBase i, IReadOnlyCollection<char> input) => i.SetValue(IgnoreProperty, input);

    #endregion

    #region (readonly) IsEmpty

    private static readonly DependencyPropertyKey IsEmptyKey = DependencyProperty.RegisterAttachedReadOnly("IsEmpty", typeof(bool), typeof(XTextBoxBase), new FrameworkPropertyMetadata(true, null, new CoerceValueCallback(OnIsEmptyCoerced)));
    public static readonly DependencyProperty IsEmptyProperty = IsEmptyKey.DependencyProperty;
    public static bool GetIsEmpty(TextBoxBase i) => (bool)i.GetValue(IsEmptyProperty);

    private static object OnIsEmptyCoerced(DependencyObject sender, object input)
    {
        if (sender is RichTextBox a)
            return a.Empty();

        if (sender is TextBox b)
            return b.Text.IsEmpty();

        return true;
    }

    #endregion

    #region Left

    public static readonly DependencyProperty LeftProperty = DependencyProperty.RegisterAttached("Left", typeof(DataTemplateList), typeof(XTextBoxBase), new FrameworkPropertyMetadata(null));
    public static DataTemplateList GetLeft(TextBoxBase i) => (DataTemplateList)i.GetValue(LeftProperty);
    public static void SetLeft(TextBoxBase i, DataTemplateList input) => i.SetValue(LeftProperty, input);

    #endregion

    #region MenuAnimation

    public static readonly DependencyProperty MenuAnimationProperty = DependencyProperty.RegisterAttached("MenuAnimation", typeof(PopupAnimation), typeof(XTextBoxBase), new FrameworkPropertyMetadata(PopupAnimation.Fade));
    public static PopupAnimation GetMenuAnimation(TextBoxBase i) => (PopupAnimation)i.GetValue(MenuAnimationProperty);
    public static void SetMenuAnimation(TextBoxBase i, PopupAnimation input) => i.SetValue(MenuAnimationProperty, input);

    #endregion

    #region MenuButtonVisibility

    public static readonly DependencyProperty MenuButtonVisibilityProperty = DependencyProperty.RegisterAttached("MenuButtonVisibility", typeof(Visibility), typeof(XTextBoxBase), new FrameworkPropertyMetadata(Visibility.Visible));
    public static Visibility GetMenuButtonVisibility(TextBoxBase i) => (Visibility)i.GetValue(MenuButtonVisibilityProperty);
    public static void SetMenuButtonVisibility(TextBoxBase i, Visibility input) => i.SetValue(MenuButtonVisibilityProperty, input);

    #endregion

    #region MenuHeight

    public static readonly DependencyProperty MenuHeightProperty = DependencyProperty.RegisterAttached("MenuHeight", typeof(Range<double>), typeof(XTextBoxBase), new FrameworkPropertyMetadata(default(Range<double>)));
    [TypeConverter(typeof(DoubleRangeTypeConverter))]
    public static Range<double> GetMenuHeight(TextBoxBase i) => (Range<double>)i.GetValue(MenuHeightProperty);
    public static void SetMenuHeight(TextBoxBase i, Range<double> input) => i.SetValue(MenuHeightProperty, input);

    #endregion

    #region MenuPlacement

    public static readonly DependencyProperty MenuPlacementProperty = DependencyProperty.RegisterAttached("MenuPlacement", typeof(PlacementMode), typeof(XTextBoxBase), new FrameworkPropertyMetadata(PlacementMode.Bottom));
    public static PlacementMode GetMenuPlacement(TextBoxBase i) => (PlacementMode)i.GetValue(MenuPlacementProperty);
    public static void SetMenuPlacement(TextBoxBase i, PlacementMode input) => i.SetValue(MenuPlacementProperty, input);

    #endregion

    #region MenuTemplate

    public static readonly DependencyProperty MenuTemplateProperty = DependencyProperty.RegisterAttached("MenuTemplate", typeof(DataTemplate), typeof(XTextBoxBase), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetMenuTemplate(TextBoxBase i) => (DataTemplate)i.GetValue(MenuTemplateProperty);
    public static void SetMenuTemplate(TextBoxBase i, DataTemplate input) => i.SetValue(MenuTemplateProperty, input);

    #endregion

    #region MenuTrigger

    public static readonly DependencyProperty MenuTriggerProperty = DependencyProperty.RegisterAttached("MenuTrigger", typeof(PopupTriggers), typeof(XTextBoxBase), new FrameworkPropertyMetadata(PopupTriggers.All));
    public static PopupTriggers GetMenuTrigger(TextBoxBase i) => (PopupTriggers)i.GetValue(MenuTriggerProperty);
    public static void SetMenuTrigger(TextBoxBase i, PopupTriggers input) => i.SetValue(MenuTriggerProperty, input);

    #endregion

    #region MenuVisibility

    public static readonly DependencyProperty MenuVisibilityProperty = DependencyProperty.RegisterAttached("MenuVisibility", typeof(Visibility), typeof(XTextBoxBase), new FrameworkPropertyMetadata(Visibility.Collapsed));
    public static Visibility GetMenuVisibility(TextBoxBase i) => (Visibility)i.GetValue(MenuVisibilityProperty);
    public static void SetMenuVisibility(TextBoxBase i, Visibility input) => i.SetValue(MenuVisibilityProperty, input);

    #endregion

    #region Placeholder

    public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.RegisterAttached("Placeholder", typeof(object), typeof(XTextBoxBase), new FrameworkPropertyMetadata(null));
    public static object GetPlaceholder(TextBoxBase i) => i.GetValue(PlaceholderProperty);
    public static void SetPlaceholder(TextBoxBase i, object input) => i.SetValue(PlaceholderProperty, input);

    #endregion

    #region PlaceholderTemplate

    public static readonly DependencyProperty PlaceholderTemplateProperty = DependencyProperty.RegisterAttached("PlaceholderTemplate", typeof(DataTemplate), typeof(XTextBoxBase), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetPlaceholderTemplate(TextBoxBase i) => (DataTemplate)i.GetValue(PlaceholderTemplateProperty);
    public static void SetPlaceholderTemplate(TextBoxBase i, DataTemplate input) => i.SetValue(PlaceholderTemplateProperty, input);

    #endregion

    #region PlaceholderTemplateSelector

    public static readonly DependencyProperty PlaceholderTemplateSelectorProperty = DependencyProperty.RegisterAttached("PlaceholderTemplateSelector", typeof(DataTemplateSelector), typeof(XTextBoxBase), new FrameworkPropertyMetadata(null));
    public static DataTemplateSelector GetPlaceholderTemplateSelector(TextBoxBase i) => (DataTemplateSelector)i.GetValue(PlaceholderTemplateProperty);
    public static void SetPlaceholderTemplateSelector(TextBoxBase i, DataTemplateSelector input) => i.SetValue(PlaceholderTemplateProperty, input);

    #endregion

    #region Regex

    public static readonly DependencyProperty RegexProperty = DependencyProperty.RegisterAttached("Regex", typeof(string), typeof(XTextBoxBase), new FrameworkPropertyMetadata(null));
    public static string GetRegex(TextBoxBase i) => (string)i.GetValue(RegexProperty);
    public static void SetRegex(TextBoxBase i, string input) => i.SetValue(RegexProperty, input);

    #endregion

    #region Right

    public static readonly DependencyProperty RightProperty = DependencyProperty.RegisterAttached("Right", typeof(DataTemplateList), typeof(XTextBoxBase), new FrameworkPropertyMetadata(null));
    public static DataTemplateList GetRight(TextBoxBase i) => (DataTemplateList)i.GetValue(RightProperty);
    public static void SetRight(TextBoxBase i, DataTemplateList input) => i.SetValue(RightProperty, input);

    #endregion

    #region ScrollToEnd

    public static readonly DependencyProperty ScrollToEndProperty = DependencyProperty.RegisterAttached("ScrollToEnd", typeof(bool), typeof(XTextBoxBase), new FrameworkPropertyMetadata(false, OnScrollToEndChanged));
    public static bool GetScrollToEnd(TextBoxBase i) => (bool)i.GetValue(ScrollToEndProperty);
    public static void SetScrollToEnd(TextBoxBase i, bool input) => i.SetValue(ScrollToEndProperty, input);

    private static void OnScrollToEndChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is TextBoxBase box)
            box.AddHandlerAttached((bool)e.NewValue, ScrollToEndProperty, i => i.TextChanged += ScrollToEnd_TextChanged, i => i.TextChanged -= ScrollToEnd_TextChanged);
    }

    private static void ScrollToEnd_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBoxBase box)
            box.ScrollToEnd();
    }

    #endregion

    #region ScrollViewerStyle

    public static readonly DependencyProperty ScrollViewerStyleProperty = DependencyProperty.RegisterAttached("ScrollViewerStyle", typeof(Style), typeof(XTextBoxBase), new FrameworkPropertyMetadata(null));
    public static Style GetScrollViewerStyle(TextBoxBase i) => (Style)i.GetValue(ScrollViewerStyleProperty);
    public static void SetScrollViewerStyle(TextBoxBase i, Style input) => i.SetValue(ScrollViewerStyleProperty, input);

    #endregion

    #region SelectAllOnFocus

    public static readonly DependencyProperty SelectAllOnFocusProperty = DependencyProperty.RegisterAttached("SelectAllOnFocus", typeof(bool), typeof(XTextBoxBase), new FrameworkPropertyMetadata(false, OnSelectAllOnFocusChanged));
    public static bool GetSelectAllOnFocus(TextBoxBase i) => (bool)i.GetValue(SelectAllOnFocusProperty);
    public static void SetSelectAllOnFocus(TextBoxBase i, bool input) => i.SetValue(SelectAllOnFocusProperty, input);

    private static void OnSelectAllOnFocusChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is TextBoxBase control)
        {
            control.AddHandlerAttached((bool)e.NewValue, SelectAllOnFocusProperty, i =>
            {
                i.GotKeyboardFocus
                    += SelectAllOnFocus_GotKeyboardFocus;
                i.PreviewMouseDown
                    += SelectAllOnFocus_PreviewMouseDown;
            }, i =>
            {
                i.GotKeyboardFocus
                    -= SelectAllOnFocus_GotKeyboardFocus;
                i.PreviewMouseDown
                    -= SelectAllOnFocus_PreviewMouseDown;
            });
        }
    }

    private static void SelectAllOnFocus_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is TextBoxBase control)
        {
            if (control is not TextBox || control is TextBox textBox && XTextBox.GetIsEditable(textBox))
                control.SelectAll();
        }
    }

    private static void SelectAllOnFocus_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is TextBoxBase control)
        {
            if (control is not TextBox || control is TextBox textBox && XTextBox.GetIsEditable(textBox))
            {
                if (!control.IsKeyboardFocusWithin)
                {
                    //Not sure if this supports RichTextBox...
                    if (e.OriginalSource?.GetType().FullName == DefaultMouseTarget)
                    {
                        control.Focus();
                        e.Handled = true;
                    }
                }
            }
        }
    }

    #endregion

    #region (private) SelectionHandle

    private static readonly DependencyProperty HandleSelectionProperty = DependencyProperty.RegisterAttached("HandleSelection", typeof(Handle), typeof(XTextBoxBase), new FrameworkPropertyMetadata(null));

    private static Handle GetHandleSelection(TextBoxBase i) => i.GetValueOrSetDefault<Handle>(HandleSelectionProperty, () => false);

    #endregion

    #region SelectionLength

    public static readonly DependencyProperty SelectionLengthProperty = DependencyProperty.RegisterAttached("SelectionLength", typeof(int), typeof(XTextBoxBase), new FrameworkPropertyMetadata(0, OnSelectionLengthChanged));
    public static int GetSelectionLength(TextBoxBase i) => (int)i.GetValue(SelectionLengthProperty);
    public static void SetSelectionLength(TextBoxBase i, int input) => i.SetValue(SelectionLengthProperty, input);

    private static void OnSelectionLengthChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is TextBox box)
        {
            GetHandleSelection(box).DoInternal(() =>
            {
                box.Select(GetSelectionStart(box), (int)e.NewValue);
                box.Focus();

                SetCaretIndex(box, box.CaretIndex);
            });
        }
    }

    #endregion

    #region SelectionStart

    public static readonly DependencyProperty SelectionStartProperty = DependencyProperty.RegisterAttached("SelectionStart", typeof(int), typeof(XTextBoxBase), new FrameworkPropertyMetadata(0, OnSelectionStartChanged));
    public static int GetSelectionStart(TextBoxBase i) => (int)i.GetValue(SelectionStartProperty);
    public static void SetSelectionStart(TextBoxBase i, int input) => i.SetValue(SelectionStartProperty, input);

    private static void OnSelectionStartChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is TextBox box)
        {
            GetHandleSelection(box).DoInternal(() =>
            {
                box.Select((int)e.NewValue, GetSelectionLength(box));
                box.Focus();

                SetCaretIndex(box, box.CaretIndex);
            });
        }
    }

    #endregion

    ///

    #region DefaultSuggestionCommand

    public static readonly RoutedUICommand DefaultSuggestionCommand = new(nameof(DefaultSuggestionCommand), nameof(DefaultSuggestionCommand), typeof(XTextBoxBase));

    private static void DefaultSuggestionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is TextBoxBase control)
        {
            var result = GetSuggestionHandler(control).Convert(e.Parameter);

            if (sender is PathBox b)
                b.SetCurrentValue(TextBox.TextProperty, result);
            if (sender is RichTextBox)
            {
            }

            if (sender is TextBox d)
                d.SetCurrentValue(TextBox.TextProperty, result);

            GetSuggestionCommand(control)?.Execute(e.Parameter);
        }
    }

    private static void DefaultSuggestionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = e.Parameter != null;

    #endregion

    #region SuggestionCommand

    public static readonly DependencyProperty SuggestionCommandProperty = DependencyProperty.RegisterAttached("SuggestionCommand", typeof(ICommand), typeof(XTextBoxBase), new FrameworkPropertyMetadata(null));
    public static ICommand GetSuggestionCommand(TextBoxBase i) => (ICommand)i.GetValue(SuggestionCommandProperty);
    public static void SetSuggestionCommand(TextBoxBase i, ICommand input) => i.SetValue(SuggestionCommandProperty, input);

    #endregion

    #region SuggestionGroupDirection

    public static readonly DependencyProperty SuggestionGroupDirectionProperty = DependencyProperty.RegisterAttached("SuggestionGroupDirection", typeof(ListSortDirection), typeof(XTextBoxBase), new FrameworkPropertyMetadata(ListSortDirection.Ascending));
    public static ListSortDirection GetSuggestionGroupDirection(TextBoxBase i) => (ListSortDirection)i.GetValue(SuggestionGroupDirectionProperty);
    public static void SetSuggestionGroupDirection(TextBoxBase i, ListSortDirection input) => i.SetValue(SuggestionGroupDirectionProperty, input);

    #endregion

    #region SuggestionGroupName

    public static readonly DependencyProperty SuggestionGroupNameProperty = DependencyProperty.RegisterAttached("SuggestionGroupName", typeof(object), typeof(XTextBoxBase), new FrameworkPropertyMetadata(null));
    public static object GetSuggestionGroupName(TextBoxBase i) => i.GetValue(SuggestionGroupNameProperty);
    public static void SetSuggestionGroupName(TextBoxBase i, object input) => i.SetValue(SuggestionGroupNameProperty, input);

    #endregion

    #region SuggestionHandler

    public static readonly DependencyProperty SuggestionHandlerProperty = DependencyProperty.RegisterAttached("SuggestionHandler", typeof(ISuggest), typeof(XTextBoxBase), new FrameworkPropertyMetadata(null));
    public static ISuggest GetSuggestionHandler(TextBoxBase i) => i.GetValueOrSetDefault<ISuggest>(SuggestionHandlerProperty, () => new StartsWithSuggestionHandler());
    public static void SetSuggestionHandler(TextBoxBase i, ISuggest input) => i.SetValue(SuggestionHandlerProperty, input);

    #endregion

    #region Suggestions

    public static readonly DependencyProperty SuggestionsProperty = DependencyProperty.RegisterAttached("Suggestions", typeof(IList), typeof(XTextBoxBase), new FrameworkPropertyMetadata(null, OnSuggestionsChanged));
    public static IList GetSuggestions(TextBoxBase i) => (IList)i.GetValue(SuggestionsProperty);
    public static void SetSuggestions(TextBoxBase i, IList input) => i.SetValue(SuggestionsProperty, input);

    private static void OnSuggestionsChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is TextBoxBase box)
        {
            box.AddHandlerAttached(e.NewValue is IList, SuggestionsProperty, i =>
            {
                i.GotFocus
                    += Suggestions_GotFocus;
                i.GotKeyboardFocus
                    += Suggestions_GotKeyboardFocus;
                i.TextChanged
                    += Suggestions_TextChanged;
            }, i =>
            {
                i.GotFocus
                    -= Suggestions_GotFocus;
                i.GotKeyboardFocus
                    -= Suggestions_GotKeyboardFocus;
                i.TextChanged
                    -= Suggestions_TextChanged;
            });
        }
    }

    ///

    private static void UpdateSuggestions(TextBoxBase control)
    {
        var handler = GetSuggestionHandler(control);
        GetSuggestionsFound(control).Clear();
        foreach (var i in GetSuggestions(control))
        {
            string result = default;
            if (control is PathBox b)
                result = b.Text;

            else if (control is RichTextBox)
            {
            }

            else if (control is TextBox d)
                result = d.Text;

            else if (control is TokenBox)
            {

            }

            if (handler.Handle(i, result))
                GetSuggestionsFound(control).Add(i);
        }
    }

    ///

    private static void Suggestions_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBoxBase box)
        {
            switch (GetMenuTrigger(box))
            {
                case PopupTriggers.All:
                case PopupTriggers.GotFocus:
                    if (GetSuggestionHandler(box).UpdateOnFocus)
                        UpdateSuggestions(box);

                    SetMenuVisibility(box, Visibility.Visible);
                    break;
            }
        }
    }

    private static void Suggestions_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is TextBoxBase box)
        {
            switch (GetMenuTrigger(box))
            {
                case PopupTriggers.All:
                case PopupTriggers.GotKeyboardFocus:
                    if (GetSuggestionHandler(box).UpdateOnFocus)
                        UpdateSuggestions(box);

                    SetMenuVisibility(box, Visibility.Visible);
                    break;
            }
        }
    }

    private static void Suggestions_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBoxBase box)
        {
            switch (GetMenuTrigger(box))
            {
                case PopupTriggers.All:
                case PopupTriggers.TextChanged:
                    if (GetSuggestionHandler(box).UpdateOnChange)
                        UpdateSuggestions(box);

                    SetMenuVisibility(box, Visibility.Visible);
                    break;
            }
        }
    }

    #endregion

    #region (readonly) SuggestionsFound

    private static readonly DependencyPropertyKey SuggestionsFoundKey = DependencyProperty.RegisterAttachedReadOnly("SuggestionsFound", typeof(ListObservable), typeof(XTextBoxBase), new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty SuggestionsFoundProperty = SuggestionsFoundKey.DependencyProperty;
    public static ListObservable GetSuggestionsFound(TextBoxBase i) => i.GetValueOrSetDefault<ListObservable>(SuggestionsFoundKey, () => []);

    #endregion

    #region SuggestionTemplate

    public static readonly DependencyProperty SuggestionTemplateProperty = DependencyProperty.RegisterAttached("SuggestionTemplate", typeof(DataTemplate), typeof(XTextBoxBase), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetSuggestionTemplate(TextBoxBase i) => (DataTemplate)i.GetValue(SuggestionTemplateProperty);
    public static void SetSuggestionTemplate(TextBoxBase i, DataTemplate input) => i.SetValue(SuggestionTemplateProperty, input);

    #endregion

    #region SuggestionTemplateSelector

    public static readonly DependencyProperty SuggestionTemplateSelectorProperty = DependencyProperty.RegisterAttached("SuggestionTemplateSelector", typeof(DataTemplateSelector), typeof(XTextBoxBase), new FrameworkPropertyMetadata(null));
    public static DataTemplateSelector GetSuggestionTemplateSelector(TextBoxBase i) => (DataTemplateSelector)i.GetValue(SuggestionTemplateSelectorProperty);
    public static void SetSuggestionTemplateSelector(TextBoxBase i, DataTemplateSelector input) => i.SetValue(SuggestionTemplateSelectorProperty, input);

    #endregion

    #region SuggestionSortDirection

    public static readonly DependencyProperty SuggestionSortDirectionProperty = DependencyProperty.RegisterAttached("SuggestionSortDirection", typeof(ListSortDirection), typeof(XTextBoxBase), new FrameworkPropertyMetadata(ListSortDirection.Ascending));
    public static ListSortDirection GetSuggestionSortDirection(TextBoxBase i) => (ListSortDirection)i.GetValue(SuggestionSortDirectionProperty);
    public static void SetSuggestionSortDirection(TextBoxBase i, ListSortDirection input) => i.SetValue(SuggestionSortDirectionProperty, input);

    #endregion

    #region SuggestionSortName

    public static readonly DependencyProperty SuggestionSortNameProperty = DependencyProperty.RegisterAttached("SuggestionSortName", typeof(object), typeof(XTextBoxBase), new FrameworkPropertyMetadata(null));
    public static object GetSuggestionSortName(TextBoxBase i) => i.GetValue(SuggestionSortNameProperty);
    public static void SetSuggestionSortName(TextBoxBase i, object input) => i.SetValue(SuggestionSortNameProperty, input);

    #endregion

    ///

    #region Tab

    public static readonly DependencyProperty TabProperty = DependencyProperty.RegisterAttached("Tab", typeof(uint), typeof(XTextBoxBase), new FrameworkPropertyMetadata((uint)4));
    public static uint GetTab(TextBoxBase i) => (uint)i.GetValue(TabProperty);
    public static void SetTab(TextBoxBase i, uint input) => i.SetValue(TabProperty, input);

    #endregion

    #region TextTrimming

    public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.RegisterAttached("TextTrimming", typeof(TextTrimming), typeof(XTextBoxBase), new FrameworkPropertyMetadata(TextTrimming.None));
    public static TextTrimming GetTextTrimming(TextBoxBase i) => (TextTrimming)i.GetValue(TextTrimmingProperty);
    public static void SetTextTrimming(TextBoxBase i, TextTrimming input) => i.SetValue(TextTrimmingProperty, input);

    #endregion

    #region TextTrimmingTemplate

    public static readonly DependencyProperty TextTrimmingTemplateProperty = DependencyProperty.RegisterAttached("TextTrimmingTemplate", typeof(DataTemplate), typeof(XTextBoxBase), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetTextTrimmingTemplate(TextBoxBase i) => (DataTemplate)i.GetValue(TextTrimmingTemplateProperty);
    public static void SetTextTrimmingTemplate(TextBoxBase i, DataTemplate input) => i.SetValue(TextTrimmingTemplateProperty, input);

    #endregion

    ///

    #region ValidationIconErrorTemplate

    public static readonly DependencyProperty ValidationIconErrorTemplateProperty = DependencyProperty.RegisterAttached("ValidationIconErrorTemplate", typeof(DataTemplate), typeof(XTextBoxBase), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetValidationIconErrorTemplate(TextBoxBase i) => (DataTemplate)i.GetValue(ValidationIconErrorTemplateProperty);
    public static void SetValidationIconErrorTemplate(TextBoxBase i, DataTemplate value) => i.SetValue(ValidationIconErrorTemplateProperty, value);

    #endregion

    #region ValidationIconSuccessTemplate

    public static readonly DependencyProperty ValidationIconSuccessTemplateProperty = DependencyProperty.RegisterAttached("ValidationIconSuccessTemplate", typeof(DataTemplate), typeof(XTextBoxBase), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetValidationIconSuccessTemplate(TextBoxBase i) => (DataTemplate)i.GetValue(ValidationIconSuccessTemplateProperty);
    public static void SetValidationIconSuccessTemplate(TextBoxBase i, DataTemplate value) => i.SetValue(ValidationIconSuccessTemplateProperty, value);

    #endregion

    #region ValidationIconSuccessVisibility

    public static readonly DependencyProperty ValidationIconSuccessVisibilityProperty = DependencyProperty.RegisterAttached("ValidationIconSuccessVisibility", typeof(Visibility), typeof(XTextBoxBase), new FrameworkPropertyMetadata(Visibility.Collapsed));
    public static Visibility GetValidationIconSuccessVisibility(TextBoxBase i) => (Visibility)i.GetValue(ValidationIconSuccessVisibilityProperty);
    public static void SetValidationIconSuccessVisibility(TextBoxBase i, Visibility value) => i.SetValue(ValidationIconSuccessVisibilityProperty, value);

    #endregion

    #region ValidationIconVisibility

    public static readonly DependencyProperty ValidationIconVisibilityProperty = DependencyProperty.RegisterAttached("ValidationIconVisibility", typeof(Visibility), typeof(XTextBoxBase), new FrameworkPropertyMetadata(Visibility.Collapsed));
    public static Visibility GetValidationIconVisibility(TextBoxBase i) => (Visibility)i.GetValue(ValidationIconVisibilityProperty);
    public static void SetValidationIconVisibility(TextBoxBase i, Visibility value) => i.SetValue(ValidationIconVisibilityProperty, value);

    #endregion

    #endregion

    #region XTextBoxBase

    static XTextBoxBase()
    {
        EventManager.RegisterClassHandler(typeof(TextBoxBase), FrameworkElement.LoadedEvent,
            new RoutedEventHandler(OnLoaded), true);
        EventManager.RegisterClassHandler(typeof(TextBoxBase), UIElement.PreviewKeyDownEvent,
            new KeyEventHandler(OnPreviewKeyDown), true);
        EventManager.RegisterClassHandler(typeof(TextBoxBase), UIElement.PreviewTextInputEvent,
            new TextCompositionEventHandler(OnPreviewTextInput), true);
        EventManager.RegisterClassHandler(typeof(TextBoxBase), TextBoxBase.SelectionChangedEvent,
            new RoutedEventHandler(OnSelectionChanged), true);
        EventManager.RegisterClassHandler(typeof(TextBoxBase), TextBoxBase.TextChangedEvent,
            new TextChangedEventHandler(OnTextChanged), true);
    }

    #endregion

    #region Methods

    private static void OnEnableCommandChanged(UIElement sender, ExecutedRoutedEventHandler handler, bool enable)
    {
        if (sender != null)
        {
            if (enable)
                CommandManager.RemovePreviewExecutedHandler(sender, handler);

            else CommandManager.AddPreviewExecutedHandler(sender, handler);
        }
    }

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox box)
        {
            box.AddOnce(new CommandBinding
                (ClearCommand, ClearCommand_Executed, ClearCommand_CanExecute));
            box.AddOnce(new CommandBinding
                (ClearSuggestionsCommand, ClearSuggestionsCommand_Executed, ClearSuggestionsCommand_CanExecute));
            box.AddOnce(new CommandBinding
                (DefaultSuggestionCommand, DefaultSuggestionCommand_Executed, DefaultSuggestionCommand_CanExecute));
        }
    }

    /// <summary>
    /// If the AcceptsTab property is set to true, the user must press CTRL+TAB to move the focus to the next control in the tab order (<see langword="https://tinyurl.com/6z62u4k5"/>).
    /// </summary>
    private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Tab)
        {
            if (sender is TextBoxBase box)
            {
                if (box.AcceptsTab)
                {
                    if (ModifierKeys.Control.Pressed())
                        return;

                    e.Handled = true;
                    if (ModifierKeys.Shift.Pressed())
                        ReverseTab(box);

                    else ApplyTab(box);
                }
            }
        }
    }

    private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is TextBoxBase box)
        {
            if (GetAllow(box)?.Any(i => i == e.Text[0]) == false)
                e.Handled = true;

            else if (GetIgnore(box)?.Any(i => i == e.Text[0]) == true)
                e.Handled = true;

            else if (GetRegex(box) is string regex)
            {
                if (!new Regex(regex).IsMatch(e.Text))
                    e.Handled = true;
            }
        }
    }

    private static void OnSelectionChanged(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox box)
        {
            GetHandleSelection(box).DoInternal(() =>
            {
                SetCaretIndex
                    (box, box.CaretIndex);
                SetSelectionLength
                    (box, box.SelectionLength);
                SetSelectionStart
                    (box, box.SelectionStart);
            });
        }
    }

    private static void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBoxBase i)
            i.InvalidateProperty(IsEmptyProperty);
    }

    ///

    private static void ApplyTab(TextBoxBase box)
    {
        var length = (int)GetTab(box);
        if (box is TextBox textBox)
        {
            var oldData = Clipboard.GetDataObject();

            var newData = new string(' ', length);
            Clipboard.SetData(DataFormats.Text, newData);
            textBox.Paste();

            oldData.IfNotNull(i => Clipboard.SetDataObject(i));
        }
    }

    private static void ReverseTab(TextBoxBase box)
    {
        if (box is TextBox textBox)
        {
            var preserve
                = false;

            var oldLength = 0;

            int focusIndex;
            if (textBox.SelectionLength > 0)
            {
                focusIndex
                    = textBox.SelectionStart;
                oldLength
                    = textBox.SelectionLength;
                preserve
                    = true;
            }
            else focusIndex = textBox.CaretIndex;

            var startIndex
                = focusIndex - 1;
            var endIndex
                = -1;

            for (var i = startIndex; i > startIndex - GetTab(box); i--)
            {
                if (i < 0)
                    break;

                if (textBox.Text[i] == ' ')
                {
                    endIndex = i;
                    continue;
                }

                break;
            }

            if (startIndex >= 0 && endIndex >= 0)
            {
                var result = new StringBuilder();
                result.Append(textBox.Text[..endIndex]);
                result.Append(textBox.Text[focusIndex..]);

                textBox.Text
                    = result.ToString();
                textBox.CaretIndex
                    = endIndex;

                if (preserve)
                {
                    textBox.SelectionStart
                        = endIndex;
                    textBox.SelectionLength
                        = oldLength;
                }
            }
        }
    }

    #endregion
}