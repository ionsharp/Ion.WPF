using Ion;
using Ion.Analysis;
using Ion.Collect;
using Ion.Controls;
using Ion.Input;
using Ion.Text;
using Ion.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Ion.Controls;

[Extend<RichTextBox>]
public static class XRichTextBox
{
    #region (enum) Actions

    private enum Actions
    {
        Load,
        Save
    }

    #endregion

    #region (class) ActionData

    private class ActionData(RichTextBox control, Actions action, string path)
    {
        public readonly Actions Action
                = action;

        public readonly RichTextBox Control
                = control;

        public readonly string Path
                = path;
    }

    #endregion

    #region Properties

    #region AutoSave

    public static readonly DependencyProperty AutoSaveProperty = DependencyProperty.RegisterAttached("AutoSave", typeof(bool), typeof(XRichTextBox), new FrameworkPropertyMetadata(false));
    public static bool GetAutoSave(RichTextBox i) => (bool)i.GetValue(AutoSaveProperty);
    public static void SetAutoSave(RichTextBox i, bool input) => i.SetValue(AutoSaveProperty, input);

    #endregion

    #region Lines

    private static readonly Dictionary<IList, RichTextBox> LineSources = [];

    private static void CreateLine(RichTextBox box, object i)
    {
        var content = new ContentPresenter() { Content = i };
        content.Bind(ContentPresenter.ContentTemplateProperty, new PropertyPath("(0)", LineTemplateProperty), box);
        content.Bind(FrameworkElement.StyleProperty, new PropertyPath("(0)", LineStyleProperty), box);

        box.Document.Blocks.Add(new Paragraph(new InlineUIContainer(content)));
    }

    public static readonly DependencyProperty LinesProperty = DependencyProperty.RegisterAttached("Lines", typeof(object), typeof(XRichTextBox), new FrameworkPropertyMetadata(null, OnLinesChanged));
    public static object GetLines(RichTextBox i) => i.GetValue(LinesProperty);
    public static void SetLines(RichTextBox i, object input) => i.SetValue(LinesProperty, input);

