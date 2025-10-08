using Ion;
using Ion.Controls;
using Ion.Data;
using Ion.Imaging;
using Ion.Input;
using Ion.Numeral;
using Ion.Reflect;
using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Ion.Controls;

public class MenuModel(MenuBase menu) : object()
{
    private enum Level { Sub, Top }

    #region private class FlagParameterData

    private class FlagParameterData(string name, object source, Enum field)
    {
        public Enum Field = field;

        public string Name = name;

        public object Source = source;
    }

    #endregion

    /// <see cref="Region.Field"/>

    public const string DefaultGroup = "General";

    public readonly MenuBase Menu = menu;

    public object Source { get; private set; }

    /// <see cref="Region.Method.Private"/>
    #region

    private static void ApplyGroup(MenuList attribute, CollectionContainer container, object source)
    {
        if (attribute.GroupSource is not null)
        {
            var groupSource = Try.Get(() => Instance.GetPropertyValue(source, attribute.GroupSource));
            if (groupSource is not null)
            {
                container.Bind(XCollectionContainer.GroupDirectionProperty,
                    attribute.GroupDirectionPath,
                    groupSource,
                    BindingMode.OneWay);
                container.Bind(XCollectionContainer.GroupNameProperty,
                    attribute.GroupNamePath,
                    groupSource,
                    BindingMode.OneWay);
            }
        }
        else if (attribute.GroupName is not null)
        {
            XCollectionContainer.SetGroupDirection(container, attribute.GroupDirection);
            XCollectionContainer.SetGroupName(container, attribute.GroupName);
        }
    }

    private static void ApplySort(MenuList attribute, CollectionContainer container, object source)
    {
        if (attribute.SortSource is not null)
        {
            var sortSource = Try.Get(() => Instance.GetPropertyValue(source, attribute.SortSource));
            if (sortSource is not null)
            {
                container.Bind(XCollectionContainer.SortDirectionProperty,
                    attribute.SortDirectionPath,
                    sortSource,
                    BindingMode.OneWay);
                container.Bind(XCollectionContainer.SortNameProperty,
                    attribute.SortNamePath,
                    sortSource,
                    BindingMode.OneWay);
            }
        }
        else if (attribute.SortName is not null)
        {
            XCollectionContainer.SetSortDirection(container, attribute.SortDirection);
            XCollectionContainer.SetSortName(container, attribute.SortName);
        }
    }

    /// <see cref="MenuItem"/>

    private MenuItem GetItem(Level level, MenuItemAttribute attribute, CompositeCollection composite, PropertyInfo property, Style style, Type type)
    {
        var value = property.GetValue(Source);

        //(1)   Boolean
        if (value is bool a && level == Level.Sub)
            return GetItemBoolean(level, attribute, property, a);

        //(2.a) Command
        if (value is MenuCommand b)
            return GetItemCommand(level, attribute, b.Command, property, Source);

        //(2.b) Command
        else if (value is ICommand c)
            return GetItemCommand(level, attribute, c, property, Source);

        //(3)   Object
        else if (value is MenuObject)
            return GetItemObject(level, attribute, property, type);

        else if (value is not null)
        {
            //(4) Enum
            if (attribute is MenuEnum)
                return GetItemEnum(level, attribute as MenuEnum, composite, property, (Enum)value, level == Level.Top ? false : null);

            //(5) List
            else if (attribute is MenuList)
                return GetItemList(level, attribute as MenuList, composite, property, style, level == Level.Top ? false : null);
        }

        return null;
    }

    private MenuItem GetItemBase(object source, object member, MenuItemAttribute attribute)
        => new() { Style = GetItemStyle(source, member, attribute) };

