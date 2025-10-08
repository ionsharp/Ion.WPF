namespace Ion.Core;

public interface IDockAppModel : IAppModel
{
    DocumentCollection Documents { get; }

    PanelCollection Panels { get; }

    new IDockViewModel ViewModel { get; }
}