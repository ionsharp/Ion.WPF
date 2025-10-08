using Ion.Analysis;
using Ion.Collect;
using Ion.Core;
using Ion.Input;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Timers;
using System.Windows.Input;

namespace Ion.Reflect;

/// <inheritdoc/>
public abstract partial record class Member : MemberBase, IComparable, IMember
{
    /// <see cref="Region.Field"/>
    #region 

    public const string DefaultGroup = GroupAttribute.Default;

    public const string DefaultTab = "Other";

    private Timer timer;

    #endregion

    /// <see cref="Region.Property"/>
    #region

    public override int Depth => Parent.Depth + 1;

    public override bool Log => Parent.Log;

    public override string Name => Info.Name;

    public override Orient Orientation => Parent.Orientation;

    //public override object Value { get => base.Value; set => UpdateSource(value); }

    ///

    public virtual Access Access => Info.GetAccess();

    public virtual Type DeclarationType { get; }

    /// <summary>Gets the class that declares this member.</summary>
    public virtual Type DeclaringType => Info.DeclaringType;

    public virtual MemberInstanceType InstanceType => Info.GetInstanceType();

    /// <summary>Gets the type of the member.</summary>
    public virtual MemberTypes MemberType => Info.MemberType;

    /// <summary>Gets the class that was used to obtain this member.</summary>
    public virtual Type ReflectedType => Info.ReflectedType;

    ///

    public MemberInfo Info { get; private set; }

    ///

    public object Group { get => Get<object>(DefaultGroup); set => Set(value); }

    public int GroupIndex { get => Get(0); set => Set(value); }

    public int Index { get => Get(0); private set => Set(value); }

    public bool IsSelected { get => Get(false); set => Set(value); }

    public bool IsVisible { get => Get(true); protected set => Set(value); }

    public bool IsTrulyVisible { get => Get(true); internal set => Set(value); }

    public ListObservable<Type> Path
    {
        get
        {
            var result = new ListObservable<Type>();

            IMemberInfo i = this;
            while (i is Member j)
            {
                i = j.Parent;
                result.Insert(0, i.ValueType);
            }

            return result;
        }
    }

    public object Tab { get => Get<object>(DefaultTab); set => Set(value); }

    public MethodInfo UpdateMethod { get; private set; }

    #endregion

    /// <see cref="Region.Constructor"/>

    protected Member(IMemberInfo parent, MemberData data) : base()
    {
        Parent = (MemberBase)parent; Data = data;
        Info = data.Member;

        var value = GetValue(parent.Value);
        Value = value;

        WriteLine(MemberLogType.Value);
        if (value is null)
            OnSetValue(new(null));
    }

    /// <see cref="Styles">

    /// <inheritdoc/>✔
    protected override StyleAttribute GetStyle()
    {
        WriteLine(MemberLogType.Style, "(a) Checking parent declaration...");

        var result =
        //a) Parent declaration
        Parent.Data?
            .FirstOrDefault<StyleAttribute>(i => i.TargetMember == ValueType
                && (View == View.None || View.HasFlag(i.View)));

        if (result is not null)
        {
            WriteLine(MemberLogType.Style, "Style inherited from parent declaration.");
            return result;
        }

        WriteLine(MemberLogType.Style, "(b) Checking parent value (or declaration) type...");

        //b) Parent value (or declaration) type
        result = Parent.ValueType?
            .GetAttributes<StyleAttribute>()
            .FirstOrDefault<StyleAttribute>(i => i.TargetMember == ValueType
                && (View == View.None || View.HasFlag(i.View)));

        if (result is not null)
        {
            WriteLine(MemberLogType.Style, "Style inherited from parent value (or declaration) type.");
            return result;
        }

        WriteLine(MemberLogType.Style, "(c) Checking member declaration...");
        WriteLine(MemberLogType.Style, $"(c) Member has {Data.Count} attributes. StyleAttribute = '{Data.FirstOrDefault<StyleAttribute>()?.GetType().FullName ?? "null"}'");

        foreach (var attribute in Data)
        {
            if (attribute is StyleAttribute sss)
            {
                WriteLine(MemberLogType.Style, $"(c) StyleAttribute = '{attribute.GetType().FullName}'. TargetType = '{sss.TargetMember}', ItemTargetType = '{sss.TargetItem}', View = '{sss.View}'. this.View = '{View}'");
            }
            else
            {
                WriteLine(MemberLogType.Style, $"(c) Attribute = '{attribute.GetType().FullName}'");
            }
        }

        //c) Member declaration
        result = Data
            .FirstOrDefault<StyleAttribute>(i => i.TargetMember is null && i.TargetItem is null
                && (View == View.None || View.HasFlag(i.View)));

        if (result is not null)
            WriteLine(MemberLogType.Style, "Style inherited from member declaration.");

        return result;
    }

