using CSharpx;
using Ion;
using Ion.Analysis;
using Ion.Collect;
using Ion.Controls;
using Ion.Core;
using Ion.Data;
using Ion.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ion.Reflect;

public record class MemberBase : Model, IMemberStylable, IStyle
{
    /// <see cref="Region.Field"/>

    public static readonly CacheByType<TypeData> Cache = new(true, i =>
    {
        var j = new TypeData(i);
        MemberList.GetMembers(i).ForEach(k => j.Members.Add(k, new(k)));
        return j;
    });

    public const string ErrorTemplateInvalid = "A template was found for type '<b>{0}</b>', but it isn't valid.";

    public const string ErrorTemplateMissing = "A template wasn't specified for type '<b>{0}</b>' and a default one can't be found.";

    public const string ErrorTemplateMissingConverter = "The template '{0}' expects type '{1}'. The type '{2}' does not match. A converter of type '" + $"{nameof(IConvert)}" + "<{3}, {4}>' must be specified.";

    /// <see cref="Region.Property"/>
    #region

    public Type BaseType => ValueType?.BaseType;

    public Guid Id { get; } = Guid.NewGuid();

    public MemberData Data { get; protected set; }

    public bool IsIndeterminate { get => Get(false); protected set => Set(value); }

    public IMemberStylable Parent { get => Get<IMemberStylable>(); protected set => Set(value); }

    public string Search { get => Get<string>(); set => Set(value); }

    public MemberSearchName SearchName { get => Get(MemberSearchName.Name); set => Set(value); }

    public SearchOptions SearchOptions { get => Get(new SearchOptions()); set => Set(value); }

    public View View { get; internal set; } = View.None;

    /// <see cref="Region.Property.Public.Virtual"/>

    public virtual int Depth => 0;

    public virtual bool Log { get => Get(false); set => Set(value); }

    public virtual MemberLogType LogType { get => Get(MemberLogType.All); set => Set(value); }

    public virtual Orient Orientation { get => Get<Orient>(); internal set => Set(value); }

    #endregion

    /// <see cref="Region.Constructor"/>

    public MemberBase() : base() { Style = []; }

    /// <see cref="Region.Method"/>
    #region

    protected virtual bool CanCopy()
        => Style?.GetValue(i => i.Options).HasFlag(Option.Copy) == true;

    protected virtual bool CanDefault()
        => Style?.GetValue(i => i.Options).HasFlag(Option.Default) == true;

    protected virtual bool CanEdit()
        => Style?.GetValue(i => i.Options).HasFlag(Option.Edit) == true;

    protected virtual bool CanInvoke(MethodInfo i)
        => i != null && (i.ReturnType != typeof(void) || i.GetParameters()?.Length == 0 || Appp.Cache.Contains(i.GetParameters()[0].ParameterType));

    protected virtual bool CanPaste()
        => Style?.GetValue(i => i.Options).HasFlag(Option.Paste) == true && Appp.Cache.Contains(ValueType);

    protected virtual bool CanReset()
        => ValueType?.Inherits<IReset>() == true && Style?.GetValue(i => i.Options).HasFlag(Option.Reset) == true;

    protected virtual bool CanRevert()
        => InitialValue is not null && Style?.GetValue(i => i.Options).HasFlag(Option.Revert) == true;

    protected virtual void Copy() => Appp.Cache.Add([Value]);

    protected virtual void Default()
    {
        if (ValueType.IsValueType || Value is string)
        {
            Value = ValueType.GetDefaultValue();
        }
        else
        {
            Instance.EachField(Value, i => i.SetValue(Value, i.FieldType.GetDefaultValue()));
            Instance.EachProperty(Value, i => i.SetValue(Value, i.PropertyType.GetDefaultValue()));
        }
    }

    protected virtual void Edit()
        => Dialog.ShowObject("Edit", Value, Resource.GetImageUri(Images.Pencil), Buttons.Done);

