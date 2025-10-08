using Ion.Controls;
using Ion.Input;
using System;
using System.Windows.Input;

namespace Ion.Core;

[Name("Theme"), Image(Images.Palette), Serializable]
public record class ThemePanel : ObjectPanel
{
    public static readonly new ResourceKey Template = new();

    /// <see cref="Region.Constructor"/>

    public ThemePanel() : base()
    {
        Source = Appp.Model.Theme.ActiveTheme;
    }

    /// <see cref="ICommand"/>

    private ICommand deleteCommand;
    [Image(Images.Trash)]
    [Name("Delete")]
    [Style(View = View.Header)]
    public ICommand DeleteCommand => deleteCommand
        ??= new RelayCommand(() => Appp.Get<AppModel>().Themes.DeleteCommand.Execute(Appp.Get<AppData>().Theme), () => Appp.Get<AppModel>().Themes.DeleteCommand.CanExecute(Appp.Get<AppData>().Theme));

    private ICommand renameCommand;
    [Image(Images.Rename)]
    [Name("Rename")]
    [Style(View = View.Header)]
    public ICommand RenameCommand => renameCommand
        ??= new RelayCommand(() => Appp.Get<AppModel>().Themes.RenameCommand.Execute(Appp.Get<AppData>().Theme), () => Appp.Get<AppModel>().Themes.RenameCommand.CanExecute(Appp.Get<AppData>().Theme));

    private ICommand saveCommand;
    [Image(Images.Save)]
    [Name("Save")]
    [Style(View = View.Header)]
    public ICommand SaveCommand => saveCommand ??= new RelayCommand(() =>
    {
        var x = new Namable(Namable.DefaultName);
        Dialog.ShowObject($"Save", x, Resource.GetImageUri(Images.Rename), j =>
        {
            if (j == 0)
                Try.Do(() => Appp.Get<AppModel>().SaveTheme(x.Name), e => Analysis.Log.Write(e));
        },
        Buttons.SaveCancel);
    },
    () => Appp.Model.Theme.ActiveTheme != null);
}