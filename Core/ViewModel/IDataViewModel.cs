namespace Ion.Core;

public interface IDataViewModel : IViewModel
{
    IViewModelData Data { get; }

    string DataFileExtension { get; set; }

    string DataFileName { get; set; }

    string DataFolderPath { get; set; }

    void Load();
}