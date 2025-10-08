using System.Windows.Controls;

namespace Ion.Controls;

public interface IObjectControl : IControl { }

public interface IObjectControl<T> : IObjectControl where T : Control { }