    protected virtual void Invoke(MethodInfo i)
    {
        Try.Do(() =>
        {
            if (i.ReturnType == typeof(void))
                i.Invoke(Value, i.GetParameters()?.Length == 0 ? null : i.GetParameters().Select<Type, object>(j => Appp.Cache[j].Value.First()).ToArray());

            else if (i.Invoke(Value, null) is IEnumerable<object> a)
                Appp.Cache.Add(a);

            else if (i.Invoke(Value, null) is object b)
                Appp.Cache.Add([b]);
        },
        e => Analysis.Log.Write(e));
    }

    protected virtual void Paste()
    {
        var result = Cache[ValueType];
        EachValue(x =>
        {
            //Instance.EachField((i, j) => i.SetValue(x, j));
            //Instance.EachProperty((i, j) => i.SetValue(x, j));
        });
    }

    protected virtual void Reset() => EachValue(i => i.If<IReset>(j => j.Reset()));

    protected virtual void Revert()
    {
        if (InitialValue?.GetType().IsValueType == true || InitialValue is string)
        {
            Value = InitialValue;
        }
        else if (InitialValue is not null)
        {
            //What do we do?!
        }
    }

    #endregion

    /// <see cref="ICommand"/>
    #region

    public ICommand CopyCommand
        => Commands[nameof(CopyCommand)] ??= new RelayCommand(Copy, CanCopy);

    public ICommand DefaultCommand
        => Commands[nameof(DefaultCommand)] ??= new RelayCommand(Default, CanDefault);

    public ICommand EditCommand
        => Commands[nameof(EditCommand)] ??= new RelayCommand(Edit, CanEdit);

    public ICommand InvokeCommand
        => Commands[nameof(InvokeCommand)] ??= new RelayCommand<MethodInfo>(Invoke, CanInvoke);

    public ICommand PasteCommand
        => Commands[nameof(PasteCommand)] ??= new RelayCommand(Paste, CanPaste);

    public ICommand ResetCommand
        => Commands[nameof(ResetCommand)] ??= new RelayCommand(Reset, CanReset);

    public ICommand RevertCommand
        => Commands[nameof(RevertCommand)] ??= new RelayCommand(Revert, CanRevert);

    #endregion

    /// <see cref="IMemberInfo"/>

    private int logSenderCount;

    private string logSenderName;

    public void WriteLine(MemberLogType type, object message = null, [CallerMemberName] string sender = null, [CallerLineNumber] int line = 0)
    {
        if (!LogType.HasFlag(type))
            return;

        if (logSenderName != sender)
        {
            logSenderCount = 1;
            logSenderName = sender;
        }

        var result = $"{Name} ({ValueType?.FullName ?? "null"}) | {GetType().Name}.{logSenderName} ({logSenderCount})";
        if (message is not null)
            result += $": {message}";

        Log.If(() => System.Diagnostics.Debug.WriteLine(result));
        logSenderCount++;
    }

    /// <see cref="IName"/>

    public virtual string Name { get => Get<string>(); protected set => Set(value); }

    /// <see cref="IPropertySet"/>

