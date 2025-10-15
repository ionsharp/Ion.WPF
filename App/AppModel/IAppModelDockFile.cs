namespace Ion.Core;

public interface IAppModelDockFile : IAppModelDock
{
    new IFileDockViewModel ViewModel { get; }
}