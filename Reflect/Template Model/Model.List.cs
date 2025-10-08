using Ion;
using Ion.Analysis;
using Ion.Collect;
using Ion.Core;
using Ion.Input;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Input;

namespace Ion.Reflect;

/// <summary>A model for different forms of lists using a managed instance of <see cref="INotifyCollectionChanged"/>.</summary>
/// <remarks><see cref="Array"/>, <see cref="IList"/>, <see cref="INotifyCollectionChanged"/>, <see cref="ListCollectionView"/></remarks>
//[Unsolved]
public record class TemplateModelList() : TemplateModel()
{
    public const string MessageNotIList
        = "Template expects '<b>" + $"{nameof(IList)}" + "</b>'. Value of type '{0}' does not implement it.";

    public const string MessageNotINotifyCollectionChanged
        = "Value of type '{0}' does not implement '<b>" + $"{nameof(INotifyCollectionChanged)}" + "</b>'. Updates to collection are ignored.";

    public const string MessageNoItemPreset
        = "The list can be changed, but an add method isn't specified.";

    public const string MessageNull
        = "The list is <b>null</b>.";

    /// <see cref="Region.Property"/>
    #region

    PropertyBinding indexBinding;

    PropertyBinding itemBinding;

    //public Type CreateHandler => Model.Style.GetValue<Styles.ListAttribute, Type>(i => i.CreateHandler);

    /// <inheritdoc cref="Styles.ListAttribute.ItemAction"/>
    public ItemAction ItemAction => Model.Style.GetValue<Styles.ListAttribute, ItemAction>(i => i.ItemAction);

    /// <inheritdoc cref="Styles.ListAttribute.ItemAddHandle"/>
    public Type ItemAddHandle => Model.Style.GetValue<Styles.ListAttribute, Type>(i => i.ItemAddHandle);

    /// <inheritdoc cref="Styles.ListAttribute.ItemAddMethod"/>
    public string ItemAddMethod => Model.Style.GetValue<Styles.ListAttribute, string>(i => i.ItemAddMethod);

    /// <inheritdoc cref="Styles.ListAttribute.ItemCloneHandle"/>
    public Type ItemCloneHandle => Model.Style.GetValue<Styles.ListAttribute, Type>(i => i.ItemCloneHandle);

    /// <inheritdoc cref="Styles.ListAttribute.SelectedItemProperty"/>
    public string ItemSelectedProperty => Model.Style.GetValue<Styles.ListAttribute, string>(i => i.SelectedItemProperty);

    /// <inheritdoc cref="Styles.ListAttribute.SelectedIndexProperty"/>
    public string IndexSelectedProperty => Model.Style.GetValue<Styles.ListAttribute, string>(i => i.SelectedIndexProperty);

    /// <inheritdoc cref="Styles.ListAttribute.ItemTypes"/>
    public ListObservable<Type> ItemTypes { get; private set; }

    /// <inheritdoc cref="Styles.ListAttribute.ItemValues"/>
    public ListObservable<object> ItemValues { get; private set; }

    /// <inheritdoc cref="Styles.ListAttribute.ItemTypeFind"/>
    public bool ItemTypeFind => Model.Style.GetValue<Styles.ListAttribute, bool>(i => i.ItemTypeFind);

    public IList List { get => Get<IList>(); private set => Set(value); }

    public object ListView { get => Get<ListCollectionView>(); private set => Set(value); }

    public int SelectedIndex { get => Get(-1); set => Set(value); }

    public object SelectedItem { get => Get<object>(); set => Set(value); }

    #endregion

    /// <see cref="Region.Method"/>
    #region

    private Type[] GetItemTypes()
        => XAssembly.GetTypes(i => !i.IsAbstract && !i.IsInterface && i.IsPublic && (i.Implements(Model.ValueType) || i.Inherits(Model.ValueType))).ToArray();

    private object[] GetItemValues()
    {
        return Try.Get(() =>
        {
            var propertyName = Model.Style.GetValue<Styles.ListAttribute, string>(i => i.ItemValues);
            return Instance.GetPropertyValue(Model.Parent.Value, propertyName) as object[];
        });
    }

    ///

    private object Create(Type i) => (ItemAddHandle?.Create<ICreateFromType>() ?? new CreateFromType()).Create(i);

    private object Create(object i)
    {
        Model.WriteLine(MemberLogType.StyleModel, $"ItemAction = {ItemAction}, Style.ItemAction = {Model.Style.GetValue<Styles.ListAttribute, ItemAction>(i => i.ItemAction)}, Style.ItemAction? = {Model.Style.GetValue<Styles.ListAttribute, object>(i => i.ItemAction)}");
        if (i is null)
            throw new Exception("An object can't be created because a creation method isn't specified.");

        Model.WriteLine(MemberLogType.StyleModel, $"i = '{Instance.AsType(i).FullName}'");