    private void GetItems(Level level, MenuItemAttribute attribute, CompositeCollection composite, PropertyInfo property, Style style, Type type)
    {
        Dictionary<MenuItemAttribute, PropertyInfo> members = [];
        //(1) From fields or properties of class
        type.GetProperties(Instance.Flag.PublicDeclared)
            .Where
            (i =>
            {
                //property = parent
                //       i = child

                var a = property.GetValue(Source) as MenuObject;
                var b = i.GetValue(Source) as MenuItemAttribute;

                var x = property.GetAttribute<MenuItemAttribute>();
                var y = i.GetAttribute<MenuItemAttribute>();

                //Parent = MenuItemObject
                if (a is not null)
                {
                    //Child = MenuItemAttribute
                    if (b is not null)
                    {
                        //MenuItemObject > MenuItemAttribute
                        return b.Parent?.Equals(property.Name) == true;
                    }
                    //Child ~ MenuItemAttribute
                    else if (y is not null)
                    {
                        //MenuItemObject > MenuItemAttribute
                        return y.Parent?.Equals(property.Name) == true;
                    }
                }
                //Parent ~ MenuItemAttribute
                else if (x is not null)
                {
                    //Child = MenuItemAttribute
                    if (b is not null)
                    {
                        //MenuItemAttribute > MenuItemAttribute
                        return b.Parent?.Equals(property.Name) == true;
                    }
                    //Child ~ MenuItemAttribute
                    else if (y is not null)
                    {
                        //MenuItemAttribute > MenuItemAttribute
                        return y.Parent?.Equals(property.Name) == true;
                    }
                }
                return false;
            })
            .ForEach(i =>
            {
                var getMethod = property.GetGetMethod(false);
                if (getMethod.GetBaseDefinition() != getMethod)
                {
                    //It's an overriden property

                }
                else
                {
                    //It's NOT an overriden property
                }

                var j = i.GetValue(Source) as MenuItemAttribute ?? i.GetAttribute<MenuItemAttribute>();
                if (j is not null)
                {
                    if (!members.ContainsKey(j))
                        members.Add(j, i);
                }
            });

        var groups = members.Select(i => i.Key)
            .OrderBy(i => i.Group is null ? 0 : 1)
            .ThenBy(GetGroupIndex).ThenBy(GetGroupName).ThenBy(GetSubGroup)
            .ThenBy(i => i.Index).ThenBy(i =>
            {
                if (members.ContainsKey(i))
                    return GetHeader(i, members[i]);

                return "";
            })
            .GroupBy(i => i.Group);

        var groupIndex = 0;
        foreach (var group in groups)
        {
            var groupName = GetGroupName(group.Key); ;
            if (groupName != DefaultGroup)
            {
                if (groups.Count() > 1)
                {
                    var separator = new Separator();
                    XSeparator.SetHeader(separator, groupName);
                    composite.Add(separator);

                    if (group.Key is Enum e)
                    {
                        if (e.GetAttribute<ImageAttribute>() is ImageAttribute imageAttribute)
                            XSeparator.SetIcon(separator, Resource.GetImageUri(imageAttribute.Name, imageAttribute.NameAssembly));
                    }
                }
            }

            var subGroups = new List<string>();
            group.ForEach(x =>
            {
                var y = $"{x.SubGroup}";
                if (!subGroups.Contains(y))
                    subGroups.Add(y);
            });

            var index = 0;
            subGroups.ForEach((Action<string>)(subGroup =>
            {
                if (index > 0)
                    composite.Add(new Separator());

                var subGroupMembers = group.Where(i => $"{i.SubGroup}".Equals(subGroup));
                subGroupMembers.ForEach(i =>
                {
                    if (members.ContainsKey(i))
                    {
                        var item = GetItem(Level.Sub, i, composite, members[i], style, type);
                        item.IfNotNull(j => composite.Add(j));
                    }
                });
                index++;
            }));
            groupIndex++;
        }
    }

    ///

    private MenuItem GetItemBoolean(Level level, MenuItemAttribute attribute, MemberInfo member, bool value)
    {
        var result = GetItemBase(Source, member, attribute);
        result.IsCheckable = true;
        result.IsChecked = value;

        result.Bind(MenuItem.IsCheckedProperty, member.Name, Source, BindingMode.TwoWay);
        return result;
    }

    private MenuItem GetItemClear(CollectionContainer container)
    {
        var item = new MenuItem();
        item.Bind(MenuItem.CommandParameterProperty, nameof(CollectionContainer.Collection), container);
        item.Bind(MenuItem.StyleProperty, new PropertyPath("(0)", XMenuBase.ItemClearStyleProperty), Menu);
        item.Command = new RelayCommand<ListCollectionView>(i => i.SourceCollection.As<IList>().Clear(), i => i.SourceCollection is IList j && j.Count > 0);
        return item;
    }

