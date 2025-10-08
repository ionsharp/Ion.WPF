using Ion;
using Ion.Collect;
using Ion.Controls;
using Ion.Core;
using Ion.Input;
using Ion.Reflect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Input;

namespace Ion.Reflect;

/// <inheritdoc/>
public record class TemplateModelObject() : TemplateModel()
{
    /// <see cref="Region.Field"/>

    private IComparer Sort;

    /// <see cref="Region.Property"/>
    #region

    public MemberList Members { get; private set; }

    public bool IsLoading { get => Get(false); set => Set(value); }

    public object SelectedTab { get => Get<object>(); set => Set(value); }

    public int SelectedTabIndex { get => Get(-1); set => Set(value); }

    ///★ The view is updated when these properties change!

    public GroupDirection GroupDirection
        => Model.Style.GetValue<Styles.ObjectAttribute, GroupDirection>(i => i.GroupDirection);

    public MemberGroupName GroupName
        => Model.Style.GetValue<Styles.ObjectAttribute, MemberGroupName>(i => i.GroupName);

    public SortDirection SortDirection
        => Model.Style.GetValue<Styles.ObjectAttribute, SortDirection>(i => i.SortDirection);

    public MemberSortName SortName
        => Model.Style.GetValue<Styles.ObjectAttribute, MemberSortName>(i => i.SortName);

    public View View
        => Model.Style.GetValue<Styles.ObjectAttribute, View>(i => i.MemberView);

    public static MemberViewType ViewType
        => MemberViewType.All; //Model.Style.GetValue<Styles.ObjectAttribute, MemberViewType>(i => i.MemberViewType);

    ///Internal

    private CollectionView<Member>[] InternalViews
        => [Default, LeftTopFloat, LeftTopPin, RightBottomFloat, RightBottomPin];

    public CollectionView<Member> Default { get; private set; }

    public CollectionView<Member> LeftTopFloat { get; private set; }

    public CollectionView<Member> LeftTopPin { get; private set; }

    public CollectionView<Member> RightBottomFloat { get; private set; }

    public CollectionView<Member> RightBottomPin { get; private set; }

    ///External

    private CollectionView<Member>[] ExternalViews
        => [LeftTopFloatExternal, LeftTopPinExternal, RightBottomFloatExternal, RightBottomPinExternal];

    public CollectionView<Member> LeftTopFloatExternal { get; private set; }

    public CollectionView<Member> LeftTopPinExternal { get; private set; }

    public CollectionView<Member> RightBottomFloatExternal { get; private set; }

    public CollectionView<Member> RightBottomPinExternal { get; private set; }

    ///Tabs

    public CollectionView<MemberTab> Tabs { get => Model.Get<CollectionView<MemberTab>>(); private set => Model.Set(value); }

    #endregion

    /// <see cref="Region.Method"/>
    #region

    private void Arrange()
    {
        Model.WriteLine(MemberLogType.StyleModel);

        //A) Arrangement initalization
        switch (ViewType)
        {
            case MemberViewType.All:
                break;
            case MemberViewType.Tab:
                //1) Initialize tabs
                Tabs = new([], Sort);

                //2) Initialize tab sort
                Tabs.View.SortDescriptions.Add(new SortDescription(nameof(MemberTab.Index),
                    ListSortDirection.Ascending));
                Tabs.View.SortDescriptions.Add(new SortDescription(nameof(MemberTab.Name),
                    ListSortDirection.Ascending));

                //3) Get and add tabs
                GetTabs(Model).ForEach(Tabs.Source.Add);
                Tabs.View.Refresh();

                //4) Select a tab for first time
                SelectedTabIndex = 0;
                break;
        }

        //B) Arrange members
        foreach (Member i in Members.Cast<Member>())
        {
            var f = i.Style.GetValue(i => i.Float);
            var p = i.Style.GetValue(i => i.Pin);

            if (f != Sides.None)
            {
                switch (f)
                {
                    case Sides.LeftOrTop:
                        if (ViewType == MemberViewType.Tab && i.Tab is null)
                            LeftTopFloatExternal.Source.Add(i);

                        else LeftTopFloat.Source.Add(i);
                        break;
                    case Sides.RightOrBottom:
                        if (ViewType == MemberViewType.Tab && i.Tab is null)
                            RightBottomFloatExternal.Source.Add(i);

                        else RightBottomFloat.Source.Add(i);
                        break;
                }
            }
            else if (p != Sides.None)
            {
                switch (p)
                {
                    case Sides.LeftOrTop:
                        if (ViewType == MemberViewType.Tab && i.Tab is null)
                            LeftTopPinExternal.Source.Add(i);

                        else LeftTopPin.Source.Add(i);
                        break;
                    case Sides.RightOrBottom:
                        if (ViewType == MemberViewType.Tab && i.Tab is null)
                            RightBottomPinExternal.Source.Add(i);

                        else RightBottomPin.Source.Add(i);
                        break;
                }
            }
            else Default.Source.Add(i);
        }

        //C) Group
        EachViewGroup();

        //D) Refresh
        EachViewRefresh();
    }

