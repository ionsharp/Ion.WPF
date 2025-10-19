using Ion.Analysis;
using Ion.Collect;
using Ion.Controls;
using Ion.Data;
using Ion.Reflect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Data;

namespace Ion.Core;

/// <see cref="Region.Constructor"/>
[Image(Images.Log)]
[Name("Log")]
[Styles.Object(MemberViewType = MemberViewType.All,
    MemberView = Ion.View.All ^ Ion.View.Option)]
[Styles.Object(MemberViewType = MemberViewType.Tab,
    MemberView = Ion.View.Option)]
public record class LogPanel : XmlDataGridPanel<Entry>
{
    private enum Group
    {
        [GroupStyle(Index = 1)]
        Level,
        Text,
        Time
    }

    [TabView(View = Ion.View.Main)]
    private enum Tab
    {
        [TabStyle(Image = Images.General)]
        General
    }

    /// <see cref="Region.Field"/>
    #region

    public static readonly ResourceKey AssemblyTemplate = new();

    public static readonly ResourceKey ColorError = new();

    public static readonly ResourceKey ColorMessage = new();

    public static readonly ResourceKey ColorSuccess = new();

    public static readonly ResourceKey ColorWarning = new();

    public static readonly ResourceKey ImageTemplate = new();

    public static readonly ResourceKey LevelTemplate = new();

    public static readonly ResourceKey ResultTemplate = new();

    public static readonly ResourceKey SenderTemplate = new();

    public static readonly new ResourceKey Template = new();

    public static readonly ResourceKey TemplateKey = new();

    public static readonly ResourceKey TextStyleKey = new();

    public static readonly ResourceKey ToolTipKey = new();

    #endregion

    /// <see cref="Region.Property.Static"/>

    private static IMultiValueConverter countConverter;
    public static IMultiValueConverter CountConverter => countConverter ??= new MultiValueConverter<string>(i =>
    {
        if (i.Values?.Length == 3)
        {
            if (i.Values[0] is IListObservable a)
            {
                if (i.Values[2] is ResultType b)
                    return $"{a.Count<Entry>(i => i.Result.Type == b)}";
            }
        }
        return $"0";
    });

    /// <see cref="Region.Constructor"/>

    public LogPanel(IList<Entry> input) : base(input as IList) { }

    /// <see cref="Region.Property"/>
    #region

    /// <see cref="DataPanel"/>

    public override bool CanAdd => false;

    public override bool CanClone => false;

    public override bool CanCopyTo => false;

    public override bool CanEdit => false;

    public override bool CanMoveTo => false;

    public override bool CanPaste => false;

    public override bool CanAddFromPreset => false;

    /// <see cref="DataGridPanel"/>

    public override bool CanAddRows => false;

    public override bool CanResizeRows => false;

    public override bool CanSortColumns => false;

    public string ErrorCount => $"Errors ({Items.Count<Entry>(i => i.Result is Error)})";

    public string MessageCount => $"Messages ({Items.Count<Entry>(i => i.Result is Message)})";

    public string SuccessCount => $"Success ({Items.Count<Entry>(i => i.Result is Success)})";

    public string WarningCount => $"Warnings ({Items.Count<Entry>(i => i.Result is Warning)})";

    public ResultType FilterType
    {
        get
        {
            var result = ResultType.None;
            if (FilterError)
                result |= ResultType.Error;

            if (FilterMessage)
                result |= ResultType.Message;

            if (FilterSuccess)
                result |= ResultType.Success;

            if (FilterWarning)
                result |= ResultType.Warning;

            return result;
        }
    }

    /// <see cref="View.Header"/>
    #region

    [Description("Show with level")]
    [Group(Group.Level)]
    [Name("Level")]
    [Style(Ion.Template.EnumFlagButton, Float = Sides.LeftOrTop, NameHide = true, Orientation = Orient.Vertical,
        View = Ion.View.Header)]
    public EntryLevel FilterLevel { get => Get(EntryLevel.All); set => Set(value); }

    [Description("Show errors.")]
    [Name("Show errors")]
    [Styles.Check(Ion.Template.CheckImage, NameHide = true, Pin = Sides.LeftOrTop,
        CheckImage = Images.XRound,
        CheckImageColor = nameof(ColorError),
        CheckImageColorType = typeof(LogPanel),
        View = Ion.View.Header)]
    [StyleTrigger(nameof(StyleAttribute.Name), nameof(ErrorCount))]
    public bool FilterError { get => Get(true); set => Set(value); }

    [Description("Show messages.")]
    [Name("Show messages")]
    [Styles.Check(Ion.Template.CheckImage, NameHide = true, Pin = Sides.LeftOrTop,
        CheckImage = Images.Info,
        CheckImageColor = nameof(ColorMessage),
        CheckImageColorType = typeof(LogPanel),
        View = Ion.View.Header)]
    [StyleTrigger(nameof(StyleAttribute.Name), nameof(MessageCount))]
    public bool FilterMessage { get => Get(true); set => Set(value); }

    [Description("Show successes.")]
    [Name("Show successes")]
    [Styles.Check(Ion.Template.CheckImage, NameHide = true, Pin = Sides.LeftOrTop,
        CheckImage = Images.CheckmarkRound,
        CheckImageColor = nameof(ColorSuccess),
        CheckImageColorType = typeof(LogPanel),
        View = Ion.View.Header)]
    [StyleTrigger(nameof(StyleAttribute.Name), nameof(SuccessCount))]
    public bool FilterSuccess { get => Get(true); set => Set(value); }

    [Description("Show warnings.")]
    [Name("Show warnings")]
    [Styles.Check(Ion.Template.CheckImage, NameHide = true, Pin = Sides.LeftOrTop,
        CheckImage = Images.Warning,
        CheckImageColor = nameof(ColorWarning),
        CheckImageColorType = typeof(LogPanel),
        View = Ion.View.Header)]
    [StyleTrigger(nameof(StyleAttribute.Name), nameof(WarningCount))]
    public bool FilterWarning { get => Get(true); set => Set(value); }

    [Group(Group.Text), Image(Images.ArrowDownLeft)]
    [Style(Ion.Template.Check, NameHide = true, Index = int.MaxValue - 1,
        View = Ion.View.Header)]
    public bool TextWrap { get => Get(false); set => Set(value); }

    #endregion

    #endregion

    /// <see cref="Region.Method"/>
    #region

    protected override bool OnItemFilter(object input)
    {
        if (base.OnItemFilter(input))
        {
            if (input is Entry logEntry)
            {
                if (FilterType == ResultType.None)
                    return false;

                if (FilterType != ResultType.All)
                {
                    if (!FilterType.HasFlag(logEntry.Result.Type))
                        return false;
                }
                if (!FilterLevel.HasFlag(logEntry.Level))
                    return false;

                return true;
            }
        }
        return false;
    }

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        switch (e.PropertyName)
        {
            case nameof(ItemCount):
                Reset(() => ErrorCount);
                Reset(() => MessageCount);
                Reset(() => SuccessCount);
                Reset(() => WarningCount);
                break;

            case nameof(FilterError):
            case nameof(FilterMessage):
            case nameof(FilterSuccess):
            case nameof(FilterWarning):
                Reset(() => FilterType);
                break;

            case nameof(FilterLevel):
            case nameof(FilterType):
                Reset(() => ItemVisibility);
                break;
        }
    }

    #endregion
}