using Ion.Analysis;
using Ion.Collect;
using Ion.Core;
using Ion.Reflect;
using System.Collections.Generic;
using System.Windows.Input;

namespace Ion;

/// <inheritdoc cref="ITemplateModel"/>
public partial record class TemplateModel() : Model(), ITemplateModel
{
    public virtual Dictionary<string, ICommand> CommandNames { get; protected set; }

    public ListObservable<Result> Messages { get; private set; } = [];

    public IMemberStylable Model { get; private set; }

    /// <inheritdoc cref="ITemplateModel.Reset(ValueTuple{object, object})"/>
    public virtual void Reset((object OldValue, object NewValue) source) { }

    /// <inheritdoc cref="ITemplateModel.Set(IMemberInfo)"/>
    public virtual void Set(IMemberStylable model) => Model = model;

    /// <inheritdoc cref="ISubscribe.Subscribe()"/>
    public virtual void Subscribe() { }

    /// <inheritdoc cref="ITemplateModel.Unset(IMemberInfo)"/>
    public virtual void Unset(IMemberStylable model)
    {
        Messages.Clear();
        Model = null;
    }

    /// <inheritdoc cref="ISubscribe.Unsubscribe()"/>
    public virtual void Unsubscribe() { }
}