using System;

namespace Ion.Data;

/// <inheritdoc/>
public abstract class MultiBindResult : MultiBind
{
    public enum Results { Boolean, Visibility }

    private bool invert;
    public virtual bool Invert
    {
        get => invert;
        set
        {
            invert = value;
            ConverterParameter = Tuple.Create(value, Result);
        }
    }

    private Results result = Results.Boolean;
    public virtual Results Result
    {
        get => result;
        set
        {
            result = value;
            ConverterParameter = Tuple.Create(Invert, value);
        }
    }

    protected MultiBindResult() : base() => ConverterParameter = Tuple.Create(Invert, Result);
}