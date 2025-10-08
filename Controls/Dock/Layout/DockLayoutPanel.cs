using Ion.Core;
using System;
using System.Xml.Serialization;

namespace Ion.Controls;

[Serializable]
public record class DockLayoutPanel : Model
{
    [XmlAttribute]
    public bool IsSelected { get => Get(false); set => Set(value); }

    [XmlAttribute]
    public string Name { get => Get(""); set => Set(value); }

    private DockLayoutPanel() : base() { }

    public DockLayoutPanel(string name) : base() => Name = name;

    public DockLayoutPanel(Core.Panel panel) : this(panel.Name) => IsSelected = panel.IsSelected;
}