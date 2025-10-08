using Ion.Core;
using System;
using System.Reflection;

namespace Ion.Reflect;

/// <inheritdoc/>
public abstract record class MemberUnassignable<T>(IMemberInfo parent, MemberData data) : Member<T>(parent, data) where T : MemberInfo
{
    /// <see cref="Region.Property"/>

    public override sealed Type DeclarationType => typeof(T);

    public override sealed object Value => null;

    public override sealed Type ValueType => DeclarationType;

    /// <see cref="IStyle">

    /// <inheritdoc/>
    protected override sealed Type GetValueType(object i) => ValueType;

    /// <see cref="ISubscribe"/>

    /// <inheritdoc/>
    public override sealed void Subscribe() { }

    /// <inheritdoc/>
    public override sealed void Unsubscribe() { }

    /// <inheritdoc/>
    protected override void SetValue(object source, object value) { }
}