using Ion.Analysis;
using Ion.Collect;
using Ion.Controls;
using Ion.Input;
using Ion.Reflect;
using Ion.Storage;
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace Ion.Core;

/// <inheritdoc/>
public class AppLinkList(string path) : ItemList(path, new Storage.Filter(ItemType.File, ExternalLink, InternalLink))
{
    /// <see cref="Region.Event"/>
    #region

    public event EventHandler<EventArgs<IAppLink>> Added;

    public event EventHandler<EventArgs<IAppLink>> Removed;

    ///

    public event EventHandler<EventArgs<IAppLink>> Disabled;

    public event EventHandler<EventArgs<IAppLink>> Enabled;

    #endregion

    /// <see cref="Region.Field"/>
    #region

    /// <summary>Extensions defined in an external assembly (compiled/not serialized).</summary>
    public const string ExternalLink = "dll";

    /// <summary>Extensions defined internally (uncompiled/serialized).</summary>
    public const string InternalLink = "ext";

    #endregion

    /// <see cref="Region.Property"/>
    #region

    public override string ItemName => "Link";

    public ListObservable<IAppLink> Instance { get => this.Get<ListObservable<IAppLink>>([]); set => this.Set(value); }

    new public IAppLink this[string filePath] => Instance.FirstOrDefault(i => i.FilePath == filePath);

    #endregion

    /// <see cref="Region.Method.Private"/>
    #region

    private static IAppLink Create(AssemblyContext assemblyContext)
    {
        var result = Find(assemblyContext.Assembly).Create<IAppLink>();
        result.AssemblyContext = assemblyContext;
        return result;
    }

    /// <summary>Search for and return extension type defined in specified assembly.</summary>
    private static Type Find(Assembly Assembly)
    {
        foreach (var i in Assembly.GetTypes())
        {
            if (i.Implements<IAppLink>())
                return i;
        }
        throw new TypeLoadException();
    }

    #endregion

    /// <see cref="Region.Method.Protected"/>
    #region

    protected override void OnAdded(ListAddedEventArgs e)
    {
        base.OnAdded(e);
        return;
        e.NewItem.If<Item>(item =>
        {
            IAppLink link = null;
            Result result = null;

            var fileExtension = System.IO.Path.GetExtension(item.Path)[1..].ToLower();
            result = fileExtension switch
            {
                ExternalLink => Try.Do(() =>
                {
                    var guid = Guid.NewGuid();

                    var assemblyContext = new AssemblyContext(guid, Assembly.Load(System.IO.File.ReadAllBytes(item.Path)), AppDomain.CreateDomain(guid.ToString()));
                    link = Create(assemblyContext);
                }),
                InternalLink => FileSerializer.Deserialize(item.Path, out link),
                _ => new Error(new NotSupportedException())
            };

            if (link?.TargetType is not null && !Appp.Model.GetType().Implements(link.TargetType))
                result = new NotSupportedException("Not supported by application.");

            if (!result)
            {
                Remove(item);
                Log.Write(result);
            }
            else
            {
                link.FilePath = item.Path;
                link.Disabled += OnDisabled;
                link.Enabled += OnEnabled;

                Instance.Add(link);
                OnAdded(link);
            }
        });
    }

    protected override void OnRemoved(ListRemovedEventArgs e)
    {
        base.OnRemoved(e);
        e.OldItem.If<Item>(item =>
        {
            if (this[item.Path] is IAppLink link)
            {
                if (link.IsEnabled)
                    link.IsEnabled = false;

                link.Enabled -= OnEnabled;

                if (link.AssemblyContext?.AppDomain is not null)
                    AppDomain.Unload(link.AssemblyContext.AppDomain);

                Instance.Remove(link);
                OnRemoved(link);
            }
        });
    }

    ///

    protected virtual void OnDisabled(object sender, EventArgs e)
    {
        var link = sender as IAppLink;
        Log.Write(new Message($"Disabled link '{link.Name}'"));
        Disabled?.Invoke(this, new EventArgs<IAppLink>(link));
    }

    protected virtual void OnEnabled(object sender, EventArgs e)
    {
        var link = sender as IAppLink;
        Log.Write(new Success($"Enabled link '{link.Name}'"));
        Enabled?.Invoke(this, new EventArgs<IAppLink>(link));
    }

    ///

    protected virtual void OnAdded(IAppLink e)
    {
        Log.Write(new Success($"Added link '{e.Name}'"));
        Added?.Invoke(this, new EventArgs<IAppLink>(e));
    }

    protected virtual void OnRemoved(IAppLink e)
    {
        Log.Write(new Message($"Removed link '{e.Name}'"));
        Removed?.Invoke(this, new EventArgs<IAppLink>(e));
    }

    #endregion

    /// <see cref="ICommand"/>
    #region

    private ICommand resetCommand;
    public virtual ICommand ResetCommand => resetCommand ??= new RelayCommand(() =>
    {
        var extensions = new ListObservable<IAppLink>(XAssembly.Get(AssemblyData.Name).GetDerivedTypes<IAppLink>().Select(i => i.Create<IAppLink>()));

        var result = new ReadOnlySelectionForm(extensions);
        Dialog.ShowObject("Reset extension", result, Resource.GetImageUri(Images.Reset), i =>
        {
            if (i == 0)
            {
                if (result.SelectedIndex > -1)
                {
                    var extension = extensions[result.SelectedIndex].GetType().Create<IAppLink>();
                    var extensionPath = $@"{Path}\{extension.Name}.ext";

                    if (System.IO.File.Exists(extensionPath))
                        Try.Do(() => System.IO.File.Delete(extensionPath));

                    FileSerializer.Serialize(extensionPath, extension);
                    Log.Write(new Message($"Reset extension '{extension.Name}'"));
                }
            }
        },
        Controls.Buttons.SaveCancel);
    });

    #endregion
}