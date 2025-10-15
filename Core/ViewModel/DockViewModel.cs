using Ion;
using Ion.Analysis;
using Ion.Collect;
using Ion.Controls;
using Ion.Input;
using Ion.Reflect;
using Ion.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;

namespace Ion.Core;

/// <see cref="DockViewModel{T}"/>
#region

/// <inheritdoc/>
public abstract record class DockViewModel<T> : DataViewModel<T>, IDockViewModel where T : DockViewModelData
{
    /// <see cref="Region.Event"/>
    #region

    public event ChangeEventHandler<Content> ActiveContentChanged;
    event ChangeEventHandler<Content> IDockViewModel.ActiveContentChanged { add => ActiveContentChanged += value; remove => ActiveContentChanged -= value; }

    public event ChangeEventHandler<Document> ActiveDocumentChanged;
    event ChangeEventHandler<Document> IDockViewModel.ActiveDocumentChanged { add => ActiveDocumentChanged += value; remove => ActiveDocumentChanged -= value; }

    public event ChangeEventHandler<Panel> ActivePanelChanged;
    event ChangeEventHandler<Panel> IDockViewModel.ActivePanelChanged { add => ActivePanelChanged += value; remove => ActivePanelChanged -= value; }

    public event EventHandle<Document> DocumentAdded;
    event EventHandle<Document> IDockViewModel.DocumentAdded { add => DocumentAdded += value; remove => DocumentAdded -= value; }

    public event EventHandle<Document> DocumentRemoved;
    event EventHandle<Document> IDockViewModel.DocumentRemoved { add => DocumentRemoved += value; remove => DocumentRemoved -= value; }

    #endregion

    /// <see cref="Region.Property"/>
    #region

    public Content ActiveContent
    {
        get => Get<Content>();
        set
        {
            var oldValue = Get<Content>();
            if (Set(value))
                OnActiveContentChanged(oldValue, value);
        }
    }

    public Document ActiveDocument
    {
        get => Get<Document>();
        set
        {
            var oldValue = Get<Document>();
            if (Set(value))
                OnActiveDocumentChanged(oldValue, value);
        }
    }

    public Panel ActivePanel
    {
        get => Get<Panel>();
        set
        {
            var oldValue = Get<Panel>();
            if (Set(value))
                OnActivePanelChanged(oldValue, value);
        }
    }

    ///

    new public T Data { get => base.Data.As<T>(); set => base.Data = value; }

    ///

    public DocumentCollection
        Documents
    { get => Get<DocumentCollection>(); private set => Set(value); }

    [Styles.List(Template = Template.List, TargetItem = typeof(bool))]
    [Styles.Check(Template.Check, TargetMember = typeof(Panel), ValuePath = nameof(Panel.IsVisible))]
    public PanelCollection
        Panels
    { get => Get<PanelCollection>(); private set => Set(value); }

    ///

    public virtual int DefaultLayout => 0;

    public virtual IReadOnlyCollection<Uri> DefaultLayouts { get; }

    public object Layout { get => Get<object>(); set => Set(value); }

    public LayoutList Layouts { get => Get<LayoutList>(); set => Set(value); }

    public virtual string LayoutPath => @$"{Appp.Model.DataFolderPath}\Layouts";

    #endregion

    /// <see cref="Region.Constructor"/>

    protected DockViewModel() : base()
    {
        Documents = []; Panels = [];
        Layouts = new LayoutList(LayoutPath);
    }

    /// <see cref="Region.Method"/>
    #region

    private void OnDataSaved(object sender, EventArgs e) => OnDataSaved();

    private void OnDataSaving(object sender, EventArgs e) => OnDataSaving();

    ///

    private void OnDocumentsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                e.OldItems?.ForEach<Document>(i => { OnDocumentAdded(i); i.PropertySet -= OnDocumentPropertySet; i.PropertySet += OnDocumentPropertySet; });
                break;