    private MenuItem GetItemCommand(Level level, MenuItemAttribute attribute, ICommand command, object member, object source)
    {
        var result = GetItemBase(source, member, attribute);
        result.Command = command;

        attribute.HideIfDisabled.If(() => result.Bind(UIElement.VisibilityProperty, nameof(FrameworkElement.IsEnabled), result, BindingMode.OneWay, new ConvertBooleanToVisibility()));

        if (level == Level.Sub)
        {
            if (attribute.CanSlide)
            {
                result.Header = GetSlider(attribute, source);
                result.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            }
        }
        return result;
    }

    private MenuItem GetItemEnum(Level level, MenuEnum attribute, CompositeCollection composite, MemberInfo member, Enum value, bool? overrideInline = null)
    {
        MenuItem result = null;

        var isFlag = attribute?.IsFlag ?? false;
        var isInline = overrideInline ?? attribute?.IsInline ?? false;

        if (!isInline)
            result = GetItemBase(Source, member, attribute);

        IEnumerable<Enum> fields = value.GetType().GetEnumValues().Cast<Enum>();
        switch (attribute?.SortName ?? MemberSortName.DeclarationOrder)
        {
            case MemberSortName.DeclarationOrder:
                //fields = fields;
                break;

            case MemberSortName.Name:
                fields = fields.OrderBy(i => i.ToString());
                break;
        }

        List<MenuItem> items = [];
        foreach (Enum i in fields)
        {
            var j = new MenuItem()
            {
                Header = Instance.GetName(i) ?? $"{i}",
                Icon = null,
                IsCheckable = true,
                IsChecked = isFlag ? value.HasFlag(i) : Equals(i, value),
                StaysOpenOnClick = attribute.ItemStaysOpenOnClick,
                ToolTip = Instance.GetDescription(i)
            };

            Func<ValueConverterInput<Enum>, ValueConverterOutput<bool>> aConvert = null;
            Func<ValueConverterInput<bool>, ValueConverterOutput<Enum>> bConvert = null;

            if (isFlag)
            {
                aConvert = k =>
                {
                    var x = k.ActualParameter as FlagParameterData;

                    Enum a = k.Value;
                    Enum b = x.Field;
                    return a.HasFlag(b);
                };
                bConvert = k =>
                {
                    var x = k.ActualParameter as FlagParameterData;

                    Enum a = (Enum)Instance.GetPropertyValue(x.Source, x.Name);
                    Enum b = x.Field;

                    return k.Value ? a.AddFlag(b) : a.RemoveFlag(b);
                };
            }
            else
            {
                aConvert = k =>
                {
                    var x = k.ActualParameter as FlagParameterData;

                    Enum a = k.Value;
                    Enum b = x.Field;
                    return Equals(a, b);
                };
                bConvert = k =>
                {
                    var x = k.ActualParameter as FlagParameterData;

                    Enum b = x.Field;
                    return k.Value ? b : No.Thing;
                };
            }

            var convert
                = new ValueConverter<Enum, bool>(false, aConvert, bConvert);
            var convertData
                = new FlagParameterData(member.Name, Source, i);

            j.Bind(MenuItem.IsCheckedProperty, member.Name, Source, BindingMode.TwoWay, convert, convertData);
            items.Add(j);
        }

        items.ForEach(i =>
        {
            if (isInline)
                composite.Add(i);

            else result.Items.Add(i);
        });

        return result;
    }

