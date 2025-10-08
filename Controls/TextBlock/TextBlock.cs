using Ion.Data;
using Ion.Numeral;
using Ion.Text;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Ion.Controls;

[Extend<TextBlock>]
public static class XTextBlock
{
    #region FontScale

    public static readonly DependencyProperty FontScaleProperty = DependencyProperty.RegisterAttached("FontScale", typeof(double), typeof(XTextBlock), new FrameworkPropertyMetadata(1.0, OnFontScaleChanged));
    public static double GetFontScale(TextBlock i) => (double)i.GetValue(FontScaleProperty);
    public static void SetFontScale(TextBlock i, double input) => i.SetValue(FontScaleProperty, input);

    private static void OnFontScaleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        var textBlock = sender as TextBlock;
        textBlock.FontSize = Math.Clamp(GetFontScaleOrigin(textBlock) * GetFontScale(textBlock), 0.01, double.MaxValue);
    }

    #endregion

    #region FontScaleOrigin

    public static readonly DependencyProperty FontScaleOriginProperty = DependencyProperty.RegisterAttached("FontScaleOrigin", typeof(double), typeof(XTextBlock), new FrameworkPropertyMetadata(SystemFonts.MessageFontSize, OnFontScaleOriginChanged));
    public static double GetFontScaleOrigin(TextBlock i) => (double)i.GetValue(FontScaleOriginProperty);
    public static void SetFontScaleOrigin(TextBlock i, double input) => i.SetValue(FontScaleOriginProperty, input);

    private static void OnFontScaleOriginChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        var textBlock = sender as TextBlock;
        textBlock.FontSize = Math.Clamp(GetFontScaleOrigin(textBlock) * GetFontScale(textBlock), 0.01, double.MaxValue);
    }

    #endregion

    #region MarkDown

    public static readonly DependencyProperty MarkDownProperty = DependencyProperty.RegisterAttached("MarkDown", typeof(string), typeof(XTextBlock), new FrameworkPropertyMetadata(null, OnMarkDownChanged));
    public static string GetMarkDown(TextBlock i) => (string)i.GetValue(MarkDownProperty);
    public static void SetMarkDown(TextBlock i, string value) => i.SetValue(MarkDownProperty, value);
    private static void OnMarkDownChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is TextBlock block)
            new MarkDownParser().Parse(block, e.NewValue);
    }

    #endregion

    #region MarkUp

    public static readonly DependencyProperty MarkUpProperty = DependencyProperty.RegisterAttached("MarkUp", typeof(string), typeof(XTextBlock), new FrameworkPropertyMetadata(null, OnMarkUpChanged));
    public static string GetMarkUp(TextBlock i) => (string)i.GetValue(MarkUpProperty);
    public static void SetMarkUp(TextBlock i, string value) => i.SetValue(MarkUpProperty, value);
    private static void OnMarkUpChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is TextBlock block)
            new MarkUpParser().Parse(block, e.NewValue);
    }

    #endregion

    #region SplitText

    public static readonly DependencyProperty SplitTextProperty = DependencyProperty.RegisterAttached("SplitText", typeof(string), typeof(XTextBlock), new FrameworkPropertyMetadata(null, OnSplitTextChanged));
    public static string GetSplitText(TextBlock i) => (string)i.GetValue(SplitTextProperty);
    public static void SetSplitText(TextBlock i, string value) => i.SetValue(SplitTextProperty, value);

    private static void OnSplitTextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is TextBlock block)
        {
            if (block.Inlines.Count > 0)
            {
                foreach (Run i in block.Inlines.Cast<Run>())
                    i.Unbind(FrameworkContentElement.StyleProperty);

                block.Inlines.Clear();
            }

            var text = GetSplitText(block);
            if (text is null) return;

            if (GetSplitTextWord(block) != SearchWord.Exact)
            {
                block.Text = text;
                return;
            }

            var key = GetSplitTextKey(block);
            if (key.IsEmpty())
            {
                block.Text = text;
                return;
            }

            var searchText = GetSplitTextCase(block) ? key : key.ToLower();
            var targetText = GetSplitTextCase(block) ? text : text.ToLower();

            Run Apply(Run run)
            {
                run.Bind(FrameworkContentElement.StyleProperty, new PropertyPath("(0)", SplitTextStyleProperty), block);
                return run;
            }

            switch (GetSplitTextCondition(block))
            {
                case SearchCondition.Contains:
                    var index = targetText.IndexOf(searchText);
                    if (index >= 0)
                    {
                        if (index > 0)
                            block.Inlines.Add(new Run(text[..index]));

                        block.Inlines.Add(Apply(new Run(text.Substring(index, searchText.Length))));

                        if (index + searchText.Length < targetText.Length)
                            block.Inlines.Add(new Run(text[(index + searchText.Length)..]));

                        return;
                    }
                    break;

                case SearchCondition.EndsWith:
                    if (targetText.EndsWith(searchText))
                    {
                        if (targetText.Length - searchText.Length > 0)
                            block.Inlines.Add(new Run(text[..(targetText.Length - searchText.Length)]));

                        block.Inlines.Add(Apply(new Run(text[(targetText.Length - searchText.Length)..])));
                        return;
                    }
                    break;

                case SearchCondition.StartsWith:
                    if (targetText.StartsWith(searchText))
                    {
                        block.Inlines.Add(Apply(new Run(text[..searchText.Length])));

                        if (searchText.Length < targetText.Length)
                            block.Inlines.Add(new Run(text[searchText.Length..]));

                        return;
                    }
                    break;
            }

            block.Text = text;
        }
    }

    #endregion

    #region SplitTextCase

    public static readonly DependencyProperty SplitTextCaseProperty = DependencyProperty.RegisterAttached("SplitTextCase", typeof(bool), typeof(XTextBlock), new FrameworkPropertyMetadata(false, OnSplitTextChanged));
    public static bool GetSplitTextCase(TextBlock i) => (bool)i.GetValue(SplitTextCaseProperty);
    public static void SetSplitTextCase(TextBlock i, bool value) => i.SetValue(SplitTextCaseProperty, value);

    #endregion

    #region SplitTextCondition

    public static readonly DependencyProperty SplitTextConditionProperty = DependencyProperty.RegisterAttached("SplitTextCondition", typeof(SearchCondition), typeof(XTextBlock), new FrameworkPropertyMetadata(SearchCondition.StartsWith, OnSplitTextChanged));
    public static SearchCondition GetSplitTextCondition(TextBlock i) => (SearchCondition)i.GetValue(SplitTextConditionProperty);
    public static void SetSplitTextCondition(TextBlock i, SearchCondition value) => i.SetValue(SplitTextConditionProperty, value);

    #endregion

    #region SplitTextKey

    public static readonly DependencyProperty SplitTextKeyProperty = DependencyProperty.RegisterAttached("SplitTextKey", typeof(string), typeof(XTextBlock), new FrameworkPropertyMetadata(null, OnSplitTextChanged));
    public static string GetSplitTextKey(TextBlock i) => (string)i.GetValue(SplitTextKeyProperty);
    public static void SetSplitTextKey(TextBlock i, string value) => i.SetValue(SplitTextKeyProperty, value);

    #endregion

    #region SplitTextStyle

    public static readonly DependencyProperty SplitTextStyleProperty = DependencyProperty.RegisterAttached("SplitTextStyle", typeof(Style), typeof(XTextBlock), new FrameworkPropertyMetadata(null));
    public static Style GetSplitTextStyle(TextBlock i) => (Style)i.GetValue(SplitTextStyleProperty);
    public static void SetSplitTextStyle(TextBlock i, Style value) => i.SetValue(SplitTextStyleProperty, value);

    #endregion

    #region SplitTextWord

    public static readonly DependencyProperty SplitTextWordProperty = DependencyProperty.RegisterAttached("SplitTextWord", typeof(SearchWord), typeof(XTextBlock), new FrameworkPropertyMetadata(SearchWord.Exact, OnSplitTextChanged));
    public static SearchWord GetSplitTextWord(TextBlock i) => (SearchWord)i.GetValue(SplitTextWordProperty);
    public static void SetSplitTextWord(TextBlock i, SearchWord value) => i.SetValue(SplitTextWordProperty, value);

    #endregion

    #region Update

    private class TimerWrapper
    {
        private TextBlock Control;
        private System.Windows.Threading.DispatcherTimer Timer = new();

        public TimerWrapper(TextBlock control)
        {
            Control = control;

            Timer.Interval = GetUpdateInterval(control);
            Timer.Tick += OnUpdate;
            Timer.Start();
        }

        public void Unload()
        {
            Timer.Stop();
            Timer.Tick -= OnUpdate;
            Timer = null;

            Control = null;
        }

        private void OnUpdate(object sender, EventArgs e)
        {
            var binding = System.Windows.Data.BindingOperations.GetBindingExpression(Control, TextBlock.TextProperty);
            binding?.UpdateTarget();
        }
    }

    public static readonly DependencyProperty UpdateProperty = DependencyProperty.RegisterAttached("Update", typeof(bool), typeof(XTextBlock), new FrameworkPropertyMetadata(false, OnUpdateChanged));
    public static bool GetUpdate(TextBlock i) => (bool)i.GetValue(UpdateProperty);
    public static void SetUpdate(TextBlock i, bool input) => i.SetValue(UpdateProperty, input);

    private static void OnUpdateChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        var textBlock = sender as TextBlock;
        textBlock.AddHandlerAttached((bool)e.NewValue, UpdateProperty, i =>
        {
            var result = new TimerWrapper(i);
            SetUpdateTimer(i, result);
        }, i =>
        {
            GetUpdateTimer(i)?.Unload();
            SetUpdateTimer(i, null);
        });
    }

    #endregion

    #region UpdateInterval

    public static readonly DependencyProperty UpdateIntervalProperty = DependencyProperty.RegisterAttached("UpdateInterval", typeof(TimeSpan), typeof(XTextBlock), new FrameworkPropertyMetadata(1.Seconds()));
    public static TimeSpan GetUpdateInterval(TextBlock i) => (TimeSpan)i.GetValue(UpdateIntervalProperty);
    public static void SetUpdateInterval(TextBlock i, TimeSpan input) => i.SetValue(UpdateIntervalProperty, input);

    #endregion

    #region (private) UpdateTimer

    private static readonly DependencyProperty UpdateTimerProperty = DependencyProperty.RegisterAttached("UpdateTimer", typeof(TimerWrapper), typeof(XTextBlock), new FrameworkPropertyMetadata(null));

    private static TimerWrapper GetUpdateTimer(TextBlock i) => (TimerWrapper)i.GetValue(UpdateTimerProperty);
    private static void SetUpdateTimer(TextBlock i, TimerWrapper input) => i.SetValue(UpdateTimerProperty, input);

    #endregion
}