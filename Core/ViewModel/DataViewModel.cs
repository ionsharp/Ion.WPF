using Ion.Reflect;
using Ion.Storage;
using System;

namespace Ion.Core;

/// <see cref="DataViewModel"/>
#region

/// <summary>A model of a view with <see cref="IViewModelData">data</see>.</summary>
public abstract record class DataViewModel() : ViewModel(), IDataViewModel
{
    public IViewModelData
        Data
    { get => Get<IViewModelData>(); protected set => Set(value); }

    public string
        DataFileExtension
    { get => Get<string>(); set => Set(value); }

    public string
        DataFileName
    { get => Get<string>(); set => Set(value); }

    public string
        DataFolderPath
    { get => Get<string>(); set => Set(value); }

    public string
        DataFilePath => $@"{DataFolderPath}\{DataFileName}.{DataFileExtension}";

    protected virtual Type
        DataType
    { get; }

    public virtual void Load()
    {
        BinarySerializer.Deserialize(DataFilePath, out IViewModelData oldData);
        Data = oldData ?? DataType?.Create<IViewModelData>();
    }
}

#endregion

/// <inheritdoc/>
public abstract record class DataViewModel<Data>() : DataViewModel() where Data : IViewModelData
{
    protected override sealed Type DataType => typeof(Data);
}