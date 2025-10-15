namespace Ion.Core;

public interface IAppModelDock : IAppModel
{
    DocumentCollection Documents { get; }

    PanelCollection Panels { get; }

    new IDockViewModel ViewModel { get; }
}