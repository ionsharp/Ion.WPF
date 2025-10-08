using Ion.Analysis;
using Ion.Data;
using Ion.Linq;
using Ion.Models;
using Ion.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ion.Apps.Explore;

[Name("Rename")]
[Description("Rename multiple files algorithmically.")]
[Icon(SmallImages.Rename)]
[ObjectStyle(Filter = Filter.None, Strict = System.Reflection.MemberTypes.All)]
[Serializable]
public class RenamePanel : TaskPanel
{
    enum Category { Extension, File, Index, Name, Other }

    [Serializable]
    public enum Order { Accessed, Created, Modified, Size }

    [Serializable]
    public enum ReplaceFormat { Original, Capitalized, Lower, Upper }

    ///<see cref="Region.Constant"/>

    public const string DefaultExtension = "file";

    ///<see cref="Region.Property.Protected"/>

    protected override MethodType Execution => MethodType.Managed;

    ///<see cref="Region.Property.Public"/>
    #region

    [ReadOnly]
    [StringStyle(StringStyle.FolderPath, Name = "Folder", Pin = Pin.AboveOrLeft)]
    public string Path { get => Get(""); set => Set(value); }

    ///<see cref="Category.Extension"/>

    [Group(Category.Extension), Lockable, Name("Replace format")]
    [Description("The format of renamed file extensions.")]
    [Style]
    public ReplaceFormat ExtensionReplaceFormat { get => Get(ReplaceFormat.Lower); set => Set(value); }

    [Group(Category.Extension), Lockable, Name("Replace with")]
    [Description("The extension to replace renamed files with.")]
    [StringStyle(Placeholder = "Replace extension with")]
    public string ExtensionReplaceWith { get => Get(""); set => Set(value); }

    ///<see cref="Category.File"/>

    [Group(Category.File), Lockable]
    [Name("Order")]
    [Description("The order of files when renaming.")]
    [Style]
    public Order FileOrder { get => Get(Order.Created); set => Set(value); }

    ///<see cref="Category.Index"/>

    [Group(Category.Index), Lockable]
    [Name("Increment")]
    [Description("How much to increment every file.")]
    [NumberStyle(1, 10, 1)]
    public int IndexIncrement { get => Get(1); set => Set(value); }

    [Group(Category.Index), Lockable]
    [Name("By extension")]
    [Description("Index resets every extension.")]
    [Style]
    public bool IndexByExtension { get => Get(true); set => Set(value); }

    [Group(Category.Index), Lockable]
    [Name("Start")]
    [Description("The index to start at.")]
    [NumberStyle(0, int.MaxValue, 1)]
    public int IndexStart { get => Get(0); set => Set(value); }

    ///<see cref="Category.Name"/>

    [Group(Category.Name), Lockable, ReadOnly]
    [Name("Preview")]
    [Description("A preview of how a sequence of files are renamed.")]
    [Style]
    public string NamePreview { get => Get(""); set => Set(value); }

    ///<see cref="Category.Other"/>

    [Group(Category.Other), Lockable]
    [Description("Include files in sub folders.")]
    [Style]
    public bool SubFolders { get => Get(false); set => Set(value); }
    
    #endregion

    ///<see cref="Region.Property.Public.Override"/>

    public override bool HideIfNoActiveDocument => true;

    ///<see cref="Region.Constructor"/>

    public RenamePanel() : base() => this.Update(nameof(IndexStart));

    ///<see cref="Region.Method.Private"/>
    #region

    private string GroupBy(string i) => GetOldExtension(i);

    private object OrderBy(string i)
    {
        var file = Try.Return(() => new FileInfo(i));
        if (file is null)
            return i;

        switch (FileOrder)
        {
            case Order.Accessed:
                return file.LastAccessTime;

            case Order.Modified:
                return file.LastWriteTime;

            case Order.Size:
                return file.Length;

            case Order.Created: default:
                return file.CreationTime;
        }
    }

    ///

    private string GetOldExtension(string i) => System.IO.Path.GetExtension(i);

