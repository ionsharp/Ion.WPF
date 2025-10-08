using Ion.Input;
using Ion.Reflect;
using System;
using System.Xml.Serialization;

namespace Ion.Core;

/// <inheritdoc/>
[Serializable]
[Styles.Object(Filter = Filter.Route | Filter.Search | Filter.Sort,
    RouteIcon = Images.Options,
    RouteName = "Options",
    MemberViewType = MemberViewType.Tab,
    MemberView = View.Option)]
[Styles.Object(Filter = Filter.None,
    MemberViewType = MemberViewType.All,
    MemberView = View.Footer | View.Header | View.HeaderItem | View.HeaderOption)]
public abstract record class Content : ViewModel, ILock
{
    [field: NonSerialized]
    public event LockEventHandler Locked;

    /// <see cref="Region.Property"/>

    [NonSerializable]
    [XmlIgnore]
    public virtual bool CanFloat { get => Get(true); set => Set(value); }

    [XmlIgnore]
    public bool IsActive { get => Get(false, false); set => Set(value, false); }

    [NonSerializable]
    [XmlIgnore]
    public bool IsLocked { get => Get(false, false); set => Set(value, false); }

    public virtual bool IsOptionVisible { get => Get(false); set => Set(value); }

    public virtual bool IsOptionDescriptionVisible { get => Get(true); set => Set(value); }

    [NonSerializable]
    [XmlIgnore]
    public bool IsSubscribed { get => Get(false); private set => Set(value); }

    public virtual DateTime? LastActive { get => Get<DateTime?>(); set => Set(value); }

    [XmlIgnore]
    public virtual string Title { get; }

    [XmlIgnore]
    public virtual object ToolTip { get; }

    /// <see cref="Region.Constructor"/>

    /// <inheritdoc/>
    protected Content() : base() { }

    /// <see cref="Region.Method"/>

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        if (e.PropertyName == nameof(IsLocked))
            OnLocked(IsLocked);
    }

    public override void Subscribe() { IsSubscribed = true; }

    public override void Unsubscribe() { IsSubscribed = false; }

    public virtual void OnLocked(bool isLocked) => Locked?.Invoke(this, new LockEventArgs(isLocked));

    public virtual void Resubscribe() { Unsubscribe(); Subscribe(); }
}