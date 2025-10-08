using Ion.Analysis;
using Ion.Core;
using System;
using System.IO;
using System.Windows;

namespace Ion.Storage;

public record class Monitor : Model, ISubscribe
{
    public const NotifyFilters DefaultFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size;

    public event FileSystemEventHandler ItemChanged;

    public event FileSystemEventHandler ItemCreated;

    public event FileSystemEventHandler ItemDeleted;

    public event RenamedEventHandler ItemRenamed;

    ///

    public event Analysis.ErrorEventHandler Failed;

    ///

    private readonly FileSystemWatcher watcher;

    ///

    public NotifyFilters Filter
    {
        get => watcher.NotifyFilter;
        set => watcher.NotifyFilter = value;
    }

    public bool IncludeChildren
    {
        get => watcher.IncludeSubdirectories;
        set => watcher.IncludeSubdirectories = value;
    }

    public string Path
    {
        get => watcher.Path;
        private set => watcher.Path = value;
    }

    ///

    public Monitor() : base() => watcher = new FileSystemWatcher();

    public Monitor(NotifyFilters input) : this() => Filter = input;

    ///

    protected static void Handle(FileSystemEventArgs e, Action handler, Action invoke)
    {
        var dispatcher = Application.Current?.Dispatcher;
        var checkAccess = dispatcher?.CheckAccess();

        if (checkAccess == false && handler is not null)
        {
            dispatcher?.Invoke(handler);
        }
        else if (checkAccess == true)
            invoke?.Invoke();
    }

    ///

    protected virtual void OnChanged(object sender, FileSystemEventArgs e)
    {
        Handle(e, () => OnChanged(sender, e), () => ItemChanged?.Invoke(this, e));
    }

    protected virtual void OnCreated(object sender, FileSystemEventArgs e)
    {
        Handle(e, () => OnCreated(sender, e), () => ItemCreated?.Invoke(this, e));
    }

    protected virtual void OnDeleted(object sender, FileSystemEventArgs e)
    {
        Handle(e, () => OnDeleted(sender, e), () => ItemDeleted?.Invoke(this, e));
    }

    protected virtual void OnRenamed(object sender, RenamedEventArgs e)
    {
        Handle(e, () => OnRenamed(sender, e), () => ItemRenamed?.Invoke(this, e));
    }

    ///

    protected virtual void OnFailed(Error result) => Failed?.Invoke(this, new(result));

    ///

    public void Dispose() => watcher.Dispose();

    ///

    public virtual void Disable() => watcher.EnableRaisingEvents = false;

    public virtual Result Enable(string path) => Try.Do(() => { Path = path; watcher.EnableRaisingEvents = true; }, e => Log.Write(e));

    ///

    public virtual void Subscribe()
    {
        watcher.Changed += OnChanged;
        watcher.Created += OnCreated;
        watcher.Deleted += OnDeleted;
        watcher.Renamed += OnRenamed;
    }

    public virtual void Unsubscribe()
    {
        watcher.Changed -= OnChanged;
        watcher.Created -= OnCreated;
        watcher.Deleted -= OnDeleted;
        watcher.Renamed -= OnRenamed;
    }
}