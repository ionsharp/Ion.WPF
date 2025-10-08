using Ion.Data;
using Ion.Reflect;
using System.Windows.Data;

namespace Ion.Controls;

public class ObjectGroupConverterSelector : ConverterSelector
{
    public static readonly ObjectGroupConverterSelector Default = new();

    public override IValueConverter SelectConverter(object input)
    {
        return $"{input}" switch
        {
            nameof(MemberGroupName.Group)
                => new ValueConverter<Member, string>(i => i.Value.Group?.ToString()),
            nameof(MemberGroupName.DeclarationType)
                => new ValueConverter<Member, string>(i => i.Value.DeclarationType?.Name),
            nameof(MemberGroupName.DeclaringType)
                => new ValueConverter<Member, string>(i => i.Value is IMember j ? j.DeclaringType?.Name : ""),
            nameof(MemberGroupName.MemberType)
                => new ValueConverter<Member, string>(i => i.Value.IfNotNullGet<IMember, string>(j => j.Info?.MemberType.ToString())),
            nameof(MemberGroupName.Name)
                => new ValueConverter<Member, string>(i => ValueConverter.Cache.Get<ConvertToStringWithFirstLetter>().Convert(i.Value.Style[j => j.Name], null, null, null)?.ToString()),
            nameof(MemberGroupName.ValueType)
                => new ValueConverter<Member, string>(i => i.Value.ValueType?.Name),
            _ => default,
        };
    }
}