using Ion.Analysis;
using Ion.Controls;
using Ion.Core;
using Ion.Data;
using Ion.Input;
using Ion.Reflect;
using Ion.Text;
using Ion.Threading;
using Ion.Validation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Ion.Storage;

[Name("CopyTask")]
[Serializable]
[Styles.Object(Filter = Ion.Filter.None, GroupName = MemberGroupName.None, MemberViewType = MemberViewType.Tab)]
[XmlType(TypeName = nameof(Task))]
public record class CopyTask : Lockable, IComparable, ISubscribe, ITask
{
    #region enum Tab

    [TabView(View = View.Main)]
    private enum Tab
    {
        [TabStyle(Group = false, Image = Images.Activity)]
        Activity,
        [TabStyle(Group = false, Image = Images.Transfer)]
        Action,
        [TabStyle(Group = false, Image = Images.Analyze)]
        Analyze,
        [TabStyle(Group = false, Image = Images.PinStart)]
        Source,
        [TabStyle(Group = false, Image = Images.PinStop)]
        Target
    }

    #endregion

    #region (class) Messages

    private class Messages
    {
        public const string Browse = "Browsing '{0}'...";

        public const string Create = "Creating '{0}'";

        public const string Delete = "Deleting '{0}'";

        public const string Skip = "Skipping '{0}'";

        public const string Synchronize = "Synchronizing '{0}'";
    }

    #endregion

    #region (class) Validator

    /// <summary>
    /// A target path is valid if:
    /// 1a) it resides on the main drive, 1b) starts with the current user's folder path, but doesn't equal it (for security reasons), and 1c) exists 
    /// or 
    /// 2a) doesn't reside on the main drive and 2b) exists.
    /// </summary>
    public class Validator : LocalPath
    {
        public override bool Exists(ItemType target, string path)
        {
            var result = base.Exists(target, path);
            var a = Environment.GetFolderPath(Environment.SpecialFolder.System);
            if (path.StartsWith(Path.GetPathRoot(a)))
            {
                var b = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return path.StartsWith(b) && !path.Equals(b) && result;
            }
            return result;
        }
    }

    #endregion

    /// <see cref="Region.Delegate"/>

    public delegate void SynchronizedEventHandler();

    /// <see cref="Region.Event"/>

    [field: NonSerialized]
    public event SynchronizedEventHandler Synchronized;

    /// <see cref="Region.Field"/>

    protected readonly Taskable internalTask;

    protected readonly List<Storage.Monitor> Monitors = [];

    /// <see cref="Region.Property"/>
    #region

    [Hide]
    public bool IsActive => Status == CopyStatus.Active;

    [Hide]
    public bool IsEnabled
    {
        get => Get(false);
        set
        {
            //If the task is already enabled
            if (Get<bool>())
            {
                //If the task should be disabled
                if (!value)
                    Disable();
            }
            //If the task is already disabled
            else
            {
                //If the task should be enabled
                if (value)
                    Enable();
            }
        }
    }

    [Hide, XmlIgnore]
    public double Progress { get => Get(.0, false); set => Set(value, false); }

    [field: NonSerialized]
    private TaskList<FileTask> queue;
    [Hide]
    [XmlIgnore]
    public TaskList<FileTask> Queue
    {
        get
        {
            if (queue != null)
                return queue;

            queue = new();
            queue.Completed += OnTaskCompleted;
            return queue;
        }
    }

    private void OnTaskCompleted(object sender, EventArgs e) => LastActive = DateTime.Now;

    [field: NonSerialized]
    private Taskable analyzeTask;
    [Hide, XmlIgnore]
    public Taskable AnalyzeTask => analyzeTask;

    [field: NonSerialized]
    private readonly Taskable synchronizeTask;
    [Hide]
    [XmlIgnore]
    public Taskable SynchronizeTask => synchronizeTask;

    private static bool IsMonitor => true;

    [XmlIgnore]
    protected Storage.Monitor DefaultWatcher
    {
        get
        {
            var result = new Storage.Monitor()
            {
                IncludeChildren = true,
                Filter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Security
            };
            result.Failed += OnWatchingFailed;
            return result;
        }
    }

    /// <see cref="Tab"/>
    #region

    #region Pin > Above

    [Hide]
    public object Category { get => Get<object>(); set => Set(value); }

    [Hide]
    public int CategoryIndex { get => Get(-1); set => Set(value); }

    [XmlIgnore]
    [Styles.List(Template.ListCombo, NameHide = true, Pin = Sides.LeftOrTop,
        SelectedIndexProperty = nameof(CategoryIndex),
        SelectedItemProperty = nameof(Category))]
    [Styles.Text(CanEdit = false,
        TargetItem = typeof(string))]
    public static IList CategorySource => Appp.Get<IAppModelDock>()?.ViewModel.Panels.FirstOrDefault<CopyPanel>().Groups;

    [Filter(Ion.Filter.Group)]
    [Hide, XmlIgnore]
    public string CategoryName => CategorySource?.Count > CategoryIndex && CategoryIndex >= 0 ? (string)CategorySource[CategoryIndex] : "General";

    #endregion

    /// <see cref="Tab.Action"/>
    #region

    [Description("How to copy files.")]
    [Styles.Object(Tab = Tab.Action,
        Filter = Ion.Filter.None,
        GroupName = MemberGroupName.None,
        IsLockable = true,
        Orientation = Orient.Vertical)]
    public TextConverter.Action Action { get => Get(new TextConverter.Action()); set => Set(value); }

