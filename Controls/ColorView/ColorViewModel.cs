using Ion.Colors;
using Ion.Controls;
using Ion.Core;
using Ion.Imaging;
using Ion.Numeral;
using Ion.Reflect;
using Ion.Text;
using Ion.Validation;
using System;
using System.Reflection;

namespace Ion.Media;

/// <inheritdoc/>
[Styles.Object(Name = "Color", NameHide = true,
    Filter = Filter.None, GroupName = MemberGroupName.None,
    Strict = MemberTypes.All)]
[Serializable]
public abstract record class ColorViewModel(IColor Color) : Model(), ICloneable
{
    private ColorViewModel() : this(default(IColor)) { }

    /// <exception cref="ArgumentOutOfRangeException"/>
    public static ColorViewModel New(IColor color)
    {
        color ??= new HSB();
        if (color is IColor3 a)
            return new ColorViewModel3(a);

        if (color is IColor4 b)
            return new ColorViewModel4(b);

        throw new ArgumentOutOfRangeException(nameof(color));
    }

    /// <see cref="Region.Event"/>

    [field: NonSerialized]
    public event EventHandler<EventArgs> ValueChanged;

    /// <see cref="Region.Field"/>

    public const string DefaultFormatName = "{0} ({1})";

    public const string DefaultFormatUnit = "{0}";

    public const string DefaultFormatValue = "N2";

    public const bool DefaultNormalize = false;

    public const int DefaultPrecision = 2;

    /// <see cref="Region.Property"/>

    public IColor Color { get; } = Color;

    public Type ColorType { get; } = Color?.GetType()
        ?? throw new ArgumentNullException(nameof(ColorType));

    public string Group { get; } = Color.GetType()?.Name[..1].ToUpper()
        ?? throw new ArgumentNullException(nameof(ColorType));

    public string Name { get; } = Color.GetType()?.Name
        ?? throw new ArgumentNullException(nameof(Color));

    ///

    public string FormatName
    { get => Get(DefaultFormatName); set => Set(value); }

    public string FormatUnit
    { get => Get(DefaultFormatUnit); set => Set(value); }

    public string FormatValue
    { get => Get(DefaultFormatValue); set => Set(value); }

    public bool Normalize
    { get => Get(DefaultNormalize); set => Set(value); }

    public int Precision
    { get => Get(DefaultPrecision); set => Set(value); }

    ///

    public abstract int Length { get; }

    /// <see cref="Region.Method"/>

    protected virtual void OnValueChanged() => ValueChanged?.Invoke(this, EventArgs.Empty);

    public override string ToString(string format, IFormatProvider provider) => GetColor().ToString(format, provider);

    public abstract IColor GetColor();
}

/// <inheritdoc/>
[Serializable]
public abstract record class ColorViewModel<T> : ColorViewModel where T : IVector<double>
{
    private ColorViewModel() : this(default(IColor)) { }

    protected ColorViewModel(ColorViewModel self) : base(self.Color) { }

    protected ColorViewModel(IColor color) : base(color) { }

    /// <see cref="DisplayX"/>
    #region

    [Styles.Text(Options = Option.Copy | Option.Paste,
        DescriptionTemplateType = typeof(XColor),
        DescriptionTemplate = nameof(ColorView.ComponentDescriptionTemplate),
        TextExpression = Expressions.DecimalNumber,
        Index = 0,
        Orientation = Orient.Horizontal,
        ValueTrigger = BindTrigger.LostFocus,
        Validate = [typeof(NumberRule), typeof(RequireRule)])]
    [StyleTrigger(nameof(StyleAttribute.Name), nameof(NameX))]
    [StyleTrigger(nameof(StyleAttribute.Description), nameof(ComponentX))]
    [StyleTrigger(nameof(StyleAttribute.RightText), nameof(UnitX))]
    public string DisplayX
    {
        get => GetDisplayValue(0);
        set => SetDisplayValue(value, 0);
    }

    #endregion

    /// <see cref="DisplayY"/>
    #region

