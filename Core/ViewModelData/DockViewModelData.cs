using Ion.Reflect;
using System;
using System.Collections.Generic;

namespace Ion.Core;

/// <inheritdoc/>
[Serializable]
public record class DockViewModelData() : ViewModelData()
{
    private enum Group { Default }

    /// <see cref="Region.Property"/>

    public object Layout { get => Get<object>(); set => Set(value); }

    public Dictionary<string, Instance.Data> PanelData { get; private set; } = [];

    /// <see cref="Tab"/>
    #region

    private enum Tab { Document, Layout }

    /// <see cref="Tab.Document"/>
    #region

    [Style(Name = "AutoSave", Tab = Tab.Document)]
    public bool AutoSaveDocuments { get => Get(false); set => Set(value); }

    public virtual bool RememberDocuments => true;

    public virtual List<Document> RememberedDocuments { get => Get(new List<Document>()); set => Set(value); }

    #endregion

    /// <see cref="Tab.Layout"/>
    #region

    [Style(Name = "AutoSave", Tab = Tab.Layout)]
    public bool AutoSaveLayout { get => Get(true); set => Set(value); }

    [Style(Tab = Tab.Layout)]
    public static PanelCollection Panels => Appp.Get<IDockAppModel>().ViewModel.Panels;

    #endregion

    #endregion

    /// <see cref="IPropertySet""/>

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        if (e.PropertyName == nameof(ViewModel))
        {
            if (e.NewValue is IDockAppModel newViewModel)
            {
                if (RememberedDocuments.Count > 0)
                {
                    RememberedDocuments.ForEach(newViewModel.Documents.Add);
                    RememberedDocuments.Clear();
                }

                foreach (var i in newViewModel.ViewModel.Panels)
                {
                    if (PanelData.ContainsKey(i.Name))
                        Instance.Devirtualize(i, PanelData[i.Name]);
                }
                PanelData.Clear();
            }
        }
    }

    protected override void Serialize(string filePath, object data) => throw new NotImplementedException();
}