namespace Ion.Controls;

/// <summary>
/// An <see cref="UpDown"/> with multiple parts.
/// </summary>
/// <typeparam name="Type"></typeparam>
/// <typeparam name="Part"></typeparam>
public abstract class MultiUpDown<Type, Part> : UpDown<Type>
{
    protected MultiUpDown() : base() { }
}