    [Style(Tab = Tab.Action, Pin = Sides.LeftOrTop)]
    public CopyDirection Direction { get => Get(CopyDirection.Right); set => Set(value); }

    [Hide]
    public string DirectionDescription => Direction == CopyDirection.Left ? "From target to source" : Direction == CopyDirection.Right ? "From source to target" : "From source to target (and back)";

    [Style(Tab = Tab.Action, IsLockable = true)]
    public Encoding Encoding { get => Get(Encoding.Unicode); set => Set(value); }

    #endregion

    #region Activity

    [Description("When the task was created.")]
    [Styles.Text(Tab = Tab.Activity,
        CanEdit = false,
        ValueConvert = typeof(ConvertToStringTimeRelative),
        CanSelect = false)]
    public DateTime Created { get => Get(DateTime.Now); set => Set(value); }

    [Description("When the task was last active.")]
    [Filter(Ion.Filter.Group | Ion.Filter.Sort)]
    [Styles.Text(Tab = Tab.Activity,
        CanEdit = false,
        ValueConvert = typeof(ConvertToStringTimeRelative),
        NullText = "Never",
        CanSelect = false)]
    public DateTime? LastActive { get => Get<DateTime?>(); set => Set(value); }

    [Description("When the task was last enabled.")]
    [Styles.Text(Tab = Tab.Activity,
        CanEdit = false,
        CanSelect = false,
        ValueConvert = typeof(ConvertToStringTimeRelative),
        NullText = "Never")]
    public DateTime? LastEnabled { get => Get<DateTime?>(); set => Set(value); }

    [Description("When the task was last disabled.")]
    [Styles.Text(Tab = Tab.Activity,
        CanEdit = false,
        CanSelect = false,
        ValueConvert = typeof(ConvertToStringTimeRelative),
        NullText = "Never")]
    public DateTime? LastDisabled { get => Get<DateTime?>(); set => Set(value); }

    [Description("When the task was last modified.")]
    [Styles.Text(Tab = Tab.Activity,
        CanEdit = false,
        CanSelect = false,
        ValueConvert = typeof(ConvertToStringTimeRelative),
        NullText = "Never")]
    public DateTime? LastModified { get => Get<DateTime?>(); set => Set(value); }

    #endregion

    #region Analyze

    [Styles.Text(Tab = Tab.Analyze,
        CanEdit = false,
        CanSelect = false)]
    [XmlIgnore]
    public uint Files { get => Get((uint)0, false); set => Set(value, false); }

    [Styles.Text(Tab = Tab.Analyze,
        CanEdit = false,
        CanSelect = false,
        ValueConvert = typeof(ConvertFileSize),
        ValueConvertParameter = FileSizeFormat.BinaryUsingSI)]
    [XmlIgnore]
    public long Size { get => Get(0L, false); set => Set(value, false); }

    #endregion

    #region Source

    [Description("The path to the folder where stuff should be written from.")]
    [Filter(Ion.Filter.Group | Ion.Filter.Search | Ion.Filter.Sort)]
    [Styles.Path(Template.PathFolder,
        Tab = Tab.Source,
        NameHide = true,
        IsLockable = true,
        Pin = Sides.LeftOrTop,
        Validate = [typeof(FolderExistsRule)])]
    public string Source { get => Get(""); set => Set(value); }

    [Description("The file extensions to include or exclude at the source.")]
    [Name("File extensions")]
    [Styles.Object(Tab = Tab.Source,
        Filter = Ion.Filter.None,
        GroupName = MemberGroupName.None,
        IsLockable = true,
        Orientation = Orient.Vertical)]
    public FilterExtensions SourceFileExtensions { get => Get(new FilterExtensions()); set => Set(value); }

    [Description("The folder extensions to include or exclude at the source.")]
    [Name("Folder extensions")]
    [Styles.Object(Tab = Tab.Source,
        Filter = Ion.Filter.None,
        GroupName = MemberGroupName.None,
        IsLockable = true,
        Orientation = Orient.Vertical)]
    public FilterExtensions SourceFolderExtensions { get => Get(new FilterExtensions()); set => Set(value); }

    [Description("Whether or not to include or exclude files with the given attributes at the source.")]
    [Name("FileAttributes")]
    [Style(Template.EnumFlag,
        Tab = Tab.Source,
        IsLockable = true)]
    public ItemAttributes SourceFileAttributes { get => Get(ItemAttributes.All); set => Set(value); }

    [Description("Whether or not to include or exclude folders with the given attributes at the source.")]
    [Name("FolderAttributes")]
    [Style(Template.EnumFlag,
        Tab = Tab.Source,
        IsLockable = true)]
    public ItemAttributes SourceFolderAttributes { get => Get(ItemAttributes.All); set => Set(value); }

    [Description("When source files should be overwritten.")]
    [Style(Tab = Tab.Source,
        IsLockable = true)]
    public FileOverwriteCondition SourceOverwriteFiles { get => Get(FileOverwriteCondition.Always); set => Set(value); }

    [Description("The types of changes to watch the source folder for.")]
    [Name(nameof(Monitor))]
    [Style(Tab = Tab.Source,
        IsLockable = true)]
    public NotifyFilters SourceMonitorFilter { get => Get(Monitor.DefaultFilter); set => Set(value); }

