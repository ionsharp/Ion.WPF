using Ion.Controls;
using Ion.Input;
using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Ion.Core;

[Image(Images.Search)]
[Name("Find")]
public record class FindResultPanel : DataGridPanel<FindResult>
{
    /// <see cref="Region.Field"/>

    public static readonly new Controls.ResourceKey Template = new();

    /// <see cref="Region.Property"/>
    #region

    /// <see cref="DataPanel"/>

    public override bool CanCopyTo => false;

    public override bool CanMoveTo => false;

    public override bool CanAddFromPreset => false;

    /// <see cref="DataGridPanel"/>

    public override bool CanAddRows => false;

    public override bool CanResizeRows => false;

    /// <see cref="Panel"/>

    public override string TitleSuffix => $" \"{Results.FindText}\"";

    /// <see cref="View.Header"/>
    #region

    [Style(NameHide = true,
        View = Ion.View.Header)]
    public FindSource FilterSource { get => Get<FindSource>(); set => Set(value); }

    [Style(View = Ion.View.Header)]
    public bool KeepResults { get => Get(false); set => Set(value); }

    [XmlIgnore]
    public FindResultList Results { get => Get<FindResultList>(null, false); set => Set(value, false); }

    [Image(Images.Search)]
    [Styles.Text(NameHide = true, Index = int.MaxValue,
        EnterCommand = nameof(SearchCommand),
        EnterImage = Images.Search,
        Pin = Sides.RightOrBottom,
        Placeholder = "Search...",
        View = Ion.View.Header,
        SuggestionProperty = nameof(SearchHistory),
        SuggestionCommandProperty = nameof(SearchSuggestionCommand),
        ValueTrigger = BindTrigger.LostFocus,
        Width = 300)]
    public string Search { get => Get(""); set => Set(value); }

    [Image(Images.ArrowDownLeft)]
    [Style(Ion.Template.Check, NameHide = true, Index = int.MaxValue - 1,
        View = Ion.View.Header)]
    public bool TextWrap { get => Get(true); set => Set(value); }

    #endregion

    #endregion

    /// <see cref="Region.Constructor"/>

    public FindResultPanel() : base() { }

    public FindResultPanel(FindResultList results) : this() => Results = results;

    /// <see cref="Region.Method"/>
    #region

    private void UpdateSearch(string input)
    {
        if (!input.IsEmpty())
        {
            if (SearchHistory.Contains(input))
                SearchHistory.Remove(input);

            SearchHistory.Insert(0, input);
        }
    }

    protected override void OnItemDoubleClick(object item)
    {
        if (item is FindResult result)
        {
            Appp.Get<IAppModelDock>()?.ViewModel.IfNotNull(i => i.ActiveContent = result.File as Content);
            //Scroll to and select matched text
        }
    }

    protected override bool OnItemFilter(object input)
    {
        if (base.OnItemFilter(input))
        {
            if (input is FindResult result)
            {
                if (FilterSource == FindSource.CurrentDocument)
                {
                    if (!ReferenceEquals(result.File, Appp.Get<IAppModelDock>()?.ViewModel.ActiveContent as Document))
                        return false;
                }
            }
        }
        return true;
    }

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        if (e.PropertyName == nameof(Results))
        {
            Items = Results;
            Reset(() => Title);
        }
    }

    #endregion

    /// <see cref="ICommand"/>

    [Image(Images.Copy)]
    [Name("Copy")]
    [Style(View = Ion.View.Header)]
    public ICommand CopyAllCommand => Commands[nameof(CopyAllCommand)] ??= new RelayCommand(() =>
    {
        var result = new StringBuilder();
        foreach (var i in Results)
            result.AppendLine($"{i.Line}: {i.Text}");

        System.Windows.Clipboard.SetText(result.ToString());
    },
    () => Results.Count > 0);
}