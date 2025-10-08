using Ion;
using Ion.Analysis;
using Ion.Controls;
using Ion.Input;
using Ion.Reflect;
using Ion.Storage;
using Ion.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ion.Core;

/// <inheritdoc/>
public abstract record class FileDockViewModel<T>() : DockViewModel<T>(), IFileDockViewModel where T : FileDockViewModelData
{
    /// <see cref="Region.Field"/>

    public const string OpenTitle = "Open";

    /// <see cref="Region.Property"/>

    public abstract IEnumerable<string> ReadableFileExtensions { get; }

    /// <see cref="Region.Method"/>
    #region

    protected abstract SerializationType GetSerializationType(string fileExtension);

    protected abstract Type GetDocumentType(string fileExtension);

    ///

    protected virtual Type GetSerializerType() => default;

    protected virtual void OnOpened(Document document)
    {
        Documents.Add(document);
    }

    protected virtual void OnOpenFailed(string filePath, Error e)
    {
        Log.Write(e);
        //Dialog.ShowError(nameof(Open), e, Buttons.Ok);
    }

    ///

    async public Task Open()
    {
        if (StorageDialog.Show(out string[] paths, $"{OpenTitle}...", StorageDialogMode.OpenFile, ReadableFileExtensions, ActiveDocument?.As<FileDocument>()?.Path))
            await Open(paths);
    }

    async public Task Open(IList<string> filePaths)
    {
        if (filePaths?.Count > 0)
        {
            foreach (var i in filePaths)
                await Open(i);
        }
    }

    [Obsolete]
    async public Task Open(string filePath)
    {
        var fileExtension = System.IO.Path.GetExtension(filePath)[1..];
        var fileText = "";

        //If the file is already open, activate it
        if (Documents.FirstOrDefault(i => i.As<FileDocument>().Path?.ToLower() == filePath.ToLower()) is Document existingDocument)
        {
            ActiveContent = existingDocument;
            return;
        }

        FileDocument document = null;
        if (!ReadableFileExtensions.Contains(fileExtension))
        {
            OnOpenFailed(filePath, new FileNotSupported(filePath));
            return;
        }

        var documentType = GetDocumentType(fileExtension);

        var serializationType = GetSerializationType(fileExtension);
        switch (serializationType)
        {
            case SerializationType.Binary:
                BinarySerializer.Deserialize(filePath, out document);
                break;

            case SerializationType.Image:
                document = documentType.Create<FileDocument>();

                if (document is ImageFileDocument imageFile)
                {
                    System.Drawing.Bitmap bitmap = null;
                    //Try.Do(() => bitmap = new MagickImage(filePath).ToBitmap(), e => Log.Add(e));

                    imageFile.Source = bitmap;
                }
                break;

            case SerializationType.Text:
                document = documentType.Create<FileDocument>();
                if (document is TextFileDocument textFile)
                {
                    fileText = Try.Get(() => File.ReadAllText(filePath, Data.Encoding.New()), e => Log.Write(e)) ?? string.Empty;
                    textFile.Set(fileText, true, true, nameof(TextFileDocument.Text));
                }
                break;
        }

        if (document is null)
        {
            OnOpenFailed(filePath, new FileNotValid(filePath));
            return;
        }
        else
        {
            document.Path = filePath;
            OnOpened(document);

            Data.RecentFiles.Insert(0, document.Path);
        }
    }

    #endregion

    /// <see cref="ICommand"/>
    #region

    public ICommand DeleteFileCommand => Commands[nameof(DeleteFileCommand)] ??= new RelayCommand<string>(i =>
    {
        Dialog.ShowResult("Delete", new Warning($"Are you sure you want to delete '{i}'?"), j =>
        {
            if (j == 0)
                Try.Do(() => XItemPath.Recycle(i, Microsoft.VisualBasic.FileIO.RecycleOption.DeletePermanently));
        },
        Buttons.YesNo);
    }, File.Exists);

    public ICommand RecycleFileCommand => Commands[nameof(RecycleFileCommand)] ??= new RelayCommand<string>(i =>
    {
        Dialog.ShowResult("Recycle", new Warning($"Are you sure you want to recycle '{i}'?"), j =>
        {
            if (j == 0)
                Try.Do(() => XItemPath.Recycle(i, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin));
        },
        Buttons.YesNo);
    }, File.Exists);

    #endregion
}