    [field: NonSerialized]
    private Storage.Monitor sourceMonitor;
    [XmlIgnore]
    private Storage.Monitor SourceMonitor => sourceMonitor ??= DefaultWatcher;

    #endregion

    #region Target

    private string target = string.Empty;
    [Description("The path to the folder where stuff should be written to.")]
    [Filter(Ion.Filter.Group | Ion.Filter.Search | Ion.Filter.Sort)]
    [Styles.Path(Template.PathFolder,
        Tab = Tab.Target,
        IsLockable = true,
        Pin = Sides.LeftOrTop,
        Validate = [typeof(FolderExistsRule)])]
    public string Target
    {
        get
        {
            return target;
        }
        set
        {
            target = value.TrimEnd('\\');
            Reset(() => Target);
        }
    }

    [Description("The file extensions to include or exclude at the target.")]
    [Name("File extensions")]
    [Styles.Object(Tab = Tab.Target,
        IsLockable = true,
        Orientation = Orient.Vertical)]
    public FilterExtensions TargetFileExtensions { get => Get(new FilterExtensions()); set => Set(value); }

    [Description("The folder extensions to include or exclude at the target.")]
    [Name("Folder extensions")]
    [Styles.Object(Tab = Tab.Target,
        IsLockable = true,
        Orientation = Orient.Vertical)]
    public FilterExtensions TargetFolderExtensions { get => Get(new FilterExtensions()); set => Set(value); }

    [Description("Whether or not to include or exclude files with the given attributes at the target.")]
    [Name("FileAttributes")]
    [Style(Template.EnumFlag,
        Tab = Tab.Target,
        IsLockable = true)]
    public ItemAttributes TargetFileAttributes { get => Get(ItemAttributes.All); set => Set(value); }

    [Description("Whether or not to include or exclude folders with the given attributes at the target.")]
    [Name("FolderAttributes")]
    [Style(Template.EnumFlag,
        Tab = Tab.Target,
        IsLockable = true)]
    public ItemAttributes TargetFolderAttributes { get => Get(ItemAttributes.All); set => Set(value); }

    [Description("When target files should be overwritten.")]
    [Style(Tab = Tab.Target,
        IsLockable = true)]
    public FileOverwriteCondition TargetOverwriteFiles { get => Get(FileOverwriteCondition.Always); set => Set(value); }

    [Hide, XmlIgnore]
    public static IExist TargetValidator => new Validator();

    [Description("The types of changes to watch the target folder for.")]
    [Name(nameof(Monitor))]
    [Style(Tab = Tab.Target,
        IsLockable = true)]
    public NotifyFilters TargetMonitorFilter { get => Get(Monitor.DefaultFilter); set => Set(value); }

    [field: NonSerialized]
    private Storage.Monitor targetMonitor;
    [XmlIgnore]
    private Storage.Monitor TargetMonitor => targetMonitor ??= DefaultWatcher;

    #endregion

    #region Pin > Below

    [Description("The status of the task.")]
    [Filter(Ion.Filter.Group | Ion.Filter.Sort)]
    [Style(CanEdit = false,
        NameHide = true,
        Pin = Sides.RightOrBottom)]
    [XmlIgnore]
    public CopyStatus Status { get => Get(CopyStatus.Inactive, false); set => Set(value, false); }

    #endregion

    #endregion

    #endregion

    /// <see cref="Region.Constructor"/>

    public CopyTask() : base()
    {
        internalTask = new Taskable(Run, TaskStrategy.Ignore);
        analyzeTask = new Taskable(AnalyzeSize, TaskStrategy.FinishAndRestart);
    }

    event TaskActiveEventHandler ITask.Active
    {
        add
        {
            throw new NotImplementedException();
        }

        remove
        {
            throw new NotImplementedException();
        }
    }

    event TaskCancelledEventHandler ITask.Cancelled
    {
        add
        {
            throw new NotImplementedException();
        }

        remove
        {
            throw new NotImplementedException();
        }
    }

    event TaskCompletedEventHandler ITask.Completed
    {
        add
        {
            throw new NotImplementedException();
        }

        remove
        {
            throw new NotImplementedException();
        }
    }

    event TaskPausedEventHandler ITask.Paused
    {
        add
        {
            throw new NotImplementedException();
        }

        remove
        {
            throw new NotImplementedException();
        }
    }

    event TaskProgressedEventHandler ITask.Progressed
    {
        add
        {
            throw new NotImplementedException();
        }

        remove
        {
            throw new NotImplementedException();
        }
    }

    /// <see cref="Region.Method.Private"/>
    #region