    [Styles.Text(Options = Option.Copy | Option.Paste,
        DescriptionTemplateType = typeof(XColor),
        DescriptionTemplate = nameof(ColorView.ComponentDescriptionTemplate),
        TextExpression = Expressions.DecimalNumber,
        Index = 1,
        Orientation = Orient.Horizontal,
        ValueTrigger = BindTrigger.LostFocus,
        Validate = [typeof(NumberRule), typeof(RequireRule)])]
    [StyleTrigger(nameof(StyleAttribute.Name), nameof(NameY))]
    [StyleTrigger(nameof(StyleAttribute.Description), nameof(ComponentY))]
    [StyleTrigger(nameof(StyleAttribute.RightText), nameof(UnitY))]
    public string DisplayY
    {
        get => GetDisplayValue(1);
        set => SetDisplayValue(value, 1);
    }

    #endregion

    /// <see cref="DisplayZ"/>
    #region

    [Styles.Text(Options = Option.Copy | Option.Paste,
        DescriptionTemplateType = typeof(XColor),
        DescriptionTemplate = nameof(ColorView.ComponentDescriptionTemplate),
        TextExpression = Expressions.DecimalNumber,
        Index = 2,
        Orientation = Orient.Horizontal,
        ValueTrigger = BindTrigger.LostFocus,
        Validate = [typeof(NumberRule), typeof(RequireRule)])]
    [StyleTrigger(nameof(StyleAttribute.Name), nameof(NameZ))]
    [StyleTrigger(nameof(StyleAttribute.Description), nameof(ComponentZ))]
    [StyleTrigger(nameof(StyleAttribute.RightText), nameof(UnitZ))]
    public string DisplayZ
    {
        get => GetDisplayValue(2);
        set => SetDisplayValue(value, 2);
    }

    #endregion

    public Component ComponentX { get => Get<Component>(); private set => Set(value); }

    public Component ComponentY { get => Get<Component>(); private set => Set(value); }

    public Component ComponentZ { get => Get<Component>(); private set => Set(value); }

    public abstract Vector Maximum { get; }

    public abstract Vector Minimum { get; }

    public string NameX => FormatName.F(ComponentX.Symbol, ComponentX.Name);

    public string NameY => FormatName.F(ComponentY.Symbol, ComponentY.Name);

    public string NameZ => FormatName.F(ComponentZ.Symbol, ComponentZ.Name);

    public string UnitX => FormatUnit.F(ComponentX.Unit);

    public string UnitY => FormatUnit.F(ComponentY.Unit);

    public string UnitZ => FormatUnit.F(ComponentZ.Unit);

    public Double1 X { get => Get<Double1>(); set => Set(value); }

    public Double1 Y { get => Get<Double1>(); set => Set(value); }

    public Double1 Z { get => Get<Double1>(); set => Set(value); }

    protected virtual Range<double> GetRange(int index) => new(Minimum[index], Maximum[index]);

    protected virtual Double1 GetValue(int index) => index switch { 0 => X, 1 => Y, 2 => Z };

    protected virtual void SetValue(int index, Double1 result) { switch (index) { case 0: X = result; break; case 1: Y = result; break; case 2: Z = result; break; } }

    protected string GetDisplayValue(int index)
    {
        var j = (double)GetValue(index);
        return (Normalize == true ? j : GetRange(index).Denormalize(j))
            .Round(Precision).ToString(FormatValue, System.Globalization.CultureInfo.CurrentCulture);
    }

    protected void SetDisplayValue(string i, int index)
    {
        _ = double.TryParse(i, out double j);

        j = Normalize == true ? j : GetRange(index).Normalize(j);
        SetValue(index, (Double1)j);
    }

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        switch (e.PropertyName)
        {
            case nameof(ColorType):
                var component = IColor.Components[ColorType];
                ComponentX = component[0];
                ComponentY = component[1];
                ComponentZ = component[2];
                break;

            case nameof(FormatName):
                Reset(() => NameX);
                Reset(() => NameY);
                Reset(() => NameZ);
                break;
            case nameof(FormatUnit):
                Reset(() => UnitX);
                Reset(() => UnitY);
                Reset(() => UnitZ);
                break;
            case nameof(Normalize):
            case nameof(Precision):
                Reset(() => DisplayX);
                Reset(() => DisplayY);
                Reset(() => DisplayZ);
                break;

            case nameof(X):
                Reset(() => DisplayX);
                OnValueChanged();
                break;
            case nameof(Y):
                Reset(() => DisplayY);
                OnValueChanged();
                break;
            case nameof(Z):
                Reset(() => DisplayZ);
                OnValueChanged();
                break;
        }
    }
}

