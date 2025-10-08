using System;

namespace Ion;

/// <inheritdoc/>
[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
public abstract class TemplateTypeAttribute(Type Type) : Attribute()
{
    public Type Type { get; } = Type;
}

/// <inheritdoc/>
[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
public sealed class TemplateTypeAttribute<T>() : TemplateTypeAttribute(typeof(T)) { }