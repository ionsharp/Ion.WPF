using System.Windows.Data;

namespace Ion.Data;

/// <see cref="MultiBindAttribute{T}"/>
#region

/// <inheritdoc/>
public abstract class MultiBindAttribute<T> : MultiBindLanguage where T : IValueConverter
{
    protected MultiBindAttribute() : this(Paths.Dot) { }

    protected MultiBindAttribute(string path) : base(path)
        => ConverterType = typeof(MultiValueConverterBox<T>);
}

#endregion

/// <see cref="MultiBindDescription"/>
#region

/// <inheritdoc/>
public class MultiBindDescription(string path) : MultiBindAttribute<ConvertAttributeDescription>(path)
{
    public MultiBindDescription() : this(Paths.Dot) { }
}

#endregion

/// <see cref="MultiBindName"/>
#region

/// <inheritdoc/>
public class MultiBindName(string path) : MultiBindAttribute<ConvertAttributeName>(path)
{
    public MultiBindName() : this(Paths.Dot) { }
}

#endregion