/// <inheritdoc/>
[Serializable]
public record class ColorViewModel3 : ColorViewModel<Vector3>
{
    private ColorViewModel3() : this(default(IColor3)) { }

    protected ColorViewModel3(ColorViewModel3 self) : base(self) { }

    public ColorViewModel3(IColor3 color) : base(color) { }

    public override int Length => 6;

    public override Vector Maximum => new(ComponentX.Maximum, ComponentY.Maximum, ComponentZ.Maximum);

    public override Vector Minimum => new(ComponentX.Minimum, ComponentY.Minimum, ComponentZ.Minimum);

    public override IColor GetColor()
    {
        var i = new Vector3<Double1>(X, Y, Z).Denormalize(new Vector3(Minimum[0], Minimum[1], Minimum[2]), new Vector3(Maximum[0], Maximum[1], Maximum[2]));
        return IColor.New(ColorType, i.X, i.Y, i.Z);
    }
}

/// <inheritdoc/>
[Serializable]
public record class ColorViewModel4 : ColorViewModel<Vector4>
{
    private ColorViewModel4() : this(default(IColor4)) { }

    protected ColorViewModel4(ColorViewModel4 self) : base(self) => DisplayW = DisplayW;

    public ColorViewModel4(IColor4 color) : base(color) { }

    public override int Length => 8;

    public override Vector Maximum => new(ComponentX.Maximum, ComponentY.Maximum, ComponentZ.Maximum, ComponentW.Maximum);

    public override Vector Minimum => new(ComponentX.Minimum, ComponentY.Minimum, ComponentZ.Minimum, ComponentW.Minimum);

    public Component ComponentW { get => Get<Component>(); private set => Set(value); }

    /// <see cref="DisplayW"/>
    #region

    [Styles.Text(Options = Option.Copy | Option.Paste,
        DescriptionTemplateType = typeof(XColor),
        DescriptionTemplate = nameof(ColorView.ComponentToolTipTemplate),
        TextExpression = Expressions.DecimalNumber,
        Index = 3,
        Orientation = Orient.Horizontal,
        ValueTrigger = BindTrigger.LostFocus,
        Validate = [typeof(NumberRule), typeof(RequireRule)])]
    [StyleTrigger(nameof(StyleAttribute.Description), nameof(ComponentW))]
    [StyleTrigger(nameof(StyleAttribute.Name), nameof(NameW))]
    [StyleTrigger(nameof(StyleAttribute.RightText), nameof(UnitW))]
    public string DisplayW { get => GetDisplayValue(3); set => SetDisplayValue(value, 3); }

    #endregion

    public string NameW => FormatName.F(ComponentW.Symbol, ComponentW.Name);

    public string UnitW => FormatUnit.F(ComponentW);

    public Double1 W { get => Get<Double1>(); set => Set(value); }

    protected override Double1 GetValue(int index) => index switch { 3 => W, _ => base.GetValue(index) };

    protected override void SetValue(int index, Double1 result)
    {
        if (index == 3)
        {
            W = result;
            return;
        }
        base.SetValue(index, result);
    }

    public override IColor GetColor()
    {
        var i = new Vector4<Double1>(X, Y, Z, W).Denormalize(new Vector3(Minimum[0], Minimum[1], Minimum[2]), new Vector3(Maximum[0], Maximum[1], Maximum[2]));
        return IColor.New(ColorType, i.X, i.Y, i.Z);
    }

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        switch (e.PropertyName)
        {
            case nameof(ColorType):
                ComponentW = IColor.Components[ColorType][3];
                break;

            case nameof(FormatName):
                Reset(() => NameW);
                break;
            case nameof(FormatUnit):
                Reset(() => UnitW);
                break;
            case nameof(Normalize):
            case nameof(Precision):
                Reset(() => DisplayW);
                break;

            case nameof(W):
                Reset(() => DisplayW);
                OnValueChanged();
                break;
        }
    }
}