using System;

namespace Ion;

/// <inheritdoc/>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public abstract class TriggerAttribute : Attribute
{
    protected TriggerAttribute() : base() { }
}

/// <inheritdoc/>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public abstract class CompareTriggerAttribute : TriggerAttribute
{
    public Comparison Comparison { get; set; }

    public string PropertyName { get; set; }

    public object Value { get; set; }

    protected CompareTriggerAttribute() : base() { }

    protected CompareTriggerAttribute(string name, Comparison comparison = Comparison.Equal, object value = null) : this()
    {
        PropertyName = name; Comparison = comparison; Value = value;
    }
}

/// <inheritdoc/>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public sealed class EnableTriggerAttribute(string Name, Comparison comparison = Comparison.Equal, object Value = null) : CompareTriggerAttribute(Name, comparison, Value) { }

/// <inheritdoc/>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public sealed class VisibilityTriggerAttribute : CompareTriggerAttribute
{
    public VisibilityTriggerAttribute(string Name, Comparison comparison, object Value) : base(Name, comparison, Value) { }

    public VisibilityTriggerAttribute(string Name, object Value) : base(Name, Comparison.Equal, Value) { }
}

/// <inheritdoc/>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public sealed class StyleTriggerAttribute(string stylePropertyName, string propertyName) : TriggerAttribute()
{
    /// <summary>The property name of the object to get a value from.</summary>
    public string PropertyName { get; set; } = propertyName;
    /// <summary>The property name of the style to set.</summary>
    public string StylePropertyName { get; set; } = stylePropertyName;
}