    private static void OnLinesChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is RichTextBox box)
        {
            box.AddHandlerAttached(e.NewValue is IListObservable, LinesProperty, i =>
            {
                box.Document = new FlowDocument() { };
                if (GetLines(box) is IListObservable lines)
                {
                    LineSources.Add(lines, box);

                    lines.ForEach(i => CreateLine(box, i));
                    lines.CollectionChanged -= OnLinesChanged; lines.CollectionChanged += OnLinesChanged;
                }
            },
            (Action<RichTextBox>)(i =>
            {
                var lines = GetLines(box) as IListObservable;
                lines.IfNotNull(j => j.CollectionChanged -= OnLinesChanged);

                Try.Do(() => box.Document.Blocks.Clear());

                if (lines != null && LineSources.ContainsKey(lines))
                    LineSources.Remove(lines);
            }));
        }
    }

    private static void OnLinesChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is IListObservable lines)
        {
            if (LineSources.ContainsKey(lines))
            {
                var box = LineSources[lines];
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        e.NewItems?.ForEach(i => CreateLine(box, i));
                        break;

                    case NotifyCollectionChangedAction.Move:
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        e.OldItems?.ForEach(i => { });
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        box.Document.Blocks.Clear();
                        break;
                }
            }
        }
    }

    #endregion

    #region LineStyle

    public static readonly DependencyProperty LineStyleProperty = DependencyProperty.RegisterAttached("LineStyle", typeof(Style), typeof(XRichTextBox), new FrameworkPropertyMetadata(null));
    public static Style GetLineStyle(RichTextBox i) => (Style)i.GetValue(LineStyleProperty);
    public static void SetLineStyle(RichTextBox i, Style input) => i.SetValue(LineStyleProperty, input);

    #endregion

    #region LineTemplate

    public static readonly DependencyProperty LineTemplateProperty = DependencyProperty.RegisterAttached("LineTemplate", typeof(DataTemplate), typeof(XRichTextBox), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetLineTemplate(RichTextBox i) => (DataTemplate)i.GetValue(LineTemplateProperty);
    public static void SetLineTemplate(RichTextBox i, DataTemplate input) => i.SetValue(LineTemplateProperty, input);

    #endregion

    #region (ReadOnly) Loading

    private static readonly DependencyPropertyKey LoadingKey = DependencyProperty.RegisterAttachedReadOnly("Loading", typeof(bool), typeof(XRichTextBox), new FrameworkPropertyMetadata(false));
    public static readonly DependencyProperty LoadingProperty = LoadingKey.DependencyProperty;
    public static bool GetLoading(RichTextBox i) => (bool)i.GetValue(LoadingProperty);
    private static void SetLoading(RichTextBox i, bool input) => i.SetValue(LoadingKey, input);

    #endregion

    #region (ReadOnly) IsModified

    private static readonly DependencyPropertyKey IsModifiedKey = DependencyProperty.RegisterAttachedReadOnly("IsModified", typeof(bool), typeof(XRichTextBox), new FrameworkPropertyMetadata(false, OnIsModifiedChanged));
    public static readonly DependencyProperty IsModifiedProperty = IsModifiedKey.DependencyProperty;
    public static bool GetIsModified(RichTextBox i) => (bool)i.GetValue(IsModifiedProperty);

    private static void OnIsModifiedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is RichTextBox box)
            box.RaiseEvent(new(ModifiedEvent, box));
    }

    #endregion

    #region MarkUp

    public static readonly DependencyProperty MarkUpProperty = DependencyProperty.RegisterAttached("MarkUp", typeof(string), typeof(XRichTextBox), new FrameworkPropertyMetadata(null, OnMarkUpChanged));
    public static string GetMarkUp(RichTextBox i) => (string)i.GetValue(MarkUpProperty);
    public static void SetMarkUp(RichTextBox i, string value) => i.SetValue(MarkUpProperty, value);
    private static void OnMarkUpChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is RichTextBox box)
            new MarkUpParser().Parse(box.Document, e.NewValue);
    }

    #endregion

    #region (RoutedEvent) Modified

    public static readonly RoutedEvent ModifiedEvent = EventManager.RegisterRoutedEvent("Modified", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RichTextBox));
    public static void AddModifiedHandler(DependencyObject i, RoutedEventHandler handler)
        => i.As<RichTextBox>().AddHandler(ModifiedEvent, handler);
    public static void RemoveModifiedHandler(DependencyObject i, RoutedEventHandler handler)
        => i.As<RichTextBox>().RemoveHandler(ModifiedEvent, handler);

    #endregion

    #region Path

    public static readonly DependencyProperty PathProperty = DependencyProperty.RegisterAttached("Path", typeof(string), typeof(XRichTextBox), new FrameworkPropertyMetadata(null, OnPathChanged));
    public static string GetPath(RichTextBox i) => (string)i.GetValue(PathProperty);
    public static void SetPath(RichTextBox i, string input) => i.SetValue(PathProperty, input);

    private static void OnPathChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is RichTextBox box)
            box.Load();
    }

    #endregion

    #region Queue

    private static readonly DependencyProperty QueueProperty = DependencyProperty.RegisterAttached("Queue", typeof(Taskable<ActionData>), typeof(XRichTextBox), new FrameworkPropertyMetadata(null));

    private static Taskable<ActionData> GetQueue(RichTextBox i) => i.GetValueOrSetDefault(QueueProperty, () => new Taskable<ActionData>(null, OnAction, TaskStrategy.Ignore));

    #endregion

    #region (ReadOnly) Saving

    private static readonly DependencyPropertyKey SavingKey = DependencyProperty.RegisterAttachedReadOnly("Saving", typeof(bool), typeof(XRichTextBox), new FrameworkPropertyMetadata(false));
    public static readonly DependencyProperty SavingProperty = SavingKey.DependencyProperty;
    public static bool GetSaving(RichTextBox i) => (bool)i.GetValue(SavingProperty);
    private static void SetSaving(RichTextBox i, bool input) => i.SetValue(SavingKey, input);

    #endregion

    #endregion

    #region XRichTextBox

    static XRichTextBox()
    {
        EventManager.RegisterClassHandler(typeof(RichTextBox), UIElement.PreviewKeyDownEvent,
            new KeyEventHandler(OnPreviewKeyDown), true);
        EventManager.RegisterClassHandler(typeof(RichTextBox), System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent,
            new TextChangedEventHandler(OnTextChanged), true);
    }

    private static async Task OnAction(ActionData data, CancellationToken token)
    {
        switch (data.Action)
        {
            case Actions.Load:
                SetLoading(data.Control, true);
                await Dispatch.BeginInvoke(() => data.Control.Load(data.Path));
                SetLoading(data.Control, false);
                break;

            case Actions.Save:
                SetSaving(data.Control, true);
                await Dispatch.BeginInvoke(() => data.Control.Save(data.Path));
                SetSaving(data.Control, false);
                break;
        }
        data.Control.SetValue(IsModifiedKey, false);
    }

    private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is RichTextBox box)
        {
            if (ModifierKeys.Control.Pressed())
            {
                if (e.Key == Key.S)
                    box.Save();
            }
        }
    }

    private static void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is RichTextBox box)
        {
            box.SetValue(IsModifiedKey, true);
            if (GetAutoSave(box))
                box.Save();
        }
    }

    #endregion

    #region Methods

    #region [Source] https://github.com/adamecr/RadProjectsExtension; https://github.com/dwmkerr/consolecontrol/issues/25#issuecomment-437586440

    /// <summary>
    /// Gets the current caret position (offset from start)
    /// </summary>
    /// <param name="richTextBox">Rich text box to work with</param>
    /// <returns>The current caret position (offset from start)</returns>
    public static int GetCaretPosition(this RichTextBox richTextBox)
    {
        return richTextBox.Document.ContentStart.GetOffsetToPosition(richTextBox.CaretPosition);
    }

    /// <summary>
    /// Gets the position of the end of the content (offset from start)
    /// </summary>
    /// <param name="richTextBox">Rich text box to work with</param>
    /// <returns>The position of the end of the content (offset from start)</returns>
    public static int GetEndPosition(this RichTextBox richTextBox)
    {
        return richTextBox.Document.ContentStart.GetOffsetToPosition(richTextBox.Document.ContentEnd);
    }

    /// <summary>
    /// Gets the pointer to the end of the content
    /// </summary>
    /// <param name="richTextBox">Rich text box to work with</param>
    /// <returns>The pointer to the end of the content</returns>
    public static TextPointer GetEndPointer(this RichTextBox richTextBox)
    {
        return richTextBox.Document.ContentEnd;
    }

    /// <summary>
    ///  Gets the pointer to given <paramref name="position"/> (offset from start) within the content
    /// </summary>
    /// <param name="richTextBox">Rich text box to work with</param>
    /// <param name="position">Offset from start</param>
    /// <returns>The pointer at given position</returns>
    public static TextPointer GetPointerAt(this RichTextBox richTextBox, int position)
    {
        return richTextBox.Document.ContentStart.GetPositionAtOffset(position);
    }

    /// <summary>
    /// Sets the caret to the end of the content
    /// </summary>
    /// <param name="richTextBox">Rich text box to work with</param>
    public static void SetCaretToEnd(this RichTextBox richTextBox)
    {
        richTextBox.CaretPosition = richTextBox.GetEndPointer();
    }

    #endregion

    public static bool Empty(this RichTextBox input)
    {
        var result = new TextRange(input.Document.ContentStart, input.Document.ContentEnd).Text;
        if (!result.IsWhite() && !result.IsEmpty())
            return false;

        if (input.Document.Blocks.OfType<BlockUIContainer>().Any())
            return false;

        var p = input.Document.Blocks.OfType<Paragraph>();
        foreach (var i in p)
        {
            if (i.Inlines.Any(j => j is InlineUIContainer || j is Run k && !k.Text.IsWhite()))
                return false;
        }
        return true;
    }

    ///

    public static Result Load(this RichTextBox input, string filePath)
    {
        Result result = true;
        try
        {
            using var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.OpenOrCreate);
            var textRange = new TextRange(input.Document.ContentStart, input.Document.ContentEnd);
            textRange.Load(fileStream, DataFormats.Rtf);
        }
        catch (Exception e)
        {
            Log.Write(e);
            result = e;
        }

        return result;
    }

    public static void Load(this RichTextBox input) => _ = GetQueue(input).Start(new ActionData(input, Actions.Load, GetPath(input)));

    ///

    public static Result Save(this RichTextBox input, string filePath)
    {
        Result result = true;
        try
        {
            using var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create);
            var textRange = new TextRange(input.Document.ContentStart, input.Document.ContentEnd);
            textRange.Save(fileStream, DataFormats.Rtf);
        }
        catch (Exception e)
        {
            Log.Write(e);
            result = e;
        }
        return result;
    }

    public static void Save(this RichTextBox input) => _ = GetQueue(input).Start(new ActionData(input, Actions.Save, GetPath(input)));

    #endregion
}