            case NotifyCollectionChangedAction.Remove:
                e.NewItems?.ForEach<Document>(i => { OnDocumentRemoved(i); i.PropertySet -= OnDocumentPropertySet; });
                break;
        }
    }

    private void OnDocumentPropertySet(IPropertySet sender, PropertySetEventArgs e)
    {
        if (Data.AutoSaveDocuments)
            sender.As<Document>().Save();
    }

    private void OnDocumentRemoving(IListChanged<Document> sender, ListRemovingEventArgs e)
    {
        if (!e.Cancel)
        {
            if (e.OldItem.As<Document>().CanClose)
            {
                OnDocumentRemoving(e.OldItem as Document);
                if (e.OldItem.As<Document>().IsChanged)
                    Dialog.ShowResult("Close", new Warning("Are you sure you want to close?"), i => e.Cancel = i == 1, Buttons.YesNo);
            }
            else e.Cancel = true;
        }
    }

    ///

    protected virtual void OnActiveContentChanged(Content oldValue, Content newValue)
        => ActiveContentChanged?.Invoke(this, new(oldValue, newValue));

    protected virtual void OnActiveDocumentChanged(Document oldValue, Document newValue)
        => ActiveDocumentChanged?.Invoke(this, new(oldValue, newValue));

    protected virtual void OnActivePanelChanged(Panel oldValue, Panel newValue)
        => ActivePanelChanged?.Invoke(this, new(oldValue, newValue));

    ///

    protected virtual void OnDataSaved() { }

    protected virtual void OnDataSaving()
    {
        if (Data.RememberDocuments)
        {
            Data.RememberedDocuments.Clear();
            Documents.ForEach(Data.RememberedDocuments.Add);
        }

        Data.PanelData.Clear();
        foreach (var i in Panels)
        {
            Data.PanelData.Add(i.Name, new(i.GetType()));
            Instance.Virtualize(i, Data.PanelData[i.Name]);
        }
    }

    protected virtual void OnDataSet()
    {
        if (Data.RememberedDocuments.Count > 0)
        {
            Data.RememberedDocuments.ForEach(Documents.Add);
            Data.RememberedDocuments.Clear();
        }

        foreach (var i in Panels)
        {
            if (Data.PanelData.ContainsKey(i.Name))
                Instance.Devirtualize(i, Data.PanelData[i.Name]);
        }
        Data.PanelData.Clear();
    }

    ///

    protected virtual void OnDocumentAdded(Document document) => DocumentAdded?.Invoke(this, new(document));

    protected virtual void OnDocumentRemoved(Document document) => DocumentRemoved?.Invoke(this, new(document));

    protected virtual void OnDocumentRemoving(Document document) { }

    ///

    public override void Load()
    {
        base.Load();
        GetDefaultPanels()?
            .ForEach(Panels.Add);
        GetPanels()?
            .ForEach(Panels.Add);
    }

    public override void Subscribe()
    {
        Data.IfNotNull(i => i.Serialized += OnDataSaved);
        Data.IfNotNull(i => i.Serializing += OnDataSaving);

        Documents.CollectionChanged
            += OnDocumentsChanged;
        Documents.Removing
            += OnDocumentRemoving;

        Layouts.IfNotNull(i =>
        {
            i.Subscribe();
            i.Refresh();
        });
    }

    public override void Unsubscribe()
    {
        Data.IfNotNull(i => i.Serialized -= OnDataSaved);
        Data.IfNotNull(i => i.Serializing -= OnDataSaving);

        Documents.CollectionChanged
            -= OnDocumentsChanged;
        Documents.Removing
            -= OnDocumentRemoving;

        Layouts.IfNotNull(i => i.Unsubscribe());
    }

    ///

    public virtual void OnLoaded(IList<string> arguments) { }

    public virtual void OnReloaded(IList<string> arguments) { }

    public virtual IEnumerable<Panel> GetDefaultPanels()
    {
        yield return new LogPanel(Appp.Model.Log);
        yield return new NotificationPanel(Appp.Model.Notifications);
        yield return new OptionPanel();
        yield return new ThemePanel();
    }

    public virtual IEnumerable<Panel> GetPanels() => default;

    #endregion

    /// <see cref="ICommand"/>
    #region

    public ICommand CloseCommand => Commands[nameof(CloseCommand)]
        ??= new RelayCommand(() => Documents.Remove(ActiveDocument), () => ActiveDocument != null);

    public ICommand CloseAllCommand => Commands[nameof(CloseAllCommand)]
        ??= new RelayCommand(Documents.Clear, () => Documents.Count > 0);

    public ICommand CloseAllButThisCommand => Commands[nameof(CloseAllButThisCommand)]
        ??= new RelayCommand(() =>
        {
            for (var j = Documents.Count - 1; j >= 0; j--)
            {
                if (!ActiveDocument.Equals(Documents[j]))
                    Documents.RemoveAt(j);
            }
        },
        () => Documents.Count > 0 && ActiveDocument != null);

    public ICommand DockDocumentCommand => Commands[nameof(DockDocumentCommand)]
        ??= new RelayCommand(() => { }, () => true);

    public ICommand DockAllDocumentsCommand => Commands[nameof(DockAllDocumentsCommand)]
        ??= new RelayCommand(() => { }, () => true);

    public ICommand DockPanelCommand => Commands[nameof(DockPanelCommand)]
        ??= new RelayCommand(() => { }, () => true);

    public ICommand DockAllPanelsCommand => Commands[nameof(DockAllPanelsCommand)]
        ??= new RelayCommand(() => { }, () => true);

    public ICommand FloatCommand => Commands[nameof(FloatCommand)]
        ??= new RelayCommand(() => { }, () => ActiveContent != null);

    public ICommand FloatAllDocumentsCommand => Commands[nameof(FloatAllDocumentsCommand)]
        ??= new RelayCommand(() => { }, () => true);

    public ICommand FloatAllPanelsCommand => Commands[nameof(FloatAllPanelsCommand)]
        ??= new RelayCommand(() => { }, () => true);

    public ICommand FloatPanelCommand => Commands[nameof(FloatPanelCommand)]
        ??= new RelayCommand(() => { }, () => true);

    public ICommand HideCommand => Commands[nameof(HideCommand)]
        ??= new RelayCommand(() => ActivePanel.IsVisible = false, () => ActivePanel?.IsVisible == true);

    public ICommand HideAllCommand => Commands[nameof(HideAllCommand)]
        ??= new RelayCommand(() => Panels.ForEach(i => i.IsVisible = false), () => Panels.Any(i => i.IsVisible));

    public ICommand MinimizeCommand => Commands[nameof(MinimizeCommand)]
        ??= new RelayCommand(() => ActiveDocument.IsMinimized = true, () => ActiveDocument?.IsMinimized == false);

    public ICommand MinimizeAllCommand => Commands[nameof(MinimizeAllCommand)]
        ??= new RelayCommand(() => Documents.ForEach(i => i.IsMinimized = true), () => Documents.Any(i => !i.IsMinimized));

    public ICommand NewDocumentHorizontalGroupCommand => Commands[nameof(NewDocumentHorizontalGroupCommand)]
        ??= new RelayCommand(() => { });

    public ICommand NewDocumentVerticalGroupCommand => Commands[nameof(NewDocumentVerticalGroupCommand)]
        ??= new RelayCommand(() => { });

    public ICommand NewPanelHorizontalGroupCommand => Commands[nameof(NewPanelHorizontalGroupCommand)]
        ??= new RelayCommand(() => { });

    public ICommand NewPanelVerticalGroupCommand => Commands[nameof(NewPanelVerticalGroupCommand)]
        ??= new RelayCommand(() => { });

    public ICommand PinCommand => Commands[nameof(PinCommand)]
        ??= new RelayCommand(() => { }, () => ActivePanel != null);

    public ICommand PinAllCommand => Commands[nameof(PinAllCommand)]
        ??= new RelayCommand(() => { }, () => true);

    public ICommand RestoreAllCommand => Commands[nameof(RestoreAllCommand)]
        ??= new RelayCommand(() => Documents.ForEach(i => i.IsMinimized = false), () => Documents.Any(i => i.IsMinimized));

    public ICommand SaveAllCommand => Commands[nameof(SaveAllCommand)]
        ??= new RelayCommand(() => Documents.ForEach(i => i.Save()), () => Documents.Count > 0);

    public ICommand ShowAllCommand => Commands[nameof(ShowAllCommand)]
        ??= new RelayCommand(() => Panels.ForEach(i => i.IsVisible = true), () => Panels.Any(i => !i.IsVisible));

    public ICommand UnpinAllCommand => Commands[nameof(UnpinAllCommand)]
        ??= new RelayCommand(() => { }, () => true);

    #endregion

    /// <see cref="IDockViewModel"/>
    #region

    public Result SelectPanel<X>() where X : Panel
    {
        if (Panels.FirstOrDefault<X>() is X panel)
        {
            if (!panel.IsVisible)
                panel.IsVisible = true;

            panel.IsSelected = true;
            return new Success();
        }
        return new Success();
    }

    #endregion

    /// <see cref="IPropertySet"/>

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        switch (e.PropertyName)
        {
            case nameof(Data):
                e.NewValue.If<DockViewModelData>(i =>
                {
                    Set(() => Layout, i.Layout, true);
                    Layout ??= DefaultLayouts.ElementAt(DefaultLayout);
                });
                OnDataSet();
                break;

            case nameof(Layout):
                Data.Layout = e.NewValue;
                break;
        }
    }
}

#endregion

/// <inheritdoc/>
public record class DockViewModel() : DockViewModel<DockViewModelData>() { }