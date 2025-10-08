using Ion.Analysis;
using Ion.Collect;
using Ion.Colors;
using Ion.Controls;
using Ion.Data;
using Ion.Input;
using Ion.Reflect;
using Ion.Threading;
using System;
using System.Collections;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Ion.Core;

/// <inheritdoc/>
[Styles.Object(Image = Images.LineGraph, Name = "Analysis",
    Description = "Analyze color.",
    MemberViewType = MemberViewType.Tab)]
[Serializable]
public record class ColorAnalysisPanel : MethodPanel
{
    [TabView(View = View.Main)]
    private enum Tab { }

    /// <see cref="Region.Key"/>

    public static readonly new Controls.ResourceKey Template = new();

    /// <see cref="Region.Field"/>

    public readonly ColorAnalyzer Analyzer = new();

    /// <see cref="Region.Property"/>
    #region

    protected override TaskStrategy MethodStrategy => TaskStrategy.Ignore;

    protected override TaskType MethodType => TaskType.Unmanaged;

    public ListObservable<ColorAnalysis>
        AResult
    { get; private set; } = [];

    public ListObservable<ColorAnalysis>
        BResult
    { get; private set; } = [];

    public ListObservable<ColorAnalysis>
        CResult
    { get; private set; } = [];

    public IListWritable Profiles { get; set; }

    public object Results { get => Get<object>(); set => Set(value); }

    #endregion

    /// <see cref="View.Header"/>
    #region

    [Styles.Number(1, 255, 1,
        IsLockable = true,
        RightText = "^3",
        View = View.Header)]
    public uint Depth { get => Get((uint)10); set => Set(value); }

    [Styles.Number(1, 8, 1,
        IsLockable = true,
        View = View.Header)]
    public int Precision { get => Get(3); set => Set(value); }

    [Style(Index = -1,
        IsLockable = true,
        View = View.Header)]
    public GroupItemForm Profile { get => Get<GroupItemForm>(); set => Set(value); }

    [Group(GroupDefault.Type, Index = -1)]
    [Style(NameHide = true,
        IsLockable = true,
        View = View.Header)]
    public ColorAnalysisType Type { get => Get(ColorAnalysisType.Accuracy); set => Set(value); }

    #endregion

    /// <see cref="Region.Constructor"/>
    #region

    public ColorAnalysisPanel() : base()
    {
        Results = AResult;
    }

    public ColorAnalysisPanel(IListWritable profiles) : this()
    {
        Profile = new(profiles, 0, 0);
    }

    #endregion

    /// <see cref="Region.Method"/>
    #region

    async protected override Task RunAsync(object parameter, CancellationToken token)
    {
        var results = Results as IList;
        results.Clear();

        Analyzer.Depth
            = Depth;
        Analyzer.Precision
            = Precision;
        Analyzer.Profile
            = Profile?.SelectedItem.As<ColorProfile>() ?? ColorProfile.Default;
        Analyzer.Type
            = Type;

        await System.Threading.Tasks.Task.Run((Action)(() =>
        {
            double a = 0, b = IColor.GetTypes().Count();
            IColor.GetTypes().ForEach(i =>
            {
                if (token.IsCancellationRequested)
                    return;

                a++;
                Dispatch.Do(() => Task.Progress = a / b);

                ColorAnalysis result = null;

                result = Try.Get(() => Analyzer.Analyze(i), e => Dispatch.Do(() => Log.Write(e)));
                result.IfNotNull(j => Dispatch.Do(() => results.Add(j)));
            });
        }), token);
    }

    protected override void RunSync(object parameter, CancellationToken token) => throw new NotImplementedException();

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        switch (e.PropertyName)
        {
            case nameof(Type):
                switch (Type)
                {
                    case ColorAnalysisType.Accuracy:
                        Results = AResult;
                        break;
                    case ColorAnalysisType.Range:
                        Results = BResult;
                        break;
                    case ColorAnalysisType.RangeInverse:
                        Results = CResult;
                        break;
                }
                break;
        }
    }

    #endregion

    /// <see cref="ICommand"/>
    #region

    [Styles.Button(Ion.Template.ButtonCancel,
        CommandImage = Images.Block,
        CommandText = "Cancel",
        NameHide = true,
        Index = int.MaxValue,
        IsLockable = true,
        Name = "Cancel",
        Pin = Sides.RightOrBottom,
        View = View.Header)]
    [VisibilityTrigger(nameof(IsActive), true)]
    public override ICommand CancelCommand => base.CancelCommand;

    [Group(GroupDefault.Command, Index = 1)]
    [Image(Images.Copy)]
    [Style(Name = "Copy",
        NameHide = true,
        Index = int.MaxValue,
        IsLockable = true,
        View = View.Header)]
    public ICommand CopyCommand => Commands[nameof(CopyCommand)] ??= new RelayCommand(() =>
    {
        var result = new StringBuilder();
        Results.As<IList>().ForEach(i => result.AppendLine($"{i}\n"));
        System.Windows.Clipboard.SetText(result.ToString());
    },
    () => Results?.As<IList>().Count > 0);

    [Styles.Button(Ion.Template.ButtonDefault,
        CommandImage = Images.Play,
        CommandText = "Start",
        NameHide = true,
        Index = int.MaxValue,
        IsLockable = true,
        Name = "Start",
        Pin = Sides.RightOrBottom,
        View = View.Header)]
    [VisibilityTrigger(nameof(IsActive), false)]
    public override ICommand StartCommand => base.StartCommand;

    #endregion
}