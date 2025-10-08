using Ion;
using Ion.Collect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ion.Reflect;

public class MemberList() : ProtectedCollection<IMemberStylable, IMemberStylable>()
{
    [Obsolete("To do: Line 49+")]
    public static IEnumerable<MemberInfo> GetMembers(Type i)
    {
        var type = Instance.AsType(i);

        var style
            = type.GetAttribute<StyleAttribute>() ?? new();
        var objectStyle
            = style as Styles.ObjectAttribute;

        return type.GetMembers(Instance.Flag.Public, Instance.Flag.Types).Where(j =>
        {
            var memberType = j.GetMemberType();

            if (j is FieldInfo field && !field.IsGettable())
                return false;

            if (j is MethodInfo method)
            {
                if (method.IsEvent() || method.IsGetter() || method.IsSetter())
                    return false;
            }

            if (j is PropertyInfo property && (property.IsIndexor() || !property.IsGettable()))
                return false;

            if (objectStyle?.IgnoreNames?.Contains(j.Name) == true)
                return false;

            if (objectStyle?.IgnoreTypes?.Contains(memberType) == true)
                return false;

            if (!j.HasAttribute<StyleAttribute>() && !j.HasAttribute<StyleInheritAttribute>())
                return false;

            /*
            if ((objectStyle?.Strict ?? MemberTypes.Event | MemberTypes.Method).HasFlag(j.MemberType))
            {
                var isImplicit = (!j.HasAttribute<BrowsableAttribute>() || !j.GetAttribute<BrowsableAttribute>().Browsable)
                //&& !member.HasAttribute<HideAttribute>()
                    && !j.HasAttribute<StyleAttribute>()
                    && !j.HasAttribute<StyleInheritAttribute>();

                if (isImplicit)
                    return false;
            }
            */

            if (Instance.IsHidden(j))
                return false;

            if (j.HasAttribute<MenuItemAttribute>())
                return false;

            return true;
        });
    }

    public override void Load(IMemberStylable model)
    {
        //1) Get type
        var type = model.ValueType;
        if (type is null) return;

        model.WriteLine(MemberLogType.StyleModel, $"Loading '{type.FullName}'");

        //2) Get type cache
        var cache = MemberBase.Cache[type];

        model.WriteLine(MemberLogType.StyleModel, $"There are '{cache.Members.Count}' members.");

        //3) Get type members from cache
        foreach (var member in cache.Members)
        {

            /// Get member if no view is specified or the view of the model matches the view of the member (only check member declaration!)
            if (member.Value.FirstOrDefault<StyleAttribute>() is StyleAttribute style)
            {
                if (model.View != View.None && !model.View.HasFlag(style.View))
                    continue;
            }

            model.WriteLine(MemberLogType.StyleModel, $"Member '{member.Key.Name}' has '{member.Value.Count}' attributes: {member.Value.ToString(", ")}");

            var modelType = member.Key.MemberType switch
            {
                MemberTypes.Event    => typeof(MemberEvent),
                MemberTypes.Field    => typeof(MemberField),
                MemberTypes.Method   => typeof(MemberMethod),
                MemberTypes.Property => typeof(MemberProperty),
            };

            modelType.IfNotNullGet(i => i.Create<IMemberStylable>(model, member.Value)).IfNotNull(Add);
        }
    }

    public override void Unload(IMemberStylable model)
    {
        model.WriteLine(MemberLogType.StyleModel);
        Clear();
    }
}