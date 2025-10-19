using Ion.Core;
using Ion.Reflect;
using System;
using System.Runtime.Serialization;

namespace Ion.Data;

public abstract record class BinaryValue<NonSerializable, Serializable> : Model
{
    private Serializable serializedValue;

    public NonSerializable Value { get => Get<NonSerializable>(default, false); set => Set(value, false); }

    protected BinaryValue() : this(default(NonSerializable)) { }

    protected BinaryValue(NonSerializable value) : base() => Value = value;

    protected abstract NonSerializable ConvertBack(Serializable input);

    protected abstract Serializable ConvertTo(NonSerializable input);

    [OnSerializing]
    public void OnSerializing(StreamingContext context)
    {
        var result = ConvertTo(Value);
        serializedValue = result is Serializable i ? i : default;
    }

    [OnDeserialized]
    public void OnDeserialized(StreamingContext context)
    {
        var result = ConvertBack(serializedValue);
        Value = result is NonSerializable i ? i : default;
    }
}

public record class BinaryValue<NonSerializable, Serializable, Converter>(NonSerializable value) : BinaryValue<NonSerializable, Serializable>(value) where Converter : Data.ValueConverter<NonSerializable, Serializable>
{
    [NonSerialized]
    private Converter converter = default;

    public BinaryValue() : this(default(NonSerializable)) { }

    protected override NonSerializable ConvertBack(Serializable input)
    {
        converter ??= typeof(Converter).Create<Converter>();
        return converter.ConvertBack(input) is NonSerializable result ? result : default;
    }

    protected override Serializable ConvertTo(NonSerializable input)
    {
        converter ??= typeof(Converter).Create<Converter>();
        return converter.Convert(input) is Serializable result ? result : default;
    }
}