namespace Ion.Data;

public sealed class ValueConverterOutput<T>
{
    public readonly object ActualValue;

    public ValueConverterOutput(T input) => ActualValue = input;

    public ValueConverterOutput(Nothing input) => ActualValue = input;

    public static implicit operator ValueConverterOutput<T>(T input) => new(input);

    public static implicit operator ValueConverterOutput<T>(Nothing input) => new(input);
}