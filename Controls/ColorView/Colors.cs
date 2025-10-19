using Ion;
using Ion.Colors;
using Ion.Core;
using Ion.Numeral;
using System;
using System.Collections.Generic;

namespace Ion.Media;

public class ColorGroupCollection : ItemGroup<ByteVector4>
{
    public ColorGroupCollection() : base() { }

    public ColorGroupCollection(string name, Type type = null) : base(name) => type.IfNotNull(Add);

    public ColorGroupCollection(string name, IEnumerable<ByteVector4> items) : base(name, items) { }

    public ColorGroupCollection(string name, IEnumerable<Item<ByteVector4>> items) : base(name, items) { }

    protected void Add(Type input)
        => input.GetFields().ForEach(i => Add(i.Name.GetCamel(), new ByteVector4((string)i.GetValue(null))));
}

public sealed class PrimaryColors()
    : ColorGroupCollection("Primary", typeof(Colors1))
{ }

public sealed class SecondaryColors()
    : ColorGroupCollection("Secondary", typeof(Colors2))
{ }

public sealed class TertiaryColors()
    : ColorGroupCollection("Tertiary", typeof(Colors3))
{ }

public sealed class QuaternaryColors()
    : ColorGroupCollection("Quaternary", typeof(Colors4))
{ }

public sealed class QuinaryColors()
    : ColorGroupCollection("Quinary", typeof(Colors5))
{ }