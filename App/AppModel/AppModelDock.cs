using Ion;
using Ion.Analysis;
using Ion.Controls;
using Ion.Reflect;
using System.ComponentModel;
using System.Linq;

namespace Ion.Core;

/// <inheritdoc/>
public abstract record class
    AppModelDock<A, B, C, D, E>() :
    AppModel<A, B, C, D>(), IAppModelDock
        where A : AppData
        where B : DockAppMenu
        where C : AppView
        where D : IDockViewModel
        where E : Document
{
    /// <see cref="Region.Field"/>

    private bool checkDocuments = true;

    private bool saveDocuments = true;

    /// <see cref="Region.Property"/>
    #region

    public Content ActiveContent => ViewModel.ActiveContent;

    public E ActiveDocument => (E)ViewModel.ActiveDocument;

    public Panel ActivePanel => ViewModel.ActivePanel;

    public DocumentCollection Documents => ViewModel.Documents;

    public PanelCollection Panels => ViewModel.Panels;

    #endregion

    protected override void OnExitCancelled(Result result)
    {
        base.OnExitCancelled(result);
        if (result is Error)
            ViewModel.SelectPanel<LogPanel>();
    }

    protected override void OnViewClosing(CancelEventArgs e)
    {
        if (!exitHide)
        {
            //0) Hide instead?
            if (Data.TaskbarIconVisibility)
            {
                if (Data.TaskbarIconHidesInsteadOfCloses)
                {
                    e.Cancel = true;
                    HideView(); return;
                }
            }

            //1) Check for unsaved documents
            if (checkDocuments)
            {
                e.Cancel = true;
                if (Data.WarnOnCloseWithUnsavedDocuments)
                {
                    if (Documents.Any(i => i.IsChanged))
                    {
                        var never = new Accessor<bool>(() => !Data.WarnOnCloseWithUnsavedDocuments, i => Data.WarnOnCloseWithUnsavedDocuments = !i);
                        Dialog.ShowResult(XAssembly.GetInfo(AssemblySource.Entry).Title, new Warning($"One or more documents have unsaved changes. Do you want to save?"), i =>
                        {
                            //Yes
                            if (i == 0)
                            {
                                //Save, then close
                                checkDocuments = false;
                                saveDocuments = true;
                            }
                            //No
                            if (i == 1)
                            {
                                //Close
                                checkDocuments = false;
                                saveDocuments = false;
                            }
                            //Cancel
                            if (i == 2)
                            {
                                //Do nothing!
                                checkDocuments = false;
                                saveDocuments = false;
                            }
                            View.Close();
                        },
                        never,
                        Buttons.YesNoCancel);
                        return;
                    }
                }
            }

            //2) Saved unsaved documents
            if (saveDocuments)
            {
                e.Cancel
                    = true;
                saveDocuments
                    = false;

                Documents.ForEach(i => i.IsChanged.If(() => i.Save()));

                View.Close();
                return;
            }

            //3) Do everything else!
            base.OnViewClosing(e);
        }
    }

    /// <see cref="IAppModelDock"/>

    IDockViewModel IAppModelDock.ViewModel => ViewModel;
}