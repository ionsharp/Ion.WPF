using Ion.Analysis;
using Ion.Controls;
using Ion.Data;
using Ion.Input;
using Ion.Local;
using Ion.Numeral;
using Ion.Reflect;
using Ion.Storage;
using Ion.Text;
using Ion.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Ion.Core;

/// <see cref="FileDocument"/>
#region

/// <inheritdoc/>
[Styles.Object(MemberViewType = MemberViewType.Tab)]
public abstract record class FileDocument : Document
{
    private enum Group { Attributes, Extension, Format, Properties, Size, Save }

    [TabView(View = View.Main)]
    private enum Tab
    {
        [TabStyle(Description = "File properties",
            Image = Images.File)]
        File
    }

    /// <see cref="Region.Property.Public"/>
    #region

    [Description("When the file was created.")]
    [Group(Group.Properties)]
    [Styles.Text(Tab = Tab.File,
        CanEdit = false,
        ValueConvert = typeof(ConvertToStringTimeRelative))]
    [XmlIgnore]
    public DateTime Created { get => Get(DateTime.Now, false); private set => Set(value, false); }

    [XmlIgnore]
    public virtual string Extension => System.IO.Path.GetExtension(Path);

    [XmlIgnore]
    public virtual IReadOnlyCollection<string> Extensions => [Extension];

    [Group(Group.Extension)]
    [Name("Extension")]
    [Style(Tab = Tab.File,
        CanEdit = false)]
    [XmlIgnore]
    public string ExtensionDescription => ShellProperties.GetDescription($".{Extension}");

    public override object Icon => Path;

    [field: NonSerialized]
    private bool isHidden = false;
    [Description("If the file is hidden or not.")]
    [Group(Group.Attributes)]
    [Name("Hidden")]
    [Style(Tab = Tab.File,
        CanEdit = false)]
    [XmlIgnore]
    public virtual bool IsHidden
    {
        get => isHidden;
        set
        {
            Try.Do(() =>
            {
                if (value)
                    File.AddAttribute(Path, System.IO.FileAttributes.Hidden);

                else File.RemoveAttribute(Path, System.IO.FileAttributes.Hidden);
                isHidden = value;
            },
            e => Log.Write(e));
            Reset(() => IsHidden);
        }
    }

    [field: NonSerialized]
    private bool isReadOnly = false;
    [Description("If the file is read only or not.")]
    [Group(Group.Attributes)]
    [Name("ReadOnly")]
    [Style(Tab = Tab.File,
        CanEdit = false)]
    [XmlIgnore]
    public virtual bool IsReadOnly
    {
        get => isReadOnly;
        set
        {
            Try.Do(() =>
            {
                if (value)
                    File.AddAttribute(Path, System.IO.FileAttributes.ReadOnly);

                else File.RemoveAttribute(Path, System.IO.FileAttributes.ReadOnly);
                isReadOnly = value;
            },
            e => Log.Write(e));
            Reset(() => IsReadOnly);
        }
    }

    [Description("When the file was last accessed.")]
    [Group(Group.Properties)]
    [Name("Accessed")]
    [Styles.Text(Tab = Tab.File,
        CanEdit = false,
        ValueConvert = typeof(ConvertToStringTimeRelative))]
    [XmlIgnore]
    public DateTime LastAccessed { get => Get(DateTime.Now, false); private set => Set(value, false); }

    [Description("When the file was last modified.")]
    [Group(Group.Properties)]
    [Name("Modified")]
    [Styles.Text(Tab = Tab.File,
        CanEdit = false,
        ValueConvert = typeof(ConvertToStringTimeRelative))]
    [XmlIgnore]
    public DateTime LastModified { get => Get(DateTime.Now, false); private set => Set(value, false); }

    [Style(Tab = Tab.File,
        Orientation = Orient.Horizontal,
        Pin = Sides.LeftOrTop,
        ValueTrigger = BindTrigger.LostFocus)]
    [XmlIgnore]
    public string Name
    {
        get => System.IO.Path.GetFileNameWithoutExtension(Path);
        set
        {
            value = FilePath.CleanName(value).TrimWhite();
            if (value.IsEmpty())
                value = $"Untitled";

            value += $".{Extension}";

            var folderPath = System.IO.Path.GetDirectoryName(Path);
            if (Folder.Exists(folderPath))
            {
                var oldPath = Path;
                var newPath = $@"{folderPath}\{value}.{Extension}";

                if (!File.Exists(newPath))
                {
                    if (Try.Do(() => System.IO.File.Move(oldPath, newPath), e => Log.Write(e)))
                        value = newPath;

                    else goto skip;
                }
                else
                {
                    Log.Write(new Error("A file with that name already exists!"));
                    goto skip;
                }

                goto handle;
            skip:
                {
                    Reset(() => Name);
                    return;
                }
            }
        handle: Path = value;
        }
    }

    [Styles.Text(Template.ImageThumb, NameHide = true,
        Index = -1,
        View = View.Footer)]
    [XmlIgnore]
    public string Path { get => Get<string>(null, false); set => Set(value, false); }

    [Description("The size of the file (in bytes).")]
    [Group(Group.Size)]
    [Styles.Text(Tab = Tab.File,
        CanEdit = false,
        ValueConvert = typeof(ConvertFileSize),
        ValueConvertParameter = FileSizeFormat.BinaryUsingSI)]
    [XmlIgnore]
    public long Size { get => Get(0L); private set => Set(value); }

