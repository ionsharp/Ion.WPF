using Ion.Analysis;
using Ion.Collect;
using Ion.Core;

namespace Ion.Reflect;

public class InstanceStyle() : ObjectDictionary<StyleAttribute, string>()
{
    public Error Error { get => this.Get<Error>(); set => this.Set(value); }
}