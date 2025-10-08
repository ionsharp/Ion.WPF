using Ion.Data;
using Ion.Reflect;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ion.Controls;

public class MemberVisibilityBinding : MultiBind
{
    public MemberVisibilityBinding() : base()
    {
        ConverterType = typeof(MemberVisibilityConverter);

        Bindings.Add(new Binding(Paths.Dot));

        Bindings.Add(new Binding(nameof(Member.Parent) + "." + nameof(MemberBase.Style) + "[" + nameof(Styles.ObjectAttribute.FilterAccess) + "]"));
        Bindings.Add(new Binding(nameof(Member.Parent) + "." + nameof(MemberBase.Style) + "[" + nameof(Styles.ObjectAttribute.FilterType) + "]"));
        Bindings.Add(new Binding(nameof(Member.Parent) + "." + nameof(MemberBase.StyleModel) + "." + nameof(TemplateModelObject.SelectedTab)));
        Bindings.Add(new Binding(nameof(Member.Parent) + "." + nameof(MemberBase.StyleModel) + "." + nameof(TemplateModelObject.SelectedTabIndex)));
        Bindings.Add(new Binding(nameof(Member.Parent) + "." + nameof(MemberBase.Search)));
        Bindings.Add(new Binding(nameof(Member.Parent) + "." + nameof(MemberBase.SearchName)));
        Bindings.Add(new Binding(nameof(Member.Parent) + "." + nameof(MemberBase.SearchOptions)));
        Bindings.Add(new Binding(nameof(Member.Parent) + "." + nameof(MemberBase.SearchOptions) + "." + nameof(SearchOptions.Case)));
        Bindings.Add(new Binding(nameof(Member.Parent) + "." + nameof(MemberBase.SearchOptions) + "." + nameof(SearchOptions.Condition)));
        Bindings.Add(new Binding(nameof(Member.Parent) + "." + nameof(MemberBase.SearchOptions) + "." + nameof(SearchOptions.Word)));
        Bindings.Add(new Binding(nameof(Member.IsVisible)));

        Bindings.Add(new Binding(nameof(MemberBase.IsIndeterminate)));
        Bindings.Add(new Binding(nameof(MemberBase.Value)));
    }
}

public class MemberVisibilityConverter() : MultiValueConverter<Visibility>()
{
    public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return Visibility.Visible;
        if (values?.Length >= 1)
        {
            if (values[0] is Member model)
            {
                Visibility getResult()
                {
                    if (!model.IsVisible)
                        return Visibility.Collapsed;

                    if (model is IMemberAssignable && !model.IsIndeterminate && model.Style.GetValue(x => x.HideNull) && (model.Value is null || (model.Value is string i && i.IsWhite())))
                        return Visibility.Collapsed;

                    //Filter

                    if (!model.Parent.Style.GetValue<Styles.ObjectAttribute, Access>(i => i.FilterAccess).HasFlag(model.Access))
                        return Visibility.Collapsed;

                    if (!model.Parent.Style.GetValue<Styles.ObjectAttribute, MemberInstanceType>(i => i.FilterType).HasFlag(model.InstanceType))
                        return Visibility.Collapsed;

                    //Search
                    /*
                    var a = string.Empty;
                    var b = model.Parent.Search;

                    if (!b.IsEmpty())
                    {
                        switch (model.Parent.SearchName)
                        {
                            case SearchName.Group:
                                a = model.Group?.ToString() ?? string.Empty;
                                break;
                            case SearchName.Name:
                                a = model.Style[nameof(StyleAttribute.Name)]?.ToString() ?? string.Empty;
                                break;
                        }

                        var searchOptions = model.Parent.SearchOptions ?? new SearchOptions();
                        if (!searchOptions.Assert(a, b))
                            return Visibility.Collapsed;
                    }
                    */

                    //View
                    /*
                    if (model.Parent.Style.GetValue<Styles.ObjectAttribute, MemberViewType>(i => i.MemberViewType) == MemberViewType.Tab)
                    {
                        if (model.Tab is not null)
                        {
                            var x = model.Tab;
                            var y = model.Parent.StyleModel.To<TemplateModelObject>().SelectedTab;
                            return Equals($"{x}", $"{y}") ? Visibility.Visible : Visibility.Collapsed;
                        }
                    }
                    */

                    return Visibility.Visible;
                }
                var result = getResult();

                model.IsTrulyVisible = result == Visibility.Visible;
                return result;
            }
        }
        return Visibility.Collapsed;
    }
}