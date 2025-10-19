using Ion.Core;
using Ion.Reflect;
using System;

namespace Ion.Storage;

[Name("Extensions")]
[Styles.Object(GroupName = MemberGroupName.None)]
public record class FilterExtensions : Model
{
    public Clude Filter { get => Get(Clude.Include); set => Set(value); }

    [Name("Extensions")]
    [Styles.Text(Template.Token)]
    public string Value { get => Get(""); set => Set(value); }

    public FilterExtensions() : base() { }

    public override void OnSettingProperty(PropertySettingEventArgs e)
    {
        base.OnSettingProperty(e);
        if (e.PropertyName == nameof(Value))
            e.NewValue = e.NewValue?.ToString().Replace(".", string.Empty);
    }

    public override string ToString(string format, IFormatProvider provider)
        => Value.IsEmpty() ? "All" : $"{Filter} .{Value.Replace(";", ", .").TrimEnd('.').TrimEnd(' ').TrimEnd(',')}";
}