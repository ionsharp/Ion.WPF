using Ion.Controls;
using Ion.Reflect;
using System.ComponentModel;

namespace Ion.Core;

public static class XXObject
{
    #region GetGroup

    public static string GetGroup(this object i)
        => i.GetAttribute<GroupAttribute>()?.Name.ToString() ?? i.GetAttribute<CategoryAttribute>()?.Category ?? GroupAttribute.Default;

    #endregion

    #region GetImage

    public static string GetImage(this object i)
        => i?.GetAttribute<ImageAttribute>().IfNotNullGet(j => Resource.GetImage(j.Name, j.NameAssembly));

    #endregion
}
