using Ion.Controls;
using Ion.Input;
using System;
using System.Reflection;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Ion.Core;

/// <summary><see cref="Content"/> that can be created multiple times.</summary>
[Image(Images.File)]
[Name("Document")]
[Styles.Object(Strict = MemberTypes.All)]
[Serializable]
public abstract record class Document : Content
{
    ///<see cref="Region.Field"/>

    public const SecondaryDocks DefaultDockPreference = SecondaryDocks.Left;

    ///<see cref="Region.Property"/>

    [NonSerializable]
    [XmlIgnore]
    public virtual bool CanClose { get => Get(true); set => Set(value); }

    [NonSerializable]
    [XmlIgnore]
    public virtual bool CanMinimize { get => Get(true); set => Set(value); }

    [XmlIgnore]
    public virtual SecondaryDocks DockPreference { get; } = DefaultDockPreference;

    [XmlIgnore]
    public virtual object Icon => default;

    [NonSerializable]
    [XmlIgnore]
    public virtual bool IsMinimized { get => Get(false); set => Set(value); }

    [XmlIgnore]
    public override object ToolTip => this;

    ///<see cref="Region.Constructor"/>

    /// <inheritdoc/>
    protected Document() : base() { }

    ///<see cref="Region.Method"/>

    public abstract void Save();

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        switch (e.PropertyName)
        {
            case nameof(IsChanged):
                Reset(() => Title);
                break;
        }
    }

    ///<see cref="ICommand"/>

    [NonSerialized]
    private ICommand saveCommand;
    [XmlIgnore]
    public virtual ICommand SaveCommand => saveCommand ??= new RelayCommand(Save);
}