using Ion.Core;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Ion.Controls;

public abstract record class DockLayoutRoot : Model
{
    [XmlArray]
    [XmlArrayItem(ElementName = "Panel")]
    public List<DockLayoutPanel> Top { get => Get(new List<DockLayoutPanel>()); set => Set(value); }

    [XmlArray]
    [XmlArrayItem(ElementName = "Panel")]
    public List<DockLayoutPanel> Left { get => Get(new List<DockLayoutPanel>()); set => Set(value); }

    [XmlArray]
    [XmlArrayItem(ElementName = "Panel")]
    public List<DockLayoutPanel> Right { get => Get(new List<DockLayoutPanel>()); set => Set(value); }

    [XmlArray]
    [XmlArrayItem(ElementName = "Panel")]
    public List<DockLayoutPanel> Bottom { get => Get(new List<DockLayoutPanel>()); set => Set(value); }

    public DockLayoutElement Root { get => Get<DockLayoutElement>(); set => Set(value); }

    protected DockLayoutRoot() : base() { }
}