    private MenuItem GetItemList(Level level, MenuList attribute, CompositeCollection composite, MemberInfo member, Style style, bool? overrideInline = null)
    {
        MenuItem item = null;

        var itemType = attribute.ItemType;
        /*
        if (itemType is null)
        {
            var itemTypes = list.SourceCollection.Select(i => i.GetType());
            itemType = TypeCache.GetSharedType(itemTypes);
        }
        */
        if (itemType is not null)
        {
            var container
                = new CollectionContainer();
            container.Bind(CollectionContainer.CollectionProperty, new Bind(member.As<MemberInfo>().Name) { Convert = typeof(ConvertToListCollectionView), Source = Source });

            var trigger
                = GetItemTrigger(itemType, member, Source, attribute);

            bool isInline = overrideInline ?? attribute.IsInline;
            if (isInline)
            {
                style.Triggers.Add(trigger);

                composite.Add(GetItemPlaceholder(container));
                composite.Add(container);

                //Grouping/sorting needs done manually here. Not supported yet!
            }
            else
            {
                var itemStyle = GetBaseStyle();
                itemStyle.Triggers.Add(trigger);

                item = GetItemBase(Source, member, attribute);
                item.ItemContainerStyle = itemStyle;

                composite =
                [
                    GetItemPlaceholder(container),
                    container
                ];

                ApplyGroup(attribute, container, Source);
                ApplySort(attribute, container, Source);
            }

            if (attribute.CanClear)
            {
                composite.Add(new Separator());
                composite.Add(GetItemClear(container));
            }

            item.IfNotNull(i => i.ItemsSource = composite);
        }
        return item;
    }

    private MenuItem GetItemObject(Level level, MenuItemAttribute attribute, PropertyInfo property, Type type)
    {
        var result = GetItemBase(Source, property, attribute);

        var composite
            = new CompositeCollection();
        var style
            = GetBaseStyle();

        GetItems(Level.Sub, attribute, composite, property, style, type);

        result.ItemsSource = composite;
        result.Resources.Add(typeof(MenuItem), style);
        return result;
    }

    private MenuItem GetItemPlaceholder(CollectionContainer container)
    {
        var item = new MenuItem();
        item.Bind(MenuItem.StyleProperty, new PropertyPath("(0)", XMenuBase.ItemPlaceholderStyleProperty), Menu, BindingMode.OneWay, new ValueConverter<object, Style>(i => i.Value is Style j ? j : GetBaseStyle()));
        item.Bind(MenuItem.VisibilityProperty, new Compare32(nameof(CollectionContainer.Collection) + "." + nameof(ListCollectionView.SourceCollection) + "." + nameof(IList.Count)) { Result = Compare.Results.Visibility, Source = container, Type = Comparison.Equal, Value = 0 });
        return item;
    }

    private static object GetSlider(MenuItemAttribute attribute, object source)
    {
        #region Old
        /*
        item = new MenuItemSlider();
        item.Bind(MenuItemSlider.ValueProperty, attribute.SlidePath, source, BindingMode.TwoWay);

        var e = attribute.SlideMinimum;
        while (e <= attribute.SlideMaximum)
        {
            var f = new MenuItem() { Header = attribute.SlideHeader.F(e), Style = GetDefaultStyle() };
            MenuItemSlider.SetValue(f, e);
            MenuItemSlider.SetSteps(f, attribute.SlideStep);
            item.Items.Add(f);
            e += attribute.SlideMaximum / attribute.SlideCut;
        }
        */
        #endregion
        #region New
        var result = new StackPanel();

        var slider = new Slider() { Maximum = attribute.SlideMaximum, Minimum = attribute.SlideMinimum, TickFrequency = attribute.SlideTick };
        slider.Bind(Slider.ValueProperty, attribute.SlidePath, source, BindingMode.TwoWay);

        result.Children.Add(new TextBlock() { Text = attribute.Header });
        result.Children.Add(slider);
        return result;
        #endregion
    }

    /// <see cref="Style"/>

    private static void AddDefaultToolTipHeader(SetterBaseCollection setters)
    {
        setters.Add(new System.Windows.Setter(XToolTip.HeaderProperty, new Bind(nameof(MenuItem.Header)) { From = RelativeSourceMode.Self, Mode = BindingMode.OneWay }));
    }

    private static void AddDefaultToolTipHeaderIcon(SetterBaseCollection setters)
    {
        setters.Add(new System.Windows.Setter(XToolTip.HeaderIconProperty, new Bind(nameof(MenuItem.Icon)) { From = RelativeSourceMode.Self, Mode = BindingMode.OneWay }));
    }