    /// <inheritdoc/>✔
    protected override void OnSetStyle(ValueChange i)
    {
        WriteLine(MemberLogType.Style);

        //★ Members always get style (regardless if null!)

        //a)  Null = declaration type
        //b) !Null = Value type

        var a = i.OldValue;
        var b = i.NewValue;

        //Differences with base implementation
        //(✔) = same (✖) = not same

        //1) ✖ There wasn't a value and there still isn't.
        if (a is null && b is null)
        {
            //First time!
            OnSetStyle();
        }

        //2) ✖ There was a value, but now there isn't
        else if (a is not null && b is null)
        {
            OnUnsetStyle();
            OnSetStyle();
        }

        //3) ✔ There wasn't a value, but now there is
        else if (a is null && b is not null)
            OnSetStyle();

        //4a) ✔ There was (and still is) a value AND both types match
        else if (a is not null && b is not null && a.GetType() == b.GetType())
            OnResetStyle(i);

        //4b) ✔ There was (and still is) a value AND both types don't match
        else if (a is not null && b is not null && a.GetType() != b.GetType())
        {
            OnUnsetStyle();
            OnSetStyle();
        }

        Subscribe();
    }

    /// <inheritdoc/>✔
    protected override void OnSetStyle()
    {
        base.OnSetStyle();
        //Caption
        if (Style.GetValue(i => i.CaptionFromDescription))
            Style[i => i.Caption] = Style[i => i.Description];

        //Group
        Group = Style[i => i.Group];
        GroupIndex = Style.GetValue(i => i.GroupIndex);

        //Index
        Index = Convert.ToInt32(Style.GetValue(i => i.Index) ?? Index);

        //Tab
        Tab = Style[i => i.Tab];
    }

    /// <inheritdoc/>✔
    protected override void OnUnsetStyle()
    {
        base.OnUnsetStyle();
        //Group
        Group = null;
        GroupIndex = 0;

        //Index
        Index = 0;

        //Tab
        Tab = null;
    }

    /// <inheritdoc/>✔
    protected override void OnSetStyleAttribute(Attribute attribute)
    {
        base.OnSetStyleAttribute(attribute);
        attribute.If<CategoryAttribute>
            (i => Style[j => j.Group] = i.Category);

        attribute.If<GroupAttribute>
            (i =>
            {
                Style[j => j.Group] = i.Name;
                Style[j => j.GroupIndex] = i.Index is not null ? Convert.ToInt32(i.Index) : i.Name.As<Enum>()?.GetAttribute<GroupStyleAttribute>()?.Index ?? Style[i => i.GroupIndex];
            });

        attribute.If<ImageAttribute>
            (i => Style.SetValue(j => j.Image, i.Name));
    }

    /// <inheritdoc/>✔
    protected override void OnSetStyleOverrides()
    {
        base.OnSetStyleOverrides();
        Style[i => i.Name] ??= Name;
        Style[i => i.Placeholder] ??= Style[i => i.Name];
    }

