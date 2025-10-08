using Ion.Analysis;
using Ion.Controls;
using Ion.Input;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Ion.Core;

public interface IDockViewModel : IDataViewModel
{
    /// <see cref="Content"/>
    #region

    event ChangeEventHandler<Content> ActiveContentChanged;

    Content ActiveContent { get; set; }

    #endregion

    /// <see cref="Document"/>
    #region

    event ChangeEventHandler<Document> ActiveDocumentChanged;

    event EventHandle<Document> DocumentAdded;

    event EventHandle<Document> DocumentRemoved;

    Document ActiveDocument { get; set; }

    DocumentCollection Documents { get; }

    #endregion

    /// <see cref="Panel"/>
    #region

    event ChangeEventHandler<Panel> ActivePanelChanged;

    Panel ActivePanel { get; set; }

    PanelCollection Panels { get; }

    Result SelectPanel<T>() where T : Panel;

    #endregion

    #region

    int DefaultLayout { get; }

    IReadOnlyCollection<Uri> DefaultLayouts { get; }

    object Layout { get; set; }

    string LayoutPath { get; }

    LayoutList Layouts { get; }

    #endregion

    /// <see cref="ICommand"/>
    #region

    ICommand CloseCommand { get; }

    ICommand CloseAllCommand { get; }

    ICommand CloseAllButThisCommand { get; }

    ICommand DockDocumentCommand { get; }

    ICommand DockAllDocumentsCommand { get; }

    ICommand DockPanelCommand { get; }

    ICommand DockAllPanelsCommand { get; }

    ICommand FloatCommand { get; }

    ICommand FloatAllDocumentsCommand { get; }

    ICommand FloatAllPanelsCommand { get; }

    ICommand FloatPanelCommand { get; }

    ICommand HideCommand { get; }

    ICommand HideAllCommand { get; }

    ICommand MinimizeCommand { get; }

    ICommand MinimizeAllCommand { get; }

    ICommand NewDocumentHorizontalGroupCommand { get; }

    ICommand NewDocumentVerticalGroupCommand { get; }

    ICommand NewPanelHorizontalGroupCommand { get; }

    ICommand NewPanelVerticalGroupCommand { get; }

    ICommand PinCommand { get; }

    ICommand PinAllCommand { get; }

    ICommand RestoreAllCommand { get; }

    ICommand ShowAllCommand { get; }

    ICommand UnpinAllCommand { get; }

    #endregion
}