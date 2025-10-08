using System.Windows.Input;

namespace Ion.Input;

/// <summary>
/// An <see cref="ICommand"/> that relays functionality to other objects by invoking delegates.
/// </summary>
public interface IRelayCommand : ICommand;