    private void Unarrange()
    {
        Model.WriteLine(MemberLogType.StyleModel);

        //B) Unarrange members
        EachView(i => i.Source.Clear());

        //A) Arrangement deinitalization
        Tabs?.Source.Clear();
        Tabs = null;

        SelectedTabIndex = -1;
    }

    /// 

    private void EachView(Action<CollectionView<Member>> i)
        => ((CollectionView<Member>[])[.. ExternalViews, .. InternalViews]).ForEach(j => i(j));

    private void EachViewGroup() => EachView(i =>
    {
        if (i.View is not null)
        {
            i.View.GroupDescriptions.Clear();
            if (ReferenceEquals(i, Default.View))
            {
                if (ViewType == MemberViewType.Tab)
                {
                    if (SelectedTab?.As<MemberTab>().Groups == false)
                        return;
                }
                //if (GroupName == GroupName.None) return; Do we want this?
            }

            if (GroupName != MemberGroupName.None)
                i.View.GroupDescriptions.Add(new PropertyGroupDescription() { Converter = ObjectGroupConverterSelector.Default.SelectConverter(GroupName) });
        }
    });

    private void EachViewRefresh() => EachView(i => i.View.Refresh());

    /// 

    private static MemberTab GetTab(Enum e, TabStyleAttribute single, TabStyleAttribute all)
    {
        var image
            = single?.Image is not null && Resource.GetImage(single.Image, single.ImageSource) is string x
            ? x
            : all?.Image is not null && Resource.GetImage(all.Image, all.ImageSource) is string y
            ? y
            : e.GetAttribute<ImageAttribute>().IfNotNullGet(z => Resource.GetImage(z.Name, z.NameAssembly));

        return new
        (
            e,
            single?.Name ?? all?.Name ?? Instance.GetName(e) ?? $"{e}",
            single?.Description ?? all?.Description ?? Instance.GetDescription(e),
            image,
            single?.Group ?? all?.Group ?? false,
            single?.Fill ?? all?.Fill ?? Fill.None
        );
    }

    private static IEnumerable<MemberTab> GetTabs(IMemberStylable model)
    {
        var result = new List<MemberTab>();

        var t = model.ValueType;
        if (t == typeof(object) || t == typeof(string) || t.IsArray || t.IsEnum || t.IsInterface || t.IsPrimitive)
            return result;

        IEnumerable<Type> types
            = t.IsClass
            ? [t, .. t.GetBaseTypes()]
            : t.IsValueType
            ? [t]
            : [];

        //★ Higher level tabs override lower level ones!
        types = types.Reverse();

        //1) Enumerate each inherited type
        foreach (var type in types)
        {
            var tabs = type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(i =>
            {
                if (i.GetAttribute<TabViewAttribute>() is TabViewAttribute j && j.View.HasFlag(model.View))
                    return true;

                return false;
            });

            if (tabs is not null)
            {
                //Style all
                var a = tabs.GetAttribute<TabStyleAttribute>();
                foreach (Enum tab in tabs.GetEnumValues())
                {
                    //Style single
                    var b = tab.GetAttribute<TabStyleAttribute>();
                    var c = GetTab(tab, b, a);

                    var index = result.IndexOf(i => $"{i.Source}" == $"{tab}");
                    if (index > -1)
                        result.RemoveAt(index);

                    result.Add(c);
                }
            }
        }
        return result;
    }

