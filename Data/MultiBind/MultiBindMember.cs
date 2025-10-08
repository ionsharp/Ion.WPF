using Ion.Imaging;
using Ion.Local;
using Ion.Reflect;
using Ion.Text;
using System;
using System.Windows.Data;

namespace Ion.Data;

/// <see cref="MultiBindMemberDescription"/>
#region

public class MultiBindMemberDescription : MultiBindLanguage
{
    public MultiBindMemberDescription() : this(Paths.Dot) { }

    public MultiBindMemberDescription(string path) : base(path)
    {
        ConverterType = typeof(MultiValueConverterBox<ConvertAttributeDescription>);
        Bindings.Add(new Binding(nameof(Member.Style) + "[" + nameof(StyleAttribute.Description) + "]"));
        Bindings.Add(new Binding(nameof(Member.Style) + "[" + nameof(StyleAttribute.DescriptionLocalize) + "]"));
        Bindings.Add(new Binding(nameof(Member.Value)));
    }
}

#endregion

/// <see cref="MultiBindMemberImage"/>
#region

public class MultiBindMemberImage : MultiBind
{
    public enum Types { Image, String }

    public Types Type { get; set; } = Types.String;

    public MultiBindMemberImage() : this(Paths.Dot) { }

    public MultiBindMemberImage(string path) : base(path)
    {
        Converter = new MultiValueConverter<object>(i =>
        {
            if (i.Values?.Length >= 1)
            {
                if (i.Values[0] is Member j)
                {
                    var result = ValueConverter.Cache.Get<ConvertAttributeImage>().Convert(j, ConverterParameter);

                    if (Type == Types.Image)
                        return XImageSource.Convert(Try.Get(() => new Uri((string)result, UriKind.Absolute)));

                    return result;
                }
            }
            return null;
        });
        Bindings.Add(new Binding($"{nameof(Member.Style)}[{nameof(StyleAttribute.Image)}]"));
        Bindings.Add(new Binding(nameof(Member.Value)));
    }
}

#endregion

/// <see cref="MultiBindMemberName"/>
#region

public class MultiBindMemberName : MultiBindLanguage
{
    public MultiBindMemberName() : this(Paths.Dot) { }

    public MultiBindMemberName(string path) : base(path)
    {
        ConverterType = typeof(MultiValueConverterBox<ConvertAttributeName>);
        Bindings.Add(new Binding(nameof(Member.Style) + "[" + nameof(StyleAttribute.Name) + "]"));
        Bindings.Add(new Binding(nameof(Member.Style) + "[" + nameof(StyleAttribute.NameLocalize) + "]"));
        Bindings.Add(new Binding(nameof(Member.Value)));
    }
}

#endregion

/// <see cref="MultiBindMemberPlaceholder"/>
#region

public class MultiBindMemberPlaceholder : MultiBindLanguage
{
    public MultiBindMemberPlaceholder() : base(Paths.Dot)
    {
        Converter = new MultiValueConverter<FormatText>(i =>
        {
            if (i.Values?.Length >= 1)
            {
                if (i.Values[0] is Member member)
                {
                    string result = $"{member.Style[i => i.Placeholder]
                        ?? member.Style[i => i.Name]
                        ?? member.Name}";

                    result = member.Style.GetValue(i => i.PlaceholderLocalize) ? result.Localize() : result;
                    return new FormatText(Format.Default, result);
                }
            }
            return null;
        });
        Bindings.Add(new Binding(nameof(Member.Style) + "[" + nameof(StyleAttribute.Name) + "]"));
        Bindings.Add(new Binding(nameof(Member.Style) + "[" + nameof(StyleAttribute.NameLocalize) + "]"));
        Bindings.Add(new Binding(nameof(Member.Style) + "[" + nameof(StyleAttribute.Placeholder) + "]"));
        Bindings.Add(new Binding(nameof(Member.Style) + "[" + nameof(StyleAttribute.PlaceholderFormat) + "]"));
        Bindings.Add(new Binding(nameof(Member.Style) + "[" + nameof(StyleAttribute.PlaceholderLocalize) + "]"));
    }
}

#endregion