    private void Browse(string source, CancellationToken token, Action<string, string> createdFile = null, Action<string, string> createdFolder = null, Action<string> deletedFile = null, Action<string> deletedFolder = null)
    {
        if (token.IsCancellationRequested)
            return;

        Result result = Try.Do(() =>
        {
            if (!Folder.Exists(source))
                throw new DirectoryNotFoundException($"'{source}' does not exist.");
        },
        e => Log.Write(e));
        if (!result)
            return;

        Log.Write(Messages.Browse.F(source));

        List<string> sItems = [], dItems = [];

        result = Try.Do(() => sItems = Folder.GetItems(source).ToList(), e => Log.Write(e));
        if (!result)
            return;

        var target = source.Replace(Source, Target);
        //Try.Invoke(() => target = Converter.FormatUri(Target, target.Replace(Target, string.Empty), ItemType.Folder, action), e => Write(log, e));

        if (!result)
            return;

        result = Try.Do(() => dItems = Folder.GetItems(target).ToList(), e => Log.Write(e));
        foreach (var i in sItems)
        {
            if (token.IsCancellationRequested)
                return;

            ItemType itemType = default;
            result = Try.Do(() => itemType = XItemPath.GetType(i), e => Log.Write(e));

            if (result)
            {
                var dOld = i.Replace(Source, Target);
                var dNew = string.Empty;

                Try.Do(() => dNew = dOld /*Converter.FormatUri(Target, dOld.Replace(Target, string.Empty), itemType, action)*/, e => Log.Write(e));
                if (ApplySource(itemType, i, dNew))
                {
                    switch (itemType)
                    {
                        case ItemType.File:
                            createdFile?.Invoke(i, dNew);
                            break;

                        case ItemType.Folder:
                            createdFolder?.Invoke(i, dNew);
                            Browse(i, token, createdFile, createdFolder, deletedFile, deletedFolder);
                            break;
                    }
                    dItems.Remove(dNew);
                }
            }
        }

        if (token.IsCancellationRequested)
            return;

        dItems.ForEach(i =>
        {
            ItemType dType = default;
            result = Try.Do(() => dType = XItemPath.GetType(i), e => Log.Write(e));

            if (result)
            {
                if (ApplyTarget(dType, i))
                {
                    switch (dType)
                    {
                        case ItemType.File:
                            deletedFile?.Invoke(i);
                            break;
                        case ItemType.Folder:
                            deletedFolder?.Invoke(i);
                            break;
                    }

                }
            }
        });
    }

    ///

    private void CreateFile(bool log, string source, string target, CancellationToken token)
    {
        Log.Write(Messages.Create.F(target));
        return;

        var stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start();

        Try.Do(() =>
        {
            var callback = new Ion.Text.TextConverter.Callback((sizeRead, size) =>
            {
                Queue.Current.Duration = stopWatch.Elapsed;
                Queue.Current.SizeRead = Convert.ToInt64(sizeRead);
                Queue.Current.Progress = sizeRead / size;
                return token.IsCancellationRequested;
            });
            Ion.Text.TextConverter.CopyFile(source, target, Encoding, true, callback);
        }, e => Log.Write(e));

        stopWatch.Stop();
        Queue.Remove(Queue.Current);
    }

    private static void CreateFolder(bool log, string target)
    {
        Log.Write(Messages.Create.F(target));
        return;

        Try.Do(() => Folder.Create(target), e => Log.Write(e));
    }

    ///

    private static void Delete(bool log, string i)
    {
        Log.Write(Messages.Delete.F(i));
        return;

        Try.Do(() =>
        {
            //This is where everything gets deleted. Test before going live!
            return;
            ItemType itemType = XItemPath.GetType(i);
            switch (itemType)
            {
                case ItemType.File:
                    Storage.File.Delete(i);
                    break;

                case ItemType.Folder:
                    Folder.Delete(i, true);
                    break;
            }
        }, e => Log.Write(e));
    }

    ///

    private static bool ApplyAttributes(ItemType type, string input, ItemAttributes fileAttributes, ItemAttributes folderAttributes)
    {
        FileAttributes attributes = Storage.File.Attributes(input);
        var h = attributes.HasFlag(FileAttributes.Hidden);
        var r = attributes.HasFlag(FileAttributes.ReadOnly);

        switch (type)
        {
            case ItemType.File:
                if (!fileAttributes.HasFlag(ItemAttributes.Hidden))
                    return !h;

                if (!fileAttributes.HasFlag(ItemAttributes.ReadOnly))
                    return !r;

                break;

            case ItemType.Folder:
                if (!folderAttributes.HasFlag(ItemAttributes.Hidden))
                    return !h;

                if (!folderAttributes.HasFlag(ItemAttributes.ReadOnly))
                    return !r;

                break;
        }
        return true;
    }

    private static bool ApplyExtensions(string input, FilterExtensions extensions)
    {
        if (extensions is null || extensions.Value.IsEmpty())
            return true;

        var fileExtension = FilePath.GetExtension(input);
        if (extensions.Value.ToLower().Contains(fileExtension))
            return extensions.Filter == Clude.Include;

        return false;
    }

    private static bool ApplyOverwrite(string source, string target, FileOverwriteCondition condition)
    {
        if (condition == FileOverwriteCondition.Always)
            return true;

        try
        {
            var a = new FileInfo(source);
            var b = new FileInfo(target);

            return condition switch
            {
                FileOverwriteCondition.IfNewer => a.LastWriteTime > b.LastWriteTime,
                FileOverwriteCondition.IfNewerOrSizeDifferent => (a.LastWriteTime > b.LastWriteTime) || (a.Length != b.Length),
                FileOverwriteCondition.IfSizeDifferent => a.Length != b.Length,
                _ => false,
            };
        }
        catch
        {
            return true;
        }
    }

    ///

    private bool ApplyTarget(ItemType type, string path)
    {
        var result = false;
        Try.Do(() =>
        {
            switch (type)
            {
                case ItemType.File:
                case ItemType.Folder:
                    result = ApplyAttributes(type, path, TargetFileAttributes, TargetFolderAttributes) && ApplyExtensions(path, TargetFileExtensions);
                    break;
            }
            result = false;
        }, e => Log.Write(e));

        if (!result)
            Log.Write(Messages.Skip.F(Source));

        return result;
    }