    /// 

    private void OnPropertySet(IPropertySet sender, PropertySetEventArgs e)
    {
        Members.IfNotNull(i => i.ForEach<Member>(j => j.OnSetStyleTriggers(e.PropertyName)));
        //Members.FirstOrDefault(i => i.Name == e.PropertyName).If(i => i.Value = e.NewValue);
    }

    private void OnStyleChanged(object sender, string key, object value)
    {
        switch (key)
        {
            case nameof(Styles.ObjectAttribute.GroupName):
                EachViewGroup();
                goto default;

            case nameof(Styles.ObjectAttribute.GroupDirection):
            case nameof(Styles.ObjectAttribute.SortDirection):
            case nameof(Styles.ObjectAttribute.SortName):
            default: EachViewRefresh(); break;


            case nameof(Styles.ObjectAttribute.MemberView):
            case nameof(Styles.ObjectAttribute.MemberViewType):
                Unarrange(); Arrange();
                goto default;
        }
    }

    #endregion

    /// <see cref="ICommand"/>

    public ICommand SelectTabCommand => Commands[nameof(SelectTabCommand)]
        ??= new RelayCommand<MemberTab>(i => SelectedTab = i, i => i is not null);

    /// <see cref="ITemplateModel"/>

    /// <inheritdoc/>
    public override void Reset((object OldValue, object NewValue) i)
    {
        Model.WriteLine(MemberLogType.StyleModel);

        base.Reset(i);
        Members.ForEach(j => j.Reset(i.NewValue));
    }

    /// <inheritdoc/>
    public override void Set(IMemberStylable i)
    {
        base.Set(i);
        Model.WriteLine(MemberLogType.StyleModel);

        IsLoading = true;

        Members = new MemberList();
        Members.Load(Model);

        IsLoading = false;

        Sort = new MemberSorter();

        //1) Initialize internal views
        Default = new([], Sort);
        LeftTopFloat = new([], Sort); LeftTopPin = new([], Sort);
        RightBottomFloat = new([], Sort); RightBottomPin = new([], Sort);

        //2) Initialize external views
        LeftTopFloatExternal = new([], Sort); RightBottomFloatExternal = new([], Sort);
        RightBottomPinExternal = new([], Sort); LeftTopPinExternal = new([], Sort);

        //3) Initialize sort
        EachView(j => j.View.CustomSort = Sort);

        //4) Arrange
        Arrange();
    }

    /// <inheritdoc/>
    public override void Unset(IMemberStylable i)
    {
        Model.WriteLine(MemberLogType.StyleModel);

        Members.Unload(this);
        Members = null;

        //1) Unarrange
        Unarrange();

        //★ Order after this point doesn't matter

        //2) Deinitialize sort
        Sort = null;

        //3) Deinitialize external
        LeftTopFloatExternal = null; LeftTopPinExternal = null;
        RightBottomFloatExternal = null; RightBottomPinExternal = null;

        //4) Deinitialize internal
        Default = null;
        LeftTopFloat = null; LeftTopPin = null;
        RightBottomFloat = null; RightBottomPin = null;

        //★ Order here does!

        base.Unset(i);
    }

    /// <see cref="ISubscribe"/>

    /// <inheritdoc/>
    public override void Subscribe()
    {
        base.Subscribe();
        Model.WriteLine(MemberLogType.StyleModel);

        Model.Style.EntryChanged -= OnStyleChanged;
        Model.Style.EntryChanged += OnStyleChanged;
        Model.Value.If<IPropertySet>(j => { j.PropertySet -= OnPropertySet; j.PropertySet += OnPropertySet; });
    }

    /// <inheritdoc/>
    public override void Unsubscribe()
    {
        base.Unsubscribe();
        Model.WriteLine(MemberLogType.StyleModel);

        Model.Style.EntryChanged -= OnStyleChanged;
        Model.Value.If<IPropertySet>(j => j.PropertySet -= OnPropertySet);
    }
}