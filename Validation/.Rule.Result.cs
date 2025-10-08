using Ion.Analysis;
using Ion.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace Ion.Validation;

public abstract class ResultRule : ValidationRule, IPropertyGet, IPropertySet
{
    public Result Result { get => this.Get<Result>(); protected set => this.Set(value); }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo) => throw new NotImplementedException();

    /// <see cref="IPropertyGet"/>
    #region

    public virtual void OnGetProperty(PropertyGetEventData e) { }

    #endregion

    /// <see cref="IPropertySet"/>
    #region

    [field: NonSerialized]
    protected event PropertyChangedEventHandler PropertyChanged;
    event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged { add => PropertyChanged += value; remove => PropertyChanged -= value; }

    [field: NonSerialized]
    public event PropertySetEventHandler PropertySet;

    [field: NonSerialized]
    [XmlIgnore]
    Dictionary<string, object> IPropertySet.NonSerializedProperties { get; set; } = [];

    [XmlIgnore]
    Dictionary<string, object> IPropertySet.SerializedProperties { get; set; } = [];

    public virtual void OnSetProperty(PropertySetEventArgs e)
    {
        PropertyChanged
            ?.Invoke(this, new(e.PropertyName));
        PropertySet
            ?.Invoke(this, e);
    }

    public virtual void OnSettingProperty(PropertySettingEventArgs e) { }

    #endregion
}