    private static void AddDefaultToolTipTemplate(SetterBaseCollection setters, object member)
    {
        setters.Add(new System.Windows.Setter(MenuItem.ToolTipProperty, member));
        setters.Add(new System.Windows.Setter(XToolTip.TemplateProperty, new DynamicResourceExtension(ObjectControlKey.ObjectToolTip)));
    }

    ///

    private static bool AddImage(SetterBaseCollection setters, object icon)
    {
        if (icon is Images x)
        {
            setters.Add(new System.Windows.Setter(MenuItem.IconProperty, Try.Get(() => XImageSource.Convert(Resource.GetImageUri(x)))));
            return true;
        }
        else if (icon is string y)
        {
            setters.Add(new System.Windows.Setter(MenuItem.IconProperty, Try.Get(() => XImageSource.Convert(Resource.GetImageUri(y, AssemblyProject.Main)))));
            return true;
        }
        else if (icon is System.Uri z)
        {
            setters.Add(new System.Windows.Setter(MenuItem.IconProperty, Try.Get(() => XImageSource.Convert(z))));
            return true;
        }
        return false;
    }

    private static void AddTemplate(SetterBaseCollection setters, DependencyProperty contentProperty, DependencyProperty contentTemplateProperty, string contentPath, Type contentConverter, object contentConverterParameter, Type contentTemplate, string contentTemplateKey)
    {
        if (contentPath is not null)
        {
            setters.Add(new System.Windows.Setter()
            {
                Property = contentProperty,
                Value = new Bind(contentPath) { Convert = contentConverter }
            });
            if (contentTemplate is not null)
            {
                setters.Add(new System.Windows.Setter()
                {
                    Property = contentTemplateProperty,
                    Value = new DynamicResourceExtension(contentTemplate.GetField(contentTemplateKey).GetValue(null))
                });
            }
        }
    }

    ///

    private Style GetBaseStyle() => new(typeof(MenuItem), (Style)Menu.FindResource(typeof(MenuItem)));

    private Style GetItemStyle(object source, object member, MenuItemAttribute attribute)
    {
        var result = GetBaseStyle();
        #region Header
        if (attribute.HeaderBinding is not null)
            result.Setters.Add(new System.Windows.Setter(MenuItem.HeaderProperty, Try.Get(() => attribute.HeaderBinding.Create<object>())));

        else if (attribute.HeaderPath is not null)
            result.Setters.Add(new System.Windows.Setter(MenuItem.HeaderProperty, attribute.HeaderConverter is not null ? new Bind(attribute.HeaderPath) { Convert = attribute.HeaderConverter } : new MultiBindLocalize(attribute.HeaderPath)));

        else result.Setters.Add(new System.Windows.Setter(MenuItem.HeaderProperty, new TextExtension(attribute.Header ?? $"{(member is MemberInfo i ? i.Name : member?.ToString())}") { Prefix = attribute.HeaderPrefix, Suffix = attribute.HeaderSuffix }));
        #endregion
        #region Icon
        if (attribute.IconCollapse)
            result.Setters.Add(new System.Windows.Setter(XMenuItem.IconVisibilityProperty, Visibility.Collapsed));

        else if (!AddImage(result.Setters, attribute.Icon))
            AddTemplate(result.Setters, MenuItem.IconProperty, XMenuItem.IconTemplateProperty, attribute.IconPath, attribute.IconConverter, null, attribute.IconTemplate, attribute.IconTemplateField);
        #endregion
        #region InputGestureText
        AddTemplate(result.Setters, MenuItem.InputGestureTextProperty, XMenuItem.InputGestureTextTemplateProperty,
            attribute.InputGestureTextPath,
            attribute.InputGestureTextConverter,
            attribute.InputGestureTextConverterParameter,
            attribute.InputGestureTextTemplate,
            attribute.InputGestureTextTemplateField);
        #endregion
        #region StaysOpenOnClick
        result.Setters.Add(new System.Windows.Setter(MenuItem.StaysOpenOnClickProperty, attribute.StaysOpenOnClick));
        #endregion
        #region ToolTip
        if (attribute.ToolTip is not null)
            result.Setters.Add(new System.Windows.Setter(MenuItem.ToolTipProperty, attribute.ToolTip));

        else if (attribute.ToolTipPath is not null)
        {
            AddTemplate(result.Setters, MenuItem.ToolTipProperty, XToolTip.TemplateProperty,
                attribute.ToolTipPath,
                null, null,
                attribute.ToolTipTemplate,
                attribute.ToolTipTemplateField);
        }
        else AddDefaultToolTipTemplate(result.Setters, member);

        if (attribute.ToolTipHeaderPath is not null)
        {
            AddTemplate(result.Setters, XToolTip.HeaderProperty, XToolTip.HeaderTemplateProperty,
                attribute.ToolTipHeaderPath,
                null, null,
                attribute.ToolTipHeaderTemplate,
                attribute.ToolTipHeaderTemplateField);
        }
        else AddDefaultToolTipHeader(result.Setters);

        if (attribute.ToolTipHeaderIconPath is not null)
        {
            AddTemplate(result.Setters, XToolTip.HeaderIconProperty, XToolTip.HeaderIconTemplateProperty,
                attribute.ToolTipHeaderIconPath,
                null, null,
                attribute.ToolTipHeaderIconTemplate,
                attribute.ToolTipHeaderIconTemplateField);
        }
        else AddDefaultToolTipHeaderIcon(result.Setters);
        #endregion
        return result;
    }