    /// <remarks>See <see cref="OnSetStyleTriggers"/>.</remarks>
    private bool GetStyleTriggerValue(CompareTriggerAttribute i)
    {
        Error error = null;
        object result = Try.Get(() => Instance.GetPropertyValue(Parent.Value, i.PropertyName), e => error = new(e));
        return error is not null; //|| Number.Compare(result, i.Operator, i.Value);
    }

    /// <remarks>See <see cref="OnSetStyleTriggers"/>.</remarks>
    private void SetStyleTriggerValue(StyleTriggerAttribute i)
    {
        var result = Instance.GetPropertyValue(Parent.Value, i.PropertyName);
        Style[i.StylePropertyName] = result;
    }

    /// <inheritdoc/>✖
    internal override void OnSetStyleTriggers(string propertyName = null)
    {
        base.OnSetStyleTriggers();

        /// <see cref="EnableTriggerAttribute"/>

        var isEnabled = true;
        Data
            .Where<EnableTriggerAttribute>()
            .ForEach(i => isEnabled = isEnabled && GetStyleTriggerValue(i));

        Style[i => i.IsEnabled] = isEnabled;

        /// <see cref="StyleTriggerAttribute"/>

        /*
        Data
            .Where<StyleTriggerAttribute>()
            .ForEach(i =>
            {
                if (propertyName == null || i.PropertyName == propertyName)
                    SetStyleTriggerValue(i);
            });
        */

        /// <see cref="VisibilityTriggerAttribute"/>

        var isVisible = true;
        Data
            .Where<VisibilityTriggerAttribute>()
            .ForEach(i => isVisible = isVisible && GetStyleTriggerValue(i));

        IsVisible = isVisible;
    }

    protected override void OnSetValueType(ValueChange e) { }

    /// <see cref="Region.Method"/>
    #region 

    private void OnUpdate(object sender, ElapsedEventArgs e) => OnUpdate(e);

    protected virtual bool CanInvokeAll() => default;

    protected virtual void InvokeAll() { }

    protected virtual void OnUpdate(ElapsedEventArgs e)
    {
        if (UpdateMethod is null)
        {
            if (Style.GetValue(i => i.UpdateMethod) is string updateMethod)
                UpdateMethod = Parent.ValueType.GetMethod(updateMethod);
        }
        UpdateMethod?.Invoke(Parent, []);
    }

    #endregion

    /// <see cref="ICommand"/>

    public ICommand InvokeAllCommand
        => Commands[nameof(InvokeAllCommand)] ??= new RelayCommand(InvokeAll, () => CanInvokeAll());

    /// <see cref="IComparable"/>

    int IComparable.CompareTo(object a)
    {
        if (a is Member b)
            return Index.CompareTo(b.Index);

        return 0;
    }

    /// <see cref="IMember"/>

    Type IMember.MemberType => DeclarationType;

    /// <see cref="IPropertySet"/>

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        switch (e.PropertyName)
        {
            case nameof(Info):
                Reset<Access>(() => Access);
                Reset<Type>(() => DeclarationType);
                Reset<Type>(() => DeclaringType);
                Reset<MemberInstanceType>(() => InstanceType);
                Reset<MemberTypes>(() => MemberType);
                Reset<string>(() => Name);
                break;
        }
    }

    /// <see cref="ISubscribe"/>

    public override void Subscribe()
    {
        base.Subscribe();
        if (Style?.GetValue(i => i.Update) == true)
        {
            timer = new() { Interval = Style.GetValue(i => i.UpdateInterval) * 1000 };
            timer.Elapsed += OnUpdate;
            timer.Start();
        }
    }

    public override void Unsubscribe()
    {
        base.Unsubscribe();
        if (Style?.GetValue(i => i.Update) == true)
        {
            timer.IfNotNull(i =>
            {
                i.Stop();
                i.Elapsed -= OnUpdate;
                i.Dispose();
            });
        }
    }

    protected virtual object GetValue(object source) => default;

    protected abstract void SetValue(object source, object value);
}