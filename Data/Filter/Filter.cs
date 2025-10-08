using System;

namespace Ion;

/// <inheritdoc/>
[AttributeUsage(AttributeTargets.Property)]
public sealed class FilterAttribute(Filter filter) : Attribute()
{
    public readonly Filter Filter = filter;
}