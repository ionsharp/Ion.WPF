using Ion;
using Ion.Collect;
using Ion.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Ion.Controls;

[Serializable]
public record class DockLayoutPanelGroup : DockLayoutElement
{
    [XmlAttribute]
    public bool Collapse { get => Get(false); set => Set(value); }

    [XmlArray]
    public List<DockLayoutPanel> Panels { get => Get(new List<DockLayoutPanel>()); set => Set(value); }

    public DockLayoutPanelGroup() : base() { }

    public DockLayoutPanelGroup(IEnumerable<DockLayoutPanel> input) : base() => input?.ForEach(Panels.Add);

    public DockLayoutPanelGroup(params DockLayoutPanel[] input) : this(input as IEnumerable<DockLayoutPanel>) { }

    public DockLayoutPanelGroup(params Panel[] input) : this(input?.ToArray(i => new DockLayoutPanel(i.Name))) { }

    public DockLayoutPanelGroup(IEnumerable<Panel> input) : this(input?.ToArray()) { }

    public DockLayoutPanelGroup(params string[] input) : this(input?.ToArray(i => new DockLayoutPanel(i.ToString()))) { }
}