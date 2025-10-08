using Ion;
using Ion.Collect;
using Ion.Data;
using Ion.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;

namespace Ion.Controls;

[ContentProperty(nameof(Source))]
public class TokenBox : RichTextBox
{
    /// <see cref="Region.Property"/>
    #region

    private readonly Handle handle = false;

    ///

    private BlockCollection Blocks => Document.Blocks;

    private Run CurrentRun
    {
        get
        {
            var Paragraph = CaretPosition.Paragraph;
            return Paragraph.Inlines.FirstOrDefault(Inline =>
            {
                var Run = Inline.As<Run>();
                var Text = CurrentText;

                if (Run != null && (Run.Text.StartsWith(Text) || Run.Text.EndsWith(Text)))
                    return true;

                return false;
            }) as Run;
        }
    }

    /// <summary>
    /// Gets the current input text.
    /// </summary>
    private string CurrentText => CaretPosition?.GetTextInRun(LogicalDirection.Backward);

    ///

    public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(TokenBox), new FrameworkPropertyMetadata(string.Empty));
    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(string), typeof(TokenBox), new FrameworkPropertyMetadata(string.Empty, OnSourceChanged));
    public string Source
    {
        get => (string)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    private static void OnSourceChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<TokenBox>().OnSourceChanged(e.Convert<string>());

    public static readonly DependencyProperty TokenDelimiterProperty = DependencyProperty.Register(nameof(TokenDelimiter), typeof(char), typeof(TokenBox), new FrameworkPropertyMetadata(';', OnTokenDelimiterChanged));
    /// <summary>
    /// The <see cref="char"/> used to delimit tokens.
    /// </summary>
    public char TokenDelimiter
    {
        get => (char)GetValue(TokenDelimiterProperty);
        set => SetValue(TokenDelimiterProperty, value);
    }

    private static void OnTokenDelimiterChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<TokenBox>().OnTokenDelimiterChanged(e.Convert<char>());

    public static readonly DependencyProperty TokenizerProperty = DependencyProperty.Register(nameof(Tokenizer), typeof(ITokenize), typeof(TokenBox), new FrameworkPropertyMetadata(default(ITokenize)));
    /// <summary>
    /// The <see cref="ITokenize"/> that handles tokenizing.
    /// </summary>
    public ITokenize Tokenizer
    {
        get => (ITokenize)GetValue(TokenizerProperty);
        set => SetValue(TokenizerProperty, value);
    }

    public static readonly DependencyProperty TokenTriggersProperty = DependencyProperty.Register(nameof(TokenTriggers), typeof(TokenBoxTrigger), typeof(TokenBox), new FrameworkPropertyMetadata(TokenBoxTrigger.Return | TokenBoxTrigger.Tab));
    /// <summary>
    /// Keys used to generate tokens when pressed.
    /// </summary>
    public TokenBoxTrigger TokenTriggers
    {
        get => (TokenBoxTrigger)GetValue(TokenTriggersProperty);
        set => SetValue(TokenTriggersProperty, value);
    }

    public static readonly DependencyProperty TokenMouseDownActionProperty = DependencyProperty.Register(nameof(TokenMouseDownAction), typeof(TokenBoxAction), typeof(TokenBox), new FrameworkPropertyMetadata(TokenBoxAction.Edit));
    /// <summary>
    /// Gets or sets the action to perform when the mouse is down on token.
    /// </summary>
    public TokenBoxAction TokenMouseDownAction
    {
        get => (TokenBoxAction)GetValue(TokenMouseDownActionProperty);
        set => SetValue(TokenMouseDownActionProperty, value);
    }

    private static readonly DependencyPropertyKey TokensKey = DependencyProperty.RegisterReadOnly(nameof(Tokens), typeof(ListObservable), typeof(TokenBox), new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty TokensProperty = TokensKey.DependencyProperty;
    public ListObservable Tokens
    {
        get => (ListObservable)GetValue(TokensProperty);
        private set => SetValue(TokensKey, value);
    }

    public static readonly DependencyProperty TokenStyleProperty = DependencyProperty.Register(nameof(TokenStyle), typeof(Style), typeof(TokenBox), new FrameworkPropertyMetadata(default(Style), OnTokenStyleChanged));
    public Style TokenStyle
    {
        get => (Style)GetValue(TokenStyleProperty);
        set => SetValue(TokenStyleProperty, value);
    }

    private static void OnTokenStyleChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<TokenBox>().OnTokenStyleChanged(e.Convert<Style>());

    #endregion

    /// <see cref="Region.Constructor"/>
    #region

    public TokenBox() : base()
    {
        SetCurrentValue(IsDocumentEnabledProperty,
            true);
        SetCurrentValue(TokenizerProperty,
            new StringTokenizer());

        Tokens = [];
    }

    #endregion

    /// <see cref="Region.Method.Private"/>
    #region

    /// <summary>
    /// For each token, perform action exposing corresponding <see cref="TokenBoxButton"/>.
    /// </summary>
    /// <typeparam name="TButton"></typeparam>
    /// <param name="Action"></param>
    private void Enumerate<TButton>(Func<TButton, bool> Action) where TButton : TokenBoxButton
    {
        Enumerate<InlineUIContainer, TButton>((i, j) => Action(j));
    }

    /// <summary>
    /// For each token, perform action exposing corresponding <see cref="TokenBoxButton"/> and <see cref="Inline"/>.
    /// </summary>
    /// <typeparam name="TInline"></typeparam>
    /// <typeparam name="TButton"></typeparam>
    /// <param name="Action"></param>
    private void Enumerate<TInline, TButton>(Func<TInline, TButton, bool> Action) where TInline : Inline where TButton : TokenBoxButton
    {
        Enumerate<Paragraph, TInline, TButton>((p, i, b) => Action(i, b));
    }

    /// <summary>
    /// For each token, perform action exposing corresponding <see cref="TokenBoxButton"/>, <see cref="Inline"/>, and <see cref="Paragraph"/>.
    /// </summary>
    /// <typeparam name="TParagraph"></typeparam>
    /// <typeparam name="TInline"></typeparam>
    /// <typeparam name="TButton"></typeparam>
    /// <param name="Action"></param>
    private void Enumerate<TParagraph, TInline, TButton>(Func<TParagraph, TInline, TButton, bool> Action) where TParagraph : Paragraph where TInline : Inline where TButton : TokenBoxButton
    {
        foreach (var i in Blocks)
        {
            if (i is TParagraph)
            {
                var Continue = true;
                foreach (var j in i.As<TParagraph>().Inlines)
                {
                    if (j is TInline)
                    {
                        if (!Action(i as TParagraph, j as TInline, j.As<InlineUIContainer>()?.Child?.As<TButton>() ?? default))
                            Continue = false;
                    }
                    if (!Continue)
                        break;
                }
                if (!Continue)
                    break;
            }
        }
    }

    ///

    /// <summary>
    /// Generates an <see cref="Inline"/> element to host the given token.
    /// </summary>
    /// <param name="Token"></param>
    /// <returns></returns>
    private InlineUIContainer GenerateInline(object token)
    {
        OnTokenAdded(token);
        return new InlineUIContainer(new TokenBoxButton(token)) { BaselineAlignment = BaselineAlignment.Center }; //Needed to align with run
    }

    /// <summary>
    /// Generates a <see cref="Run"/> to host the <see cref="string"/> representation of the given token.
    /// </summary>
    /// <param name="Token"></param>
    /// <returns></returns>
    private Run GenerateRun(object token)
    {
        OnTokenRemoved(token);
        return new Run(Tokenizer.ToString(token));
    }

    ///

    private string ParseInlines()
    {
        var result = new StringBuilder();
        Enumerate<Inline, TokenBoxButton>((inline, button) =>
        {
            if (inline is InlineUIContainer)
                result.Append("{0}{1}".F(Tokenizer.ToString(button?.Content), TokenDelimiter));

            else if (inline is Run run)
                result.Append(run.Text);

            return true;
        });
        return result.ToString();
    }

    ///

    /// <summary>
    /// Converts the given <see cref="TokenBoxButton"/> to a <see cref="Run"/>.
    /// </summary>
    /// <param name="Button"></param>
    private void EditToken(TokenBoxButton input)
    {
        var result = default(Inline);
        Enumerate<Paragraph, InlineUIContainer, TokenBoxButton>((paragraph, inline, button) =>
        {
            if (button == input)
            {
                result = GenerateRun(button.Content);

                paragraph.Inlines.InsertBefore(inline, result);
                paragraph.Inlines.Remove(inline);
                return false;
            }
            return true;
        });
        Focus();

        if (result != null)
            Selection.Select(result.ContentStart, result.ContentEnd);
    }

    private void IntersectTokens()
    {
        var tokens = new List<object>();
        Enumerate<TokenBoxButton>(button =>
        {
            tokens.Add(button.Content);
            return true;
        });

        var result = Tokens.Intersect(tokens).ToList();

        Tokens.Clear();
        while (result.Any<object>())
        {
            foreach (var i in result)
            {
                result.Remove(i);
                Tokens.Add(i);
                break;
            }
        }
    }

    /// <summary>
    /// Removes the <see cref="Inline"/> containing the given <see cref="TokenBoxButton"/>.
    /// </summary>
    /// <param name="input"></param>
    private void RemoveToken(TokenBoxButton input)
    {
        Enumerate<Paragraph, InlineUIContainer, TokenBoxButton>((paragraph, inline, button) =>
        {
            if (button == input)
            {
                paragraph.Inlines.Remove(inline);
                return false;
            }
            return true;
        });
    }

    /// <summary>
    /// Replaces the given input text with the given token.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="Token"></param>
    private void ReplaceWithToken(string input, object token)
    {
        var paragraph = CaretPosition.Paragraph;

        var currentRun = CurrentRun;
        if (currentRun != null)
        {
            paragraph.Inlines.InsertBefore(currentRun, GenerateInline(token));
            if (currentRun.Text == input)
                paragraph.Inlines.Remove(currentRun);

            else
            {
                var tail = new Run(currentRun.Text[(currentRun.Text.IndexOf(input) + input.Length)..]);
                paragraph.Inlines.InsertAfter(currentRun, tail);
                paragraph.Inlines.Remove(currentRun);
            }
        }
    }

    #endregion

    /// <see cref="Region.Method.Protected"/>
    #region

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if (e.Key.Character() == TokenDelimiter || $"{e.Key}".TryParse(out TokenBoxTrigger key) && TokenTriggers.HasFlag(key))
        {
            OnTokenTriggered();
            e.Handled = true;
        }
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);
        if (e.Source is TokenBoxButton button)
        {
            switch (TokenMouseDownAction)
            {
                case TokenBoxAction.Edit:
                    EditToken(button);
                    break;
                case TokenBoxAction.Remove:
                    RemoveToken(button);
                    break;
            }
            e.Handled = true;
        }
    }

    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        base.OnTextChanged(e);
        handle.DoInternal(() =>
        {
            IntersectTokens();
            Source = ParseInlines();
        });
    }

    protected virtual void OnTokenDelimiterChanged(ValueChange<char> input) => SetCurrentValue(SourceProperty, Source.Replace(input.OldValue, input.NewValue));

    protected virtual void OnTokenAdded(object token) => Tokens.Add(token);

    protected virtual void OnTokenRemoved(object token) => Tokens.Remove(token);

    protected virtual void OnSourceChanged(ValueChange<string> input)
    {
        handle.DoInternal(() =>
        {
            if (input.NewValue != null)
            {
                //Remove repeating delimiters
                var clean = Regex.Replace(input.NewValue, $"{TokenDelimiter}+", $"{TokenDelimiter}");
                if (clean != input.NewValue)
                    SetCurrentValue(SourceProperty, clean);
            }

            Blocks.Clear();
            Tokens.Clear();

            if (input.NewValue?.ToString().IsEmpty() == false)
            {
                var paragraph = new Paragraph();
                Tokenizer?.Tokenize(input.NewValue, TokenDelimiter)?.ForEach(Token => paragraph.Inlines.Add(GenerateInline(Token)));
                Blocks.Add(paragraph);
            }
        });
    }

    protected virtual void OnTokenStyleChanged(ValueChange<Style> input)
    {
        if (input.OldValue != null)
            Resources.Remove(input.OldValue.TargetType);

        if (input.NewValue != null)
            Resources.Add(input.NewValue.TargetType, input.NewValue);
    }

    protected virtual void OnTokenTriggered()
    {
        handle.DoInternal(() =>
        {
            var currentText = CurrentText;

            //Attempt to get token from current text
            var token = Tokenizer?.ToToken(currentText);

            //If token was created, replace current text with it
            if (token != null)
                ReplaceWithToken(currentText, token);

            SetCurrentValue(SourceProperty, ParseInlines());
        });
    }

    #endregion
}