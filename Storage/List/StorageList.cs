using Ion.Analysis;
using Ion.Collect;
using Ion.Controls;
using Ion.Core;
using Ion.Input;
using Ion.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace Ion.Storage;

/// <inheritdoc/>
public abstract class StorageList<T> : ListObservable<T>, IItemList, ISubscribe
{
    private enum Category { Import }

    /// <see cref="Automator"/>
    #region

    private class Automator : List<DispatcherOperation>
    {
        private void OnCompleted(object sender, EventArgs e)
        {
            var i = (DispatcherOperation)sender;
            i.Completed -= OnCompleted;
            Remove(i);
        }

        new public void Add(DispatcherOperation i)
        {
            base.Add(i);
            i.Completed += OnCompleted;
        }

        new public void Clear()
        {
            for (var i = Count - 1; i >= 0; i--)
            {
                this[i].Completed -= OnCompleted;
                this[i].Abort();
                RemoveAt(i);
            }
        }
    }

    #endregion

    /// <see cref="Region.Delegate"/>

    public delegate void EventHandler(StorageList<T> sender);

    /// <see cref="Region.Event"/>
    #region

    public event EventHandler Refreshing;

    public event EventHandler Refreshed;

    #endregion

    /// <see cref="Region.Field"/>
    #region

    private readonly Automator automator = new();

    private Monitor monitor;

    private readonly Taskable<string> refreshTask;

    protected bool subscribed;

    public static Monitor DefaultWatcher => new(NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.Size | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Security);

    #endregion

    /// <see cref="Region.Property"/>
    #region

    [Hide]
    public virtual string ItemName => "Item";

    [Hide]
    public bool IsRefreshing { get => this.Get(false); private set => this.Set(value); }

    private Filter filter = Filter.Default;
    [Hide]
    public Filter Filter
    {
        get => filter;
        set => filter = value ?? Filter.Default;
    }

    [Hide]
    public string Path { get => this.Get(""); set => this.Set(value); }

    [Hide]
    public double Progress { get => this.Get(.0); set => this.Set(value); }

    #endregion

    /// <see cref="Region.Property.Indexor"/>

    protected abstract T this[string path] { get; }

    /// <see cref="Region.Constructor"/>
    #region

    protected StorageList() : this(string.Empty, null) { }

    protected StorageList(Filter filter) : this(string.Empty, filter) { }

    protected StorageList(string path, Filter filter) : base()
    {
        refreshTask = new(null, RefreshAsync, TaskStrategy.CancelAndRestart);
        Path = path; Filter = filter;
    }

    #endregion

    /// <see cref="Region.Method.Private"/>
    #region

    private IEnumerable<T> Query(string path, Filter filter)
    {
        if (filter.Types.HasFlag(ItemType.Drive))
        {
            if (path.IsEmpty() || path == FilePath.Root)
            {
                var drives = Try.Get(Drive.Get);
                if (drives is not null)
                {
                    foreach (var i in drives)
                        yield return ToDrive(i);
                }
            }
        }
        if (filter.Types.HasFlag(ItemType.Folder))
        {
            var folders = Try.Get(() => Folder.GetFolders(path).Where(j => j != ".."));
            if (folders is not null)
            {
                foreach (var i in folders)
                    yield return ToFolder(i);
            }
        }
        if (filter.Types.HasFlag(ItemType.File))
        {
            var files = Try.Get(() => Folder.GetFiles(path));
            if (files is not null)
            {
                foreach (var i in files)
                {
                    if (filter.Evaluate(i, ItemType.File))
                        yield return ToFile(i);
                }
            }
        }
    }

    ///

    private void OnItemChanged(object sender, FileSystemEventArgs e)
    {
        var i = this[e.FullPath];
        if (i is IItemProperties j)
        {
            ItemProperties oldProperties = j.Properties;
            ItemProperties newProperties = new(e.FullPath);

            ItemProperties.Compare(oldProperties, newProperties).If(k => k != ItemProperty.None, k => OnItemChanged(i, k));
            //j.Properties = newProperties;
        }
    }