    private static DataTrigger GetItemTrigger(Type itemType, object member, object source, MenuList attribute)
    {
        var result = new DataTrigger() { Binding = new Is(Paths.Dot, itemType), Value = true };
        #region Command
        if (attribute.ItemCommand is not null)
            result.Setters.Add(new System.Windows.Setter(MenuItem.CommandProperty, Try.Get(() => Instance.GetPropertyValue(source, attribute.ItemCommand))));

        if (attribute.ItemCommandParameterPath is not null)
            result.Setters.Add(new System.Windows.Setter(MenuItem.CommandParameterProperty, new Binding(attribute.ItemCommandParameterPath)));
        #endregion
        #region Header
        if (attribute.ItemHeaderBinding is not null)
            result.Setters.Add(new System.Windows.Setter(MenuItem.HeaderProperty, Try.Get(() => attribute.ItemHeaderBinding.Create<object>())));

        else if (attribute.ItemHeaderPath is not null)
            result.Setters.Add(new System.Windows.Setter(MenuItem.HeaderProperty, attribute.ItemHeaderConverter is not null ? new Bind(attribute.ItemHeaderPath) { Convert = attribute.ItemHeaderConverter, ConverterParameter = attribute.ItemHeaderConverterParameter } : new MultiBindLocalize(attribute.ItemHeaderPath)));

        else result.Setters.Add(new System.Windows.Setter(MenuItem.HeaderProperty, new TextExtension(attribute.ItemHeader) { Prefix = attribute.ItemHeaderPrefix, Suffix = attribute.ItemHeaderSuffix }));
        #endregion
        #region Icon
        if (attribute.ItemIconCollapse)
            result.Setters.Add(new System.Windows.Setter(XMenuItem.IconVisibilityProperty, Visibility.Collapsed));

        else if (!AddImage(result.Setters, attribute.ItemIcon))
            AddTemplate(result.Setters, MenuItem.IconProperty, XMenuItem.IconTemplateProperty, attribute.ItemIconPath, attribute.ItemIconConverter, null, attribute.ItemIconTemplate, attribute.ItemIconTemplateField);
        #endregion
        #region InputGestureText
        AddTemplate(result.Setters, MenuItem.InputGestureTextProperty, XMenuItem.InputGestureTextTemplateProperty,
            attribute.ItemInputGestureTextPath,
            attribute.ItemInputGestureTextConverter,
            attribute.ItemInputGestureTextConverterParameter,
            attribute.ItemInputGestureTextTemplate,
            attribute.ItemInputGestureTextTemplateField);
        #endregion
        #region IsCheckable
        if (attribute.ItemCheckable)
        {
            result.Setters.Add(new System.Windows.Setter(MenuItem.IsCheckableProperty, true));
            if (attribute.ItemCheckablePath is not null)
                result.Setters.Add(new System.Windows.Setter(MenuItem.IsCheckedProperty, new Binding() { Path = new(attribute.ItemCheckablePath), Mode = (BindingMode)attribute.ItemCheckableMode }));
        }
        #endregion
        #region StaysOpenOnClick
        result.Setters.Add(new System.Windows.Setter(MenuItem.StaysOpenOnClickProperty, attribute.ItemStaysOpenOnClick));
        #endregion
        #region ToolTip
        if (attribute.ItemToolTipPath is not null)
        {
            AddTemplate(result.Setters, MenuItem.ToolTipProperty, XToolTip.TemplateProperty,
                attribute.ItemToolTipPath,
                null, null,
                attribute.ItemToolTipTemplate,
                attribute.ItemToolTipTemplateField);
        }
        else AddDefaultToolTipTemplate(result.Setters, member);

        AddDefaultToolTipHeader(result.Setters);
        AddDefaultToolTipHeaderIcon(result.Setters);
        #endregion
        return result;
    }

