using Ion.Collect;
using System;
using System.Xml.Serialization;

namespace Ion.Controls;

public record class DockLayoutGroup : DockLayoutElement
{
    [XmlArray]
    [XmlArrayItem("Element")]
    public ListObservable<DockLayoutElement> Elements { get => Get(new ListObservable<DockLayoutElement>()); set => Set(value); }

    [XmlAttribute]
    public Orient Orientation { get => Get(Orient.Horizontal); set => Set(value); }

    public DockLayoutGroup() : base() { }

    public DockLayoutGroup(Orient orientation) : base()
    {
        Orientation = orientation;
    }
}