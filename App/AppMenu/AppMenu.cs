using Ion.Analysis;
using Ion.Collect;
using Ion.Controls;
using Ion.Data;
using Ion.Input;
using Ion.Reflect;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace Ion.Core;

/// <inheritdoc/>
[Menu(SortBy = [MenuSort.Index, MenuSort.Header], SortByClass = true)]
public record class AppMenu(AppModel model) : Model(), IAppComponent
{
    private enum Group { Extensions }

    /// <see cref="Region.Property"/>

    public AppModel Model { get; private set; } = model;

    /// <see cref="ICommand"/>
    #region

    public virtual MenuObject More { get; private set; } = new(nameof(More), Images.TriangleDown);

    [MenuCommand("About", Images.Info,
        ToolTip = "About the app.",
        Parent = nameof(More))]
    public ICommand AboutCommand => Commands[nameof(AboutCommand)] ??= new RelayCommand(() => Dialog.ShowAbout());

    public MenuObject Extension { get; private set; } = new(nameof(Extension), Images.Puzzle)
    {
        ToolTip = "Manage extensions.",
        Parent = nameof(More)
    };

    public MenuCommand ExtensionExport => new(nameof(ExtensionExport), Images.Export)
    {
        Command = Appp.Model.Links.ExportCommand,
        ToolTip = "Export app extensions.",
        Header = "Export",
        Parent = nameof(Extension),
        SubGroup = 0
    };

    public MenuCommand ExtensionImport => new(nameof(ExtensionImport), Images.Import)
    {
        Command = Appp.Model.Links.ImportCommand,
        ToolTip = "Import app extensions.",
        Header = "Import",
        Parent = nameof(Extension),
        SubGroup = 0
    };

    public MenuCommand ExtensionReset => new(nameof(ExtensionReset), Images.Reset)
    {
        Command = Appp.Model.Links.ResetCommand,
        ToolTip = "Reset app extensions.",
        Header = "Reset",
        Parent = nameof(Extension),
        SubGroup = 0
    };

    [Group(Group.Extensions)]
    [MenuList(Parent = nameof(Extension), SubGroup = 1,
        Index = 1,
        ItemCheckable = true,
        ItemCheckableMode = BindingMode.TwoWay,
        ItemCheckablePath = nameof(IAppLink.IsEnabled),
        ItemHeaderPath = nameof(IAppLink.Name),
        ItemIconPath = nameof(IAppLink.Icon),
        ItemInputGestureTextPath = nameof(IAppLink.Version),
        ItemInputGestureTextConverter = typeof(ConvertVersionToString),
        ItemInputGestureTextConverterParameter = 1,
        ItemToolTipPath = Paths.Dot,
        ItemToolTipTemplate = typeof(ObjectControlKey),
        ItemToolTipTemplateField = nameof(ObjectControlKey.ObjectToolTip),
        ItemType = typeof(IAppLink),
        SortDirection = ListSortDirection.Ascending,
        SortName = nameof(IAppLink.Name),
        ToolTip = "Enable extension.")]
    public static object Extensions => Appp.Model.Links.Instance;

    public MenuObject GitHub { get; private set; } = new MenuObject(nameof(GitHub), Images.GitHub)
    {
        ToolTip = "Open GitHub links.",
        Parent = nameof(More)
    };

    [MenuItem(Parent = nameof(GitHub), Header = "Home", Icon = Images.Home,
        ToolTip = "Explore GitHub project on the web.")]
    public ICommand GitHubCode
        => Commands[nameof(GitHubCode)] ??= new RelayCommand(() => System.Diagnostics.Process.Start(Model.GitHubProject));

    [MenuItem(Parent = nameof(GitHub), Header = "Help", Icon = Images.Help,
        ToolTip = "Explore GitHub project documentation on the web.")]
    public ICommand GitHubHelp
        => Commands[nameof(GitHubHelp)] ??= new RelayCommand(() => System.Diagnostics.Process.Start(Model.GitHubProject + "/wiki"));

    [MenuItem(Parent = nameof(GitHub), Header = "Issues", Icon = Images.Bug,
        ToolTip = "Explore GitHub project issues on the web.")]
    public ICommand GitHubIssues
        => Commands[nameof(GitHubIssues)] ??= new RelayCommand(() => System.Diagnostics.Process.Start(Model.GitHubProject + "/issues"));

    [MenuItem(Parent = nameof(GitHub), Header = "Explore", Icon = Images.LightBulb, SubGroup = 1,
        ToolTip = "Explore other GitHub projects on the web.")]
    public ICommand GitHubExplore
        => Commands[nameof(GitHubExplore)] ??= new RelayCommand(() => System.Diagnostics.Process.Start(Model.GitHub));

    [MenuCommand("Log", Images.Log,
        ToolTip = "Show log in a new window.",
        Parent = nameof(More))]
    public ICommand LogCommand => Commands[nameof(LogCommand)] ??= new RelayCommand(() => Dialog.ShowPanel(new LogPanel(Appp.Model.Log as IListObservable<IEntry>)));

    [MenuCommand("Options", Images.Options,
        ToolTip = "Show options in a new window.",
        Parent = nameof(More))]
    public ICommand OptionCommand => Commands[nameof(OptionCommand)] ??= new RelayCommand(() => Dialog.ShowObject("Options", Model.Data, Resource.GetImageUri(Images.Options), Buttons.Done));

    public MenuObject Theme => new("Theme", Images.Palette)
    {
        ToolTip = "Change application theme.",
        Parent = nameof(More)
    };

    public ICommand ThemeCommand
        => Commands[nameof(ThemeCommand)] ??= new RelayCommand<string>(i => Model.Data.Theme = i);

    [MenuList(Parent = nameof(Theme),
        IsInline = true,
        ItemCommand = nameof(ThemeCommand),
        ItemCommandParameterPath = Paths.Dot,
        ItemHeaderPath = Paths.Dot,
        ItemIconPath = Paths.Dot,
        ItemIconTemplate = typeof(XStorage),
        ItemIconTemplateField = nameof(XStorage.IconTemplateKey),
        ItemToolTipPath = Paths.Dot,
        ItemToolTipTemplate = typeof(XStorage),
        ItemToolTipTemplateField = nameof(XStorage.ToolTipTemplateKey),
        ItemType = typeof(string))]
    public static ListObservableOfString Themes => new(typeof(DefaultThemes).GetEnumValues<DefaultThemes>(Browse.Visible).Select(i => i.ToString()));

    #endregion
}