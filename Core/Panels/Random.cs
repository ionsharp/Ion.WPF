using Ion.Collect;
using Ion.Controls;
using Ion.Data;
using Ion.Input;
using Ion.Numeral;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Ion.Core;

[Style("Random", Image = Images.Dice)]
public record class RandomPanel : Panel
{
    private enum Category { Characters }

    public static readonly new ResourceKey Template = new();

    public static readonly int LengthMinimumAbsolute = 1;

    public static readonly int LengthMaximumAbsolute = int.MaxValue;

    /// <see cref="Region.Property"/>
    #region

    [Hide]
    public bool Generating { get => Get(false); set => Set(value); }

    [Hide]
    public string Text { get => Get(""); set => Set(value); }

    /// <see cref="View.Main"/>
    #region

    [Group(Category.Characters)]
    [Style(NameHide = true,
        Index = 1,
        IsLockable = true,
        Pin = Sides.LeftOrTop,
        Placeholder = "Characters",
        View = View.Header,
        Width = 200)]
    public string Characters { get => Get(""); set => Set(value); }

    [Group(Category.Characters), Image(Images.Plus)]
    [Styles.List(Ion.Template.ListButton,
        NameHide = true,
        Index = 0,
        IsLockable = true,
        ItemCommand = nameof(AddCharactersCommand),
        Pin = Sides.LeftOrTop,
        View = View.Header)]
    public ListObservableOfString CharacterGroups
    {
        get
        {
            var result = new ListObservableOfString() { Ion.Text.Characters.Lower, Ion.Text.Characters.Numbers, Ion.Text.Characters.Special, Ion.Text.Characters.Upper };
            CustomCharacters.Split(' ', StringSplitOptions.RemoveEmptyEntries).ForEach(result.Add);
            return result;
        }
    }

    [Style(IsLockable = true,
        View = View.Header)]
    public bool Distinct { get => Get(false); set => Set(value); }

    [Image(Images.Clock)]
    [Styles.List(Ion.Template.ListButton, NameHide = true, ItemCommand = nameof(FillCommand), Pin = Sides.RightOrBottom,
        View = View.Header)]
    public ListObservableOfString History { get => Get(new ListObservableOfString()); set => Set(value); }

    [Styles.Number(1, int.MaxValue, 1, CanUpDown = true,
        IsLockable = true,
        View = View.Header)]
    public int Length { get => Get(50); set => Set(value); }

    #endregion

    /// <see cref="View.Option"/>
    #region

    [Name("CustomCharacters")]
    [Styles.Token(Delimiter = ' ', View = View.Option)]
    public string CustomCharacters { get => Get(""); set => Set(value); }

    [Name("FontFamily")]
    [Style(View = View.Option)]
    public FontFamily FontFamily { get => Get(new FontFamily("Calibri"), ValueConverter.Cache.Get<ConvertFontFamilyToString>()); set => Set(value, ValueConverter.Cache.Get<ConvertFontFamilyToString>()); }

    [Name("FontSize")]
    [Styles.Number(8.0, 72.0, 1.0,
        View = View.Option)]
    public double FontSize { get => Get(16.0); set => Set(value); }

    [Name("HistoryLimit")]
    [Styles.Number(0, 64, 1,
        View = View.Option)]
    public int HistoryLimit { get => Get(20); set => Set(value); }

    [Name("TextAlignment")]
    [Styles.Object(View = View.Option)]
    public Alignment TextAlignment { get => Get(Alignment.Center); set => Set(value); }

    #endregion

    #endregion

    /// <see cref="Region.Constructor"/>
    #region

    public RandomPanel() : base() { }

    #endregion

    /// <see cref="Region.Method"/>
    #region

    private async Task Generate()
    {
        Generating = true;

        if (Length > 0)
        {
            await Dispatch.BeginInvoke(() => Text = string.Empty);

            var result = new StringBuilder();
            var length = (int)Length;

            var characters = Characters;
            await Task.Run(() => result.Append(characters.Random(length, length)));

            await Dispatch.BeginInvoke(() =>
            {
                var finalResult = result.ToString();
                finalResult = Distinct ? string.Concat(finalResult.Distinct()) : finalResult;

                Text = finalResult;

                if (HistoryLimit > 0)
                {
                    if (History.Count == HistoryLimit)
                        History.RemoveAt(HistoryLimit - 1);

                    History.Insert(0, finalResult);
                }
            });
        }

        Generating = false;
    }

    public override void OnSettingProperty(PropertySettingEventArgs e)
    {
        base.OnSettingProperty(e);
        if (e.PropertyName == nameof(CustomCharacters))
        {
            var old = string.Empty;

            var result = string.Empty;
            foreach (var i in CustomCharacters)
            {
                if (i != ' ')
                {

                    if (!old.Contains(i))
                    {
                        result += i;
                        old += i;
                    }
                }
                else
                {
                    result += i;
                    old = string.Empty;
                }
            }

            e.NewValue = result;
        }
    }

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        if (e.PropertyName == nameof(CustomCharacters))
            Reset(() => CharacterGroups);
    }

    #endregion

    /// <see cref="ICommand"/>
    #region

    private ICommand addCharactersCommand;
    [Hide]
    public ICommand AddCharactersCommand
        => addCharactersCommand ??= new RelayCommand<string>(i => Characters = $"{Characters}{i}");

    private ICommand clearHistoryCommand;
    [Hide]
    public ICommand ClearHistoryCommand
        => clearHistoryCommand ??= new RelayCommand(() => History.Clear(), () => History.Count > 0);

    private ICommand copyCommand;
    [Hide]
    public ICommand CopyCommand
        => copyCommand ??= new RelayCommand(() => System.Windows.Clipboard.SetText(Text), () => !Text.IsEmpty());

    private ICommand fillCommand;
    [Hide]
    public ICommand FillCommand
        => fillCommand ??= new RelayCommand<string>(i => Text = i, i => !i.IsEmpty());

    private ICommand generateCommand;
    [Image(Images.Dice)]
    [Name("Generate")]
    [Style(Ion.Template.ButtonDefault, Index = int.MaxValue, Pin = Sides.RightOrBottom,
        View = View.Header)]
    public ICommand GenerateCommand
        => generateCommand ??= new RelayCommand(() => _ = Generate(), () => Characters.Length > 0 && Length > 0);

    #endregion
}