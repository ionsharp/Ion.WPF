using Ion.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Ion.Controls;

[Serializable]
public record class DockLayoutDocumentGroup : DockLayoutElement
{
    public bool Default { get; set; }

    [XmlIgnore]
    public readonly List<Document> Documents = [];

    [XmlArray]
    public List<DockLayoutPanel> Panels { get => Get(new List<DockLayoutPanel>()); set => Set(value); }

    public DockLayoutDocumentGroup() : base() { }

    public DockLayoutDocumentGroup(params Content[] content) : this((IEnumerable<Content>)content) { }

    public DockLayoutDocumentGroup(IEnumerable<Content> content) : base()
    {
        foreach (var i in content)
        {
            if (i is Document j)
                Documents.Add(j);

            else if (i is Panel k)
                Panels.Add(new DockLayoutPanel(k.Name));
        }
    }

    public DockLayoutDocumentGroup(params Document[] documents)
        : this((IEnumerable<Document>)documents) { }

    public DockLayoutDocumentGroup(IEnumerable<Document> documents) : base()
        => documents.ForEach(i => Documents.Add(i));

    public DockLayoutDocumentGroup(IList input)
        : this((IEnumerable<Content>)input) { }
}