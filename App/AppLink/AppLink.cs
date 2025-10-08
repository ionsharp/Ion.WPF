using Ion.Controls;
using Ion.Reflect;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Ion.Core;

/// <see cref="AppLink"/>
#region

/// <summary>A (partially) independent component that extends functionality of an application.</summary>
[Styles.Object(GroupName = MemberGroupName.None)]
[Serializable]
public abstract record class AppLink() : Model(), IAppLink
{
    [field: NonSerialized]
    public event EventHandler<EventArgs> Disabled;

    [field: NonSerialized]
    public event EventHandler<EventArgs> Enabled;

    ///

    [field: NonSerialized]
    public AssemblyContext AssemblyContext { get; set; }

    public virtual Type TargetType { get; }

    ///

    public string FilePath { get; set; }

    public bool IsEnabled
    {
        get => Get(false);
        set
        {
            Set(value);
            value.If(OnEnabled, OnDisabled);
        }
    }

    ///

    public AppLinkAttribute Attribute => this.GetAttribute<AppLinkAttribute>();

    ///

    public virtual string Author => nameof(Ion);

    public virtual string Description => Attribute.Description;

    public virtual string Icon => Resource.GetImage(Attribute.Icon);

    public virtual string Name => Attribute.Name;

    public virtual string Uri => Attribute.Uri;

    public virtual Version Version => Attribute.Version;

    ///

    public virtual void OnDisabled() => Disabled?.Invoke(this, new());

    public virtual void OnEnabled() => Enabled?.Invoke(this, new());
}

#endregion

/// <see cref="AppLink{T}"/>
#region

/// <inheritdoc/>
[Serializable]
public abstract record class AppLink<T>() : AppLink() where T : IAppModel
{
    public override Type TargetType => typeof(T);
}

#endregion

/// <see cref="PanelLink{T}"/>
#region

/// <inheritdoc/>
[Serializable]
public abstract record class PanelLink<T>() : AppLink<IDockAppModel>() where T : Panel
{
    [field: NonSerialized]
    public T Panel { get; private set; }

    public virtual IAppLinkResources Resources { get; }

    [NonSerialized]
    private DataTemplate template = null;
    public DataTemplate Template
    {
        get
        {
            if (template is null)
            {
                if (Resources?.GetValues() is IEnumerable<object> values)
                {
                    foreach (var i in values)
                    {
                        if (i is DataTemplate j)
                        {
                            if ((Type)j.DataType == Panel.GetType())
                            {
                                template = j;
                                break;
                            }
                        }
                    }
                }
            }
            return template;
        }
    }

    protected override void OnConstructed()
    {
        base.OnConstructed();
        Panel = typeof(T).Create<T>();
    }

    public override void OnDisabled()
    {
        base.OnEnabled();
        Panel.IfNotNull(i => Appp.Get<IDockAppModel>().ViewModel.Panels.Remove(i));
    }

    public override void OnEnabled()
    {
        base.OnEnabled();
        Panel.IfNotNull(i => Appp.Get<IDockAppModel>().ViewModel.Panels.Add(i));
    }
}

#endregion