    #endregion

    /// <see cref="Region.Method.Public"/>
    #region

    public void Load(object source)
    {
        var composite = new CompositeCollection();
        composite.Clear();

        Menu.ItemsSource ??= composite;
        Source = source;

        if (source is null) return;

        var type = source.GetType();

        var menu = type.GetAttribute<MenuAttribute>() ?? new MenuAttribute();

        List<Type> types = [type];
        if (menu.SortByClass)
            type.GetBaseTypes().ForEach(types.Add);

        var count = types.Count;
        for (var index = 0; index < count; index++)
        {
            type = types.ElementAt(index);

            var properties = new Dictionary<MenuItemAttribute, PropertyInfo>();
            type.GetProperties(Instance.Flag.Public)
                .Where(i => i.DeclaringType == type)
                .ForEach(i =>
                {
                    ///(1) From properties defined by class with type <see cref="MenuObject">.
                    if (i.GetValue(source) is MenuItemAttribute j && j.Parent is null)
                        properties.Add(j, i);

                    ///(2) From fields or properties defined by class marked with <see cref="MenuItemAttribute">.
                    else if (i.GetAttribute<MenuItemAttribute>() is MenuItemAttribute k && k.Parent is null)
                        properties.Add(i.GetAttribute<MenuItemAttribute>(), i);
                });


            var attributes = properties.Select(i => i.Key);
            attributes = attributes
                .OrderBy(i => i.Index)
                .ThenBy(i => i.Header ?? (properties[i] is MemberInfo j ? j.Name : properties[i].ToString()));

            if (!attributes.Any())
                return;

            int added = 0;
            foreach (MenuItemAttribute attribute in attributes)
            {
                var property = properties[attribute];
                if (attribute is MenuObject aa)
                {
                    if (aa.Unpack)
                    {
                        GetItems(Level.Top, attribute, composite, property, Menu.Style, type);
                        continue;
                    }
                }

                var item = GetItem(Level.Top, attribute, null, property, null, type);
                if (item is not null)
                {
                    composite.Add(item);
                    added++;
                }
            }

            if (added > 0 && menu.SortByClass)
                composite.Add(new Separator());
        }

        composite.Last().If<Separator>(i => composite.RemoveAt(composite.Count - 1));
    }

    #endregion

    /// <see cref="Region.Method.Static"/>
    #region

    private static int GetGroupIndex(MenuItemAttribute i)
    {
        if (i.GroupIndex is int j)
            return j;

        return 0;
    }

    private static string GetGroupName(object input)
    {
        if (input is MenuItemAttribute i)
            input = i.Group;

        if (input is Enum j)
            return Instance.GetName(j) ?? $"{j}";

        return input?.ToString() ?? DefaultGroup;
    }

    private static string GetHeader(MenuItemAttribute i, object j)
        => i.Header ?? (j is MemberInfo k ? k.Name : j?.ToString());

    private static int GetSubGroup(MenuItemAttribute input)
    {
        if (input.SubGroup is int i)
            return i;

        return 0;
    }

    #endregion
}

public enum MenuSort { Header, Index }