namespace Ion.Core;

public interface IFileDockAppModel : IDockAppModel
{
    new IFileDockViewModel ViewModel { get; }
}