    private void OnItemCreated(object sender, FileSystemEventArgs e)
    {
        T i = default;

        var a = File.Exists(e.FullPath);
        var b = Filter?.Evaluate(e.FullPath, ItemType.File) != false;

        if (a && b)
        {
            i = ToFile(e.FullPath);
        }
        else
        {
            a = Folder.Exists(e.FullPath);
            b = Filter?.Evaluate(e.FullPath, ItemType.Folder) != false;

            if (a && b)
                i = ToFolder(e.FullPath);
        }

        if (i != null)
        {
            OnItemCreated(i);
            Add(i);
        }
    }

    private void OnItemDeleted(object sender, FileSystemEventArgs e)
    {
        var item = this[e.FullPath];
        if (item != null)
        {
            OnItemDeleted(item);
            Remove(item);
        }
    }

    private void OnItemRenamed(object sender, RenamedEventArgs e) => OnItemRenamed(e);

    ///

    private void InternalClear()
    {
        automator
            .Clear();
        base
            .Clear();
    }

    private async Task InternalClearAsync()
    {
        InternalClear();
        await While.DoAwait(automator, i => 0 < i.Count && 0 < Count);
    }

    ///

    private void Refresh(string path, Filter filter, CancellationToken token)
    {
        IEnumerable<T> items = null;
        Try.Do(() => items = Query(path, filter), e => Log.Write(new Error(e)));

        if (items is not null)
        {
            foreach (var i in items)
            {
                if (token.IsCancellationRequested)
                    return;

                automator.Add(Dispatch.InvokeReturn(() => Add(i)));
            }
        }
    }

    private async Task RefreshAsync(string path, CancellationToken token)
    {
        IsRefreshing = true;

        Path = path;
        await InternalClearAsync();

        OnRefreshing();

        var filter = Filter;
        await Task.Run(() => Refresh(path, filter, token), token);

        OnRefreshed();
        IsRefreshing = false;
    }

    private void RefreshSync(string path)
    {
        IsRefreshing = true;

        Path = path;
        InternalClear();

        OnRefreshing();
        Refresh(path, Filter, new(false));

        OnRefreshed();
        IsRefreshing = false;
    }

    #endregion

    /// <see cref="Region.Method.Protected"/>
    #region

    protected abstract T ToDrive(DriveInfo input);

    protected abstract T ToFile(string input);

    protected abstract T ToFolder(string input);

    ///

    protected virtual void OnExported(string path) { }

    protected virtual void OnItemChanged(T i, ItemProperty property) { }

    protected virtual void OnItemCreated(T i) { }

    protected virtual void OnItemDeleted(T i) { }

    protected virtual void OnItemRenamed(RenamedEventArgs e) { }

    protected virtual void OnRefreshing() => Refreshing?.Invoke(this);

    protected virtual void OnRefreshed()
    {
        if (subscribed)
            monitor.Enable(Path);

        Refreshed?.Invoke(this);
    }

    #endregion

    /// <see cref="Region.Method.Public"/>
    #region

    public override void Clear()
    {
        refreshTask.Cancel();
        InternalClear();
    }

    ///

    public void Refresh() => Refresh(Path);

    public void Refresh(string path) => RefreshSync(path);

    ///

    public async Task RefreshAsync() => await RefreshAsync(Path);

    public async Task RefreshAsync(string path) => await refreshTask.Start(path);

    ///

    public virtual void Subscribe()
    {
        return;
        if (!subscribed)
        {
            subscribed = true;

            monitor = DefaultWatcher;
            monitor.Subscribe();

            monitor.ItemChanged += OnItemChanged;
            monitor.ItemCreated += OnItemCreated;
            monitor.ItemDeleted += OnItemDeleted;
            monitor.ItemRenamed += OnItemRenamed;

            if (!IsRefreshing)
                monitor.Enable(Path);
        }
    }