    private bool ApplySource(ItemType type, string source, string target)
    {
        var result = false;
        Try.Do(() =>
        {
            switch (type)
            {
                case ItemType.File:
                    result = ApplyAttributes(type, source, SourceFileAttributes, SourceFolderAttributes) && ApplyExtensions(source, SourceFileExtensions) && ApplyOverwrite(source, target, TargetOverwriteFiles);
                    return;
                case ItemType.Folder:
                    result = ApplyAttributes(type, source, SourceFileAttributes, SourceFolderAttributes) && ApplyExtensions(source, SourceFileExtensions);
                    return;
            }
            result = false;
        }, e => Log.Write(e));

        if (!result)
            Log.Write(Messages.Skip.F(source));

        return result;
    }

    ///

    private void OnTargetItemChanged(object sender, FileSystemEventArgs e)
    {
    }

    private void OnTargetItemCreated(object sender, FileSystemEventArgs e)
    {
    }

    private void OnTargetItemDeleted(object sender, FileSystemEventArgs e)
    {
    }

    private void OnTargetItemRenamed(object sender, RenamedEventArgs e)
    {
    }

    ///

    private void OnSourceItemChanged(object sender, FileSystemEventArgs e)
    {
        var itemType = XItemPath.GetType(e.FullPath);
        if (itemType == ItemType.File)
        {
            var dOld = e.FullPath.Replace(Source, Target);
            var dNew = dOld; //Converter.FormatUri(Target, dOld.Replace(Target, string.Empty), ItemType.File, action);

            if (ApplySource(itemType, e.FullPath, dNew))
            {
                if (!analyzeTask.IsStarted)
                {
                    Size -= new FileInfo(dNew).Length;
                    Size += new FileInfo(e.FullPath).Length;
                }
                else _ = analyzeTask.Start();

                if (IsEnabled)
                {
                    if (!synchronizeTask.IsStarted)
                        Queue.Add(new(FileTaskType.Create, e.FullPath, dNew, token => Try.Do(() => CreateFile(true, e.FullPath, dNew, token), e => Log.Write(e))));
                }
            }
        }
        if (itemType == ItemType.Folder)
        {
            //To do: Handle changed attributes!
        }
    }

    private void OnSourceItemCreated(object sender, FileSystemEventArgs e)
    {
        var itemType = XItemPath.GetType(e.FullPath);

        var oldTarget = e.FullPath.Replace(Source, Target);
        var newTarget = oldTarget; //Converter.FormatUri(Target, oldTarget.Replace(Target, string.Empty), itemType, action);

        if (ApplySource(itemType, e.FullPath, newTarget))
        {
            if (itemType == ItemType.File)
            {
                if (!analyzeTask.IsStarted)
                {
                    Files++;
                    Size += new FileInfo(e.FullPath).Length;
                }
                else _ = analyzeTask.Start();

                if (IsEnabled)
                {
                    if (!synchronizeTask.IsStarted)
                        Queue.Add(new(FileTaskType.Create, e.FullPath, newTarget, token => Try.Do(() => CreateFile(true, e.FullPath, newTarget, token), e => Log.Write(e))));
                }
            }
            else if (itemType == ItemType.Folder)
            {
                if (IsEnabled)
                {
                    if (!synchronizeTask.IsStarted)
                        Queue.Add(new(FileTaskType.Create, e.FullPath, newTarget, token => Try.Do(() => CreateFolder(true, newTarget), e => Log.Write(e))));
                }
            }
        }
    }

    private void OnSourceItemDeleted(object sender, FileSystemEventArgs e)
    {
        /*
        var head = Target;
        var tail = e.FullPath.Replace(Source, Target).Replace(Target, string.Empty);

        var filePath = Converter.FormatUri(head, tail, ItemType.File, action);
        var folderPath = Converter.FormatUri(head, tail, ItemType.Folder, action);

        ItemType? itemType = null;

        var result = Try.Invoke(() => itemType = Computer.GetType(filePath), e => Log.Write<CopyTask>(e));
        result = !result ? Try.Invoke(() => itemType = Computer.GetType(folderPath), e => Log.Write<CopyTask>(e)) : result;

        if (itemType == ItemType.File)
        {
            if (!analyzeSizeTask.IsStarted)
            {
                Files--;
                Size -= new FileInfo(filePath).Length;
            }
            else _ = analyzeSizeTask.Start();

            if (enable)
            {
                if (!synchronizeTask.IsStarted)
                    Queue.Add(Operation.Types.Delete, itemType.Value, filePath, string.Empty, token => Delete(true, filePath));
            }
        }
        else if (itemType == ItemType.Folder)
        {
            if (enable)
            {
                if (!synchronizeTask.IsStarted)
                    Queue.Add(Operation.Types.Delete, itemType.Value, folderPath, string.Empty, token => Delete(true, folderPath));
            }
        }
        */
    }

