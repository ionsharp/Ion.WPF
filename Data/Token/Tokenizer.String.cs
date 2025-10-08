using System;
using System.Collections.Generic;

namespace Ion.Data;

/// <summary>
/// Tokenizes a <see cref="string"/> into multiple <see cref="string"/>s.
/// </summary>
public sealed class StringTokenizer : Tokenizer<string>
{
    public override IEnumerable<string> Tokenize(string input, char delimiter)
    {
        var result = input.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
        foreach (var i in result)
            yield return i;
    }

    public override string ToToken(string input)
    {
        var result = input.Trim();
        return !result.IsEmpty() ? result : null;
    }

    public override string ToString(string input) => input;

    public StringTokenizer() : base(null) { }
}