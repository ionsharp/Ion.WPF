using System.Windows;

namespace Ion.Core;

public interface IApp
{
    IAppModel Model { get; }

    ResourceDictionary Resources { get; }
}