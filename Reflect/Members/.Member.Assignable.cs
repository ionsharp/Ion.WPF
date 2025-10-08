using Ion.Core;
using Ion.Data;
using Ion.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Timers;
using System.Windows.Input;

namespace Ion.Reflect;

/// <inheritdoc/>
public abstract record class MemberAssignable<T>(IMemberInfo parent, MemberData data) : Member<T>(parent, data), IMemberAssignable where T : MemberInfo
{
    /// <see cref="Region.Property"/>
    #region 

    public abstract bool CanSet { get; }

    #endregion

    /// <see cref="Region.Method"/>
    #region 

    protected override bool CanCopy()
        => Value is not null
        && Style?.GetValue(i => i.Options).HasFlag(Option.Copy) == true;

    protected override bool CanDefault()
        => Style?.GetValue(i => i.Options).HasFlag(Option.Default) == true
        && (Style?.GetValue(i => i.CanEdit) == true && CanSet)
        && (HasDefaultValue || !(ValueType ?? DeclarationType).IsAbstract);

    protected override bool CanPaste()
        => Style?.GetValue(i => i.Options).HasFlag(Option.Paste) == true
        && (Style?.GetValue(i => i.CanEdit) == true && CanSet)
        && !(ValueType ?? DeclarationType).IsAbstract
        && (Appp.Cache.Contains(ValueType) || Appp.Cache.Contains(DeclarationType));

    protected override bool CanReset()
        => Style?.GetValue(i => i.Options).HasFlag(Option.Reset) == true
        && Value is IReset;

    protected override bool CanRevert()
        => Style?.GetValue(i => i.Options).HasFlag(Option.Revert) == true
        && (Style?.GetValue(i => i.CanEdit) == true && CanSet) && InitialValue is not null;

    protected override void Copy()
        => Appp.Cache.Add([Value]);

    protected override void Default()
        => Value = HasDefaultValue ? DefaultValue : (ValueType ?? DeclarationType).GetDefaultValue();

    protected override void Paste()
        => Value = Appp.Cache[ValueType ?? DeclarationType].Value.First().CloneDeep(new CreateFromObject());

    protected override void Reset()
        => Value.If<IReset>(i => i.Reset());

    protected override void Revert()
        => Value = InitialValue;

    protected override bool CanInvokeAll()
    {
        return true;
        /*
        var result = true;
        Parent.EachValue(i =>
        {
            if (Try.Get(() => GetValue(i)) is ICommand j)
                result = result && j.CanExecute(null);
        });
        return result;
        */
    }

    protected override IEnumerable<Type> GetReplaceTypes()
        => XAssembly.GetTypes(i => !i.IsAbstract && !i.IsInterface && i.IsPublic && (i.Implements(DeclarationType) || i.Inherits(DeclarationType) || i.Implements(ValueType) || i.Inherits(ValueType)));

    protected override void InvokeAll()
    {
        /*
        Parent.EachValue(i =>
        {
            if (Try.Get(() => GetValue(i)) is ICommand j)
                j.Execute();
        });
        */
    }

    protected override void OnUpdate(ElapsedEventArgs e)
    {
        base.OnUpdate(e);
        if (!Style.GetValue(i => i.IsEditingText))
            Reset<object>(() => Value);
    }

    #endregion

    /// <see cref="ICommand"/>

    public ICommand UnsetCommand => Commands[nameof(UnsetCommand)]
        ??= new RelayCommand
        (
            () => Value = null,
            ()
            => Style?.GetValue(i => i.Options).HasFlag(Option.Unset) == true
            && (Style?.GetValue(i => i.CanEdit) == true && CanSet)
            && (ValueType?.IsValueType == false || ValueType?.IsNullable() == true)
            && Value is not null
        );

    /// <see cref="IMemberInfo"/>

    /// <inheritdoc/>
    public override void Reset(object value)
    {
        /*
        handleValue.SafeInvoke(() =>
        {
            if (CanSet && Style.GetValue(i => i.CanEdit))
            {
                Set(() => Value, value);
                Parent.EachValue(i => SetValue(i, value));

                if (Parent.ValueTypeForm == TypeForm.Value && Parent is AssignableModel<T> parent)
                    parent.SetValue(parent.Parent.Value, Parent.Value);
            }
        });
        */
    }

