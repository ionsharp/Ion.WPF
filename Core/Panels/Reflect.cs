using Ion.Analysis;
using Ion.Collect;
using Ion.Controls;
using Ion.Data;
using Ion.Input;
using Ion.Reflect;
using Ion.Threading;
using Mono.Cecil;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace Ion.Core;

[Name("Reflect"), Image(Images.Code)]
[Styles.Object(Strict = MemberTypes.All,
    MemberViewType = MemberViewType.Tab)]
public record class ReflectPanel : MethodPanel
{
    [TabView(View = View.Main)]
    private enum Tab { }

    public static readonly new ResourceKey Template = new();

    #region public class MemberModel

    public record class MemberModel : Model<MemberInfo>
    {
        public Access Access { get => Get(Access.Undefined); set => Set(value); }

        public string Name { get => Get(""); set => Set(value); }

        public string Kind { get => Get(""); set => Set(value); }

        public Type Type { get => Get<Type>(); set => Set(value); }

        public MemberModel(MemberInfo member) : base(member)
        {
            Name
                = member.Name;
            Access
                = member.GetAccess();
            Kind
                = member.MemberType.ToString();
            Type
                = member.GetMemberType();
        }
    }

    #endregion

    /// <see cref="Region.Property"/>

    protected override TaskStrategy MethodStrategy => TaskStrategy.Ignore;

    protected override TaskType MethodType => TaskType.Unmanaged;

    /// <see cref="Region.Property.Public"/>

    private readonly ListObservable<MemberModel> members = [];
    [Hide]
    public ListCollectionView ViewMembers { get => Get<ListCollectionView>(); set => Set(value); }

    ///

    private readonly ListObservable<Assembly> assemblies = [];
    [Styles.List(Ion.Template.ListCombo, NameHide = true, Index = 0,
        View = View.Header,
        SelectedIndexProperty = nameof(FilterAssemblyIndex),
        SelectedItemProperty = nameof(FilterAssembly))]
    [Styles.Text(CanEdit = false,
        ValueConvert = typeof(ConvertAssemblyName),
        TargetItem = typeof(Assembly))]
    [Group(0), Name("Assembly")]
    public ListCollectionView ViewAssemblies { get => Get<ListCollectionView>(); set => Set(value); }

    private readonly ListObservableOfString namespaces = [];
    [Styles.List(Ion.Template.ListCombo, NameHide = true, Index = 1,
        View = View.Header,
        SelectedIndexProperty = nameof(FilterNamespaceIndex),
        SelectedItemProperty = nameof(FilterNamespace))]
    [Group(0), Name("Namespace")]
    public ListCollectionView ViewNamespaces { get => Get<ListCollectionView>(); set => Set(value); }

    private readonly ListObservable<Type> types = [];
    [Styles.List(Ion.Template.ListCombo, NameHide = true, Index = 2,
        View = View.Header,
        SelectedIndexProperty = nameof(FilterTypeIndex),
        SelectedItemProperty = nameof(FilterType))]
    [Styles.Text(CanEdit = false,
        ValueConvert = typeof(ConvertTypeToString),
        TargetItem = typeof(Type))]
    [Group(0), Name("Type")]
    public ListCollectionView ViewTypes { get => Get<ListCollectionView>(); set => Set(value); }

    /// <see cref="Views.Header"/>

    [Hide]
    public object FilterAssembly { get => Get<object>(); set => Set(value); }

    [Hide]
    public int FilterAssemblyIndex { get => Get(-1); set => Set(value); }

    [Hide]
    public object FilterNamespace { get => Get<object>(); set => Set(value); }

    [Hide]
    public int FilterNamespaceIndex { get => Get(-1); set => Set(value); }

    [Hide]
    public object FilterType { get => Get<object>(); set => Set(value); }

    [Hide]
    public int FilterTypeIndex { get => Get(-1); set => Set(value); }

    ///

    [Group(0), Name("Type kind")]
    [Style(Ion.Template.EnumFlagButton, NameHide = true, Index = 0, Orientation = Orient.Vertical,
        View = View.Footer)]
    public TypeKind FilterTypeKind { get => Get(TypeKind.All); set => Set(value); }

    [Group(1), Name("Access")]
    [Style(Ion.Template.EnumFlagButton, NameHide = true, Index = 0, Orientation = Orient.Vertical,
        View = View.Footer)]
    public Access FilterMemberAccess { get => Get(Access.All); set => Set(value); }

    [Group(1), Name("Flags")]
    [Style(Ion.Template.EnumFlagButton, NameHide = true, Index = 1, Orientation = Orient.Vertical,
        View = View.Footer)]
    public BindingFlags FilterMemberFlags { get => Get(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static); set => Set(value); }

    [Group(1), Name("Kind")]
    [Style(Ion.Template.EnumFlagButton, NameHide = true, Index = 2, Orientation = Orient.Vertical,
        View = View.Footer)]
    public MemberTypes FilterMemberType { get => Get(MemberTypes.All); set => Set(value); }

    ///

    [Name("View")]
    [Style(NameHide = true,
        View = View.HeaderItem)]
    public Views ViewType { get => Get(Views.Text); set => Set(value); }

    public string Code { get => Get(""); set => Set(value); }

