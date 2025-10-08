using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Ion.Controls;

[Serializable]
[XmlRoot("Layout")]
public record class DockLayout : DockLayoutRoot
{
    [XmlArray]
    [XmlArrayItem(ElementName = "Window")]
    public List<DockLayoutWindow> Floating { get => Get(new List<DockLayoutWindow>()); set => Set(value); }

    public DockLayout() : base() { }

    public T First<T>(DockLayoutGroup parent = null) where T : DockLayoutElement
    {
        parent ??= Root as DockLayoutGroup;
        if (parent is T t)
            return t;

        if (parent != null)
        {
            foreach (var i in parent.Elements)
            {
                if (i is T j)
                    return j;

                if (i is DockLayoutGroup layoutGroup)
                {
                    var result = First<T>(layoutGroup);
                    if (result != null)
                        return result;
                }
            }
        }
        return default;
    }
}