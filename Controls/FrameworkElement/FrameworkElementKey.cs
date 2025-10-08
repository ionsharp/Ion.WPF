using System.Windows;

namespace Ion.Controls;

public interface IFrameworkElementKey { }

public abstract class FrameworkElementKey : IFrameworkElementKey { }

public class ReferenceKey<T> : FrameworkElementKey where T : FrameworkElement
{
    public ReferenceKey() : base() { }
}

public class ResourceKey : FrameworkElementKey { }