using System;
using System.Collections.Generic;

namespace Ion.Data;

public class BooleanTokenizer : Tokenizer<bool>
{
    public override IEnumerable<bool> Tokenize(string input, char delimiter)
    {
        var result = input.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
        foreach (var i in result)
            yield return i == "1";
    }

    public override bool ToToken(string input) => input == "1";

    public override string ToString(bool input) => input ? "1" : "0";

    public BooleanTokenizer() : base(null) { }
}