    [XmlIgnore]
    public override string Title => $"{System.IO.Path.GetFileNameWithoutExtension(Path)}{(IsChanged ? "*" : string.Empty)}";

    [XmlIgnore]
    public override object ToolTip => Path;

    #endregion

    /// <see cref="Region.Property.Public.Abstract"/>

    public abstract IReadOnlyCollection<string> WritableExtensions { get; }

    /// <see cref="Region.Constructor"/>

    /// <inheritdoc/>
    protected FileDocument() : base() { }

    /// <see cref="Region.Method"/>
    #region

    private bool CheckBusy()
    {
        if (IsActive)
        {
            Dialog.ShowResult("Save", new Warning("The document is already saving."), Buttons.Ok);
            return true;
        }
        return false;
    }

    private async void Save(string filePath)
    {
        Log.Write($"async Document.Save(string filePath)");

        if (CheckBusy()) return;
        IsChanged = false;

        IsActive = true;

        var result = await SaveAsync(filePath);

        if (result && Extension == System.IO.Path.GetExtension(filePath)[1..])
            Path = filePath;

        IsActive = false;
    }

    private void SaveAs()
    {
        if (StorageDialog.Show(out string path, "SaveAs".Localize() + "...", StorageDialogMode.SaveFile, WritableExtensions, Path))
            Save(path);
    }

    protected abstract Task<bool> SaveAsync(string filePath);

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        switch (e.PropertyName)
        {
            case nameof(IsChanged):
                LastModified = DateTime.Now;
                break;

            case nameof(Path):

                Try.Do(() =>
                {
                    var fileInfo = new System.IO.FileInfo(Path);
                    LastAccessed
                        = fileInfo.LastAccessTime;
                    Created
                        = fileInfo.CreationTime;
                    LastModified
                        = fileInfo.LastWriteTime;
                    Size
                        = fileInfo.Length;
                });

                Reset(() => Name);
                Reset(() => Title);
                Reset(() => ToolTip);
                break;
        }
    }

    public sealed override void Save()
    {
        if (CheckBusy()) return;
        if (!File.Exists(Path))
            SaveAs();

        else Save(Path);
    }

    #endregion

    /// <see cref="ICommand"/>
    #region

    [Group(Group.Save)]
    [Image(Images.Save)]
    [Name("Save")]
    [Style(Tab = Tab.File,
        Float = Sides.LeftOrTop)]
    [XmlIgnore]
    public override ICommand SaveCommand => base.SaveCommand;

    [field: NonSerialized]
    private ICommand saveAsCommand;
    [Group(Group.Save)]
    [Image(Images.SaveAs)]
    [Name("SaveAs")]
    [Style(Tab = Tab.File,
        Float = Sides.LeftOrTop)]
    [XmlIgnore]
    public ICommand SaveAsCommand => saveAsCommand ??= new RelayCommand(SaveAs);

    #endregion
}

#endregion

/// <see cref="ImageFileDocument"/>
#region

/// <inheritdoc/>
[Description("Interface for managing an image file.")]
[Name("Image file document")]
public abstract record class ImageFileDocument : FileDocument
{
    /// <see cref="Region.Field"/>

    internal Bitmap Source;

    /// <see cref="Region.Constructor"/>

    /// <inheritdoc/>
    protected ImageFileDocument() : base() { }
}

#endregion

/// <see cref="TextFileDocument"/>
#region

/// <inheritdoc/>
[Description("Interface for managing a text file.")]
[Name("Text file document")]
public record class TextFileDocument : FileDocument
{
    private enum Group { Count, Format }

    /// <see cref="Region.Property"/>

    [Group(Group.Count)]
    [Name("Characters")]
    [Style(CanEdit = false,
        View = View.Footer,
        ValueFormat = NumberFormat.Default)]
    [XmlIgnore]
    public virtual int CountCharacters => Text.Length;

    [Group(Group.Count)]
    [Name("Lines")]
    [Style(CanEdit = false,
        View = View.Footer,
        ValueFormat = NumberFormat.Default)]
    [XmlIgnore]
    public virtual int CountLines => Text.GetLineCount();

    [Group(Group.Count)]
    [Name("Words")]
    [Style(CanEdit = false,
        View = View.Footer,
        ValueFormat = NumberFormat.Default)]
    [XmlIgnore]
    public virtual int CountWords => Text.GetWordCount();

    [Group(Group.Format)]
    [Style(CanEdit = false,
        View = View.Footer)]
    [XmlIgnore]
    public Encoding Encoding { get => Get<Encoding>(default, false); private set => Set(value, false); }

    public string Text { get => Get(""); set => Set(value); }

    public override string[] WritableExtensions => throw new NotImplementedException();

    /// <see cref="Region.Constructor"/>

    /// <inheritdoc/>
    public TextFileDocument() : base() { }

    /// <see cref="Region.Method"/>

    protected virtual void OnTextChanged()
    {
        Reset(() => CountCharacters);
        Reset(() => CountLines);
        Reset(() => CountWords);
    }

    protected override Task<bool> SaveAsync(string filePath)
    {
        throw new NotImplementedException();
    }

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        switch (e.PropertyName)
        {
            case nameof(Text):
                OnTextChanged();
                break;
        }
    }
}

#endregion