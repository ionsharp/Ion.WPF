using Ion.Controls;
using Ion.Data;
using Ion.Input;
using System;
using System.Windows.Data;
using System.Windows.Input;

namespace Ion.Core;

/// <inheritdoc/>
public record class DockAppMenu(AppModel model) : AppMenu(model)
{
    private enum Group { Custom, Default }

    new public IDockAppModel Model => base.Model as IDockAppModel;

    public MenuObject Document => new(nameof(Document), Images.File) { };
    #region

    [Description("Close the active document.")]
    [MenuItem(Parent = nameof(Document), SubGroup = 2, Header = "Close", Icon = Images.Close)]
    public ICommand CloseCommand => Model.ViewModel.CloseCommand;

    [Description("Close all documents.")]
    [MenuItem(Parent = nameof(Document), SubGroup = 2, Header = "CloseAll", Icon = Images.CloseAll)]
    public ICommand CloseAllCommand => Model.ViewModel.CloseAllCommand;

    [Description("Close all, but the active document.")]
    [MenuItem(Parent = nameof(Document), SubGroup = 2, Header = "CloseAllButThis", Icon = Images.CloseAllButThis)]
    public ICommand CloseAllButThisCommand => Model.ViewModel.CloseAllButThisCommand;

    [Description("Minimize the active document.")]
    [MenuItem(Parent = nameof(Document), SubGroup = 0, Header = "Minimize", Icon = Images.Minimize)]
    public ICommand MinimizeCommand => Model.ViewModel.MinimizeCommand;

    [Description("Minimize all documents.")]
    [MenuItem(Parent = nameof(Document), SubGroup = 0, Header = "MinimizeAll", Icon = Images.MinimizeAll)]
    public ICommand MinimizeAllCommand => Model.ViewModel.MinimizeAllCommand;

    [Description("Restore all minimized documents.")]
    [MenuItem(Parent = nameof(Document), SubGroup = 0,
        Header = "RestoreAll", Icon = Images.RestoreAll)]
    public ICommand RestoreAllCommand => Model.ViewModel.RestoreAllCommand;

    [Description("Dock the active floating document.")]
    [MenuItem(Parent = nameof(Document), SubGroup = 1,
        Header = "Dock", Icon = Images.Dock)]
    public ICommand DockDocumentCommand => Model.ViewModel.DockAllDocumentsCommand;

    [Description("Dock all floating documents.")]
    [MenuItem(Parent = nameof(Document), SubGroup = 1,
        Header = "DockAll", Icon = Images.DockAll)]
    public ICommand DockAllDocumentsCommand => Model.ViewModel.DockAllDocumentsCommand;

    [Description("Float the active docked document.")]
    [MenuItem(Parent = nameof(Document), SubGroup = 1,
        Header = "Float", Icon = Images.Float)]
    public ICommand FloatDocumentCommand => Model.ViewModel.FloatCommand;

    [Description("Float all docked documents.")]
    [MenuItem(Parent = nameof(Document), SubGroup = 1,
        Header = "FloatAll", Icon = Images.FloatAll)]
    public ICommand FloatAllDocumentsCommand => Model.ViewModel.FloatAllDocumentsCommand;

    [Description("Add a new horizontal document group.")]
    [MenuItem(Parent = nameof(Document), SubGroup = 3,
        Header = "NewHorizontalGroup", Icon = Images.GroupHorizontal)]
    public ICommand NewDocumentHorizontalGroupCommand => Model.ViewModel.NewDocumentHorizontalGroupCommand;

    [Description("Add a new vertical document group.")]
    [MenuItem(Parent = nameof(Document), SubGroup = 3,
        Header = "NewVerticalGroup", Icon = Images.GroupVertical)]
    public ICommand NewDocumentVerticalGroupCommand => Model.ViewModel.NewDocumentVerticalGroupCommand;

    #endregion

    public MenuObject Layout => new(nameof(Layout), Images.Layout) { };
    #region

    private ICommand applyLayoutCommand;
    public ICommand ApplyLayoutCommand => applyLayoutCommand ??= new RelayCommand<object>(i => Model.ViewModel.Layout = i, i => i != null);

    [MenuList(Parent = nameof(Layout), Group = Group.Default,
        ItemCommand = nameof(ApplyLayoutCommand),
        ItemCommandParameterPath = Paths.Dot,
        ItemHeaderPath = Paths.Dot,
        ItemHeaderConverter = typeof(ConvertFileNameFromUri),
        ItemIcon = Images.SmallPeriod,
        ItemType = typeof(Uri))]
    public object DefaultLayouts => Model.ViewModel.DefaultLayouts;

    [MenuList(Parent = nameof(Layout), Group = Group.Custom,
        IsInline = true,

        ItemCommand = nameof(ApplyLayoutCommand),
        ItemCommandParameterPath = Paths.Dot,
        ItemHeaderConverter = typeof(ConvertFileName),
        ItemHeaderPath = Paths.Dot,
        ItemIcon = Images.SmallPeriod,
        ItemType = typeof(string))]
    public object CustomLayouts => Model.ViewModel.Layouts;

    [Description("Export layouts.")]
    [MenuItem(Parent = nameof(Layout), SubGroup = 1,
        Header = "Export",
        Icon = Images.Export)]
    public ICommand ExportLayoutCommand => Model.ViewModel.Layouts.ExportCommand;

    [Description("Import layouts.")]
    [MenuItem(Parent = nameof(Layout), SubGroup = 1,
        Header = "Import",
        Icon = Images.Import)]
    public ICommand ImportLayoutCommand => Model.ViewModel.Layouts.ImportCommand;