    private void OnSourceItemRenamed(object sender, RenamedEventArgs e)
    {
        /*
        if (!enable || synchronizeTask.IsStarted)
            return;

        var source = e.FullPath;
        var target = e.FullPath.Replace(Source, Target);

        var oldPath = e.OldFullPath;
        var newPath = e.FullPath;

        string a = string.Empty, b = string.Empty;
        //We know this will always be in target scope
        a = oldPath.Replace(Source, Target);
        //We won't know that this will always be in target scope
        b = newPath.Replace(Source, Target);

        var aTail = a.Replace(Target, string.Empty);
        var bTail = b.Replace(Target, string.Empty);

        if (b.StartsWith(Target))
        {
            var itemType = Computer.GetType(newPath);
            if (itemType == ItemType.Folder)
            {
                var dA = Converter.FormatUri(Target, aTail, ItemType.Folder, action);
                var dB = Converter.FormatUri(Target, bTail, ItemType.Folder, action);
                Queue.Add(Operation.Types.Move, itemType, dA, dB, token => Try.Invoke(() => Folder.Long.Move(dA, dB), e => Log.Write<CopyTask>(e)));
            }
            else if (itemType == ItemType.File)
            {
                var dA = Converter.FormatUri(Target, aTail, ItemType.File, action);
                var dB = Converter.FormatUri(Target, bTail, ItemType.File, action);
                Queue.Add(Operation.Types.Move, itemType, dA, dB, token => Try.Invoke(() => Ion.Storage.File.Long.Move(dA, dB), e => Log.Write<CopyTask>(e)));
            }
        }
        //Delete object at path, a: Since the original object has moved outside scope of target folder, it must no longer be present there!
        else
        {
            var dA = Converter.FormatUri(Target, aTail, Computer.GetType(a), action);
            Queue.Add(Operation.Types.Delete, Computer.GetType(dA), dA, string.Empty, token => Try.Invoke(() => { /-Delete(dA)-/ }, e => Log.Write<CopyTask>(e)));
        }
        */
    }

    ///

    private void OnWatchingFailed(object sender, EventArgs<Error> e) => Log.Write(e.A);

    ///

    private void AnalyzeSize(CancellationToken token)
    {
        uint files = 0;
        long size = 0;

        Browse(Source, token, (i, j) =>
        {
            Try.Do(() =>
            {
                var file = new Ion.Storage.File(i);
                file.Refresh();

                files++;
                size += file.Size;
            });
        },
        null, null, null);
        Dispatch.Do(() =>
        {
            Files = files;
            Size = size;
        });
    }

    ///

    /*
    async Task TryCompressAsync()
    {
        await Task.Run(() =>
        {
            var outName = compressionOptions.OutputName;

            if (outName.IsNullOrEmpty())
                outName = Source.GetFileName();

            var outExtension = string.Empty;
            switch (compressionOptions.Type)
            {
                case CompressionFormat.BZip2:
                    outExtension = "tar.bz2";
                    break;
                case CompressionFormat.GZip:
                    outExtension = "tar.gz";
                    break;
                case CompressionFormat.Tar:
                    outExtension = "tar";
                    break;
                case CompressionFormat.Zip:
                    outExtension = "zip";
                    break;
            }

            var outPath = @"{0}\{1}.{2}".F(Target, outName, outExtension);

            try
            {
                var fileStream = Ion.Storage.File.Long.Create(outPath);

                var stream = default(Stream);
                switch (compressionOptions.Type)
                {
                    case CompressionFormat.BZip2:
                        stream = new BZip2OutputStream(fileStream);
                        break;
                    case CompressionFormat.GZip:
                        stream = new GZipOutputStream(fileStream);
                        break;
                    case CompressionFormat.Tar:
                        stream = new TarOutputStream(fileStream);
                        break;
                    case CompressionFormat.Zip:
                        stream = new ZipOutputStream(fileStream);

                        var zipStream = stream as ZipOutputStream;
                        zipStream.SetLevel(compressionOptions.Level);
                        zipStream.Password = compressionOptions.Password;
                        break;
                }

                switch (compressionOptions.Type)
                {
                    case CompressionFormat.BZip2:
                    case CompressionFormat.GZip:
                    case CompressionFormat.Tar:
                        var tarArchive = TarArchive.CreateOutputTarArchive(stream);

                        tarArchive.RootPath = Target.Replace('\\', '/');
                        if (tarArchive.RootPath.EndsWith("/"))
                            tarArchive.RootPath = tarArchive.RootPath.Remove(tarArchive.RootPath.Length - 1);

                        var tarEntry = TarEntry.CreateEntryFromFile(Target);
                        tarArchive.WriteEntry(tarEntry, false);

                        CompressTar(tarArchive, Target, outPath);
                        break;
                    case CompressionFormat.Zip:
                        int folderOffset = Target.Length + (Target.EndsWith("\\") ? 0 : 1);
                        CompressZip(stream as ZipOutputStream, Target, folderOffset, outPath);
                        break;
                }

                switch (compressionOptions.Type)
                {
                    case CompressionFormat.BZip2:
                        var bZip2Stream = stream as BZip2OutputStream;
                        bZip2Stream.IsStreamOwner = true;
                        bZip2Stream.Close();
                        break;
                    case CompressionFormat.GZip:
                        var gZipStream = stream as GZipOutputStream;
                        gZipStream.IsStreamOwner = true;
                        gZipStream.Close();
                        break;
                    case CompressionFormat.Tar:
                        var tarStream = stream as TarOutputStream;
                        tarStream.IsStreamOwner = true;
                        tarStream.Close();
                        break;
                    case CompressionFormat.Zip:
                        var zipStream = stream as ZipOutputStream;
                        zipStream.IsStreamOwner = true;
                        zipStream.Close();
                        break;
                }

                fileStream?.Close();
                fileStream?.Dispose();
            }
            catch
            {

            }
        });
    }

    void CompressFile(ZipOutputStream ZipStream, string Path, int FolderOffset)
    {
        var FileInfo = new FileInfo(Path);

        //Makes the name in zip based on the folder
        var EntryName = Path.Substring(FolderOffset);
        //Removes drive from name and fixes slash direction 
        EntryName = ZipEntry.CleanName(EntryName);

        var NewEntry = new ZipEntry(EntryName);
        NewEntry.DateTime = FileInfo.LastWriteTime;
        NewEntry.Size = FileInfo.Length;

        ZipStream.PutNextEntry(NewEntry);

        var Buffer = new byte[4096];
        using (var StreamReader = Ion.Storage.File.Long.OpenRead(Path))
            StreamUtils.Copy(StreamReader, ZipStream, Buffer);

        ZipStream.CloseEntry();
    }

    void CompressFile(TarArchive Archive, string Path)
    {
        var tarEntry = TarEntry.CreateEntryFromFile(Path);
        Archive.WriteEntry(tarEntry, true);
    }

    void CompressTar(TarArchive archive, string folderPath, string outPath)
    {
        var Files = Enumerable.Empty<string>();
        try
        {
            Files = Folder.GetFiles(folderPath);
        }
        catch (Exception e)
        {
            Write(e.Message, EntryType.Error);
        }

        foreach (var i in Files)
        {
            if (i != outPath)
                CompressFile(archive, i);
        }

        var Folders = Enumerable.Empty<string>();
        try
        {
            Folders = Folder.GetFolders(Target);
        }
        catch (Exception e)
        {
            Write(e.Message, EntryType.Error);
        }

        foreach (var i in Folders)
            CompressTar(archive, i, outPath);
    }

    void CompressZip(ZipOutputStream stream, string folderPath, int folderOffset, string outPath)
    {
        var Files = Enumerable.Empty<string>();
        try
        {
            Files = Folder.GetFiles(folderPath);
        }
        catch (Exception e)
        {
            Write(e.Message, EntryType.Error);
        }

        foreach (var i in Files)
        {
            if (i != outPath)
            {
                try
                {
                    CompressFile(stream, i, folderOffset);
                    Ion.Storage.File.Long.Delete(i);
                }
                catch (Exception e)
                {
                    Write(e.Message, EntryType.Error);
                }
            }
        }

        var Folders = Enumerable.Empty<string>();
        try
        {
            Folders = Folder.GetFolders(Target);
        }
        catch (Exception e)
        {
            Write(e.Message, EntryType.Error);
        }

        foreach (var i in Folders)
        {
            CompressZip(stream, i, folderOffset, outPath);

            try
            {
                Folder.Delete(i);
            }
            catch (Exception e)
            {
                Write(e.Message, EntryType.Error);
            }
        }
    }
    */

