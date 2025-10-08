using Ion.Numeral;

namespace Ion;

/// <summary>A visual representation for various forms of data.</summary>
/// <remarks><b>Actual templates must be defined in higher level assembly with accessible framework (example, WPF).</b></remarks>
public enum Template
{
    /// <remarks>Expects <see cref="string"/>.</remarks>
    [TemplateType<string>]
    Address,
    /// <remarks>Expects <see cref="double"/>.</remarks>
    [TemplateType<double>]
    Angle,
    Button,
    ButtonCancel,
    ButtonDefault,
    ButtonImage,
    /// <remarks>Expects <see cref="bool"/>.</remarks>
    [TemplateType<bool>]
    Check,
    /// <remarks>Expects <see cref="bool"/>.</remarks>
    [TemplateType<bool>]
    CheckImage,
    /// <remarks>Expects <see cref="bool"/>.</remarks>
    [TemplateType<bool>]
    CheckSwitch,
    /// <remarks>Expects <see cref="ByteVector4"/>.</remarks>
    [TemplateType<ByteVector4>]
    Color,
    ColorModel,
    /// <remarks>Expects <see cref="string"/>.</remarks>
    [TemplateType<string>]
    ColorText,
    Enum,
    EnumFlag,
    EnumFlagButton,
    Gradient,
    Image,
    ImageColor,
    ImageSlide,
    /// <remarks>Expects <see cref="string"/>.</remarks>
    [TemplateType<string>]
    ImageThumb,
    List,
    ListCombo,
    ListButton,
    Matrix,
    /// <remarks>Expects <see cref="double"/>.</remarks>
    [TemplateType<double>]
    Number,
    Object,
    /// <remarks>Expects <see cref="string"/>.</remarks>
    [TemplateType<string>]
    Password,
    /// <remarks>Expects <see cref="string"/>.</remarks>
    [TemplateType<string>]
    Path,
    /// <remarks>Expects <see cref="string"/>.</remarks>
    [TemplateType<string>]
    PathFile,
    /// <remarks>Expects <see cref="string"/>.</remarks>
    [TemplateType<string>]
    PathFolder,
    /// <remarks>Expects <see cref="Ion.Numeral.Pattern"/>.</remarks>
    [TemplateType<Pattern>]
    Pattern,
    Point,
    /// <remarks>Expects <see cref="double"/>.</remarks>
    [TemplateType<double>]
    Progress,
    /// <remarks>Expects <see cref="double"/>.</remarks>
    [TemplateType<double>]
    ProgressRound,
    /// <remarks>Expects <see cref="string"/>.</remarks>
    [TemplateType<string>]
    Text,
    /// <remarks>Expects <see cref="string"/>.</remarks>
    [TemplateType<string>]
    TextMarkDown,
    /// <remarks>Expects <see cref="string"/>.</remarks>
    [TemplateType<string>]
    TextMarkUp,
    /// <remarks>Expects <see cref="string"/>.</remarks>
    [TemplateType<string>]
    Token,
    Unit
}