    private ICommand manageLayoutsCommand;
    [Description("Manage layouts.")]
    [MenuItem(Parent = nameof(Layout), SubGroup = 2,
        Header = "Manage",
        Icon = Images.Options)]
    public ICommand ManageLayoutsCommand => manageLayoutsCommand ??= new RelayCommand(() => Dialog.ShowObject("Manage layouts", Model.ViewModel.Layouts, Resource.GetImageUri(Images.Options)),
    () => Model.ViewModel.Layouts.Count > 0);

    private ICommand saveLayoutCommand;
    [Description("Save a new layout from the current layout.")]
    [MenuItem(Parent = nameof(Layout), SubGroup = 0,
        Header = "Save",
        Icon = Images.Save)]
    public ICommand SaveLayoutCommand => saveLayoutCommand ??= new RelayCommand(() =>
    {
        var file = new Namable("Untitled");
        Dialog.ShowObject($"Save layout", file, Resource.GetImageUri(Images.Save), i =>
        {
            if (i == 0)
                ContentSerializer.Serialize(@$"{Model.ViewModel.LayoutPath}\{file.Name}.xml", null);
        },
        Buttons.SaveCancel);
    });

    #endregion

    public MenuObject Panel => new(nameof(Panel), Images.Panel) { };
    #region

    [Description("Hide the active shown panel.")]
    [MenuItem(Parent = nameof(Panel), SubGroup = 0, Header = "Hide", Icon = Images.Hide)]
    public ICommand HideActivePanelCommand => Model.ViewModel.HideCommand;

    [Description("Hide all shown panels.")]
    [MenuItem(Parent = nameof(Panel), SubGroup = 0, Header = "HideAll", Icon = Images.HideAll)]
    public ICommand HideAllCommand => Model.ViewModel.HideAllCommand;

    [Description("Show all hidden panels.")]
    [MenuItem(Parent = nameof(Panel), SubGroup = 0, Header = "ShowAll", Icon = Images.ShowAll)]
    public ICommand ShowAllCommand => Model.ViewModel.ShowAllCommand;

    [Description("Pin the active panel.")]
    [MenuItem(Parent = nameof(Panel), SubGroup = 1, Header = "Pin", Icon = Images.Pin)]
    public ICommand PinCommand => Model.ViewModel.PinCommand;

    [Description("Pin all panels.")]
    [MenuItem(Parent = nameof(Panel), SubGroup = 1, Header = "PinAll", Icon = Images.PinAll)]
    public ICommand PinAllCommand => Model.ViewModel.PinAllCommand;

    [Description("Unpin all pinned panels.")]
    [MenuItem(Parent = nameof(Panel), SubGroup = 1, Header = "UnpinAll", Icon = Images.UnpinAll)]
    public ICommand UnpinAllCommand => Model.ViewModel.UnpinAllCommand;

    [Description("Dock the active floating panel.")]
    [MenuItem(Parent = nameof(Panel), SubGroup = 2, Header = "Dock", Icon = Images.Dock)]
    public ICommand DockPanelCommand => Model.ViewModel.DockPanelCommand;

    [Description("Dock all floating panels.")]
    [MenuItem(Parent = nameof(Panel), SubGroup = 2, Header = "DockAll", Icon = Images.DockAll)]
    public ICommand DockAllPanelsCommand => Model.ViewModel.DockAllPanelsCommand;

    [Description("Float the active docked panel.")]
    [MenuItem(Parent = nameof(Panel), SubGroup = 2,
        Header = "Float", Icon = Images.Float)]
    public ICommand FloatPanelCommand => Model.ViewModel.FloatPanelCommand;

    [Description("Float all docked panels.")]
    [MenuItem(Parent = nameof(Panel), SubGroup = 2,
        Header = "FloatAll", Icon = Images.FloatAll)]
    public ICommand FloatAllPanelsCommand => Model.ViewModel.FloatAllPanelsCommand;

    [Description("Add a new horizontal panel group.")]
    [MenuItem(Parent = nameof(Panel), SubGroup = 3,
        Header = "NewHorizontalGroup", Icon = Images.GroupHorizontal)]
    public ICommand NewPanelHorizontalGroupCommand => Model.ViewModel.NewPanelHorizontalGroupCommand;

    [Description("Add a new vertical panel group.")]
    [MenuItem(Parent = nameof(Panel), SubGroup = 3,
        Header = "NewVerticalGroup", Icon = Images.GroupVertical)]
    public ICommand NewPanelVerticalGroupCommand => Model.ViewModel.NewPanelVerticalGroupCommand;

    [MenuList(Parent = nameof(Panel), SubGroup = 4, Header = nameof(Core.Panel.Title), Icon = nameof(Panel.Icon),
        ItemCheckable = true,
        ItemCheckableMode = BindingMode.TwoWay,
        ItemCheckablePath = nameof(Core.Panel.IsVisible),
        ItemHeaderPath = nameof(Core.Panel.Title),
        ItemIconPath = Paths.Dot,
        ItemIconConverter = typeof(ConvertAttributeImageSource),
        ItemToolTipPath = Paths.Dot,
        ItemToolTipTemplate = typeof(ObjectControlKey),
        ItemToolTipTemplateField = nameof(ObjectControlKey.ObjectToolTip),
        ItemType = typeof(Panel),
        SortDirection = System.ComponentModel.ListSortDirection.Ascending,
        SortName = nameof(Core.Panel.Title))]
    public PanelCollection Panels => Model.ViewModel.Panels;

    #endregion
}