        object result = null;
        if (i is not null)
        {
            /// From <see cref="MethodInfo"/>
            if (i is MethodInfo method)
            {
                Model.WriteLine(MemberLogType.StyleModel, $"Creating from '{nameof(MethodInfo)}' ({method.Name})...");
                result = method.Invoke(Model.Parent, null);
            }

            /// From <see cref="System.Type"/>
            else if (i is Type type)
            {
                Model.WriteLine(MemberLogType.StyleModel, $"Creating from '{nameof(Type)}' ({Instance.AsType(i).FullName})...");
                result = Create(type);
            }

            /// From <see cref="object"/>
            else
            {
                Model.WriteLine(MemberLogType.StyleModel, $"Creating from '{nameof(Object)}' ({Instance.AsType(i).FullName})...");
                result = (ItemCloneHandle?.Create<ICreateFromObject>() ?? new CreateFromObject()).Create(i);
            }
        }
        /// From <see cref="MethodInfo"/>
        else if (ItemAddMethod is not null)
        {
            Model.WriteLine(MemberLogType.StyleModel, $"Creating from '{nameof(MethodInfo)}' ({ItemAddMethod})...");
            result = Model.Parent.ValueType.GetMethod(ItemAddMethod).Invoke(Model.Parent, null);
        }
        /// From <see cref="System.Type"/>
        else
        {
            Model.WriteLine(MemberLogType.StyleModel, $"Creating from '{nameof(Type)}' ({ItemTypes.FirstOrDefault<Type>()?.FullName ?? "null"})...");
            result = Create(ItemTypes.FirstOrDefault());
        }

        if (result is null)
        {
            Model.WriteLine(MemberLogType.StyleModel, $"The object couldn't be created.");
            throw new Exception("An object couldn't be created.");
        }

        return result;
    }

    ///

    /// <summary>Remove all items.</summary>
    private void Clear() => Try.Do(List.Clear, e => Dialog.ShowResult(nameof(Clear), e));

    /// <summary>Clone the selected items.</summary>
    private void Duplicate()
    => Try.Do(() =>
    {
        var clone = (ItemCloneHandle?.Create<ICreateFromObject>() ?? new CreateFromObject())
            .Create(SelectedItem);

        List.InsertBelow(SelectedIndex, clone);
    },
    e => Dialog.ShowResult(nameof(Duplicate), e));

    /// <summary>Copy the selected items.</summary>
    private void Copy()
    => Try.Do(() => Appp.Cache.Add([SelectedItem]),
    e => Dialog.ShowResult(nameof(Copy), e));

    /// <summary>(Create and) insert an item above the selected one.</summary>
    private void InsertAbove(object i)
    => Try.Do(() => Create(i).IfNotNull(j => List.InsertAbove(SelectedIndex, j)),
    e => Dialog.ShowResult(nameof(InsertAbove), e));

    /// <summary>(Create and) insert an item below the selected one.</summary>
    private void InsertBelow(object i)
    => Try.Do(() => Create(i).IfNotNull(j => List.InsertBelow(SelectedIndex, j)),
    e => Dialog.ShowResult(nameof(InsertBelow), e));

    /// <summary>Move the selected items down.</summary>
    private void MoveDown()
    => Try.Do(() => List.MoveDown(SelectedIndex, true),
    e => Dialog.ShowResult(nameof(MoveDown), e));

    /// <summary>Move the selected items up.</summary>
    private void MoveUp()
    => Try.Do(() => List.MoveUp(SelectedIndex, true),
    e => Dialog.ShowResult(nameof(MoveUp), e));

    /// <summary>Paste the items that were copied.</summary>
    private void Paste()
    => Try.Do(() =>
    {
        var from = ItemCloneHandle?.Create<ICreateFromObject>() ?? new CreateFromObject();

        var values = ItemTypes.Select(i => Appp.Cache.GetValues(i)).SelectMany(i => i);
        foreach (var i in values)
            XObject.If<object>(from.Create(i), j => List.Add(j));
    },
    e => Dialog.ShowResult(nameof(Paste), e));

    /// <summary>Remove the selected items.</summary>
    private void Remove()
    => Try.Do(() => List.RemoveAt(SelectedIndex),
    e => Dialog.ShowResult(nameof(Remove), e));

    #endregion

    /// <see cref="ICommand"/>
    #region

    /// <inheritdoc cref="Clear"/>
    public ICommand ClearCommand
        => Commands[nameof(ClearCommand)]
        ??= new RelayCommand(Clear,
        () => ItemAction.HasFlag(ItemAction.Clear)
        && List?.Count > 0);

    /// <inheritdoc cref="Duplicate"/>
    public ICommand CloneCommand
        => Commands[nameof(CloneCommand)]
        ??= new RelayCommand(Duplicate,
        () => ItemAction.HasFlag(ItemAction.Clone)
        && List is not null && SelectedItem is not null);

