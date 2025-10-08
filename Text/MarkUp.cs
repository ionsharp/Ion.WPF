using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Ion.Text;

/// <summary>A parser that parses mark up text.</summary>
/// <remarks>
/// <para><see cref="FlowDocument"/></para>
/// <para>[b, code, i, p, s, u]</para>
/// <para><see cref="TextBlock"/></para>
/// <para>[b, code, i, s, u]</para>
/// </remarks>
public class MarkUpParser() : Parser()
{
    /// <see cref="Region.Field"/>

    public const string TagBold = "b";

    public const string TagCode = "code";

    public const string TagItalic = "i";

    public const string TagParagraph = "p";

    public const string TagStrikethrough = "s";

    public const string TagUnderline = "u";

    /// <see cref="Region.Property"/>

    private static Dictionary<string, bool> DefaultTags => new()
    {
        { TagBold,
            false },
        { TagCode,
            false },
        { TagItalic,
            false },
        { TagStrikethrough,
            false },
        { TagUnderline,
            false },
    };

    /// <see cref="Region.Method"/>

    public override void Parse(object document, object text)
    {
        if (document is FlowDocument a)
            Parse(a, text);

        else if (document is TextBlock b)
            Parse(b, text);
    }

    private static void Add(Dictionary<string, bool> tags, string text, TextBlock textBlock)
    {
        var run = new Run(text);
        if (tags[TagBold])
            run.FontWeight = FontWeights.Bold;

        if (tags[TagCode])
        {
            run.Background = System.Windows.Media.Brushes.LightGray;
            run.FontFamily = new System.Windows.Media.FontFamily("Consolas");
        }

        if (tags[TagItalic])
            run.FontStyle = FontStyles.Italic;

        if (tags[TagStrikethrough])
            run.TextDecorations.Add(TextDecorations.Strikethrough);

        if (tags[TagUnderline])
            run.TextDecorations.Add(TextDecorations.Underline);

        textBlock.Inlines.Add(run);
    }

    ///

    private static void Add(FlowDocument document, string text)
    {
        if (text?.Length > 0)
        {
            var block = new TextBlock();
            var result = "";

            var tags = DefaultTags;
            for (var i = 0; i < text.Length; i++)
            {
                bool handle = false;
                foreach (var j in tags)
                {
                    //< * >
                    if (i < text.Length - (j.Key.Length + 2) && text.Substring(i, j.Key.Length + 2).ToLower() == "<" + j.Key + ">")
                    {
                        if (result?.Length > 0)
                            Add(tags, result, block);

                        tags[j.Key] = true;
                        result = "";

                        i += j.Key.Length + 1;
                        handle = true;
                        break;
                    }
                    //</ * >
                    else if (i < text.Length - (j.Key.Length + 3) && text.Substring(i, j.Key.Length + 3).ToLower() == "</" + j.Key + ">")
                    {
                        Add(tags, result, block);

                        tags[j.Key] = false;
                        result = "";

                        i += j.Key.Length + 2;
                        handle = true;
                        break;
                    }
                }

                if (!handle)
                    result += text[i];

                /*
                if (i < input.Length - 3 && input.Substring(i, 3) == "<b>")
                {
                    if (result?.Length > 0)
                    {
                        var run = new Run(result);
                        if (bold)
                            run.FontWeight = FontWeights.Bold;

                        if (code)
                            run.Background = System.Windows.Media.Brushes.LightGray;

                        if (italic)
                            run.FontStyle = FontStyles.Italic;

                        block.Inlines.Add(run);
                    }

                    bold = true;
                    result = "";
                    i += 2;
                }
                else if (i < input.Length - 6 && input.Substring(i, 6) == "<code>")
                {
                    if (result?.Length > 0)
                    {
                        var run = new Run(result);
                        if (bold)
                            run.FontWeight = FontWeights.Bold;

                        if (code)
                            run.Background = System.Windows.Media.Brushes.LightGray;

                        if (italic)
                            run.FontStyle = FontStyles.Italic;

                        block.Inlines.Add(run);
                    }

                    code = true;
                    result = "";
                    i += 5;
                }
                else if (i < input.Length - 3 && input.Substring(i, 3) == "<i>")
                {
                    if (result?.Length > 0)
                    {
                        var run = new Run(result);
                        if (bold)
                            run.FontWeight = FontWeights.Bold;

                        if (code)
                            run.Background = System.Windows.Media.Brushes.LightGray;

                        if (italic)
                            run.FontStyle = FontStyles.Italic;

                        block.Inlines.Add(run);
                    }

                    italic = true;
                    result = "";
                    i += 2;
                }

                else if (i < input.Length - 4 && input.Substring(i, 4) == "</b>")
                {
                    bold = false;

                    var run = new Run(result) { FontWeight = FontWeights.Bold };
                    if (code)
                        run.Background = System.Windows.Media.Brushes.LightGray;

                    if (italic)
                        run.FontStyle = FontStyles.Italic;

                    block.Inlines.Add(run);
                    result = "";

                    i += 3;
                }
                else if (i < input.Length - 7 && input.Substring(i, 7) == "</code>")
                {
                    code = false;

                    var run = new Run(result) { Background = System.Windows.Media.Brushes.LightGray };
                    if (bold)
                        run.FontWeight = FontWeights.Bold;

                    if (italic)
                        run.FontStyle = FontStyles.Italic;

                    block.Inlines.Add(run);
                    result = "";

                    i += 6;
                }
                else if (i < input.Length - 4 && input.Substring(i, 4) == "</i>")
                {
                    italic = false;

                    var run = new Run(result) { FontStyle = FontStyles.Italic };
                    if (bold)
                        run.FontWeight = FontWeights.Bold;

                    if (code)
                        run.Background = System.Windows.Media.Brushes.LightGray;

                    block.Inlines.Add(run);
                    result = "";

                    i += 3;
                }
                else
                {
                    result += input[i];
                }
                */
            }

            if (result.Length > 0)
                block.Inlines.Add(new Run(result));

            var paragraph = new Paragraph();
            paragraph.Inlines.Add(block);
            document.Blocks.Add(paragraph);
        }
    }

