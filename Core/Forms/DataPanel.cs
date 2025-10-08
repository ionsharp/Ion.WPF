using Ion.Analysis;
using Ion.Collect;
using Ion.Imaging;
using Ion.Numeral;
using Ion.Reflect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Media;

namespace Ion.Core;

[Styles.Object(GroupName = MemberGroupName.None, Filter = Filter.None,
    Strict = MemberTypes.All)]
public record class DataPanelForm : Model
{
    /// <see cref="Region.Field"/>
    #region

    private static readonly Dictionary<Type, Type[]> ConversionTypes = new()
    {
        { typeof(ByteVector4),
            new Type[]
            {
                typeof(Color),
                typeof(SolidColorBrush)
            }
        },
        { typeof(Color),
            new Type[]
            {
                typeof(ByteVector4),
                typeof(SolidColorBrush)
            }
        },
        { typeof(SolidColorBrush),
            new Type[]
            {
                typeof(ByteVector4),
                typeof(Color)
            }
        },
    };

    #endregion

    /// <see cref="Region.Property"/>
    #region

    public object Group { get => Get<object>(); set => Set(value); }

    public int GroupIndex { get => Get(-1); set => Set(value); }

    [Styles.List(Template.ListCombo,
        Index = 1,
        Placeholder = "Select a group",
        SelectedIndexProperty = nameof(GroupIndex),
        SelectedItemProperty = nameof(Group))]
    [Styles.Text(CanEdit = false,
        ValuePath = nameof(IItemGroup.Name),
        TargetItem = typeof(IItemGroup))]
    [Name("Group")]
    [VisibilityTrigger(nameof(GroupCount), Comparison.Greater, (object)0)]
    public IList Groups { get => Get<IList>(); private set => Set(value); }

    public int GroupCount => Groups?.Count ?? 0;

    public object Panel { get => Get<object>(); set => Set(value); }

    public int PanelIndex { get => Get(-1); set => Set(value); }

    [Styles.List(Template.ListCombo,
        Index = 0,
        Placeholder = "Select a panel",
        SelectedIndexProperty = nameof(PanelIndex),
        SelectedItemProperty = nameof(Panel))]
    [Styles.Text(CanEdit = false,
        ValuePath = nameof(Core.Panel.NameRaw),
        TargetItem = typeof(Core.Panel))]
    [Name("Panel")]
    public ListObservable<Panel> Panels { get => Get(new ListObservable<Panel>()); private set => Set(value); }

    public DataPanel SourcePanel { get => Get<DataPanel>(); private set => Set(value); }

    public IList TargetList { get => Get<IList>(); private set => Set(value); }

    public DataPanel TargetPanel { get => Get<DataPanel>(); private set => Set(value); }

    #endregion

    /// <see cref="Region.Constructor"/>

    public DataPanelForm(DataPanel panel, IEnumerable<Panel> panels) : base()
    {
        SourcePanel = panel;
        foreach (var i in panels)
        {
            if (i is DataPanel j && CanCopyTo(SourcePanel, j))
                Panels.Add(j);
        }
    }

    /// <see cref="Region.Method.Private"/>
    #region