    /// <inheritdoc cref="Copy"/>
    public ICommand CopyCommand
        => Commands[nameof(CopyCommand)]
        ??= new RelayCommand(Copy,
        () => ItemAction.HasFlag(ItemAction.Copy)
        && List is not null && SelectedItem is not null);

    /// <inheritdoc cref="InsertAbove"/>
    public ICommand InsertAboveCommand
        => Commands[nameof(InsertAboveCommand)]
        ??= new RelayCommand<object>(InsertAbove,
        i => List is not null);

    /// <inheritdoc cref="InsertBelow"/>
    public ICommand InsertBelowCommand
        => Commands[nameof(InsertBelowCommand)]
        ??= new RelayCommand<object>(InsertBelow,
        i => List is not null);

    /// <inheritdoc cref="MoveDown"/>
    public ICommand MoveDownCommand
        => Commands[nameof(MoveDownCommand)]
        ??= new RelayCommand(MoveDown,
        () => ItemAction.HasFlag(ItemAction.Move)
        && List is not null && SelectedItem is not null);

    /// <inheritdoc cref="MoveUp"/>
    public ICommand MoveUpCommand
        => Commands[nameof(MoveUpCommand)]
        ??= new RelayCommand(MoveUp,
        () => ItemAction.HasFlag(ItemAction.Move)
        && List is not null && SelectedItem is not null);

    /// <inheritdoc cref="Paste"/>
    public ICommand PasteCommand
        => Commands[nameof(PasteCommand)]
        ??= new RelayCommand(Paste,
        () => ItemAction.HasFlag(ItemAction.Paste)
        && List is not null);

    /// <inheritdoc cref="Remove"/>
    public ICommand RemoveCommand
        => Commands[nameof(RemoveCommand)]
        ??= new RelayCommand(Remove,
        () => ItemAction.HasFlag(ItemAction.Remove)
        && List is not null && SelectedItem is not null);

    #endregion

    /// <see cref="ISubscribe"/>

    public override void Subscribe()
    {
        base.Subscribe();
        return;
        /// Does the instance this member came from support binding?
        var targetSource = Model.Parent.Value as IPropertySet;
        if (targetSource is not null)
        {
            /// Does the member have sibling properties that want to listen?
            if (IndexSelectedProperty is not null)
            {
                indexBinding = new(BindMode.TwoWay, nameof(SelectedIndex), this, IndexSelectedProperty, targetSource);
                BindingManager.Set(indexBinding);
            }
            if (ItemSelectedProperty is not null)
            {
                itemBinding = new(BindMode.TwoWay, nameof(SelectedItem), this, ItemSelectedProperty, targetSource);
                BindingManager.Set(itemBinding);
            }
        }
    }

    public override void Unsubscribe()
    {
        base.Unsubscribe();
        BindingManager.Unset(indexBinding);
        BindingManager.Unset(itemBinding);
    }

    /// <see cref="TemplateModel"/>

    public override void Reset((object OldValue, object NewValue) source)
    {
        base.Reset(source);
    }

    public override void Set(IMemberStylable model)
    {
        base.Set(model);

        List = model.Value as IList;
        List.IfNotNull(i => ListView = new ListCollectionView(i));

        if (model.Value is null)
        {
            model.WriteLine(MemberLogType.StyleModel, "The list is null.");
            Messages.Add(new Warning(MessageNull));
        }
        if (model.Value is not null)
        {
            if (model.Value is not IList)
            {
                model.WriteLine(MemberLogType.StyleModel, $"The list is not '{nameof(IList)}'.");
                Messages.Add(new Warning(MessageNotIList.F(model.ValueType.FullName)));
            }
            if (model.Value is not INotifyCollectionChanged)
            {
                model.WriteLine(MemberLogType.StyleModel, $"The list is not '{nameof(INotifyCollectionChanged)}'.");
                Messages.Add(new Warning(MessageNotINotifyCollectionChanged.F(model.ValueType.FullName)));
            }
        }

        ItemTypes = [];
        Model.Style.GetValue<Styles.ListAttribute, Type[]>(i => i.ItemTypes)
            .IfNotNull(i => i.ForEach(j => ItemTypes.Add(j)));

        if (ItemTypeFind)
        {
            model.WriteLine(MemberLogType.StyleModel, new Message("[d] Testing..."));

            ItemTypes.Clear();
            GetItemTypes().ForEach(i => ItemTypes.Add(i));
        }

        ItemValues = [];
        Try.Do(() => GetItemValues()?.ForEach(ItemValues.Add));

        if (!ItemTypes.Any() && !ItemValues.Any())
        {
            if (ItemAction.HasFlag(ItemAction.Add))
                Messages.Add(new Warning(MessageNoItemPreset.F(model.ValueType.FullName)));
        }
    }

    public override void Unset(IMemberStylable model)
    {
        List = null;
        ListView = null;

        SelectedIndex = -1;
        SelectedItem = null;

        ItemTypes.Clear();
        ItemTypes = null;

        ItemValues.Clear();
        ItemValues = null;

        base.Unset(model);
    }
}