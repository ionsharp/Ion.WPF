using Ion.Input;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ion.Core;

[AppLink(Name = "Find",
    Description = "Find and replace text in documents.",
    Icon = Images.Search)]
[Serializable]
public record class FindLink() : PanelLink<FindPanel>()
{
    private enum Menu { Find }

    /// <see cref="ICommand"/>
    #region

    [MenuItem(Menu.Find, Header = "Find", Icon = Images.Find)]
    public ICommand FindCommand
        => Commands[nameof(FindCommand)] ??= new RelayCommand(() => { } /*DockControl.Find(ActiveDocument)*/, () => true /*ActiveDocument is IFind && DockControl != null*/);

    ///

    [MenuItem(Menu.Find, Header = "FindAll", Icon = Images.FindAll)]
    public ICommand FindAllCommand => Panel.FindAllCommand;

    [MenuItem(Menu.Find, Header = "FindNext", Icon = Images.FindNext)]
    public ICommand FindNextCommand => Panel.FindNextCommand;

    [MenuItem(Menu.Find, Header = "FindPrevious", Icon = Images.FindPrevious)]
    public ICommand FindPreviousCommand => Panel.FindPreviousCommand;

    ///

    [MenuItem(Menu.Find, Header = "ReplaceAll", Icon = Images.ReplaceAll)]
    public ICommand ReplaceAllCommand => Panel.ReplaceAllCommand;

    [MenuItem(Menu.Find, Header = "ReplaceNext", Icon = Images.ReplaceNext)]
    public ICommand ReplaceNextCommand => Panel.ReplaceNextCommand;

    [MenuItem(Menu.Find, Header = "ReplacePrevious", Icon = Images.ReplacePrevious)]
    public ICommand ReplacePreviousCommand => Panel.ReplacePreviousCommand;

    #endregion
}