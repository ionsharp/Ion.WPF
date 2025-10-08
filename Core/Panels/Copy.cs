using Ion.Collect;
using Ion.Controls;
using Ion.Input;
using Ion.Reflect;
using Ion.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ion.Core;

public record class CopyPanelItem() : Model()
{
    private readonly object[] Values;

    [Filter(Filter.Sort)]
    public int Count => Values?.Length ?? 0;

    [Filter(Filter.Sort)]
    public long Size => Try.Get(() => Instance.SizeOf(Values));

    [Filter(Filter.Search | Filter.Sort)]
    public Type Type { get => Get<Type>(); set => Set(value); }

    public CopyPanelItem(Type type, object[] values) : this()
    {
        Type = type;
        Values = values;
    }

    public object[] ToArray() => Values;
}

[Description("Sync file contents of multiple folders.")]
[Image(Images.Copy)]
[Name("Copy")]
[Serializable]
public record class CopyPanel : DataGridPanel<CopyTask>
{
    private enum Group { Disable, Enable, Pause, Remove, Task, Warnings }

    /// <see cref="Region.Field"/>

    public static readonly new ResourceKey Template = new();

    /// <see cref="Region.Property"/>
    #region

    /// <see cref="DataGridPanel"/>

    public override bool CanAddRows => false;

    public override bool CanDeleteRows => false;

    public override bool CanResizeRows => false;

    [Description("The exit method to perform when all tasks are complete.")]
    [Style(Float = Sides.LeftOrTop,
        View = Ion.View.Option)]
    public AppExitMethod AutoExit { get => Get(AppExitMethod.None); set => Set(value); }

    [Group(Group.Task)]
    [Styles.ListAttribute(ItemTypes = [typeof(string)],
        View = Ion.View.Option)]
    public ListObservableOfString Groups { get => Get(new ListObservableOfString()); set => Set(value); }

    [Group(nameof(Group.Warnings)), Name("Before disabling task")]
    [Style(View = Ion.View.Option)]
    public bool ShowWarningBeforeDisablingTask { get => Get(false); set => Set(value); }

    [Group(nameof(Group.Warnings)), Name("Before enabling task")]
    [Style(View = Ion.View.Option)]
    public bool ShowWarningBeforeEnablingTask { get => Get(true); set => Set(value); }

    [Hide]
    public ListObservable<CopyTask> Tasks { get => Get<ListObservable<CopyTask>>(); set => Set(value); }

    #endregion

    /// <see cref="Region.Constructor"/>

    public CopyPanel() : base() => Tasks = [];

    /// <see cref="Region.Method"/>
    #region

    private void OnTaskChanged(IPropertySet sender, PropertySetEventArgs e)
    {
        if (e.PropertyName == nameof(CopyTask.IsActive))
        {
            if (sender is CopyTask i)
                IsActive = i.IsActive;
        }
    }

    protected override void OnItemAdded(object input)
    {
        base.OnItemRemoved(input);
        if (input is CopyTask i)
        {
            i.Unsubscribe(); i.Subscribe();
            i.PropertySet -= OnTaskChanged; i.PropertySet += OnTaskChanged;
        }
    }

    protected override void OnItemRemoved(object input)
    {
        base.OnItemRemoved(input);
        if (input is CopyTask i)
        {
            i.Unsubscribe();
            i.PropertySet -= OnTaskChanged;
        }
    }

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        switch (e.PropertyName)
        {
            case nameof(SelectedItems):
                Get<PropertyPanel>().IfNotNull(i => i.Source = SelectedItems);
                break;

            case nameof(Tasks):
                Items = Tasks; break;
        }
    }

    ///

    public override void Subscribe()
    {
        foreach (var i in Tasks)
        {
            i.Unsubscribe(); i.Subscribe();
            i.PropertySet -= OnTaskChanged; i.PropertySet += OnTaskChanged;
        }
    }

    public override void Unsubscribe()
    {
        foreach (var i in Tasks)
        {
            i.Unsubscribe();
            i.PropertySet -= OnTaskChanged;
        }
    }

    #endregion

    public static ListObservable<CopyPanelItem> Data { get; private set; } = [];

    public static bool Contains(Type type) => type is not null && Data.Any(i => i.Type == type);

    public static T FirstOrDefault<T>() => FirstOrDefault(typeof(T)).As<T>();

    public static object FirstOrDefault(Type type) => GetValues(type).FirstOrDefault();

    public static IEnumerable<object> GetValues(Type type) => Contains(type) ? Data.Where(i => i.Type.Inherits(type)).SelectMany(i => i.ToArray()) : [];

    public static void SetValue(object i) { }

    /// <see cref="ICommand"/>
    #region

    private ICommand disableCommand;
    [Group(nameof(Group.Disable))]
    [Image(Images.Stop)]
    [Name("Disable")]
    [Style(Index = 0,
        View = Ion.View.HeaderOption)]
    public ICommand DisableCommand => disableCommand ??= new RelayCommand(() => { });

    private ICommand disableAllCommand;
    [Group(nameof(Group.Disable))]
    [Image(Images.StopAll)]
    [Name("DisableAll")]
    [Style(Index = 1,
        View = Ion.View.HeaderOption)]
    public ICommand DisableAllCommand => disableAllCommand ??= new RelayCommand(() => Tasks.ForEach(j => j.IsEnabled = false), () => Tasks.Count > 0);

    private ICommand enableCommand;
    [Group(nameof(Group.Enable))]
    [Image(Images.Play)]
    [Name("Enable")]
    [Style(Index = 0,
        View = Ion.View.HeaderOption)]
    public ICommand EnableCommand => enableCommand ??= new RelayCommand(() => { });

    private ICommand enableAllCommand;
    [Group(nameof(Group.Enable))]
    [Image(Images.PlayAll)]
    [Name("EnableAll")]
    [Style(Index = 1,
        View = Ion.View.HeaderOption)]
    public ICommand EnableAllCommand => enableAllCommand ??= new RelayCommand(() => Tasks.ForEach(j => j.IsEnabled = true), () => Tasks.Count > 0);

    #endregion
}