    public override void OnSettingProperty(PropertySettingEventArgs e)
    {
        base.OnSettingProperty(e);
        switch (e.PropertyName)
        {
            case nameof(Value):
                e.NewValue = OnSettingValue(e);
                break;
        }
    }

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        switch (e.PropertyName)
        {
            case nameof(Value):
                OnSetValue(new(e.OldValue, e.NewValue));
                break;
            case nameof(ValueType):
                base.Reset(() => ValueTypeAccess);
                base.Reset(() => ValueTypeForm);
                OnSetValueType(new(e.OldValue, e.NewValue));
                break;
        }
    }

    /// <see cref="IStyle">
    #region 

    object IStyle.Style => Style;

    public InstanceStyle Style { get => Get<InstanceStyle>(); private set => Set(value); }

    public ITemplateModel StyleModel { get => Get<ITemplateModel>(); private set => Set(value); }

    /// <summary>✔Step 1a) Get a new value before setting it.</summary>
    /// <remarks>This occurs before setting a new value.</remarks>
    protected virtual object OnSettingValue(PropertySettingEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("");

        Unsubscribe();
        WriteLine(MemberLogType.Value);

        View = View.None;
        if (e.NewValue is SourceFilter i)
        {
            View
                = i.Section;
            e.NewValue
                = i.Source;
        }

        return e.NewValue;
    }

    /// <summary>✔Step 1b) Set the value.</summary>
    /// <remarks>This occurs after setting a new value.</remarks>
    protected virtual void OnSetValue(ValueChange i)
    {
        WriteLine(MemberLogType.Value);

        ///★ If both values are equal (regardless if null), we will never get here!

        InitialValue
            ??= i.NewValue;
        ValueType
            = GetValueType(i.NewValue);

        OnSetStyle(i);
    }

    /// <summary>✔Step 1c) Get the type of the value.</summary>
    /// <remarks>This occurs after the new value is actually set. <para>Next: <b>Step 2</b>.</para></remarks>
    protected virtual Type GetValueType(object value) => value?.GetType();

    protected virtual void OnSetValueType(ValueChange e)
    {
        Data = e.NewValue.As<Type>().IfNotNullGet(i => Cache[i].Attributes);
    }

    /// <summary>✔ Step 2) Set the style based on the given value.</summary>
    /// <remarks>This occurs after the value type has been set. <para>Next: <b>Step 3a-c</b>.</para></remarks>
    protected virtual void OnSetStyle(ValueChange i)
    {
        var a = i.OldValue;
        var b = i.NewValue;

        //1) There wasn't a value and there still isn't. 
        if (a is null && b is null)
            return;

        //2) There was a value, but now there isn't
        if (a is not null && b is null)
        {
            OnUnsetStyle();
            return;
        }

        //3) There wasn't a value, but now there is
        else if (a is null && b is not null)
            OnSetStyle();

        //4a) There was (and still is) a value AND both types match
        else if (a is not null && b is not null && a.GetType() == b.GetType())
            OnResetStyle(i);

        //4b) There was (and still is) a value AND both types don't match
        else if (a is not null && b is not not null && a.GetType() != b.GetType())
        {
            OnUnsetStyle();
            OnSetStyle();
        }

        Subscribe();
    }

    /// <summary>✔Step 3a) Reset the style based on the given value</summary>
    /// <remarks>This occurs when the value changes, but the style shouldn't.</remarks>
    protected virtual void OnResetStyle(ValueChange i)
    {
        WriteLine(MemberLogType.Style);
        StyleModel.IfNotNull(j => j.Reset(i));
    }

    /// <summary>✔Step 3b) Unset changes made by old style.</summary>
    /// <remarks>This must occur before setting a new style.</remarks>
    protected virtual void OnUnsetStyle()
    {
        WriteLine(MemberLogType.Style);

        //✔  f) Style triggers

        //✔  e) Style model
        StyleModel?.Unset(this);
        StyleModel = null;

        //✔ a-d) Everything else
        Style?.Clear();
    }

    /// <summary>Step 3c) Set changes based on new style.</summary>
    /// <remarks>Next: <b>Step 4a-g</b>.</remarks>
    protected virtual void OnSetStyle()
    {
        WriteLine(MemberLogType.Style);

        //a)
        var attribute = GetStyle() ?? new();

        //b)
        attribute = FixStyle(attribute);
        Instance.EachProperty(attribute, (i, j) => Style[i.Name] = j);

        //b)
        OnSetStyleAttributes();

        //c)
        OnSetStyleOverrides();

        //d)
        OnSetStyleModel();

        //e)
        OnSetStyleTriggers();
    }

    /// <summary>Step 4a1) Get a valid <see cref="StyleAttribute"/>.</summary>
    protected virtual StyleAttribute GetStyle()
    {
        WriteLine(MemberLogType.Style);

        //a) Type declaration
        if (ValueType is not null)
        {
            Type[] types
                = ValueType.IsClass
                ? [ValueType, .. ValueType.GetBaseTypes()]
                : ValueType.IsValueType
                ? [ValueType] : [];

            foreach (var i in types)
            {
                foreach (var j in i.GetAttributes<StyleAttribute>())
                {
                    if (j.TargetMember is null && j.TargetItem is null)
                    {
                        if (View == View.None || j.View.HasFlag(View))
                            return j;
                    }
                }
            }
        }
        return null;
    }

    /// <summary>Step 4a2) Get correct <see cref="StyleAttribute"/> based on desired one.</summary>
    protected virtual StyleAttribute FixStyle(StyleAttribute i)
    {
        WriteLine(MemberLogType.Style);

        /// To do: Clone attribute before modifying as instance lives in static memory (<see cref="Cache)"/>).
        //i = i.CloneShallow();

        var type
            = ValueType;
        var styleType
            = i.GetType();

        bool fix1 = false;

        WriteLine(MemberLogType.Style, $"Old template = '{i.Template}'");

        /// Check if template exists (if not, find a default one).
        if (i.Template is null)
        {
            WriteLine(MemberLogType.Style, $"Template is null. Looking for default one...");

            Type[] types = null;

            /// <see cref="Array"/>
            if (type.IsArray)
            {
                types = [typeof(Array)];
                WriteLine(MemberLogType.Style, $"Type '{type}' is < Array >.");
            }

            /// <see cref="Enum"/>
            else if (type.IsEnum)
            {
                types = [type, typeof(Enum)];
                WriteLine(MemberLogType.Style, $"Type '{type}' (enum): [{type}, Enum].");
            }

            /// <see cref="struct"/>, ...
            else if (type.IsPrimitive || type.IsValueType)
            {
                types = [type];
                WriteLine(MemberLogType.Style, $"Type '{type}' (struct): [{type}].");
            }

            /// <see cref="interface"/>, ...
            else if (type.IsInterface)
            {
                types = [type];
                WriteLine(MemberLogType.Style, $"Type '{type}' (interface): [{type}].");
            }

            /// <see cref="class"/>, ...
            else if (type.IsClass)
            {
                types = [type, .. type.GetBaseTypes()];
                WriteLine(MemberLogType.Style, $"Type '{type}' (class): [{types.ToString(", ")}].");
            }

            /// Check if default template exists that corresponds to each value type (in order of importance!)
            if (types?.Any() == true)
            {
                foreach (var s in types)
                {
                    foreach (var a in ObjectControlTemplate.Default)
                    {
                        /// The template targets the value type
                        if (s == a.Key)
                        {
                            /// The style attribute type doesn't match the default style attribute type
                            if (styleType != a.Value.Item1)
                            {
                                WriteLine(MemberLogType.Style, $"Template for type '{s.FullName}' found.");
                                WriteLine(MemberLogType.Style, $"Old attribute = '{styleType.Name}'. New attribute = '{a.Value.Item1.Name}'");

                                /// Create a valid style attribute (properties therein are required for template to function)
                                StyleAttribute newAttribute = a.Value.Item2?.Invoke()
                                    ?? a.Value.Item1.Create<StyleAttribute>();

                                /// Copy old (invalid) attribute properties to new attribute
                                Instance.EachProperty(i, (x, y) =>
                                {
                                    /// Don't copy over a missing template!
                                    if (x.Name == nameof(StyleAttribute.Template))
                                    {
                                        if (y is null)
                                            return;
                                    }

                                    Try.Do(() =>
                                    {
                                        WriteLine(MemberLogType.Style, $"Copying property '{x.Name}' = '{y}'.");
                                        x.SetValue(newAttribute, y);
                                    },
                                    e => WriteLine(MemberLogType.Style, $"Copying property '{x.Name}' failed: {e.Message}"));
                                },
                                Instance.Flag.Public);
                                i = newAttribute;

                                /// Set default template model (if one isn't specified)
                                i.TemplateModel ??= a.Value.Item3;
                                WriteLine(MemberLogType.Style, $"New template = '{i.Template}', Template model = '{a.Value.Item3?.FullName}'");

                                fix1 = true;
                                goto ContinueOn;
                            }
                        }
                    }
                }
            }
        }

    ContinueOn: { }
        Style.Error = null;

        if (i.Template is Template tt)
        {
            if (tt == Template.Number)
            {
                WriteLine(MemberLogType.Style, $"Template = Template.Number");
                i.TemplateModel = typeof(TemplateModelNumber);
            }
            if (tt == Template.Object)
            {
                WriteLine(MemberLogType.Style, $"Template = Template.Object");
                i.TemplateModel = typeof(TemplateModelObject);
            }
        }

        /// Check if template (still) doesn't exist.
        if (i.Template is null)
        {
            /// Notify of error
            WriteLine(MemberLogType.Style, $"Template is null");

            if (fix1)
            {
                Style.Error = new Error(ErrorTemplateInvalid.F(type.FullName));
            }
            else Style.Error = new Error(ErrorTemplateMissing.F(type.FullName));
        }
        /// Check if template expects a specific type.
        else if (i.Template is Enum && i.Template.GetAttribute<TemplateTypeAttribute>() is TemplateTypeAttribute expects)
        {
            WriteLine(MemberLogType.Style, $"Template expects type '{expects.Type.FullName}'");

            /// The value type matches the expected type
            if (type == expects.Type)
            {
                WriteLine(MemberLogType.Style, $"Value type matches expected template type!");

                /// Nothing to do here
            }
            /// The value type does NOT match the expected type
            else
            {
                WriteLine(MemberLogType.Style, $"Value type does not match expected template type.");

                /// Converter was specified and types match
                if (i.ValueConvert is IConvert oldConverter && oldConverter.SourceType == type && oldConverter.TargetType == expects.Type)
                {
                    WriteLine(MemberLogType.Style, $"The value converter specified is valid.");
                    /// Nothing to do here
                }
                /// Converter wasn't specified OR types do not match
                else
                {
                    WriteLine(MemberLogType.Style, $"The value converter isn't valid or one wasn't specified.");

                    /// A valid converter (that matches types) lives in cache
                    if (ValueConverter.Cache.FirstOrDefault(j => j is IConvert k && k.SourceType == type && k.TargetType == expects.Type) is IConvert newConverter)
                    {
                        WriteLine(MemberLogType.Style, $"A valid converter was found (in cache) and assigned.");
                        i.ValueConvert = newConverter.GetType();
                    }

                    else
                    {
                        /// Look for a valid converter in the relevant namespace
                        var converters = XAssembly.GetTypes("Ion.Data", j =>
                        {
                            if (j.Implements<IConvert>())
                            {
                                var x = j.GetInterfaces().FirstOrDefault(k => k.IsGenericType && k.GetGenericTypeDefinition() == typeof(IConvert<,>));
                                if (x is not null)
                                {
                                    var ab = x.GetGenericArguments();
                                    if (ab[0] == type && ab[1] == expects.Type)
                                        return true;
                                }
                            }
                            return false;
                        });

                        /// A valid converter is defined somewhere
                        if (converters.Any())
                        {
                            WriteLine(MemberLogType.Style, $"A valid converter was found (defined somewhere) and assigned.");
                            i.ValueConvert = converters.FirstOrDefault<Type>();
                        }
                        else
                        {
                            /// A valid converter doesn't exist
                            WriteLine(MemberLogType.Style, $"No valid converter can be assigned.");

                            /// Notify of error
                            Style.Error = new Error(ErrorTemplateMissingConverter.F(i.Template, expects.Type.FullName, type.FullName, type.Name, expects.Type.Name));
                        }
                    }
                }
            }
        }
        return i;
    }

    /// <summary>Step 4b1) </summary>
    protected virtual void OnSetStyleAttribute(Attribute i)
    {
        i.If<DescriptionAttribute>
            (j =>
            {
                Style[k => k.Description] = j.Description;
                Style[k => k.DescriptionFormat] = j.Format;
                Style[k => k.DescriptionLocalize] = j.Localize;
            });
        i.If<System.ComponentModel.DescriptionAttribute>
            (j => Style[k => k.Description] = j.Description);

        i.If<NameAttribute>
            (j =>
            {
                Style[k => k.Name] = j.Name;
                Style[k => k.NameLocalize] = j.Localize;
            });
        i.If<DisplayNameAttribute>
            (j => Style[k => k.Name] = j.DisplayName);

        i.If<ReadOnlyAttribute>
            (j => Style[k => k.CanEdit] = false);
    }

    /// <summary>Step 4b2) </summary>
    protected virtual void OnSetStyleAttributes()
    {
        WriteLine(MemberLogType.Style);
        Data.ForEach(OnSetStyleAttribute);
    }

    /// <summary>Step 4c) </summary>
    protected virtual void OnSetStyleOverrides()
    {
        WriteLine(MemberLogType.Style);

        //1) Declarations

        Data.Where<StyleOverrideAttribute>().ForEach(i => Style[i.PropertyName] = i.Value);

        //2) Other

        //- ReplaceTypes
        if (Style.GetValue(i => i.Options).HasFlag(Option.Replace))
        {
            if (Style[i => i.ReplaceCommand] is null && Style[i => i.ReplaceItems] is null && Style[i => i.ReplaceTypes] is null)
                Style[i => i.ReplaceTypes] = GetReplaceTypes()?.ToArray();
        }

        //- RouteName
        Style[i => i.RouteName] ??= Style[i => i.Name];

        //- Validation
        Style.GetValue(i => i.Validate)
        .IfNotNull(i =>
        {
            var result = new ListObservable<ValidationRule>(Enumerable.Select(i, j => j.Create<ValidationRule>()));
            Style.Add("Validation", result);
        });
    }

    /// <summary>Step 4d) </summary>
    protected virtual void OnSetStyleModel()
    {
        WriteLine(MemberLogType.Style | MemberLogType.StyleModel);
        StyleModel = Style.GetValue(i => i.TemplateModel)?.Create<ITemplateModel>();
        StyleModel?.Set(this);
    }

    /// <summary>Step 4e) Occurs after all other style actions. These affect the style by evaluating the value of other properties (adjacent or nested).</summary>
    /// <remarks>
    /// <b>Only adjacent evaluation is supported (members with siblings).</b> Nested evaluation
    /// (not supported) requires evaluating members of the type (type declarations are ignored!).
    /// </remarks>
    internal virtual void OnSetStyleTriggers(string propertyName = null)
    {
        WriteLine(MemberLogType.Style);
    }

    #endregion

    /// <see cref="ISubscribe"/>

    public virtual void Subscribe()
    {
        WriteLine(MemberLogType.Style);
        StyleModel.IfNotNull<ISubscribe>(i => i.Subscribe());
    }

    public virtual void Unsubscribe()
    {
        WriteLine(MemberLogType.Style);
        StyleModel.IfNotNull<ISubscribe>(i => i.Unsubscribe());
    }

    #region 

    public object DefaultValue { get => Get<object>(); protected set => Set(value); }

    public bool HasDefaultValue { get => Get(false); protected set => Set(value); }

    public object InitialValue { get; private set; }

    public virtual object Value { get => Get<object>(); set => Set(value); }

    public virtual Type ValueType { get => Get<Type>(); set => Set(value); }

    public virtual Access ValueTypeAccess => ValueType?.GetAccess() ?? Access.Undefined;

    public virtual TypeForm ValueTypeForm => ValueType?.IsValueType == true ? TypeForm.Value : TypeForm.Reference;

    protected virtual IEnumerable<Type> GetReplaceTypes()
        => XAssembly.GetTypes(i => !i.IsAbstract && !i.IsInterface && i.IsPublic && (i.Implements(ValueType) || i.Inherits(ValueType)));

    /// <inheritdoc cref="IMemberInfo.Reset(object)"/>
    public virtual void Reset(object value) { }

    [Obsolete]
    private void EachValue(Action<object> action) => GetValues(Value).IfNotNull(i => (i.Length > 0).If(() => { i.ForEach(action); }));

    [Obsolete]
    private static object[] GetValues(object input)
    {
        object[] result = null;
        if (input is not null)
        {
            if (input.GetType().IsArray)
            {
                result = (object[])input;
                if (result.Length == 0)
                    return null;
            }
            else result = [input];

            foreach (var i in result)
            {
                if (i is null) return null;
            }
        }
        return result;
    }

    #endregion
}