    ///

    [OnDeserialized]
    public void OnDeserialized(StreamingContext context)
    {
        analyzeTask = new Taskable(AnalyzeSize, TaskStrategy.FinishAndRestart);
        _ = analyzeTask.Start();
    }

    ///

    private async Task Run(CancellationToken token)
    {
        if (Appp.Get<IAppModelDock>()?.ViewModel.Panels.First<CopyPanel>().ShowWarningBeforeEnablingTask == true)
        {
            //if (Dialog.ShowWarning(nameof(IsEnabled), new Warning($"Are you sure you want to synchronize '{Source}' with '{target}'?"), Buttons.ContinueCancel) != 0)
            //return;
        }

        bool result = Try.Do(() =>
        {
            if (Target == Source || Target.StartsWith($@"{Source}\"))
                throw new InvalidDataException($"'{nameof(Target)}' cannot equal or derive from '{nameof(Source)}'.");

            if (!Folder.Exists(Target))
                throw new DirectoryNotFoundException($"'{nameof(Target)}' does not exist.");

            if (!TargetValidator.Exists(ItemType.Folder, Target))
                throw new InvalidDataException($"'{nameof(Target)}' must start with (but not equal) '{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}'.");

            //Warn if source/target is

            //- drive
            foreach (var i in Drive.Get())
            {
                if (Source == i.Name)
                    Log.Write(new Warning($"Source folder '{Source}' is drive path."));

                if (Target == i.Name)
                    Log.Write(new Warning($"Target folder '{Target}' is drive path."));
            }

            //- folder located outside local user folder
            if (!Source.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)))
                Log.Write(new Warning($"Source folder '{Source}' is located outside of '{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}'."));

            if (!Target.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)))
                Log.Write(new Warning($"Target folder '{Target}' is located outside of '{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}'."));

        }, e => Log.Write(new Error(e)));

        if (!result)
        {
            Disable();
            return;
        }

        Log.Write(Messages.Synchronize.F(Source));
        await Task.Run(() => Browse
        (
            Source, token,
            (i, j) => Queue.Add(new(FileTaskType.Create, i, j, k => CreateFile(true, i, j, k))),
            (i, j) => Queue.Add(new(FileTaskType.Create, i, j, k => CreateFolder(true, j))),
            i => Queue.Add(new(FileTaskType.Delete, i, string.Empty, k => Delete(true, i))),
            i => Queue.Add(new(FileTaskType.Delete, i, string.Empty, k => Delete(true, i)))
        ), token);
    }

    ///

    private void Disable()
    {
        if (Appp.Get<IAppModelDock>()?.ViewModel.Panels.First<CopyPanel>().ShowWarningBeforeDisablingTask == true)
        {
            //if (Dialog.ShowWarning(nameof(Disable), new Warning($"Are you sure you want to stop synchronizing '{Source}' with '{target}'?"), Buttons.ContinueCancel) != 0)
            //return;
        }

        Queue.Clear();
        if (!IsEnabled) return;

        Status = CopyStatus.Inactive;
        IsLocked = false;

        LastDisabled = DateTime.Now;
        Set(() => IsEnabled, false);
    }

    private async void Enable()
    {
        if (IsEnabled) return;

        Set(() => IsEnabled, true);
        LastEnabled = DateTime.Now;

        IsLocked = true;
        Status = CopyStatus.Active;

        await internalTask.Start();

        if (IsMonitor)
        {
            Status = CopyStatus.Monitoring;
            OnMonitoring();
        }
        else Disable();
    }

    #endregion

    /// <see cref="Region.Method.Protected"/>

    protected virtual void OnMonitoring()
    {
        Try.Do(() => SourceMonitor.Enable(Source), e => Log.Write(e));
        Try.Do(() => TargetMonitor.Enable(Target), e => Log.Write(e));
    }

    /// <see cref="Region.Method.Public.Override"/>
    #region

    public override int CompareTo(object a)
    {
        if (a is CopyTask b)
            return IsEnabled.CompareTo(b.IsEnabled);

        return base.CompareTo(a);
    }

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        LastModified = DateTime.Now;
        switch (e.PropertyName)
        {
            case nameof(CategoryIndex):
                Reset(() => CategoryName);
                break;

            case nameof(Direction):
                Reset(() => DirectionDescription);
                break;

            case nameof(Status): Reset(() => IsActive); break;

            case nameof(Target):
            case nameof(TargetFileExtensions):
            case nameof(TargetFileAttributes):
            case nameof(TargetFolderAttributes):

            case nameof(SourceOverwriteFiles):

            case nameof(Source):
            case nameof(SourceFileExtensions):
            case nameof(SourceFileAttributes):
            case nameof(SourceFolderAttributes):
                _ = analyzeTask?.Start(); break;
        }
    }

    public override void OnSettingProperty(PropertySettingEventArgs e)
    {
        base.OnSettingProperty(e);
        if (e.PropertyName == nameof(Source))
            e.NewValue = e.NewValue?.ToString().TrimEnd('\\');
    }

    #endregion

    /// <see cref="Region.Method.Public"/>

    public void Subscribe()
    {
        foreach (var m in Monitors)
        {
            m.Failed -= OnWatchingFailed; m.Failed += OnWatchingFailed;
            m.Subscribe();
        }

        if (SourceMonitor is Storage.Monitor i)
        {
            i.ItemChanged -= OnSourceItemChanged; i.ItemCreated -= OnSourceItemCreated; i.ItemDeleted -= OnSourceItemDeleted; i.ItemRenamed -= OnSourceItemRenamed;
            i.ItemChanged += OnSourceItemChanged; i.ItemCreated += OnSourceItemCreated; i.ItemDeleted += OnSourceItemDeleted; i.ItemRenamed += OnSourceItemRenamed;
        }
        if (TargetMonitor is Storage.Monitor j)
        {
            j.ItemChanged -= OnTargetItemChanged; j.ItemCreated -= OnTargetItemCreated; j.ItemDeleted -= OnTargetItemDeleted; j.ItemRenamed -= OnTargetItemRenamed;
            j.ItemChanged += OnTargetItemChanged; j.ItemCreated += OnTargetItemCreated; j.ItemDeleted += OnTargetItemDeleted; j.ItemRenamed += OnTargetItemRenamed;
        }
    }

    public void Unsubscribe()
    {
        foreach (var m in Monitors)
        {
            m.Failed -= OnWatchingFailed;
            m.Unsubscribe();
        }

        if (SourceMonitor is Storage.Monitor i)
        {
            i.ItemChanged -= OnSourceItemChanged; i.ItemCreated -= OnSourceItemCreated; i.ItemDeleted -= OnSourceItemDeleted; i.ItemRenamed -= OnSourceItemRenamed;
        }
        if (TargetMonitor is Storage.Monitor j)
        {
            j.ItemChanged -= OnSourceItemChanged; j.ItemCreated -= OnSourceItemCreated; j.ItemDeleted -= OnSourceItemDeleted; j.ItemRenamed -= OnSourceItemRenamed;
        }
    }

    void ITask.Cancel() => throw new NotImplementedException();
    void ITask.Pause() => throw new NotImplementedException();
    Task ITask.Start() => throw new NotImplementedException();

    /// <see cref="ICommand"/>

    [field: NonSerialized]
    private ICommand deleteCommand;
    [Hide]
    [XmlIgnore]
    public ICommand DeleteCommand => deleteCommand ??= new RelayCommand(() =>
    {
        Dialog.ShowResult("Delete", new Warning("Are you sure you want to delete this?"), i =>
        {
            if (i == 0)
                Appp.Get<IAppModelDock>()?.ViewModel.Panels.First<CopyPanel>().Tasks.Remove(this);
        },
        Buttons.YesNo);
    },
    () => true);

    TimeSpan ITask.Duration => throw new NotImplementedException();

    bool ITask.IsCancelled => throw new NotImplementedException();

    bool ITask.IsPaused => throw new NotImplementedException();

    bool ITask.IsStarted => throw new NotImplementedException();

    DateTime? ITask.LastCancelled => throw new NotImplementedException();

    DateTime? ITask.LastCompleted => throw new NotImplementedException();

    DateTime? ITask.LastPaused => throw new NotImplementedException();
}