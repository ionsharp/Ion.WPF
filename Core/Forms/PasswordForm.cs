using Ion.Data;
using Ion.Numeral;
using Ion.Reflect;

namespace Ion;

[Styles.Object(GroupName = MemberGroupName.None, Filter = Filter.None)]
public record class PasswordForm(PasswordType type) : Form()
{
    [Hide]
    public PasswordType Type { get; private set; } = type;

    [Name("Password"), VisibilityTrigger(nameof(Type), Comparison.Equal, PasswordType.Default)]
    public string Password { get => Get(""); set => Set(value); }

    [Name("Pattern"), VisibilityTrigger(nameof(Type), Comparison.Equal, PasswordType.Pattern)]
    public Pattern PasswordPattern { get => Get<Pattern>(); set => Set(value); }

    [Name("Pin"), VisibilityTrigger(nameof(Type), Comparison.Equal, PasswordType.Pin)]
    public int PasswordPin { get => Get(0); set => Set(value); }
}