    private static void Clear(FlowDocument i) => i.Blocks.Clear();

    public void Parse(FlowDocument document, object text)
    {
        Clear(document);

        var input = text?.ToString();
        if (input.IsWhite())
            return;

        string x = "<p>", y = "</p>";

        var result = "";
        for (var i = 0; i < input.Length; i++)
        {
            if (i < input.Length - x.Length && input.Substring(i, x.Length) == x)
            {
                Add(document, result);
                result = "";

                i += x.Length - 1;
            }
            else if (i < input.Length - y.Length && input.Substring(i, y.Length) == y)
            {
                Add(document, result);
                result = "";

                i += y.Length - 1;
            }
            else result += input[i];
        }

        Add(document, result);
    }

    ///

    private static void Clear(TextBlock i) => i.Inlines.Clear();

    public void Parse(TextBlock textBlock, object text)
    {
        Clear(textBlock);

        var input = text?.ToString();
        if (input.IsWhite())
            return;

        var result = "";

        var tags = DefaultTags;
        for (var i = 0; i < input.Length; i++)
        {
            bool handle = false;
            foreach (var j in tags)
            {
                //< * >
                if (i < input.Length - (j.Key.Length + 2) && input.Substring(i, j.Key.Length + 2).ToLower() == "<" + j.Key + ">")
                {
                    if (result?.Length > 0)
                        Add(tags, result, textBlock);

                    tags[j.Key] = true;
                    result = "";

                    i += j.Key.Length + 1;
                    handle = true;
                    break;
                }
                //</ * >
                else if (i < input.Length - (j.Key.Length + 3) && input.Substring(i, j.Key.Length + 3).ToLower() == "</" + j.Key + ">")
                {
                    Add(tags, result, textBlock);

                    tags[j.Key] = false;
                    result = "";

                    i += j.Key.Length + 2;
                    handle = true;
                    break;
                }
            }
            if (!handle)
                result += input[i];
        }

        if (result.Length > 0)
            textBlock.Inlines.Add(new Run(result));
    }
}