    public enum Views { List, Text }

    /// <see cref="Region.Constructor"/>

    public ReflectPanel() : base()
    {
        ViewAssemblies = new ListCollectionView(assemblies);
        ViewAssemblies.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(Assembly.FullName), System.ComponentModel.ListSortDirection.Ascending));

        ViewNamespaces = new ListCollectionView(namespaces);
        ViewNamespaces.SortDescriptions.Add(new System.ComponentModel.SortDescription(".", System.ComponentModel.ListSortDirection.Ascending));

        ViewTypes = new ListCollectionView(types);
        ViewTypes.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(Type.Name), System.ComponentModel.ListSortDirection.Ascending));

        XAssembly.All.ForEach(assemblies.Add);

        ViewMembers = new ListCollectionView(members);
        ViewMembers.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(MemberModel.Name), System.ComponentModel.ListSortDirection.Ascending));
    }

    /// <see cref="Region.Method"/>

    private void Update(Type type = null)
    {
        StartCommand.Execute(type);
    }

    private void UpdateNames()
    {
        namespaces.Clear();
        ViewAssemblies.GetItemAt(FilterAssemblyIndex).As<Assembly>().GetTypes().ToArray(t => t.Namespace).Distinct().ForEach(namespaces.Add);
    }

    private void UpdateTypes()
    {
        types.Clear();

        var assembly = Try.Get(() => ViewAssemblies.GetItemAt(FilterAssemblyIndex).As<Assembly>(), e => Log.Write(e));
        if (assembly != null)
        {
            var name = Try.Get(() => ViewNamespaces.GetItemAt(FilterNamespaceIndex).As<string>(), e => Log.Write(e));
            if (name != null)
                XAssembly.GetTypes(assembly, name, i => FilterTypeKind.HasFlag(i.GetKind())).ForEach(types.Add);
        }
    }

    async protected override Task RunAsync(object parameter, CancellationToken token)
    {
        members.Clear();

        var type = parameter is Type overrideType ? overrideType : Try.Get(() => ViewTypes.GetItemAt(FilterTypeIndex).As<Type>(), e => Log.Write(e));
        if (type != null)
        {
            var text = $"{(type.IsPublic ? "public" : "private")}{(type.IsAbstract ? " abstract" : type.IsSealed ? " sealed" : "")} {(type.IsClass ? "class" : type.IsEnum ? "enum" : type.IsInterface ? "interface" : type.IsValueType ? "struct" : "")} {type.Name}";
            text += "\n{";

            await System.Threading.Tasks.Task.Run(() =>
            {
                var members = type.GetMembers(FilterMemberFlags, FilterMemberType).OrderBy(i => i.MemberType).ThenBy(i => i.GetAccess()).ThenBy(i => i.Name);

                var count = members.Count();
                var index = 0;

                var indent = "    ";

                Access? lastAccess = null;
                MemberTypes? lastType = null;

                foreach (var member in members)
                {
                    if (!FilterMemberAccess.HasFlag(member.GetAccess()))
                        continue;

                    var result = new MemberModel(member);
                    var progress = Convert.ToDouble(index) / Convert.ToDouble(count);

                    ///

                    if (lastType != member.MemberType)
                    {
                        text += "\n" + indent + "#endregion" + "\n";
                        lastType = member.MemberType;

                        text +=
                        $"\n" + indent + $"/// <see cref=\"Region.{lastType}\"/>" +
                         "\n" + indent + "#region" + "\n";
                    }

                    ///

                    if (lastAccess != member.GetAccess())
                    {
                        text += "\n" + indent + "#endregion" + "\n";
                        lastAccess = member.GetAccess();

                        text +=
                        $"\n" + indent + $"/// <see cref=\"Region.{lastType}.{lastAccess}\"/>" +
                         "\n" + indent + "#region" + "\n";
                    }

                    ///

                    text += "\n" + indent;
                    text += $"{result.Access.ToString().ToLower()} ";

                    text += member.MemberType == MemberTypes.Constructor
                        ? $"{type.Name}" : $"{result.Type.Name} {result.Name}";
                    text += member.MemberType == MemberTypes.Constructor || member.MemberType == MemberTypes.Method
                        ? "()" : "";

                    text += ";\n";

                    Dispatch.Do(() =>
                    {
                        Task.Progress = progress;
                        this.members.Add(result);
                    });
                    index++;
                }
            }, token);

            text += "\n}";
            Code = text;
        }
    }

    protected override void RunSync(object parameter, CancellationToken token) => throw new NotImplementedException();

    /// <see cref="Region.Method.Public.Override"/>

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        switch (e.PropertyName)
        {
            case nameof(FilterAssemblyIndex):
                UpdateNames();
                break;

            case nameof(FilterNamespaceIndex):
            case nameof(FilterTypeKind):
                UpdateTypes();
                break;

            case nameof(FilterMemberAccess):
            case nameof(FilterMemberFlags):
            case nameof(FilterMemberType):
            case nameof(FilterTypeIndex):
                Update();
                break;
        }
    }

    private ICommand selectCommand;
    public ICommand SelectCommand => selectCommand
        ??= new RelayCommand<Type>(Update, i => i != null);
}