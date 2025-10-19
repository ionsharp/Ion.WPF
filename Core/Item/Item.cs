using Ion.Reflect;
using System;
using System.Reflection;

namespace Ion.Core;

[Styles.Object(GroupName = MemberGroupName.None,
    Strict = MemberTypes.All)]
public record class Item<T>(string name) : Namable<T>(name), IGeneric, IItem
{
    /// <see cref="Region.Property"/>
    #region

    [Filter(Filter.Group | Filter.Show | Filter.Sort)]
    public DateTime Created { get => Get(DateTime.Now); private set => Set(value); }

    [Filter(Filter.Search | Filter.Show | Filter.Sort)]
    [Styles.Text(TextLineMultiple = true, Index = 1, Orientation = Orient.Vertical, Pin = Sides.LeftOrTop)]
    public string Description { get => Get(""); set => Set(value); }

    [Filter(Filter.Group | Filter.Show | Filter.Sort)]
    public DateTime? LastModified { get => Get<DateTime?>(); private set => Set(value); }

    [Styles.Object(NameHide = true)]
    public override T Value { get => base.Value; set => base.Value = value; }

    #endregion

    /// <see cref="Region.Constructor"/>

    public Item() : this("") { }

    public Item(T value) : this("", value) { }

    public Item(string name, T value) : this(name) => Value = value;

    public Item(string name, string description, T value) : this(name, value) => Description = description;

    /// <see cref="IItem"/>

    object IItem.Value => Value;

    Type IItem.ValueType => typeof(T);

    /// <see cref="IGeneric"/>

    Type IGeneric.GetGenericType() => Value is T i ? i.GetType() : typeof(T);
}