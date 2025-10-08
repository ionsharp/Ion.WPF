using System.Collections.Generic;
using System.Linq;

namespace Ion.Data;

public abstract class Tokenizer<Token>(object input = null) : ITokenize, ITokenize<Token>
{
    private readonly object source = input;
    public object Source => source;
    object ITokenize.Source => source;

    public abstract IEnumerable<Token> Tokenize(string input, char delimiter);
    IEnumerable<object> ITokenize.Tokenize(string input, char delimiter) => Tokenize(input, delimiter).Cast<object>();

    public abstract Token ToToken(string input);
    object ITokenize.ToToken(string input) => ToToken(input);

    public abstract string ToString(Token input);
    string ITokenize.ToString(object input) => ToString((Token)input);
}