    /// <see cref="IPropertySet"/>

    /// <inheritdoc cref="IPropertySet.OnSetProperty(PropertySetEventArgs)"/>
    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        switch (e.PropertyName)
        {
            case nameof(Value):
                if (Style.GetValue(i => i.DescriptionFromValue))
                    Style[i => i.Description] = ValueConverter.Cache.Get<ConvertAttributeDescription>().Convert(Value ?? ValueType);

                if (Style.GetValue(i => i.NameFromValue))
                    Style[i => i.Name] = ValueConverter.Cache.Get<ConvertAttributeName>().Convert(Value ?? ValueType);
                break;
        }
    }

    ///<see cref="IStyle">

    /// <inheritdoc/>
    protected override StyleAttribute GetStyle()
    {
        //a) Parent declaration
        //b) Parent value (or declaration) type
        //c) Member declaration
        WriteLine(MemberLogType.Style, "Checking underlying implementation...");

        var result = base.GetStyle();
        if (result is not null)
            return result;

        //d) Member value type

        WriteLine(MemberLogType.Style, "Checking overlying implementation...");
        WriteLine(MemberLogType.Style, "(d) Checking member value type...");

        result = ValueType
            ?.GetAttributes<StyleAttribute>()
            .FirstOrDefault<StyleAttribute>(i => i.TargetMember is null && i.TargetItem is null
                && (View == View.None || View.HasFlag(i.View)));

        if (result is not null)
        {
            WriteLine(MemberLogType.Style, "Style inherited from member value type.");
            return result;
        }

        //e) Member declaration type 

        WriteLine(MemberLogType.Style, "(e) Checking member declaration type...");

        result = DeclarationType
            ?.GetAttributes<StyleAttribute>()
            .FirstOrDefault<StyleAttribute>(i => i.TargetMember is null && i.TargetItem is null
                && (View == View.None || View.HasFlag(i.View)));

        if (result is not null)
            WriteLine(MemberLogType.Style, "Style inherited from member declaration type.");

        return result;
    }

    /// <inheritdoc/>
    protected override void OnSetStyleAttribute(Attribute i)
    {
        base.OnSetStyleAttribute(i);
        i.If<DefaultValueAttribute>(j =>
        {
            HasDefaultValue = true;
            DefaultValue = j.Value;
        });
    }

    /// <inheritdoc/>
    protected override void OnSetStyleAttributes()
    {
        //(1) Type declaration
        DeclarationType.GetAttributes()?
            .ForEach(OnSetStyleAttribute);

        //(2) Value type
        if (DeclarationType != ValueType)
        {
            ValueType.GetAttributes()?
                .ForEach(OnSetStyleAttribute);
        }

        //(3) Member declaration
        base.OnSetStyleAttributes();
    }

    /// <inheritdoc/>
    protected override void OnSetStyleOverrides()
    {
        base.OnSetStyleOverrides();
        Style[i => i.CanEdit] = Parent.Style[i => i.CanEdit];
    }

    /// <inheritdoc/>
    protected override void OnUnsetStyle()
    {
        base.OnUnsetStyle();
        HasDefaultValue = false;
        DefaultValue = null;
    }

    /// <inheritdoc/>
    protected override Type GetValueType(object value) => value?.GetType() ?? DeclarationType;

    [Obsolete]
    protected object GetValueFrom()
    {
        object result = null;

        /*
        var allEqual = true;
        object last = null;

        Try.Do(() =>
        {
            var index = 0;
            Parent.EachValue(i =>
            {
                var j = GetValue(i);

                if (index > 0)
                {
                    if (last != j)
                        allEqual = false;
                }

                last = j;
                index++;
            });
        });

        if (allEqual)
        {
            result = last;
            IsIndeterminate = false;
        }
        else
        {
            result = null;
            IsIndeterminate = true;
        }
        */

        return result;
    }

    [Obsolete]
    protected void SetValueFrom() { }//=> handleValue.SafeInvoke(() => Value = GetValueFrom());
}