    private static Type GetTargetInterface(Type x, Type y)
    {
        foreach (Type i in x.GetInterfaces())
        {
            if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConvert<>))
            {
                var j = i.GetGenericArguments()[0];
                if (j == y)
                    return i;
            }
        }
        return null;
    }

    private static bool CanConvertTo(Type a, Type b)
        => ConversionTypes.ContainsKey(a) && ConversionTypes[a] is Type[] x && x.Contains(b);

    private bool CanCopyTo(DataPanel a, DataPanel b)
    {
        if (a.CanCopy)
        {
            if (b.CanPaste)
            {
                if (a.ItemType == b.ItemType)
                    return true;

                var x = a is IDataGroupPanel aPanel ? aPanel.ItemValueType : a.ItemType;
                var y = b is IDataGroupPanel bPanel ? bPanel.ItemValueType : b.ItemType;

                //Does a default conversion exist for (x) and (y)?
                if (CanConvertTo(x, y)) return true;

                //Does (x) implement (IConvert<T>) where (T) = (y)?
                if (GetTargetInterface(x, y) is not null) return true;

                //Does (y) implement (IConvert<T>) where (T) = (x)?
                if (GetTargetInterface(y, x) is not null) return true;
            }
        }
        return false;
    }

    private object ConvertTo(object item, Type targetType)
    {
        var sourceType = item.GetType();

        /// <see cref="IConvert"/>

        if (sourceType.Implements<IConvert>())
        {
            if (GetTargetInterface(sourceType, targetType) is Type e)
                return e.GetMethod(nameof(IConvert<Object>.ConvertTo)).Invoke(item, null);
        }
        if (targetType.Implements<IConvert>())
        {
            if (GetTargetInterface(targetType, sourceType) is Type e)
                return e.GetMethod(nameof(IConvert<Object>.ConvertBack)).Invoke(item, null);
        }

        ///Default

        if (item is ByteVector4 a)
        {
            //Byte4 - Color
            if (targetType == typeof(Color))
                return XColor.Convert(a);

            //Byte4 - SolidColorBrush
            if (targetType == typeof(SolidColorBrush))
                return new SolidColorBrush(XColor.Convert(a));
        }
        if (item is Color b)
        {
            //Color - Byte4
            if (targetType == typeof(Color))
            {
                XColor.Convert(b, out ByteVector4 bb);
                return bb;
            }

            //Color - SolidColorBrush
            if (targetType == typeof(SolidColorBrush))
                return new SolidColorBrush(b);
        }
        if (item is SolidColorBrush c)
        {
            //SolidColorBrush - Byte4
            if (targetType == typeof(Color))
            {
                XColor.Convert(c.Color, out ByteVector4 cc);
                return cc;
            }

            //SolidColorBrush - Color
            if (targetType == typeof(SolidColorBrush))
                return c.Color;
        }

        return null;
    }

    private void CopyTo(IReadOnlyCollection<object> items, bool remove)
    {
        if (TargetList is not null)
        {
            foreach (var j in items)
            {
                object a = null, b = null;

                Try.Do(() =>
                {
                    //A != B
                    if (SourcePanel.ItemType != TargetPanel.ItemType)
                    {
                        if (SourcePanel.ItemType.Implements<IItem>())
                        {
                            //✓) [a != b] GroupItem<a> - GroupItem<b>
                            if (TargetPanel.ItemType.Implements<IItem>())
                            {
                                //✓) Convert (GroupItem<a>) to (a)
                                a = j.As<IItem>().Value;
                                //✓) Convert (a) to (b)
                                b = ConvertTo(a, TargetPanel.ItemValueType);
                                //✓) Convert (b) to (GroupItem<b>)
                                b = TargetPanel.As<IDataGroupPanel>()
                                    .Create(j.As<IItem>().Name, j.As<IItem>().Description, b);
                            }
                            //✓) [a ~= b] GroupItem<a> - b
                            else
                            {
                                //✓) Convert (GroupItem<a>) to (a)
                                a = j.As<IItem>().Value;
                                //✓) Convert (a) to (b)
                                b = ConvertTo(a, TargetPanel.ItemType);
                            }
                        }
                        else
                        {
                            a = j;
                            //✓) [a ~= b] a - GroupItem<b>
                            if (TargetPanel.ItemType.Implements<IItem>())
                            {
                                //✓) Convert (a) to (b)
                                b = ConvertTo(a, TargetPanel.ItemValueType);
                                //✓) Convert (b) to (GroupItem<b>)
                                b = TargetPanel.As<IDataGroupPanel>()
                                    .Create(b);
                            }
                            //✓) [a != b] a - b
                            else
                            {
                                //✓) Convert (a) to (b)
                                b = ConvertTo(a, TargetPanel.ItemType);
                            }
                        }
                    }
                    //A == B
                    else
                    {
                        //Clone (a) - (b)
                        a = j;
                        b = Instance.CloneDeep(a, new CreateFromObject());
                    }
                },
                e => Log.Write(e));

                b.IfNotNull(k =>
                {
                    if (remove)
                    {
                        SourcePanel.Items.Remove(j);
                    }

                    TargetList.Add(b);
                });
            }
        }
    }

    #endregion

    /// <see cref="Region.Method.Public"/>
    #region

    public void CopyTo(IReadOnlyCollection<object> items) => CopyTo(items, false);

    public void MoveTo(IReadOnlyCollection<object> items) => CopyTo(items, true);

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        switch (e.PropertyName)
        {
            case nameof(Groups):
                Reset(() => GroupCount);
                break;

            case nameof(Panel):
                TargetPanel = Panel is DataPanel dataPanel
                    ? dataPanel : null;

                TargetList
                    = TargetPanel is IDataGroupPanel ? Group as IList : TargetPanel?.Items;

                if (Panel is IDataGroupPanel panel)
                    Groups = panel.Groups as IList;
                break;
        }
    }

    #endregion
}