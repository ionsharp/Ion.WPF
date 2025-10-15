using Ion.Controls;
using Ion.Input;
using System;
using System.Reflection;
using System.Windows.Input;

namespace Ion.Core;

[Name("Find")]
[Image(Images.Search)]
[Styles.Object(Strict = MemberTypes.All)]
[Serializable]
public record class FindPanel : Panel, IFrameworkElementReference
{
    /// <see cref="Region.Field"/>

    public static readonly ReferenceKey<FindControl> ControlKey = new();

    public static readonly new ResourceKey Template = new();
    private FindControl Control;

    public string FindText { get => Get(""); set => Set(value); }

    public bool MatchCase { get => Get(false); set => Set(value); }

    public bool MatchWord { get => Get(false); set => Set(value); }

    public FindSource Source { get => Get(FindSource.CurrentDocument); set => Set(value); }

    public string ReplaceText { get => Get(""); set => Set(value); }

    public FindPanel() : base() { }

    void IFrameworkElementReference.SetReference(IFrameworkElementKey key, System.Windows.FrameworkElement element)
    {
        if (key == ControlKey)
            Control = element as FindControl;
    }

    ///

    private ICommand findAllCommand;
    public ICommand FindAllCommand
        => findAllCommand ??= new RelayCommand(() => Control.FindAllCommand.Execute(), () => Control?.FindAllCommand.CanExecute(null) == true);

    private ICommand findNextCommand;
    public ICommand FindNextCommand
        => findNextCommand ??= new RelayCommand(() => Control.FindNextCommand.Execute(), () => Control?.FindNextCommand.CanExecute(null) == true);

    private ICommand findPreviousCommand;
    public ICommand FindPreviousCommand
        => findPreviousCommand ??= new RelayCommand(() => Control.FindNextCommand.Execute(), () => Control?.FindNextCommand.CanExecute(null) == true);

    ///

    private ICommand replaceAllCommand;
    public ICommand ReplaceAllCommand
        => replaceAllCommand ??= new RelayCommand(() => Control.ReplaceAllCommand.Execute(), () => Control?.ReplaceAllCommand.CanExecute(null) == true);

    private ICommand replaceNextCommand;
    public ICommand ReplaceNextCommand
        => replaceNextCommand ??= new RelayCommand(() => Control.ReplaceNextCommand.Execute(), () => Control?.ReplaceNextCommand.CanExecute(null) == true);

    private ICommand replacePreviousCommand;
    public ICommand ReplacePreviousCommand
        => replacePreviousCommand ??= new RelayCommand(() => Control.ReplacePreviousCommand.Execute(), () => Control?.ReplacePreviousCommand.CanExecute(null) == true);

    ///

    private ICommand resultsCommand;
    public ICommand ResultsCommand => resultsCommand ??= new RelayCommand<FindResultList>(i =>
    {
        if (Appp.Get<IAppModelDock>()?.ViewModel.Panels.FirstOrDefault<FindResultPanel>() is FindResultPanel oldPanel)
        {
            if (!oldPanel.KeepResults)
            {
                oldPanel.Results = i;
                return;
            }
        }
        Appp.Get<IAppModelDock>()?.ViewModel.Panels.Add(new FindResultPanel(i));
    },
    i => i != null);
}