    private string GetNewExtension(string i)
    {
        switch (ExtensionReplaceFormat)
        {
            case ReplaceFormat.Capitalized:
                return Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(i);

            case ReplaceFormat.Lower:
                return i.ToLower();

            case ReplaceFormat.Upper:
                return i.ToUpper();

            default: return $".{i}";
        }
    }

    ///

    private void Rename(string oldPath, ref int index)
    {
        var newPath = "";

        var oldExtension = GetOldExtension(oldPath);
        var newExtension = GetNewExtension(oldExtension);

        //Define new file path based on current index
        newPath = $@"{System.IO.Path.GetDirectoryName(oldPath)}\{index}{newExtension}";

        //Does a file with that path exist?
        if (Ion.Storage.File.Exists(newPath))
        {
            //Rename existing file first
            var tempPath = Ion.Storage.File.ClonePath(newPath);
            if (!Try.Invoke(() => Ion.Storage.File.Move(newPath, tempPath)))
                return;
        }

        var result = Try.Invoke(() => Ion.Storage.File.Move(oldPath, newPath));
        Log.Add(result ? new FileRenamedMessage(oldPath, newPath) : result);

        index += IndexIncrement;
    }

    private void Rename(string folderPath, bool subFolders, CancellationToken token)
    {
        IEnumerable<string> files = null;
        if (Try.Invoke(() => files = Ion.Storage.Folder.GetFiles(Path)) && files?.Count() > 0)
        {
            var index = IndexStart;
            if (IndexByExtension)
            {
                var fileGroups = files.OrderBy(OrderBy).GroupBy(GroupBy);
                foreach (var i in fileGroups)
                {
                    index = IndexStart;
                    i.ForEach(j => Rename(j, ref index));
                }
            }
            else
            {
                files = files.OrderBy(OrderBy);
                foreach (var i in files)
                    Rename(i, ref index);
            }
        }

        if (subFolders)
        {
            IEnumerable<string> folders = null;
            if (Try.Invoke(() => folders = Ion.Storage.Folder.GetFolders(folderPath)) && folders?.Any() == true)
                folders.ForEach(i => Rename(i, subFolders, token));
        }
    }

    ///

    private void OnPathChanged(object sender, string e)
    {
        Path = sender.As<ItemViewDocument>().Panel.Path;
    }

    #endregion

    ///<see cref="Region.Method.Protected"/>
    #region

    protected override Task ExecuteAsync(object parameter, CancellationToken token) => throw new NotImplementedException();

    protected override void ExecuteSync(object parameter, CancellationToken token) => Rename(Path, SubFolders, token);

    protected override void OnActiveContentChanged(Value<Content> value)
    {
        base.OnActiveContentChanged(value);
        if (value.Old is ItemViewDocument oldDocument)
        {
            this.Unbind(nameof(Path), oldDocument.Panel, nameof(ItemViewPanel.Path));
            Path = null;
        }
        if (value.New is ItemViewDocument newDocument)
        {
            Path = newDocument.Panel.Path;
            this.Bind(nameof(Path), newDocument.Panel, nameof(ItemViewPanel.Path));
        }
    }

    protected override void OnExecuted() { }

    #endregion

    ///<see cref="Region.Method.Public"/>

    public override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        switch (e.PropertyName)
        {
            case nameof(ExtensionReplaceFormat):
            case nameof(ExtensionReplaceWith):
            case nameof(IndexIncrement):
            case nameof(IndexStart):
                NamePreview = $@"";
                int j = IndexStart;
                for (var i = 0; i < 5; i++, j += IndexIncrement)
                    NamePreview += $@"{j}.{GetNewExtension(ExtensionReplaceWith.Length > 0 ? ExtensionReplaceWith : DefaultExtension)}, ";

                NamePreview.Substring(0, NamePreview.Length - 2);
                break;
        }
    }

    ///<see cref="Region.Command"/>

    [Icon(SmallImages.Rename)]
    [MethodStyle(MethodStyle.Button, Name = "Rename", NameSecondary = "Rename", Pin = Pin.BelowOrRight)]
    [VisibilityTrigger(nameof(IsNotBusy), true)]
    public override ICommand StartCommand => base.StartCommand;
}