    public virtual void Unsubscribe()
    {
        if (false)
        {
            if (monitor != null)
            {
                monitor.Unsubscribe();

                monitor.ItemChanged -= OnItemChanged;
                monitor.ItemCreated -= OnItemCreated;
                monitor.ItemDeleted -= OnItemDeleted;
                monitor.ItemRenamed -= OnItemRenamed;

                monitor.Disable();
                monitor.Dispose();

                monitor = null;
            }
        }
        subscribed = false;
    }

    #endregion

    /// <see cref="ICommand"/>
    #region

    [field: NonSerialized]
    private ICommand exportCommand;
    [Group(nameof(Category.Import)), Image(Images.Export), Name("Export")]
    [Style(Float = Sides.LeftOrTop)]
    public virtual ICommand ExportCommand => exportCommand ??= new RelayCommand(() =>
    {
        if (StorageDialog.Show(out string destination, $"Export {ItemName.ToLower()}(s)", StorageDialogMode.OpenFolder, null, Path))
        {
            var j = 0;
            foreach (var i in this)
            {
                string source = "";
                if (i is string path)
                {
                    source = path;
                }
                else if (i is File file)
                    source = file.Path;

                if (Try.Do(() => System.IO.File.Copy(source, $@"{destination}\{System.IO.Path.GetFileName(source)}"), e => Log.Write(e)))
                    j++;
            }

            if (j > 0)
            {
                Log.Notify($"Export {ItemName.ToLower()}", new Success($"Exported {ItemName.ToLower()}!"), Notification.DefaultExpiration);
            }
        }
    });

    [field: NonSerialized]
    private ICommand importCommand;
    [Group(nameof(Category.Import)), Image(Images.Import), Name("Import")]
    [Style(Float = Sides.LeftOrTop)]
    public virtual ICommand ImportCommand => importCommand ??= new RelayCommand(() =>
    {
        if (StorageDialog.Show(out string[] paths, $"Import {ItemName.ToLower()}(s)", StorageDialogMode.OpenFile, Filter?.Extensions, Path))
        {
            if (paths?.Length > 0)
            {
                var j = 0;
                foreach (var i in paths)
                {
                    if (Try.Do(() => System.IO.File.Copy(i, $@"{Path}\{System.IO.Path.GetFileName(i)}"), e => Log.Write(e)))
                        j++;
                }

                if (j > 0)
                    Log.Notify($"Import {ItemName.ToLower()}", new Success($"Imported {ItemName.ToLower()}!"), Notification.DefaultExpiration);
            }
        }
    },
    () => true);

    private ICommand refreshCommand;
    [Hide]
    public ICommand RefreshCommand => refreshCommand ??= new RelayCommand(Refresh);

    private ICommand refreshAsyncCommand;
    [Hide]
    public ICommand RefreshAsyncCommand => refreshAsyncCommand ??= new RelayCommand(async () => await RefreshAsync());

    private ICommand renameCommand;
    [Image(Images.Rename), Name("Rename")]
    [Style(Float = Sides.LeftOrTop)]
    public ICommand RenameCommand => renameCommand ??= new RelayCommand<string>(i =>
    {
        var x = new Namable(System.IO.Path.GetFileNameWithoutExtension(i));
        Dialog.ShowObject($"Rename", x, Resource.GetImageUri(Images.Rename), j =>
        {
            if (j == 0)
            {
                var newFilePath = $@"{System.IO.Path.GetDirectoryName(i)}\{x.Name}{System.IO.Path.GetExtension(i)}";
                Try.Do(() => File.Move(i, newFilePath), e => Log.Write(e));
            }
        },
        Buttons.SaveCancel);
    },
    i => Try.Get(() => File.Exists(i)));

    private ICommand deleteCommand;
    [Image(Images.Trash), Name("Delete")]
    [Style(Float = Sides.LeftOrTop)]
    public ICommand DeleteCommand => deleteCommand ??= new RelayCommand<string>(i => Dialog.ShowResult("Delete", new Warning($"Are you sure you want to delete '{i}'?"), j => { if (j == 0) { XItemPath.Recycle(i); } }, Buttons.YesNo),
    i => Try.Get(() => File.Exists(i)));

    #endregion
}